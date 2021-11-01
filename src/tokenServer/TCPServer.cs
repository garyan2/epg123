using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json;

namespace tokenServer
{
    public static class WebRequestExtensions
    {
        public static WebResponse GetResponseWithoutException(this WebRequest request)
        {
            try
            {
                return request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response == null) throw e;
                return e.Response;
            }
        }
    }

    public partial class Server
    {
        private TcpListener _tcpListener;
        private bool _limitExceeded;
        private readonly object _limitLock = new object();
        private readonly object _tokenLock = new object();

        public void StartTcpListener()
        {
            _tcpListener = new TcpListener(IPAddress.Any, Helper.TcpPort);
            _tcpListener.Start(200);

            try
            {
                while (true)
                {
                    new Thread(HandleDevice).Start(_tcpListener.AcceptTcpClient());
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.Interrupted)
                {
                    Helper.WriteLogEntry($"StartTcpListener() - {e.Message}");
                    Cleanup();
                }
            }
        }

        public void HandleDevice(object obj)
        {
            using (var client = obj as TcpClient)
            using (var netStream = client.GetStream())
            {
                // wait until we have data available on the stream to read
                try
                {
                    var tick = 200;
                    do
                    {
                        Thread.Sleep(1);
                    } while (client.Available == 0 && --tick > 0);

                    if (client.Available == 0) return;
                }
                catch { return; }

                var asset = string.Empty;
                try
                {
                    // read the client request and store
                    var bytes = new byte[client.Available];
                    if (netStream.Read(bytes, 0, bytes.Length) == 0) return;
                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower();

                    // determine asset requested and if-modified-since header
                    var ifModifiedSince = new DateTime();
                    using (var reader = new StringReader(data))
                    {
                        var startline = HttpUtility.UrlDecode(reader.ReadLine()).Split(' ');
                        if (!startline[0].Equals("get")) return;
                        asset = startline[1];

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var separator = line.IndexOf(':');
                            if (separator > 0 && line.Substring(0, separator).Equals("if-modified-since"))
                            {
                                ifModifiedSince = DateTime.Parse(line.Substring(separator + 1).Trim()) + TimeSpan.FromSeconds(1);
                            }
                        }
                    }

                    if (asset.StartsWith("/image/"))
                    {
                        ProcessImageRequest(netStream, asset, ifModifiedSince);
                        return;
                    }
                    if (asset.StartsWith("/logos/"))
                    {
                        ProcessLogoRequest(netStream, asset, ifModifiedSince);
                        return;
                    }
                    if (asset.Equals("/") || asset.Equals("/status.html"))
                    {
                        var htmlBytes = Encoding.UTF8.GetBytes(WebStats.Html);
                        var headerBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n" +
                                                                 $"Date: {DateTime.UtcNow:R}\r\n" +
                                                                 $"Content-Type: text/html;charset=utf-8\r\n" +
                                                                 $"Content-Length: {htmlBytes.Length}\r\n\r\n");
                        netStream.Write(headerBytes, 0, headerBytes.Length);
                        netStream.Write(htmlBytes, 0, htmlBytes.Length);
                        return;
                    }
                    ProcessFileRequest(netStream, asset);
                }
                catch (Exception e)
                {
                    Helper.WriteLogEntry($"{asset}\nHandleDevice() - {e.Message}\n{e.StackTrace}");
                }
            }
        }

        private static void ProcessLogoRequest(Stream stream, string asset, DateTime ifModifiedSince)
        {
            WebStats.IncrementLogoRequestReceived();
            var fileInfo = new FileInfo($"{Helper.Epg123LogosFolder}\\{asset.Substring(7)}");
            if (fileInfo.Exists && Send304OrImageFromCache(stream, ifModifiedSince, fileInfo, true)) return;

            // nothing to give the client
            WebStats.Increment404Response();
            var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        #region ========== Send Images ==========
        private void ProcessImageRequest(Stream stream, string asset, DateTime ifModifiedSince, bool retry = false)
        {
            // update web stats
            if (ifModifiedSince == DateTime.MinValue) WebStats.IncrementRequestReceived();
            else WebStats.IncrementConditionalRequestReceived();

            // throttle refreshing images that have already been checked recently
            if (JsonImageCache.cacheImages && JsonImageCache.IsImageRecent(asset.Substring(7)))
            {
                if (Send304OrImageFromCache(stream, ifModifiedSince, JsonImageCache.GetCachedImage(asset)))
                    return;
            }

            // build url for Schedules Direct with token
            var url = $"{Helper.SdBaseName}{asset}";
            if (!string.IsNullOrEmpty(TokenService.Token)) url += $"?token={TokenService.Token}";

            // create web request
            var request = (HttpWebRequest)WebRequest.Create(url);

            // add if-modified-since as needed
            var fileInfo = JsonImageCache.cacheImages ? JsonImageCache.GetCachedImage(asset) : null;
            if (ifModifiedSince != DateTime.MinValue || fileInfo != null)
            {
                request.IfModifiedSince = new[] {ifModifiedSince.ToUniversalTime(), fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue}.Max();
            }

            try
            {
                // update web stats
                if (request.IfModifiedSince == DateTime.MinValue) WebStats.IncrementRequestSent();
                else WebStats.IncrementConditionalRequestSent();

                // get response
                using (var response = request.GetResponseWithoutException() as HttpWebResponse)
                using (var memStream = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(memStream);
                    memStream.Position = 0;

                    // if image is included
                    if (memStream.Length > 0 && response.ContentType.StartsWith("image"))
                    {
                        // update web stats
                        WebStats.AddSdDownload(memStream.Length);

                        // success implies limit has not been exceeded
                        lock (_limitLock) {_limitExceeded = WebStats.LimitLocked = false;}

                        // send response to client
                        var message = $"HTTP/1.1 {(int) response.StatusCode} {response.StatusDescription}\r\n";
                        for (var i = 0; i < response.Headers.Count; ++i)
                        {
                            message += $"{response.Headers.Keys[i]}: {response.Headers[i]}\r\n";
                        }
                        var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        memStream.CopyTo(stream);
                        if (!JsonImageCache.cacheImages) return;

                        // save image to cache if enabled
                        memStream.Position = 0;
                        var filename = asset.Substring(7);
                        var dirInfo = Directory.CreateDirectory($"{Helper.Epg123ImageCache}\\{filename.Substring(0, 1)}");
                        var location = $"{dirInfo.FullName}\\{filename}";
                        using (var fStream = new FileStream(location, FileMode.Create, FileAccess.Write))
                        {
                            memStream.CopyTo(fStream);
                            fStream.Flush();
                        }
                        File.SetLastWriteTimeUtc(location, response.LastModified);
                        JsonImageCache.AddImageToCache(filename);
                        return;
                    }

                    // handle any 304 or 404 responses by providing either a 304 response or the cached image if available
                    if (Send304OrImageFromCache(stream, ifModifiedSince, fileInfo)) return;

                    // reject a 200 response that has no content
                    if (response.StatusCode == HttpStatusCode.OK && memStream.Length == 0)
                    {
                        WebStats.Increment502Response();
                        var message = "HTTP/1.1 502 Bad Gateway\r\n" +
                                      $"Date: {DateTime.UtcNow:R}\r\n";
                        var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        return;
                    }

                    // handle any errors from SD
                    HandleRequestError(stream, asset, response, ifModifiedSince, memStream, retry); // 200 for exceeded image limit or unknown user
                    return;
                }
            }
            catch (WebException e)
            {
                Helper.WriteLogEntry($"{asset}\nWebException: {e.Status} - {e.Message}");
            }

            // fallback for failed connection
            if (Send304OrImageFromCache(stream, ifModifiedSince, fileInfo)) return;

            // nothing to give the client
            WebStats.Increment404Response();
            var messageBytes2 = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
            stream.Write(messageBytes2, 0, messageBytes2.Length);
        }

        private static bool Send304OrImageFromCache(Stream stream, DateTime ifModifiedSince, FileInfo fileInfo, bool logo = false)
        {
            if (ifModifiedSince != DateTime.MinValue && ifModifiedSince.ToUniversalTime() + TimeSpan.FromSeconds(1) > (fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue))
            {
                WebStats.Increment304Response();
                var message = "HTTP/1.1 304 Not Modified\r\n" +
                              $"Date: {DateTime.UtcNow:R}\r\n" +
                              $"Last-Modified: {ifModifiedSince.ToUniversalTime() - TimeSpan.FromSeconds(1):R}\r\n";
                var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
                return true;
            }

            if (fileInfo == null) return false;
            if (!logo) WebStats.AddCacheDownload(fileInfo.Length);
            else WebStats.AddLogoDownload(fileInfo.Length);
            SendImage(stream, fileInfo);
            return true;
        }

        private static void SendImage(Stream stream, FileInfo fileInfo)
        {
            if (fileInfo != null && fileInfo.Exists)
            {
                var ext = fileInfo.Extension.ToLower().Substring(1);
                var messageBytes = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n" +
                                                          "Accept-Ranges: bytes\r\n" +
                                                          $"Content-Length: {fileInfo.Length}\r\n" +
                                                          $"Content-Type: image/{ext}\r\n" +
                                                          $"Date: {DateTime.UtcNow:R}\r\n" +
                                                          $"Last-Modified: {fileInfo.LastWriteTimeUtc:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);

                try
                {
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        fs.CopyTo(stream);
                    }
                }
                catch (Exception e)
                {
                    Helper.WriteLogEntry($"{fileInfo.FullName}\n{e.Message}");
                }
            }
            else
            {
                WebStats.Increment404Response();
                var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
        #endregion

        #region ========== Send Files ==========
        private static void ProcessFileRequest(Stream stream, string asset)
        {
            string filepath = null, type = "text/xml", disposition = "attachment";
            switch (asset)
            {
                case "/trace.log":
                    filepath = Helper.Epg123TraceLogPath;
                    type = "text/plain";
                    disposition = "inline";
                    break;
                case "/server.log":
                    filepath = Helper.Epg123ServerLogPath;
                    type = "text/plain";
                    disposition = "inline";
                    break;
                case "/output/epg123.mxf":
                    filepath = Helper.Epg123MxfPath;
                    break;
                case "/output/epg123.xmltv":
                    filepath = Helper.Epg123XmltvPath;
                    break;
            }

            if (filepath != null)
            {
                WebStats.IncrementFileRequestReceived();
                SendFile(stream, new FileInfo(filepath), type, disposition);
            }
            else
            {
                var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }

        private static void SendFile(Stream stream, FileInfo fileInfo, string contentType, string contentDisposition)
        {
            if (fileInfo.Exists)
            {
                var messageBytes = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n" +
                                                          $"Date: {DateTime.UtcNow:R}\r\n" +
                                                          $"Content-Type: {contentType}\r\n" +
                                                          $"Content-Disposition: {contentDisposition}; filename=\"{fileInfo.Name}\"\r\n" +
                                                          $"Last-Modified: {fileInfo.LastWriteTimeUtc:R}\r\n" +
                                                          "Content-Transfer-Encoding: binary\r\n" +
                                                          $"Content-Length: {fileInfo.Length}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);

                try
                {
                    WebStats.AddFileDownload(fileInfo.Length);
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        fs.CopyTo(stream);
                    }
                }
                catch (Exception e)
                {
                    Helper.WriteLogEntry($"{fileInfo.FullName}\n{e.Message}");
                }
            }
            else
            {
                WebStats.Increment404Response();
                var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
        #endregion

        private void HandleRequestError(Stream stream, string asset, WebResponse webResponse, DateTime ifModifiedSince, MemoryStream memStream, bool retry)
        {
            using (var sr = new StreamReader(memStream, Encoding.UTF8))
            using (var response = webResponse as HttpWebResponse)
            {
                var statusCode = (int) response.StatusCode;
                var statusDescription = response.StatusDescription;

                var resp = sr.ReadToEnd();
                var err = new BaseResponse();
                try
                {
                    err = JsonConvert.DeserializeObject<BaseResponse>(resp);
                    switch (err.Code)
                    {
                        case 5000: // IMAGE_NOT_FOUND
                            statusCode = 404;
                            statusDescription = "Not Found";
                            break;
                        case 5002: // MAX_IMAGE_DOWNLOADS
                            lock (_limitLock) _limitExceeded = WebStats.LimitLocked = true;
                            statusCode = 429;
                            statusDescription = "Too Many Requests";
                            break;
                        case 5004: // UNKNOWN_USER
                            bool refresh;
                            lock (_tokenLock) refresh = TokenService.GoodToken && TokenService.RefreshToken && !retry && TokenService.RefreshTokenFromSD();

                            if (refresh)
                            {
                                WebStats.AddSdDownload(0);
                                ProcessImageRequest(stream, asset, ifModifiedSince, true);
                                return;
                            }
                            statusCode = 401;
                            statusDescription = "Unauthorized";
                            break;
                    }
                }
                catch
                {
                    // do nothing
                }

                switch (statusCode)
                {
                    case 401:
                        WebStats.Increment401Response();
                        break;
                    case 404:
                        WebStats.Increment404Response();
                        break;
                    case 429:
                        WebStats.Increment429Response();
                        break;
                    default:
                        WebStats.IncrementOtherResponse();
                        break;
                }

                // log the error response
                if (!(err.Code == 5002 && _limitExceeded) && !(err.Code == 5004 && !TokenService.GoodToken))
                {
                    Helper.WriteLogEntry($"{asset}: {statusCode} {statusDescription}{(!string.IsNullOrEmpty(resp) ? $"\n{resp}" : "")}");
                }

                // send response header and body to client
                var message = $"HTTP/1.1 {statusCode} {statusDescription}\r\n";
                for (var i = 0; i < response.Headers.Count; ++i)
                {
                    message += $"{response.Headers.Keys[i]}: {response.Headers[i]}\r\n";
                }
                var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);

                if (string.IsNullOrEmpty(resp)) return;
                var body = Encoding.UTF8.GetBytes(resp);
                stream.Write(body, 0, body.Length);
            }
        }
    }
}
