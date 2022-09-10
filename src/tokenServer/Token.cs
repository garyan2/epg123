using System;
using System.Drawing.Design;
using System.Net;
using epg123.SchedulesDirect;
using Microsoft.Win32;

namespace tokenServer
{
    public static class TokenService
    {
        public static bool GoodToken;
        public static string Token = "";
        public static bool RefreshToken;
        private static DateTime lastRefresh = DateTime.MinValue;

        public static bool RefreshTokenFromSD()
        {
            if (DateTime.Now - lastRefresh < TimeSpan.FromMinutes(1)) return true;

            // get username and passwordhash
            var config = Config.GetEpgConfig();
            if (config?.UserAccount == null) goto End;

            // send request and get response
            try
            {
                var response = SdApi.GetToken(new TokenRequest { Username = config.UserAccount.LoginName, PasswordHash = config.UserAccount.PasswordHash }).Result;
                if (response == null) goto End;

                WebStats.IncrementTokenRefresh();
                Token = response.Token;
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\GaRyan2\epg123", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key != null)
                    {
                        key.SetValue("token", Token);
                        key.SetValue("tokenExpires", $"{response.Datetime.AddDays(1):O}");
                        Helper.WriteLogEntry("Refreshed token upon receiving a user/token error code.");
                        lastRefresh = DateTime.Now;
                        return GoodToken = true;
                    }
                }
            }
            catch (WebException ex)
            {
                Helper.WriteLogEntry($"SD API WebException Thrown. Message: {ex.Message} , Status: {ex.Status}");
            }

            End:
            Helper.WriteLogEntry("Failed to refresh token upon receiving a user/token error code.");
            return GoodToken = false;
        }
    }
}