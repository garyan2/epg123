using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tokenServer;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static string SdBaseAddr = @"https://json.schedulesdirect.org";
        public static string SdApiNode = @"/20141201";

        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate })
        {
            BaseAddress = new Uri($"{SdBaseAddr}{SdApiNode}"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        public static void Initialize(string UserAgent)
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // Tls12
            ServicePointManager.DefaultConnectionLimit = 6;

            // set http client headers
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);

            // negotiate first contact
            _ = _httpClient.GetAsync("/");
        }

        public static async Task<HttpResponseMessage> GetSdImage(string uri, DateTime ifModifiedSince)
        {
            if (ifModifiedSince > DateTime.MinValue) WebStats.IncrementConditionalRequestSent();
            else WebStats.IncrementRequestSent();

            try
            {
                var message = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri($"{SdBaseAddr}{SdApiNode}{uri}?token={TokenService.Token}") };
                if (ifModifiedSince > DateTime.MinValue) message.Headers.IfModifiedSince = ifModifiedSince;
                var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType.MediaType == "application/json")
                    return HandleHttpResponseError(response, await response.Content.ReadAsStringAsync());
                return response;
            }
            catch (HttpRequestException e)
            {
                Helper.WriteLogEntry($"{uri} GetSdImage() Exception: {e.Message} {e.InnerException?.Message}");
            }
            return null;
        }

        public static async Task<TokenResponse> GetToken(TokenRequest request)
        {
            try
            {
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{SdBaseAddr}{SdApiNode}/token"),
                    Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
                };
                var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _ = HandleHttpResponseError(response, await response.Content.ReadAsStringAsync());
                    return null;
                }

                using (var stream = response.Content.ReadAsStreamAsync().Result)
                using (var sr = new StreamReader(stream))
                using (var jr = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<TokenResponse>(jr);
                }
            }
            catch (HttpRequestException e)
            {
                Helper.WriteLogEntry($"GetToken() Exception: {e.Message} {e.InnerException?.Message}");
                return null;
            }
        }

        private static HttpResponseMessage HandleHttpResponseError(HttpResponseMessage response, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var err = JsonConvert.DeserializeObject<BaseResponse>(content);
                switch (err.Code)
                {
                    case 3000: // SERVICE_OFFLINE
                        response.StatusCode = HttpStatusCode.ServiceUnavailable; // 503
                        response.ReasonPhrase = "Service Unavailable";
                        break;
                    case 4001: // ACCOUNT_EXPIRED
                    case 4005: // ACCOUNT_DISABLED
                        response.StatusCode = HttpStatusCode.Forbidden; // 403
                        response.ReasonPhrase = "Forbidden";
                        break;
                    case 4004: // ACCOUNT_LOCKOUT
                        response.StatusCode = (HttpStatusCode)423; // 423
                        response.ReasonPhrase = "Locked";
                        break;
                    case 5000: // IMAGE_NOT_FOUND
                    case 5001: // IMAGE_QUEUED
                        response.StatusCode = HttpStatusCode.NotFound; // 404
                        response.ReasonPhrase = "Not Found";
                        break;
                    case 5002: // MAX_IMAGE_DOWNLOADS
                    case 5003: // MAX_IMAGE_DOWNLOADS_TRIAL
                        response.StatusCode = (HttpStatusCode)429; // 429
                        response.ReasonPhrase = "Too Many Requests";
                        break;
                    case 4003: // INVALID_USER
                    case 4006: // TOKEN_EXPIRED
                    case 4007: // TOKEN_INVALID
                    case 4008: // TOKEN_DUPLICATED
                    case 5004: // UNKNOWN_USER
                        response.StatusCode = HttpStatusCode.Unauthorized; // 401
                        response.ReasonPhrase = "Unauthorized";
                        break;
                }
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.StatusCode = (HttpStatusCode)418;
                response.ReasonPhrase = "I'm a teapot";
            }
            WebStats.IncrementHttpStat((int)response.StatusCode);
            if (response.StatusCode != HttpStatusCode.NotModified) Helper.WriteLogEntry($"{response.RequestMessage.RequestUri.AbsolutePath.Replace(SdApiNode, "")}: {(int)response.StatusCode} {response.ReasonPhrase}{(!string.IsNullOrEmpty(content) ? $"\n{content}" : "")}");
            return response;
        }
    }

    public class TokenRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string PasswordHash { get; set; }
    }

    public class TokenResponse : BaseResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class BaseResponse
    {
        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("serverID")]
        public string ServerId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("datetime")]
        public DateTime Datetime { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}