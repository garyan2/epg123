using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace epg123Server
{
    class HttpFileServer : IDisposable
    {
        private readonly object _plutoLock = new object();
        private readonly object _hdhrLock = new object();
        private readonly object _epg123Lock = new object();
        private readonly object _stirrLock = new object();

        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private readonly Queue<HttpListenerContext> _queue;

        private bool IsListening => _listener != null && _listener.IsListening;

        public HttpFileServer()
        {
            _workers = new Thread[Environment.ProcessorCount];
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
            _listener.Prefixes.Add($"http://*:{Helper.TcpUdpPort}/output/");
            _listener.Prefixes.Add($"http://*:{Helper.TcpUdpPort}/");
            _listener.Start();
            _listenerThread.Start();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
            Logger.WriteInformation($"File server initialized with {_workers.Length} worker threads.");
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            foreach (var worker in _workers)
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
                try { context.Response.OutputStream?.Close(); } catch { }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            // ensure http GET methods only for images
            if (context.Request.HttpMethod != "GET")
            {
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                context.Response.Headers[HttpResponseHeader.Allow] = "GET";
                return;
            }
            string filepath = null, contentType = null;

            // service webpage
            switch (context.Request.RawUrl.ToLower())
            {
                case "/":
                case "/status.html":
                    var htmlBytes = Encoding.UTF8.GetBytes(WebStats.Html);
                    context.Response.ContentType = "text/html;charset-utf-8";
                    context.Response.Headers.Add("X-Robots-Tag", "none");
                    context.Response.OutputStream.Write(htmlBytes, 0, htmlBytes.Length);
                    return;
                case "/favicon.ico":
                    using (var memStream = new MemoryStream())
                    {
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("tokenServer.favicon.ico").CopyTo(memStream);
                        memStream.Position = 0;
                        context.Response.ContentType = "image/vnd.microsoft.icon";
                        memStream.CopyTo(context.Response.OutputStream);
                    }
                    return;
                case "/trace.log":
                    var fi1 = new FileInfo(Helper.Epg123TraceLogPath);
                    if (fi1.Exists)
                    {
                        using (var fs = new FileStream(fi1.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            context.Response.ContentType = "text/plain";
                            context.Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fi1.Name}\"");
                            fs.CopyTo(context.Response.OutputStream);
                        }
                    }
                    else context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                case "/server.log":
                    var fi2 = new FileInfo(Helper.ServerLogPath);
                    if (fi2.Exists)
                    {
                        using (var fs = new FileStream(fi2.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            context.Response.ContentType = "text/plain";
                            context.Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fi2.Name}\"");
                            fs.CopyTo(context.Response.OutputStream);
                        }
                    }
                    else context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
            }

            // ignore hacker/bot probing open ports if exposed to WAN
            if (!context.Request.RawUrl.ToLower().StartsWith("/output/"))
            {
                WebStats.IncrementBadActorRequest();
                context.Response.Abort();
                return;
            }

            // service files
            WebStats.IncrementFileRequestReceived();
            switch (context.Request.RawUrl.ToLower())
            {
                case "/output/epg123.mxf":
                    if (!File.Exists(Helper.Epg123ExePath)) break;
                    filepath = Helper.Epg123MxfPath;
                    contentType = "text/xml";
                    lock (_epg123Lock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(3))
                        {
                            Logger.WriteInformation($"EPG123 update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.Epg123ExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/epg123.xmltv":
                    if (!File.Exists(Helper.Epg123ExePath)) break;
                    filepath = Helper.Epg123XmltvPath;
                    contentType = "text/xml";
                    lock (_epg123Lock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(3))
                        {
                            Logger.WriteInformation($"EPG123 update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.Epg123ExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/hdhr2mxf.m3u": // SiliconDust provides 14 days of guide listings
                    if (!File.Exists(Helper.Hdhr2MxfExePath)) break;
                    filepath = Helper.Hdhr2MxfM3uPath;
                    contentType = "text/plain";
                    lock (_hdhrLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(3))
                        {
                            Logger.WriteInformation($"HDHR2MXF update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.Hdhr2MxfExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/hdhr2mxf.mxf": // SiliconDust provides 14 days of guide listings
                    if (!File.Exists(Helper.Hdhr2MxfExePath)) break;
                    filepath = Helper.Hdhr2MxfMxfPath;
                    contentType = "text/xml";
                    lock (_hdhrLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(3))
                        {
                            Logger.WriteInformation($"HDHR2MXF update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.Hdhr2MxfExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/hdhr2mxf.xmltv": // SiliconDust provides 14 days of guide listings
                    if (!File.Exists(Helper.Hdhr2MxfExePath)) break;
                    filepath = Helper.Hdhr2mxfXmltvPath;
                    contentType = "text/xml";
                    lock (_hdhrLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(3))
                        {
                            Logger.WriteInformation($"HDHR2MXF update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.Hdhr2MxfExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/plutotv.m3u": // PlutoTV provides 12 hours of guide listings
                    if (!File.Exists(Helper.PlutoTvExePath)) break;
                    filepath = Helper.PlutoTvM3uPath;
                    contentType = "text/plain";
                    lock (_plutoLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(1))
                        {
                            Logger.WriteInformation($"PlutoTV update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.PlutoTvExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/plutotv.xmltv": // PlutoTV provides 12 hours of guide listings
                    if (!File.Exists(Helper.PlutoTvExePath)) break;
                    filepath = Helper.PlutoTvXmltvPath;
                    contentType = "text/xml";
                    lock (_plutoLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(1))
                        {
                            Logger.WriteInformation($"PlutoTV update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.PlutoTvExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/stirrtv.m3u": // Stirr provides 24 hours of guide listings
                    if (!File.Exists(Helper.StirrTvExePath)) break;
                    filepath = Helper.StirrTvM3uPath;
                    contentType = "text/plain";
                    lock (_stirrLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(1))
                        {
                            Logger.WriteInformation($"Stirr update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.StirrTvExePath).WaitForExit();
                        }
                    }
                    break;
                case "/output/stirrtv.xmltv": // Stirr provides 24 hours of guide listings
                    if (!File.Exists(Helper.StirrTvExePath)) break;
                    filepath = Helper.StirrTvXmltvPath;
                    contentType = "text/xml";
                    lock (_stirrLock)
                    {
                        var fi = new FileInfo(filepath);
                        if (fi.LastWriteTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(1))
                        {
                            Logger.WriteInformation($"Stirr update triggered by {context.Request.RemoteEndPoint} (UserAgent: {context.Request.UserAgent}).");
                            Process.Start(Helper.StirrTvExePath).WaitForExit();
                        }
                    }
                    break;
            }
            if (filepath == null || !File.Exists(filepath))
            {
                Send404NotFound(context.Response, context.Request.RawUrl);
                return;
            }

            var acceptEncoding = context.Request.Headers.Get("Accept-Encoding");
            var acceptGzip = acceptEncoding?.Contains("gzip") ?? false;
            var acceptDeflate = acceptEncoding?.Contains("deflate") ?? false;

            var fileInfo = new FileInfo(filepath);
            context.Response.ContentType = contentType;
            context.Response.Headers[HttpResponseHeader.LastModified] = $"{fileInfo.LastWriteTime:R}";
            context.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileInfo.Name}\"");
            if (acceptGzip && File.Exists(fileInfo.FullName + ".gz"))
            {
                fileInfo = new FileInfo(fileInfo.FullName + ".gz");
                context.Response.Headers.Add(HttpResponseHeader.ContentEncoding, "gzip");
            }
            else if (acceptDeflate && File.Exists(fileInfo.FullName + ".zz"))
            {
                fileInfo = new FileInfo(fileInfo.FullName + ".zz");
                context.Response.Headers.Add(HttpResponseHeader.ContentEncoding, "deflate");
            }
            else
            {
                context.Response.ContentLength64 = fileInfo.Length;
            }

            WebStats.AddFileDownload(fileInfo.Length);
            try
            {
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.CopyTo(context.Response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{fileInfo.FullName} HandleFileRequest() Exception:{Helper.ReportExceptionMessages(ex)}");
            }
        }

        private static void Send404NotFound(HttpListenerResponse response, string asset)
        {
            WebStats.IncrementHttpStat(404);
            response.StatusCode = (int)HttpStatusCode.NotFound;
            Logger.WriteError($"{asset} 404 Not Found");
        }
    }
}