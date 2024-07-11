using GaRyan2;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace epg123Server
{
    class ConfigServer
    {
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly ManualResetEvent _stop;
        private bool IsListening => _listener != null && _listener.IsListening;

        public ConfigServer()
        {
            _stop = new ManualResetEvent(false);
            _listener = new HttpListener
            {
                IgnoreWriteExceptions = true
            };
            _listenerThread = new Thread(HandleRequest);
        }

        public void Start()
        {
            _listener.Prefixes.Add($"http://*:{Helper.TcpUdpPort}/epg123/");
            _listener.Start();
            _listenerThread.Start();
        }

        public void Stop()
        {
            _stop.Set();
            _listenerThread?.Join();
            _listener.Stop();
        }

        public void HandleRequest()
        {
            while (IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);
                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle })) return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            var context = _listener.EndGetContext(ar);

            var path = context.Request.Url.LocalPath.ToLower();
            if (path.StartsWith("/epg123/available") || path.StartsWith("/epg123/headends") || path.StartsWith("/epg123/lineups") || path.StartsWith("/epg123/metadata") ||
                path.StartsWith("/epg123/programs") || path.StartsWith("/epg123/schedules") || path.StartsWith("/epg123/status") || path.StartsWith("/epg123/transmitters") ||
                path.StartsWith("/epg123/metadata"))
            {
                // redirect to schedules direct servers
                context.Response.StatusCode = (int)HttpStatusCode.Redirect;
                context.Response.RedirectLocation = $"{SchedulesDirect.ApiBaseAddress}{context.Request.Url.PathAndQuery.Substring("/epg123/".Length)}";
                context.Response.OutputStream.Close();
                return;
            }

            if (context.Request.HttpMethod == "GET")
            {
                string content;
                TokenResponse response;
                switch (context.Request.Url.LocalPath.ToLower())
                {
                    case "/epg123/epg123.cfg":
                        var fi1 = new FileInfo(Helper.Epg123CfgPath);
                        if (fi1.Exists)
                        {
                            using (var fs = new FileStream(fi1.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                context.Response.ContentType = "text/xml";
                                fs.CopyTo(context.Response.OutputStream);
                            }
                        }
                        else context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case "/epg123/token":
                    case "/epg123/newtoken": // deprecated with 1.8.2.13
                        var rawParams = context.Request.Url.Query.TrimStart('?').Split('&');
                        if (rawParams.Length > 1)
                        {
                            var parameters = new Dictionary<string, string>();
                            foreach (var param in rawParams)
                            {
                                var kvPair = param.Split('=');
                                var key = kvPair[0];
                                var value = HttpUtility.UrlDecode(kvPair[1]);
                                parameters.Add(key, value);
                            }
                            SchedulesDirect.GetToken(parameters["username"], parameters["password"], true);
                        }
                        else if (!SchedulesDirect.GoodToken || DateTime.UtcNow - SchedulesDirect.TokenTimestamp > TimeSpan.FromHours(22)) SchedulesDirect.GetToken();
                        response = SchedulesDirect.LastTokenResponse;
                        
                        if (response == null)
                        {
                            response = new TokenResponse
                            {
                                Response = "NO_RESPONSE",
                                Code = 9009,
                                ServerId = Dns.GetHostName(),
                                Message = "Failed to get any response from Schedules Direct. Check your internet connection.",
                                Datetime = DateTime.UtcNow,
                                Token = "CAFEDEADBEEFCAFEDEADBEEFCAFEDEADBEEFCAFE"
                            };
                        }

                        if (!SchedulesDirect.GoodToken) context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";
                        content = JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                        {
                            ms.Position = 0;
                            ms.CopyTo(context.Response.OutputStream);
                        }
                        break;
                    case "/epg123/clearcache":
                        if (!(Helper.DeleteFile(Helper.Epg123CacheJsonPath) && Helper.DeleteFile(Helper.Epg123MmuiplusJsonPath)))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        break;
                }
            }
            else if (context.Request.HttpMethod == "PUT")
            {
                switch (context.Request.Url.LocalPath)
                {
                    case "/epg123/epg123.cfg":
                        using (var fs = new FileStream(Helper.Epg123CfgPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            context.Request.InputStream.CopyTo(fs);
                        }
                        var response = new BaseResponse
                        {
                            Code = 0,
                            Message = "OK",
                            ServerId = $"{Dns.GetHostName()}",
                            Datetime = SchedulesDirect.TokenTimestamp,
                        };
                        context.Response.ContentType = "application/json";
                        var content = JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                        {
                            ms.Position = 0;
                            ms.CopyTo(context.Response.OutputStream);
                        }
                        break;
                }
            }
            context.Response.OutputStream.Close();
        }
    }
}
