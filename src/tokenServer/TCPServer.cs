using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json;

namespace tokenServer
{
    public partial class Server
    {
        private TcpListener _tcpListener;
        private bool _limitExceeded;
        private readonly object _limitLock = new object();
        private readonly object _tokenLock = new object();

        public void StartTcpListener()
        {
            _tcpListener = new TcpListener(IPAddress.Any, Helper.TcpPort);
            _tcpListener.Start();

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
                    var timer = 200;
                    do
                    {
                        Thread.Sleep(1);
                    } while (client.Available == 0 && --timer > 0);

                    if (client.Available == 0) return;
                }
                catch { return; }

                try
                {
                    // read the client message and store
                    var bytes = new byte[client.Available];
                    if (netStream.Read(bytes, 0, bytes.Length) == 0) return;
                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower();

                    // determine asset requested
                    var asset = string.Empty;
                    using (var reader = new StringReader(data))
                    {
                        var startline = HttpUtility.UrlDecode(reader.ReadLine()).Split(' ');
                        if (!startline[0].Equals("get")) return;
                        asset = startline[1];
                    }

                    if (asset.StartsWith("/image/"))
                    {
                        ProcessImageRequest(netStream, asset);
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

        private void ProcessImageRequest(Stream stream, string asset, bool retry = false)
        {
            // use cached image if available and allowed
            if (_cache.cacheImages && _cache.GetCachedImage(asset) is var fileInfo && fileInfo != null)
            {
                SendFile(stream, fileInfo, "image/jpg", "inline");
                return;
            }

            // build url for Schedules Direct with token
            var url = $"{Helper.SdBaseName}{asset}";
            if (!string.IsNullOrEmpty(TokenService.Token)) url += $"?token={TokenService.Token}";

            // connect to SD servers to get redirect or download image
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 3000;
            try
            {
                if (_cache.cacheImages)
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (!response.ContentType.ToLower().Contains("image"))
                        {
                            using (var sr = new StreamReader(response.GetResponseStream()))
                            {
                                var body = sr.ReadToEnd();
                                var message = $"HTTP/1.1 {(int)response.StatusCode} {response.StatusDescription}\r\n";
                                for (var i = 0; i < response.Headers.Count; ++i)
                                {
                                    message += $"{response.Headers.Keys[i]}: {response.Headers[i]}\r\n";
                                }
                                var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                                var bodyBytes = Encoding.UTF8.GetBytes(body);
                                stream.Write(messageBytes, 0, messageBytes.Length);
                                stream.Write(bodyBytes, 0, bodyBytes.Length);
                                Helper.WriteLogEntry($"{asset}\n{body}");
                                return;
                            }
                        }
                        SendFile(stream, _cache.SaveImageToCache(asset, response.GetResponseStream()), "image/jpg", "inline");
                    }
                }
                else
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        lock (_limitLock) _limitExceeded = false;
                        var message = $"HTTP/1.1 {(int)response.StatusCode} {response.StatusDescription}\r\n";
                        for (var i = 0; i < response.Headers.Count; ++i)
                        {
                            message += $"{response.Headers.Keys[i]}: {response.Headers[i]}\r\n";
                        }
                        var messageBytes = Encoding.UTF8.GetBytes($"{message}\r\n");
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        response.GetResponseStream()?.CopyTo(stream);
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    var messageBytes = Encoding.UTF8.GetBytes("HTTP/1.1 408 Request Timeout\r\n" +
                                                                $"Date: {DateTime.UtcNow:r}\r\n" +
                                                                "Connection: close\r\n\r\n");
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    return;
                }
                if (e.Response == null) return;

                using (var sr = new StreamReader(e.Response.GetResponseStream(), Encoding.UTF8))
                {
                    var resp = sr.ReadToEnd();
                    var err = JsonConvert.DeserializeObject<BaseResponse>(resp);
                    Helper.WriteLogEntry($"{asset}:\n{resp}");

                    switch (err?.Code)
                    {
                        case 5002:
                        {
                            lock (_limitLock)
                            {
                                if (_limitExceeded) break;
                                _limitExceeded = true;
                            }
                            break;
                        }
                        case 5004:
                        {
                            lock (_tokenLock)
                            {
                                if (!TokenService.GoodToken) break;
                                if (TokenService.RefreshToken && !retry && TokenService.RefreshTokenFromSD())
                                {
                                    ProcessImageRequest(stream, asset, true);
                                    return;
                                }
                            }
                            break;
                        }
                    }

                    using (var response = e.Response as HttpWebResponse)
                    {
                        if (response != null)
                        {
                            var body = Encoding.UTF8.GetBytes(resp);
                            var messageBytes = Encoding.UTF8.GetBytes($"HTTP/1.1 {(int)response.StatusCode} {response.StatusDescription}\r\n" +
                                                                 $"Date: {DateTime.UtcNow:r}\r\n" +
                                                                 "Content-Type: application/json;charset=utf-8\r\n" +
                                                                 $"Content-Length: {body.Length}\r\n" +
                                                                 "Connection: close\r\n\r\n");
                            stream.Write(messageBytes, 0, messageBytes.Length);
                            stream.Write(body, 0, body.Length);
                        }
                    }
                }
            }
        }

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
                                                    $"Date: {DateTime.UtcNow:r}\r\n" +
                                                    $"Content-Type: {contentType}\r\n" +
                                                    $"Content-Disposition: {contentDisposition}; filename=\"{fileInfo.Name}\"\r\n" +
                                                    $"Last-Modified: {fileInfo.LastWriteTimeUtc:r}\r\n" +
                                                    "Content-Transfer-Encoding: binary\r\n" +
                                                    $"Content-Length: {fileInfo.Length}\r\n" +
                                                    "Connection: close\r\n\r\n");
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
    }
}
