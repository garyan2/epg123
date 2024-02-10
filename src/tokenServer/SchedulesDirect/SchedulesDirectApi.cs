using epg123Server;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GaRyan2.SchedulesDirectAPI
{
    internal class API : BaseAPI
    {
        public string BaseArtworkAddress { get; set; }
        public bool UseDebug { get; set; }

        public override T HandleHttpResponseError<T>(HttpResponseMessage response, string content)
        {
            if (string.IsNullOrEmpty(content) || !(response.Content?.Headers?.ContentType?.MediaType?.Contains("json") ?? false)) Logger.WriteVerbose($"HTTP request failed with status code \"{(int)response.StatusCode} {response.ReasonPhrase}\"");
            else
            {
                var err = JsonConvert.DeserializeObject<BaseResponse>(content);
                Logger.WriteVerbose($"SD responded with error code: {err.Code} , message: {err.Message ?? err.Response} , serverID: {err.ServerId} , datetime: {err.Datetime:s}Z");
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

        #region ========== Image Downloads ==========
        public async Task<HttpResponseMessage> GetSdImage(string uri, DateTimeOffset ifModifiedSince)
        {
            if (ifModifiedSince.Ticks > DateTime.MinValue.Ticks) WebStats.IncrementConditionalRequestSent();
            else WebStats.IncrementRequestSent();

            try
            {
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{BaseArtworkAddress}{uri}")
                };
                if (ifModifiedSince.Ticks > DateTime.MinValue.Ticks) message.Headers.IfModifiedSince = ifModifiedSince;
                var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode || response.Content?.Headers?.ContentType?.MediaType == "application/json")
                {
                    if (response.Content?.Headers?.ContentType?.MediaType == "application/json")
                        return HandleHttpResponseError(response, await response.Content.ReadAsStringAsync());
                    else
                        return HandleHttpResponseError(response, null);
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                Logger.WriteError($"{uri} GetSdImage() Exception: {ex?.InnerException.Message ?? ex.Message}");
            }
            return null;
        }

        private HttpResponseMessage HandleHttpResponseError(HttpResponseMessage response, string content)
        {
            var tokenUsed = response.RequestMessage.Headers.GetValues("token")?.FirstOrDefault();

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
                    case 4007: // APPLICATION_DISABLED
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
                    case 1004: // TOKEN_MISSING - special case when token is getting refreshed due to below responses from a separate request
                    case 4003: // INVALID_USER
                    case 4006: // TOKEN_EXPIRED
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
            if (response.StatusCode != HttpStatusCode.NotModified) Logger.WriteError($"{response.RequestMessage.RequestUri.AbsolutePath.Replace(BaseAddress, "/")}: {(int)response.StatusCode} {response.ReasonPhrase} : Token={tokenUsed.Substring(0, 5)}...{(!string.IsNullOrEmpty(content) ? $"\n{content}" : "")}");
            return response;
        }
        #endregion

        public void ClearToken()
        {
            _ = _httpClient.DefaultRequestHeaders.Remove("token");
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Add("token", token);
        }

        public void RouteApiToDebugServer()
        {
            if (UseDebug) _httpClient.DefaultRequestHeaders.Add("RouteTo", "debug");
            else _httpClient.DefaultRequestHeaders.Remove("RouteTo");
        }
    }
}