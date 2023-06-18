using GaRyan2.Utilities;
using Newtonsoft.Json;
using System.Net.Http;

namespace GaRyan2.SchedulesDirectAPI
{
    internal class API : BaseAPI
    {
        public string SdErrorMessage;

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