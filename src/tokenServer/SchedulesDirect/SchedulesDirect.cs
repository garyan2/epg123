using epg123;
using epg123Server;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using static GaRyan2.BaseAPI;

namespace GaRyan2
{
    public static class SchedulesDirect
    {
        private static API api;
        private static readonly Timer _timer = new Timer(TimerEvent);
        private static object _tokenLock = new object();

        public static string Token { get; private set; }
        public static DateTime TokenTimestamp = DateTime.MinValue;
        public static bool GoodToken;
        public static string Username { get; private set; }
        public static string PasswordHash { get; private set; }
        public static string ApiBaseAddress { get; private set; }
        public static string ApiBaseArtwork { get; private set; }
        public static bool ApiDebug { get; private set; }

        /// <summary>
        /// Initializes the http client to communicate with Schedules Direct
        /// </summary>
        public static void Initialize()
        {
            RefreshConfiguration();
        }

        public static void RefreshConfiguration()
        {
            if (!File.Exists(Helper.Epg123ExePath)) return;

            // load epg123 config file
            epgConfig config = Helper.ReadXmlFile(Helper.Epg123CfgPath, typeof(epgConfig)) ?? new epgConfig();
            if (api == null || api.BaseAddress != config.BaseApiUrl || api.BaseArtworkAddress != config.BaseArtworkUrl || api.UseDebug != config.UseDebug)
            {
                Logger.WriteInformation($"BaseApi: {config.BaseApiUrl} , BaseArtwork: {config.BaseArtworkUrl} , Debug: {config.UseDebug}");
                api = new API()
                {
                    BaseAddress = ApiBaseAddress = config.BaseApiUrl,
                    BaseArtworkAddress = ApiBaseArtwork = config.BaseArtworkUrl,
                    UserAgent = $"EPG123/{Helper.Epg123Version}",
                    UseDebug = ApiDebug = config.UseDebug
                };
                api.Initialize();
                api.RouteApiToDebugServer();
                _ = GetToken(config.UserAccount?.LoginName, config.UserAccount?.PasswordHash, true);
            }
            else if (Username != config.UserAccount?.LoginName || PasswordHash != config.UserAccount?.PasswordHash)
            {
                _ = GetToken(config.UserAccount?.LoginName, config.UserAccount?.PasswordHash, true);
            }
            JsonImageCache.cacheRetention = config.CacheRetention;
        }

        /// <summary>
        /// Retrieves a session token from Schedules Direct
        /// </summary>
        /// <returns>true if successful</returns>
        public static bool GetToken()
        {
            return GetToken(Username, PasswordHash);
        }
        public static bool GetToken(string username = null, string password = null, bool requestNew = false)
        {
            if (!requestNew && DateTime.UtcNow - TokenTimestamp < TimeSpan.FromMinutes(1)) return true;
            if (username == null || password == null) return false;

            lock (_tokenLock)
            {
                api.ClearToken();
                var ret = api.GetApiResponse<TokenResponse>(Method.POST, "token", new TokenRequest { Username = username, PasswordHash = password });
                if ((ret?.Code ?? -1) == 0)
                {
                    api.SetToken(Token = ret.Token);
                    WebStats.IncrementTokenRefresh();
                    Username = username; PasswordHash = password;
                    GoodToken = true;
                    TokenTimestamp = ret.Datetime;
                    _ = _timer.Change(60000, 60000); // timer event every 60 seconds
                    Logger.WriteInformation($"Refreshed Schedules Direct API token. Token={Token.Substring(0, 5)}...");
                }
                else
                {
                    GoodToken = false;
                    TokenTimestamp = DateTime.MinValue;
                    _ = _timer.Change(900000, 900000); // timer event every 15 minutes
                    if (ret != null) Logger.WriteError($"Failed to get a token from Schedules Direct.\n{JsonConvert.SerializeObject(ret)}");
                    else Logger.WriteError("Did not receive a response from Schedules Direct for a token request.");
                }
                return (ret?.Code ?? -1) == 0;
            }
        }

        public static HttpResponseMessage GetImage(string uri, DateTimeOffset ifModifiedSince)
        {
            return api.GetSdImage(uri.Substring(1), ifModifiedSince).Result;
        }

        private static void TimerEvent(object state)
        {
            if (DateTime.UtcNow - TokenTimestamp > TimeSpan.FromHours(23))
            {
                if (GetToken()) JsonImageCache.Save();
            }
        }
    }
}