using System;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static bool GetToken(string username, string passwordHash, ref string errorString)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GaRyan2\epg123", false))
            {
                if (key != null)
                {
                    var expires = DateTime.Parse((string) key.GetValue("tokenExpires", DateTime.MinValue.ToString()));
                    myToken = (string)key.GetValue("token", "");
                    if (expires.ToLocalTime() - DateTime.Now > TimeSpan.FromHours(1.0))
                    {
                        if (ValidateToken()) return true;
                        Logger.WriteVerbose("Validation of cached token failed. Requesting new token.");
                    }
                }
            }

            var sr = GetRequestResponse(methods.POST, "token", new TokenRequest { Username = username, PasswordHash = passwordHash }, false);
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
                var ret = JsonConvert.DeserializeObject<TokenResponse>(sr, jSettings);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose($"Token request successful. serverID: {ret.ServerId} , datetime: {ret.Datetime:s}Z");
                    try
                    {
                        using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\GaRyan2\epg123", RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            if (key != null)
                            {
                                key.SetValue("token", ret.Token);
                                key.SetValue("tokenExpires", $"{ret.Datetime.AddDays(1):R}");
                            }
                        }
                        myToken = ret.Token;
                        return true;
                    }
                    catch
                    {
                        Logger.WriteError("Failed to update token information in registry. Image downloads may be unsuccessful until registry is created. Open the configuration GUI as administrator to fix.");
                    }
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

        private static bool ValidateToken()
        {
            var sr = GetRequestResponse(methods.GET, "status");
            if (sr == null) return false;

            var ret = JsonConvert.DeserializeObject<UserStatus>(sr, jSettings);
            if (ret.Code == 0) return true;
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
