using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static bool GetToken(string username, string passwordHash, ref string errorString)
        {
            if (!string.IsNullOrEmpty(myToken)) return true;

            var sr = GetRequestResponse(methods.POST, "token", new TokenRequest() { Username = username, PasswordHash = passwordHash }, false);
            if (sr == null)
            {
                if (string.IsNullOrEmpty(ErrorString))
                {
                    ErrorString = "Did not receive a response from Schedules Direct for a token request.";
                }
                Logger.WriteError(errorString = ErrorString);
                return false;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<TokenResponse>(sr);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose($"Token request successful. serverID: {ret.ServerId} , datetime: {ret.Datetime:s}Z");
                    myToken = ret.Token;
                    return true;
                }
                errorString = $"Failed token request. code: {ret.Code} , message: {ret.Message} , datetime: {ret.Datetime:s}Z";
            }
            catch (Exception ex)
            {
                errorString = $"GetToken() Unknown exception thrown. Message: {ex.Message}";
            }
            Logger.WriteError(errorString);
            return false;
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
}
