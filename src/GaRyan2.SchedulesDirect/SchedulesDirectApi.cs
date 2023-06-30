using GaRyan2.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GaRyan2.SchedulesDirectAPI
{
    internal class API : BaseAPI
    {
        public string SdErrorMessage;

        public override async Task<T> GetHttpResponse<T>(HttpMethod method, string uri, object content = null)
        {
            using (var request = new HttpRequestMessage(method, uri)
            {
                Content = (content != null)
                    ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
                    : null
            })
            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) return HandleHttpResponseError<T>(response, await response.Content.ReadAsStringAsync());
                if (typeof(T) != typeof(List<LineupPreviewChannel>) &&
                    typeof(T) != typeof(StationChannelMap) &&
                    typeof(T) != typeof(Dictionary<string, Dictionary<string, ScheduleMd5Response>>))
                {
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), JsonOptions);
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var sr = new StreamReader(stream))
                using (var jr = new JsonTextReader(sr))
                {
                    if (typeof(T) == typeof(List<LineupPreviewChannel>) || typeof(T) == typeof(StationChannelMap))
                    {
                        return JsonConvert.DeserializeObject<T>(sr.ReadToEnd().Replace("[],", ""));
                    }
                    if (typeof(T) == typeof(Dictionary<string, Dictionary<string, ScheduleMd5Response>>))
                    {
                        return JsonConvert.DeserializeObject<T>(sr.ReadToEnd().Replace("[]", "{}"));
                    }
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(jr);
                }
            }
        }

        public override T HandleHttpResponseError<T>(HttpResponseMessage response, string content)
        {
            if (string.IsNullOrEmpty(content)) Logger.WriteVerbose($"HTTP request failed with status code \"{(int)response.StatusCode} {response.ReasonPhrase}\"");
            else
            {
                var err = JsonConvert.DeserializeObject<BaseResponse>(content);
                Logger.WriteVerbose($"SD responded with error code: {err.Code} , message: {err.Message ?? err.Response} , serverID: {err.ServerId} , datetime: {err.Datetime:s}Z");
                SdErrorMessage = $"{err.Response}: {err.Message}";
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
                    case 9009: // Failed token request from user's server
                        Logger.WriteVerbose("***** The EPG123 Server service could not obtain a token from Schedules Direct. View the server.log file for details.");
                        break;
                }
            }
            return default;
        }

        public void ClearToken()
        {
            _ = _httpClient.DefaultRequestHeaders.Remove("token");
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Add("token", token);
        }
    }
}