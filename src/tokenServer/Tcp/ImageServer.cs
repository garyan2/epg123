using GaRyan2;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace epg123Server
{
    class HttpImageServer : IDisposable
    {
        private bool _limitExceeded;
        private bool _serviceUnavailable;
        private HashSet<string> queuedImages = new HashSet<string>();
        private readonly object _limitLock = new object();
        private readonly object _tokenLock = new object();
        private readonly object _serverLock = new object();
        private readonly object _queueLock = new object();

        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private readonly Queue<HttpListenerContext> _queue;

        private bool IsListening => _listener != null && _listener.IsListening;

        public HttpImageServer()
        {
            _workers = new Thread[Environment.ProcessorCount * 3];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener
            {
                IgnoreWriteExceptions = true
            };
            _listenerThread = new Thread(HandleRequests);
        }

        public void Start()
        {
            _listener.Prefixes.Add($"http://*:{Helper.TcpUdpPort}/image/");
            _listener.Prefixes.Add($"http://*:{Helper.TcpUdpPort}/logos/");
            _listener.Start();
            _listenerThread.Start();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
            Logger.WriteInformation($"Image server initialized with {_workers.Length} worker threads.");
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            foreach (Thread worker in _workers)
            {
                worker.Join();
            }
            _listener.Stop();
        }

        private void HandleRequests()
        {
            while (IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);
                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle })) return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch { return; }
        }

        private void Worker()
        {
            WaitHandle[] wait = new[] { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        context = _queue.Dequeue();
                    }
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try { ProcessRequest(context); }
                catch (Exception ex) { Logger.WriteError($"Worker() Exception:{Helper.ReportExceptionMessages(ex)}"); }
                context.Response.OutputStream?.Close();
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            if (context.Request.RawUrl.StartsWith("/logos/custom"))
            {
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        var response = Directory.GetFiles(Helper.Epg123LogosFolder, "*.png").ToList().Select(logo => logo.Substring(Helper.Epg123LogosFolder.Length)).ToList();
                        context.Response.ContentType = "application/json";
                        var content = JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                        {
                            ms.Position = 0;
                            ms.CopyTo(context.Response.OutputStream);
                        }
                        break;
                    case "PUT":
                        var file = context.Request.RawUrl.Substring("/logos/custom/".Length);
                        using (var filestream = new FileStream($"{Helper.Epg123LogosFolder}{file}", FileMode.Create, FileAccess.Write))
                        {
                            context.Request.InputStream.CopyTo(filestream);
                            filestream.Flush();
                        }
                        context.Response.ContentType = "application/json";
                        var pContent = JsonConvert.SerializeObject(new BaseResponse(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(pContent)))
                        {
                            ms.Position = 0;
                            ms.CopyTo(context.Response.OutputStream);
                        }
                        break;
                    case "DELETE":
                        var logofile = context.Request.RawUrl.Substring("/logos/custom/".Length);
                        if (!string.IsNullOrEmpty(logofile) && File.Exists($"{Helper.Epg123LogosFolder}{logofile}"))
                        {
                            if (Helper.DeleteFile($"{Helper.Epg123LogosFolder}{logofile}"))
                            {
                                context.Response.ContentType = "application/json";
                                var dContent = JsonConvert.SerializeObject(new BaseResponse(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(dContent)))
                                {
                                    ms.Position = 0;
                                    ms.CopyTo(context.Response.OutputStream);
                                }
                                return;
                            }
                        }
                        Send404NotFound(context.Response, $"/logos/{logofile}");
                        break;
                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        context.Response.Headers[HttpResponseHeader.Allow] = "GET, PUT, DELETE";
                        break;
                }
            }
            // ensure http GET methods only for images
            else if (context.Request.HttpMethod != "GET")
            {
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                context.Response.Headers[HttpResponseHeader.Allow] = "GET";
            }
            else if (context.Request.RawUrl.StartsWith("/image/"))
            {
                var dupe = false;
                lock (_queueLock) dupe = !queuedImages.Add(context.Request.RawUrl);
                if (dupe)
                {
                    while (queuedImages.Contains(context.Request.RawUrl)) Thread.Sleep(10);
                }
                ServiceImageRequest(context);
                lock (_queueLock) { queuedImages.Remove(context.Request.RawUrl); }
            }
            else if (context.Request.RawUrl.StartsWith("/logos/"))
            {
                _ = DateTime.TryParse(context.Request.Headers.Get("If-Modified-Since"), out DateTime ifModifiedSince);
                ServiceLogoRequest(context.Response, context.Request.RawUrl, ifModifiedSince.ToUniversalTime());
            }
        }

        public void ServiceImageRequest(HttpListenerContext context, bool retry = false)
        {
            if (!JsonImageCache.cacheReady)
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }

            var request = context.Request;
            var asset = request.RawUrl;

            // determine if conditional request
            var conditional = DateTimeOffset.TryParse(request.Headers.Get("If-Modified-Since"), out DateTimeOffset ifModifiedSince);

            // update stats
            if (!retry)
            {
                if (conditional) WebStats.IncrementConditionalRequestReceived();
                else WebStats.IncrementRequestReceived();
            }

            // throttle refreshing images that have already been checked recently
            var fileInfo = JsonImageCache.cacheImages ? JsonImageCache.GetCachedImage(asset) : null;
            if (JsonImageCache.cacheImages && JsonImageCache.IsImageRecent(asset.Substring(7), ifModifiedSince) &&
                Send304OrImageFromCache(context.Response, ifModifiedSince, fileInfo)) return;

            // don't try to download from SD if token is invalid
            if (!SchedulesDirect.GoodToken) goto NoToken;

            // add if-modified-since as needed
            if ((fileInfo?.LastWriteTimeUtc.Ticks ?? DateTime.MinValue.Ticks) > ifModifiedSince.Ticks)
            {
                ifModifiedSince = fileInfo.LastWriteTimeUtc;
                conditional = false;
            }

            try
            {
                using (var response = SchedulesDirect.GetImage(asset, ifModifiedSince))
                using (var memStream = new MemoryStream())
                {
                    if (response == null) goto NoToken;
                    response.Content?.ReadAsStreamAsync()?.Result?.CopyTo(memStream);

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotModified)
                    {
                        if (_limitExceeded) lock (_limitLock) { _limitExceeded = WebStats.LimitLocked = false; }
                        if (_serviceUnavailable) { lock (_serverLock) _serviceUnavailable = false; }
                    }

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            // save image to cache if enabled
                            if (JsonImageCache.cacheImages)
                            {
                                memStream.Position = 0;
                                var filename = asset.Substring(7);
                                var dirInfo = Directory.CreateDirectory($"{Helper.Epg123ImageCache}{filename.Substring(0, 1)}");
                                var location = $"{dirInfo.FullName}\\{filename}";
                                using (var fStream = new FileStream(location, FileMode.Create, FileAccess.Write))
                                {
                                    memStream.CopyTo(fStream);
                                    fStream.Flush();
                                }
                                File.SetLastWriteTimeUtc(location, response.Content.Headers.LastModified.Value.UtcDateTime);
                                JsonImageCache.AddImageToCache(filename, response.Content.Headers.LastModified.Value, memStream.Length);
                            }
                            // send response to client
                            SendHttpResponse(context.Response, memStream, response);
                            WebStats.AddSdDownload(memStream.Length);
                            return;
                        case HttpStatusCode.NotModified:
                            if (conditional) SendHttpResponse(context.Response, memStream, response);
                            else SendImage(context.Response, fileInfo);
                            return;
                        case HttpStatusCode.ServiceUnavailable:
                            if (!_serviceUnavailable) { lock (_serverLock) _serviceUnavailable = true; }
                            break;
                        case HttpStatusCode.Unauthorized:
                            lock (_tokenLock)
                            {
                                if (!SchedulesDirect.GoodToken) break;
                                WebStats.DecrementHttpStat(401);
                                if (SchedulesDirect.GetToken())
                                {
                                    ServiceImageRequest(context, true);
                                    return;
                                }
                            }
                            break;
                        case (HttpStatusCode)429:
                            if (!_limitExceeded) { lock (_limitLock) _limitExceeded = WebStats.LimitLocked = true; }
                            break;
                    }
                    if (Send304OrImageFromCache(context.Response, ifModifiedSince, fileInfo)) return;

                    // send response to client
                    SendHttpResponse(context.Response, memStream, response);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{asset} ServiceImageRequest() Exception:{Helper.ReportExceptionMessages(ex)}");
                if (context.Response.StatusCode <= (int)HttpStatusCode.OK) context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                WebStats.IncrementHttpStat(context.Response.StatusCode);
                return;
            }

        NoToken:
            // fallback for failed connection or no token
            if (Send304OrImageFromCache(context.Response, ifModifiedSince, fileInfo)) return;

            // nothing to give the client
            Send503NotUnavailable(context.Response, asset);
        }

        private static bool Send304OrImageFromCache(HttpListenerResponse response, DateTimeOffset ifModifiedSince, FileInfo fileInfo, bool logo = false)
        {
            // determine if 304 response is appropriate
            if (ifModifiedSince.Ticks != DateTime.MinValue.Ticks && ifModifiedSince.UtcDateTime.AddSeconds(1) > (fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue))
            {
                WebStats.IncrementHttpStat(304);
                response.StatusCode = (int)HttpStatusCode.NotModified;
                response.Headers[HttpResponseHeader.LastModified] = $"{ifModifiedSince.UtcDateTime:R}";
                return true;
            }

            // if for some reason cache is wrong and file does not exist
            if (fileInfo == null || !fileInfo.Exists) return false;

            // send image from cache
            SendImage(response, fileInfo);

            // update stats
            if (!logo) WebStats.AddCacheDownload(fileInfo.Length);
            else WebStats.AddLogoDownload(fileInfo.Length);
            return true;
        }

        private static void Send404NotFound(HttpListenerResponse response, string asset)
        {
            WebStats.IncrementHttpStat(404);
            response.StatusCode = (int)HttpStatusCode.NotFound;
            Logger.WriteError($"{asset} 404 Not Found");
        }

        private static void Send503NotUnavailable(HttpListenerResponse response, string asset)
        {
            WebStats.IncrementHttpStat(503);
            response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            Logger.WriteError($"{asset} 503 Service Unavailable");
        }

        private static void SendImage(HttpListenerResponse response, FileInfo fileInfo)
        {
            var ext = fileInfo.Extension.ToLower().Replace("jpg", "jpeg").Substring(1);

            // http
            response.StatusCode = (int)HttpStatusCode.OK;

            // response headers
            response.Headers[HttpResponseHeader.AcceptRanges] = "bytes";

            // content headers
            response.ContentType = $"image/{ext}";
            response.Headers[HttpResponseHeader.LastModified] = $"{fileInfo.LastWriteTimeUtc:R}";
            response.ContentLength64 = fileInfo.Length;

            try
            {
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.CopyTo(response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{fileInfo.FullName} SendImage() Exception:{Helper.ReportExceptionMessages(ex)}");
            }
        }

        private static void SendHttpResponse(HttpListenerResponse response, MemoryStream memStream, HttpResponseMessage httpResponse)
        {
            memStream.Position = 0;

            // http
            response.StatusCode = (int)httpResponse.StatusCode;

            // response headers
            response.Headers[HttpResponseHeader.AcceptRanges] = httpResponse.Headers?.AcceptRanges?.ToString();
            response.Headers[HttpResponseHeader.Server] = httpResponse.Headers?.Server?.ToString();

            // content headers
            if (httpResponse.Content?.Headers?.ContentEncoding?.Count > 0) response.ContentEncoding = Encoding.GetEncoding(httpResponse.Content.Headers.ContentEncoding.ToString());
            response.ContentType = httpResponse.Content?.Headers?.ContentType?.MediaType;
            if (httpResponse.Content?.Headers?.LastModified != null) response.Headers[HttpResponseHeader.LastModified] = httpResponse.Content.Headers.LastModified.Value.ToString("R");
            response.ContentLength64 = memStream.Length;

            if (memStream.Length > 0) memStream.CopyTo(response.OutputStream);
        }

        private static void ServiceLogoRequest(HttpListenerResponse response, string asset, DateTimeOffset ifModifiedSince)
        {
            WebStats.IncrementLogoRequestReceived();
            var fileInfo = new FileInfo($"{Helper.Epg123LogosFolder}{asset.Substring(7)}");
            if (fileInfo.Exists && Send304OrImageFromCache(response, ifModifiedSince, fileInfo, true)) return;

            // nothing to give the client
            Send404NotFound(response, asset);
        }
    }
}