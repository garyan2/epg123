using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using epg123.SchedulesDirect;

namespace tokenServer
{
    public partial class Server
    {
        private TcpListener _tcpListener;
        private bool _limitExceeded;
        private bool _serviceUnavailable;
        private readonly object _limitLock = new object();
        private readonly object _tokenLock = new object();
        private readonly object _serverLock = new object();

        public void StartTcpListener()
        {
            SdApi.Initialize($"EPG123/{Helper.Epg123Version}");

            _tcpListener = new TcpListener(IPAddress.Any, Helper.TcpPort);
            _tcpListener.Start(20);

            try
            {
                while (true)
                {
                    var client = _tcpListener.AcceptTcpClient();
                    _ = Task.Run(() => HandleDevice(client));
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
                    Helper.WriteLogEntry($"{asset} HandleDevice() Exception: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        private static void ProcessLogoRequest(Stream stream, string asset, DateTime ifModifiedSince)
        {
            WebStats.IncrementLogoRequestReceived();
            var fileInfo = new FileInfo($"{Helper.Epg123LogosFolder}\\{asset.Substring(7)}");
            if (fileInfo.Exists && Send304OrImageFromCache(stream, ifModifiedSince, fileInfo, true)) return;

            // nothing to give the client
            WebStats.IncrementHttpStat(404);
            Helper.WriteLogEntry($"{asset} 404 Not Found");
            var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        #region ========== Send Images ==========
        private void ProcessImageRequest(Stream stream, string asset, DateTime ifModifiedSince, bool retry = false)
        {
            // update web stats
            if (!retry)
            {
                if (ifModifiedSince == DateTime.MinValue) WebStats.IncrementRequestReceived();
                else WebStats.IncrementConditionalRequestReceived();
            }

            // throttle refreshing images that have already been checked recently
            var fileInfo = JsonImageCache.cacheImages ? JsonImageCache.GetCachedImage(asset) : null;
            if (JsonImageCache.cacheImages && JsonImageCache.IsImageRecent(asset.Substring(7), ifModifiedSince))
            {
                if (Send304OrImageFromCache(stream, ifModifiedSince, fileInfo))
                    return;
            }

            // don't try to download from SD if token is invalid
            if (!TokenService.GoodToken) goto NoToken;

            // add if-modified-since as needed
            if (ifModifiedSince != DateTime.MinValue || fileInfo != null)
            {
                ifModifiedSince = new[] {ifModifiedSince.ToUniversalTime(), fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue}.Max();
            }

            try
            {
                using (var response = SdApi.GetSdImage(asset, ifModifiedSince).Result)
                using (var memStream = new MemoryStream())
                {
                    if (response == null) goto NoToken;
                    response.Content.ReadAsStreamAsync()?.Result?.CopyTo(memStream);
                    memStream.Position = 0;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            WebStats.AddSdDownload(memStream.Length);
                            if (_limitExceeded) lock (_limitLock) { _limitExceeded = WebStats.LimitLocked = false; }
                            if (_serviceUnavailable) { lock (_serverLock) _serviceUnavailable = false; }
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            if (!_serviceUnavailable) { lock (_serverLock) _serviceUnavailable = true; }
                            break;
                        case HttpStatusCode.Unauthorized:
                            lock (_tokenLock)
                            {
                                if (!TokenService.GoodToken || !TokenService.RefreshToken) break;
                                if (TokenService.RefreshTokenFromSD())
                                {
                                    ProcessImageRequest(stream, asset, ifModifiedSince, true);
                                    WebStats.DecrementHttpStat(401);
                                    return;
                                }
                            }
                            break;
                        case (HttpStatusCode)429:
                            if (!_limitExceeded) { lock (_limitLock) _limitExceeded = WebStats.LimitLocked = true; }
                            break;
                    }

                    // send response to client
                    var message = $"HTTP/1.1 {(int)response.StatusCode} {response.ReasonPhrase}\r\n";
                    foreach (var header in response.Headers)
                    {
                        foreach (var hdrValue in header.Value)
                        {
                            message += $"{header.Key}: {hdrValue}\r\n";
                        }
                    }
                    foreach (var header in response.Content.Headers)
                    {
                        foreach (var hdrValue in header.Value)
                        {
                            message += $"{header.Key}: {hdrValue}\r\n";
                        }
                    }
                    var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    if (memStream.Length > 0) memStream.CopyTo(stream);
                    if (!JsonImageCache.cacheImages) return;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
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
                        File.SetLastWriteTimeUtc(location, response.Content.Headers.LastModified.Value.DateTime);
                        JsonImageCache.AddImageToCache(filename, response.Content.Headers.LastModified.Value.DateTime, memStream.Length);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Helper.WriteLogEntry($"{asset} ProcessImageRequest() Exception: {e.Message}\n{e.StackTrace}");
            }

            NoToken:
            // fallback for failed connection or no token
            if (Send304OrImageFromCache(stream, ifModifiedSince, fileInfo)) return;

            // nothing to give the client
            WebStats.IncrementHttpStat(404);
            Helper.WriteLogEntry($"{asset} 404 Not Found");
            var messageBytes2 = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
            stream.Write(messageBytes2, 0, messageBytes2.Length);
        }

        private static bool Send304OrImageFromCache(Stream stream, DateTime ifModifiedSince, FileInfo fileInfo, bool logo = false)
        {
            if (ifModifiedSince != DateTime.MinValue && ifModifiedSince.ToUniversalTime() + TimeSpan.FromSeconds(1) > (fileInfo?.LastWriteTimeUtc ?? DateTime.MinValue))
            {
                WebStats.IncrementHttpStat(304);
                var message = "HTTP/1.1 304 Not Modified\r\n" +
                              $"Date: {DateTime.UtcNow:R}\r\n" +
                              $"Last-Modified: {ifModifiedSince.ToUniversalTime() - TimeSpan.FromSeconds(1):R}\r\n";
                var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
                return true;
            }

            if (fileInfo == null || !fileInfo.Exists) return false;
            if (!logo) WebStats.AddCacheDownload(fileInfo.Length);
            else WebStats.AddLogoDownload(fileInfo.Length);
            SendImage(stream, fileInfo);
            return true;
        }

        private static void SendImage(Stream stream, FileInfo fileInfo)
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
                Helper.WriteLogEntry($"{fileInfo.FullName} SendImage() Exception: {e.Message}\n{e.StackTrace}");
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
                    filepath = Config.GetXmltvPath();
                    break;
            }

            if (filepath != null)
            {
                WebStats.IncrementFileRequestReceived();
                SendFile(stream, new FileInfo(filepath), type, disposition);
            }
            else
            {
                Helper.WriteLogEntry($"{asset} 404 Not Found");
                var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }

        private static void SendFile(Stream stream, FileInfo fileInfo, string contentType, string contentDisposition)
        {
            if (fileInfo.Exists)
            {
                WebStats.AddFileDownload(fileInfo.Length);
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
                    Helper.WriteLogEntry($"{fileInfo.FullName} SendFile() Exception: {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                WebStats.IncrementHttpStat(404);
                var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\nDate: {DateTime.UtcNow:R}\r\n\r\n");
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
        #endregion
    }
}