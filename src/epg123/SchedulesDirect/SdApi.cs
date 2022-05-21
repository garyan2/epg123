using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        enum methods
        {
            GET,
            GETVERBOSEMAP,
            POST,
            PUT,
            DELETE
        };

        private static string userAgent;
        public static string myToken;
        private static long totalBytes;

        private static readonly JsonSerializerSettings jSettings = new JsonSerializerSettings
            {NullValueHandling = NullValueHandling.Ignore};

        public static string JsonBaseUrl = @"https://json.schedulesdirect.org";
        public static string JsonApi = @"/20141201/";

        public static string ErrorString { get; set; }
        public static int MaxLineups { get; set; }
        public static string TotalDownloadBytes => GetStringByteLength(totalBytes);

        public static void Initialize(string agent)
        {
            userAgent = agent;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // Tls12
        }

        private static string GetRequestResponse(methods method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // clear errorstring
            ErrorString = string.Empty;

            // build url
            var url = $"{JsonBaseUrl}{JsonApi}{uri}";

            // send request and get response
            var maxTries = (uri.Equals("token") || uri.Equals("status")) ? 1 : 2;
            var cntTries = 0;
            var timeout = (uri.Equals("token") || uri.Equals("status")) ? 30000 : 30000;
            do
            {
                try
                {
                    // create the request with defaults
                    var req = (HttpWebRequest)WebRequest.Create(url);
                    req.UserAgent = userAgent;
                    req.AutomaticDecompression = DecompressionMethods.Deflate;
                    req.Timeout = timeout; ++cntTries;

                    // add token if it is required
                    if (!string.IsNullOrEmpty(myToken) && tkRequired)
                    {
                        req.Headers.Add("token", myToken);
                    }

                    // setup request
                    switch (method)
                    {
                        case methods.GET:
                            req.Method = "GET";
                            break;
                        case methods.GETVERBOSEMAP:
                            req.Method = "GET";
                            req.Headers["verboseMap"] = "true";
                            break;
                        case methods.PUT:
                            req.Method = "PUT";
                            break;
                        case methods.DELETE:
                            req.Method = "DELETE";
                            break;
                        case methods.POST:
                            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonRequest));
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Accept = "application/json";
                            req.ContentLength = body.Length;

                            var reqStream = req.GetRequestStream();
                            reqStream.Write(body, 0, body.Length);
                            reqStream.Close();
                            break;
                    }

                    var resp = req.GetResponse();
                    return new StreamReader(resp.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                }
                catch (WebException wex)
                {
                    switch (wex.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            if ((wex.Status == WebExceptionStatus.Timeout) && (cntTries <= maxTries))
                            {
                                Logger.WriteVerbose($"SD API WebException Thrown. Message: {wex.Message} , Status: {wex.Status} . Trying again.");
                            }
                            break;
                        default:
                            Logger.WriteVerbose($"SD API WebException Thrown. Message: {wex.Message} , Status: {wex.Status}");
                            try
                            {
                                var sr = new StreamReader(wex.Response?.GetResponseStream(), Encoding.UTF8);
                                var err = JsonConvert.DeserializeObject<BaseResponse>(sr?.ReadToEnd());
                                if (err != null)
                                {
                                    ErrorString = $"Message: {err.Message ?? string.Empty} Response: {err.Response ?? string.Empty}";
                                    Logger.WriteVerbose($"SD responded with error code: {err.Code} , message: {err.Message ?? err.Response} , serverID: {err.ServerId} , datetime: {err.Datetime:s}Z");
                                    if (err.Code == 4003 || err.Code == 4006) // invalid user or token expired
                                    {
                                        return null;
                                    }
                                }
                            }
                            catch
                            {
                                // ignored
                            }

                            break; // try again until maxTries
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteVerbose($"SD API Unknown exception thrown. Message: {ex.Message}");
                }
            } while (cntTries < maxTries);

            // failed miserably
            Logger.WriteVerbose("Failed to complete request. Exiting");
            return null;
        }

        private static string GetStringTimeAndByteLength(TimeSpan span, long length = 0)
        {
            var ret = span.ToString("G");
            if (length == 0) return ret;

            Interlocked.Add(ref totalBytes, length);
            return $"{ret} / {GetStringByteLength(length)}";
        }

        private static string GetStringByteLength(long length)
        {
            string[] units = { "", "K", "M", "G", "T" };
            for (var i = 0; i < units.Length; ++i)
            {
                double calc;
                if ((calc = length / Math.Pow(1024, i)) < 1024)
                {
                    return $"{calc,9:N3} {units[i]}B";
                }
            }
            return string.Empty;
        }
    }
}