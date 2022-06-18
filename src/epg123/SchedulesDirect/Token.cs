using System;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static bool GetToken(string username, string passwordHash)
        {
            _ = _httpClient.DefaultRequestHeaders.Remove("token");
            if (!Helper.Standalone)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GaRyan2\epg123", false))
                {
                    if (key != null)
                    {
                        DateTime.TryParse((string) key.GetValue("tokenExpires"), out var expires);
                        if (expires.ToLocalTime() - DateTime.Now > TimeSpan.FromHours(1.0))
                        {
                            _httpClient.DefaultRequestHeaders.Add("token", (string)key.GetValue("token", ""));
                            if (ValidateToken()) return true;
                            Logger.WriteVerbose("Validation of cached token failed. Requesting new token.");
                            _ = _httpClient.DefaultRequestHeaders.Remove("token");
                        }
                    }
                }
            }

            var ret = GetSdApiResponse<TokenResponse>("POST", "token", new TokenRequest { Username = username, PasswordHash = passwordHash });
            if (ret != null)
            {
                _httpClient.DefaultRequestHeaders.Add("token", ret.Token);
                Logger.WriteVerbose($"Token request successful. serverID: {ret.ServerId} , datetime: {ret.Datetime:s}Z");
                try
                {
                    if (!Helper.Standalone)
                    {
                        using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\GaRyan2\epg123", RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            if (key != null)
                            {
                                key.SetValue("token", ret.Token);
                                key.SetValue("tokenExpires", $"{ret.Datetime.AddDays(1):O}");
                            }
                        }
                    }
                }
                catch
                {
                    Logger.WriteError("Failed to update token information in registry. Image downloads may be unsuccessful until registry is created. Open the configuration GUI as administrator to fix.");
                }
            }
            else Logger.WriteError("Did not receive a response from Schedules Direct for a token request.");
            return ret != null;
        }

        private static bool ValidateToken()
        {
            var ret = GetSdApiResponse<LineupResponse>("GET", "lineups");
            if (ret == null) return false;
            return ret.Code == 0;
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
