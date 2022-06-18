using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static string JsonBaseUrl = @"https://json.schedulesdirect.org";
        public static string JsonApi = @"/20141201/";
        public static string uiMessage = null;
        public static int MaxLineups;

        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate })
        {
            BaseAddress = new Uri($"{JsonBaseUrl}{JsonApi}"),
            Timeout = TimeSpan.FromMinutes(5)
        };

        public static void Initialize(string UserAgent)
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // Tls12
            ServicePointManager.DefaultConnectionLimit = 4;

            // set http client headers
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
            _httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
        }

        public static T GetSdApiResponse<T>(string method, string uri, object jsonRequest = null)
        {
            uiMessage = null;
            try
            {
                switch (method)
                {
                    case "GET":
                        return GetHttpResponse<T>(HttpMethod.Get, uri).Result;
                    case "POST":
                        return GetHttpResponse<T>(HttpMethod.Post, uri, jsonRequest).Result;
                    case "PUT":
                        return GetHttpResponse<T>(HttpMethod.Put, uri).Result;
                    case "DELETE":
                        return GetHttpResponse<T>(HttpMethod.Delete, uri).Result;
                }
            }
            catch (Exception e)
            {
                var messages = $"{e.Message} {e.InnerException?.Message} {e.InnerException?.InnerException?.Message}";
                Logger.WriteVerbose($"HTTP {method} request exception thrown. Messages: {messages}");
            }
            return default;
        }

        private static async Task<T> GetHttpResponse<T>(HttpMethod method, string uri, object content = null)
        {
            var message = new HttpRequestMessage { Method = method, RequestUri = new Uri($"{JsonBaseUrl}{JsonApi}{uri}") };
            if (method == HttpMethod.Post) message.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode) return HandleHttpResponseError<T>(response, await response.Content.ReadAsStringAsync());
            using (var stream = response.Content.ReadAsStreamAsync().Result)
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jr);
            }
        }

        private static T HandleHttpResponseError<T>(HttpResponseMessage response, string content)
        {
            if (string.IsNullOrEmpty(content)) Logger.WriteVerbose($"HTTP request failed with status code \"{(int)response.StatusCode} {response.ReasonPhrase}\"");
            else
            {
                var err = JsonConvert.DeserializeObject<BaseResponse>(content);
                Logger.WriteVerbose($"SD responded with error code: {err.Code} , message: {err.Message ?? err.Response} , serverID: {err.ServerId} , datetime: {err.Datetime:s}Z");
                uiMessage = $"{err.Response}: {err.Message}";
                switch (err.Code)
                {
                    case 3000: // SERVICE_OFFLINE
                        Logger.WriteVerbose("***** Schedules Direct servers are offline. Try again later. *****");
                        break;
                    case 4001: // ACCOUNT_EXPIRED
                        Logger.WriteVerbose("***** Renew your Schedules Direct membership at https://schedulesdirect.org. *****");
                        break;
                    case 4003: // INVALID_USER
                        //Logger.WriteVerbose("***** Verify your Schedules Direct membership at https://schedulesdirect.org. *****");
                        break;
                    case 4004: // ACCOUNT_LOCKOUT
                        Logger.WriteVerbose("***** Account is locked out due to too many login attempts. Try again later. *****");
                        break;
                    case 4100: // MAX_LINEUP_CHANGES_REACHED
                        Logger.WriteVerbose("***** You have reached the maximum number of lineup additions to this account for today. Try again tomorrow. *****");
                        break;
                    case 4101: // MAX_LINEUPS
                        Logger.WriteVerbose("***** You must remove a lineup in your account to add another lineup. *****");
                        break;
                }
            }
            return default;
        }
    }
}