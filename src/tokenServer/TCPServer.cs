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
            _tcpListener.Start(100);

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

                try
                {
                    // read the client request and store
                    var bytes = new byte[client.Available];
                    if (netStream.Read(bytes, 0, bytes.Length) == 0) return;
                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower();

                    // determine asset requested and if-modified-since header
                    var asset = string.Empty;
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
                    ProcessFileRequest(netStream, asset);
                }
                catch (Exception e)
                {
                    Helper.WriteLogEntry($"HandleDevice() - {e.Message}");
                }
            }
        }

        private static void ProcessLogoRequest(Stream stream, string asset, DateTime ifModifiedSince)
        {
            var fileInfo = new FileInfo($"{Helper.Epg123LogosFolder}\\{asset.Substring(7)}");
            if (fileInfo.Exists && Send304OrImageFromCache(stream, ifModifiedSince, fileInfo)) return;

            // nothing to give the client
            var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        #region ========== Send Images ==========
        private void ProcessImageRequest(Stream stream, string asset, DateTime ifModifiedSince, bool retry = false)
        {
            // throttle refreshing images that have already been checked recently
            if (_cache.cacheImages && _cache.ImageCache.ContainsKey(asset.Substring(7)) &&
                _cache.ImageCache[asset.Substring(7)].LastUsed + TimeSpan.FromHours(12) > DateTime.Now)
            {
                if (Send304OrImageFromCache(stream, ifModifiedSince, new FileInfo($"{Helper.Epg123ImageCache}\\{asset.Substring(7, 1)}\\{asset.Substring(7)}")))
                    return;
            }

            // build url for Schedules Direct with token
            var url = $"{Helper.SdBaseName}{asset}";
            if (!string.IsNullOrEmpty(TokenService.Token)) url += $"?token={TokenService.Token}";

            // create web request
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 6000;

            // add if-modified-since as needed
            var fileInfo = _cache.cacheImages ? _cache.GetCachedImage(asset) : null;
            if (ifModifiedSince != DateTime.MinValue || fileInfo != null)
            {
                request.IfModifiedSince = new[] {ifModifiedSince.ToUniversalTime(), fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue}.Max();
            }

            try
            {
                // get response
                using (var response = request.GetResponseWithoutException() as HttpWebResponse)
                {
                    // if image is included
                    if (response.ContentLength > 0 && response.ContentType.StartsWith("image"))
                    {
                        // success implies limit has not been exceeded
                        lock (_limitLock) _limitExceeded = false;

                        // update cache if enabled and send to client
                        if (_cache.cacheImages)
                        {
                            SendImage(stream, _cache.SaveImageToCache(asset, response.GetResponseStream(), response.LastModified));
                            return;
                        }

                        // send response to client
                        var message = $"HTTP/1.1 {(int) response.StatusCode} {response.StatusDescription}\r\n";
                        for (var i = 0; i < response.Headers.Count; ++i)
                        {
                            message += $"{response.Headers.Keys[i]}: {response.Headers[i]}\r\n";
                        }

                        var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        response.GetResponseStream()?.CopyTo(stream);
                        return;
                    }

                    // handle responses that do not include images (200 w/ Exceeded Limit, 304, and 404 primarily)
                    if (Send304OrImageFromCache(stream, ifModifiedSince, fileInfo)) return; // 304 or 404 for either client cache or server cache
                    HandleRequestError(stream, asset, response, ifModifiedSince, retry); // 200 for exceeded image limit or unknown user
                    return;
                }
            }
            catch (WebException e)
            {
                Helper.WriteLogEntry($"WebException: {e.Status}\n{e.Message}");
            }

            // fallback for failed connection
            if (Send304OrImageFromCache(stream, ifModifiedSince, fileInfo)) return;

            // nothing to give the client
            var messageBytes2 = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
            stream.Write(messageBytes2, 0, messageBytes2.Length);
        }

        private static bool Send304OrImageFromCache(Stream stream, DateTime ifModifiedSince, FileInfo fileInfo)
        {
            if (ifModifiedSince != DateTime.MinValue && ifModifiedSince.ToUniversalTime() + TimeSpan.FromSeconds(1) > (fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue))
            {
                var message = "HTTP/1.1 304 Not Modified\r\n" +
                              $"Date: {DateTime.UtcNow:R}\r\n" +
                              $"Last-Modified: {ifModifiedSince.ToUniversalTime() - TimeSpan.FromSeconds(1):R}\r\n";
                var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
                return true;
            }

            if (fileInfo == null) return false;
            SendImage(stream, fileInfo);
            return true;
        }

        private static void SendImage(Stream stream, FileInfo fileInfo)
        {
            if (fileInfo != null && fileInfo.Exists)
            {
                var messageBytes = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n" +
                                                          "Accept-Ranges: bytes\r\n" +
                                                          $"Content-Length: {fileInfo.Length}\r\n" +
                                                          "Content-Type: image/jpg\r\n" +
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
            SendFile(stream, filepath == null ? null : new FileInfo(filepath), type, disposition);
        }

        private static void SendFile(Stream stream, FileInfo fileInfo, string contentType, string contentDisposition)
        {
            if (fileInfo != null && fileInfo.Exists)
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
                var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
        #endregion

        private void HandleRequestError(Stream stream, string asset, WebResponse webResponse, DateTime ifModifiedSince, bool retry)
        {
            using (var sr = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
            {
                var resp = sr.ReadToEnd();
                var err = JsonConvert.DeserializeObject<BaseResponse>(resp);
                if (!string.IsNullOrEmpty(resp) && !_limitExceeded) Helper.WriteLogEntry($"{asset}:\n{resp}");

                using (var response = webResponse as HttpWebResponse)
                {
                    if (response == null) return;

                    var statusCode = (int) response.StatusCode;
                    var statusDescription = response.StatusDescription;

                    switch (err?.Code)
                    {
                        case 5002: // MAX_IMAGE_DOWNLOADS
                            lock (_limitLock)
                            {
                                statusCode = 429;
                                statusDescription = "Too Many Requests";
                                if (_limitExceeded) break;
                                _limitExceeded = true;
                            }
                            break;
                        case 5004: // UNKNOWN_USER
                            lock (_tokenLock)
                            {
                                statusCode = 401;
                                statusDescription = "Unauthorized";
                                if (!TokenService.GoodToken) break;
                                if (TokenService.RefreshToken && !retry && TokenService.RefreshTokenFromSD())
                                {
                                    ProcessImageRequest(stream, asset, ifModifiedSince, true);
                                    return;
                                }
                            }
                            break;
                    }

                    var body = Encoding.UTF8.GetBytes(resp);
                    var messageBytes = Encoding.UTF8.GetBytes(
                        $"HTTP/1.1 {statusCode} {statusDescription}\r\n" +
                        $"Date: {DateTime.UtcNow:R}\r\n" +
                        $"Content-Type: {response.ContentType}\r\n" +
                        $"Content-Length: {body.Length}\r\n\r\n");
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    stream.Write(body, 0, body.Length);
                }
            }
        }
    }
}
