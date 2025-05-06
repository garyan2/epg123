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
        private static Random _random = new Random();
        private static API api;
        private static readonly Timer _timer = new Timer(TimerEvent);
        private static object _tokenLock = new object();

        public static TokenResponse LastTokenResponse { get; private set; } = new TokenResponse
        {
            Code = 0,
            tokenExpiresEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1) + TimeSpan.FromDays(1)).TotalSeconds,
            Datetime = DateTime.UtcNow - TimeSpan.FromHours(24)
        };

        public static DateTime TokenTimestamp => GoodToken ? LastTokenResponse.Datetime : DateTime.MinValue;
        public static bool GoodToken => (LastTokenResponse?.Code ?? -1) == 0 &&
                                         !string.IsNullOrEmpty(LastTokenResponse.Token) &&
                                         LastTokenResponse.TokenExpires > DateTime.UtcNow;

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
            }
            _ = GetToken(Username = config.UserAccount?.LoginName, PasswordHash = config.UserAccount?.PasswordHash);
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
            if (username == null || password == null) return false;

            lock (_tokenLock)
            {
                requestNew |= (Username != username) || (PasswordHash != password);
                if (!requestNew && DateTime.UtcNow - TokenTimestamp < TimeSpan.FromMinutes(1)) return GoodToken;

                api.ClearToken();
                LastTokenResponse = api.GetApiResponse<TokenResponse>(Method.POST, "token", new TokenRequest { Username = username, PasswordHash = password , NewToken = requestNew });

                if (GoodToken && LastTokenResponse.TokenExpires - DateTime.UtcNow < TimeSpan.FromMinutes(15))
                {
                    LastTokenResponse = api.GetApiResponse<TokenResponse>(Method.POST, "token", new TokenRequest { Username = username, PasswordHash = password, NewToken = true });
                }

                if (GoodToken)
                {
                    api.SetToken(LastTokenResponse.Token);
                    WebStats.IncrementTokenRefresh();
                    Username = username; PasswordHash = password;
                    _timer.Change(LastTokenResponse.TokenExpires - DateTime.UtcNow - TimeSpan.FromSeconds(_random.Next(300, 900)), TimeSpan.FromHours(24));
                    Logger.WriteInformation($"Refreshed Schedules Direct API token. token: {LastTokenResponse.Token.Substring(0, 5)}... , expires: {LastTokenResponse.TokenExpires:s}Z");
                }
                else
                {
                    _ = _timer.Change(900000, 900000); // timer event every 15 minutes
                    if (LastTokenResponse != null) Logger.WriteError($"Failed to get a token from Schedules Direct.\n{JsonConvert.SerializeObject(LastTokenResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore })}");
                    else Logger.WriteError("Did not receive a response from Schedules Direct for a token request.");
                }
            }
            return GoodToken;
        }

        public static HttpResponseMessage GetImage(string uri, DateTimeOffset ifModifiedSince)
        {
            return api.GetSdImage(uri.Substring(1), ifModifiedSince).Result;
        }

        private static void TimerEvent(object state)
        {
            if (GetToken())
            {
                JsonImageCache.Save();
            }
        }
    }
}