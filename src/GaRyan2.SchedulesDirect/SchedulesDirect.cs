using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using static GaRyan2.BaseAPI;

namespace GaRyan2
{
    public static class SchedulesDirect
    {
        private static readonly API api = new API();
        public static string BaseAddress
        {
            get
            {
                return api.BaseAddress;
            }
        }
        public static string BaseArtworkAddress { get; private set; }

        public static string ErrorMessage
        {
            get
            {
                return api.SdErrorMessage;
            }
        }
        public static int MaxLineups;

        /// <summary>
        /// Initializes the http client to communicate with Schedules Direct
        /// </summary>
        /// <param name="apiBaseAddress">optional API address to override default https://json.schedulesdirect.org/20141201/</param>
        /// <param name="artworkBaseAddress">optional API address to override default https://json.schedulesdirect.org/20141201/</param>
        public static void Initialize(string userAgent, string apiBaseAddress, string artworkBaseAddress)
        {
            api.BaseAddress = apiBaseAddress;
            BaseArtworkAddress = artworkBaseAddress;
            api.UserAgent = userAgent;
            api.Initialize();
        }

        public static bool UploadConfiguration(string url, object config)
        {
            var ret = api.GetApiResponse<BaseResponse>(Method.UPCFG, url, config);
            if (ret != null) Logger.WriteVerbose("Successfully uploaded configuration file to server.");
            else Logger.WriteError("Failed to upload configuration file to server.");
            return ret != null;
        }

        #region ========== User Management ==========
        /// <summary>
        /// Retrieves a session token from Schedules Direct
        /// </summary>
        /// <param name="username">account username</param>
        /// <param name="passwordHash">optional to provide password hash if known</param>
        /// <param name="requestNew">demand request for new token</param>
        /// <returns>true if successful</returns>
        public static bool GetToken(string username, string passwordHash, bool requestNew = false)
        {
            if (Helper.InstallMethod != Helper.Installation.PORTABLE)
            {
                api.ClearToken();
                var baseApi = Helper.InstallMethod == Helper.Installation.CLIENT ? api.BaseAddress : $"http://localhost:{Helper.TcpUdpPort}/epg123/";
                if (!requestNew)
                {
                    var ret1 = api.GetApiResponse<TokenResponse>(Method.GET, $"{baseApi}token");
                    if (ret1 != null && ret1.Code == 0)
                    {
                        api.SetToken(ret1.Token);
                        Logger.WriteVerbose($"Token request successful. serverID: {ret1.ServerId} , datetime: {ret1.Datetime:s}Z");
                        if (ValidateToken()) return true;
                        Logger.WriteVerbose("Validation of cached token failed. Requesting new token.");
                        api.ClearToken();
                    }
                    else if (ret1 == null) return false;
                }

                var ret = api.GetApiResponse<TokenResponse>(Method.GET, $"{baseApi}newtoken?username={username}&password={passwordHash}");
                if (ret != null && ret.Code == 0)
                {
                    api.SetToken(ret.Token);
                    Logger.WriteVerbose($"Token refresh successful. serverID: {ret.ServerId} , datetime: {ret.Datetime:s}Z");
                    return true;
                }
                else if (ret != null)
                {
                    api.SdErrorMessage = ret.Message;
                }
                else Logger.WriteError("Did not receive a response from EPG123 Server for a token request.");
                return false;
            }

            if (passwordHash != null)
            {
                api.ClearToken();
                var ret = api.GetApiResponse<TokenResponse>(Method.POST, "token", new TokenRequest { Username = username, PasswordHash = passwordHash });
                if (ret != null && ret.Code == 0)
                {
                    api.SetToken(ret.Token);
                    Logger.WriteVerbose($"Token request successful. serverID: {ret.ServerId} , datetime: {ret.Datetime:s}Z");
                }
                else Logger.WriteError("Did not receive a response from Schedules Direct for a token request.");
                return ret != null;
            }
            Logger.WriteError("No password provided to attempt token request from Schedules Direct.");
            return false;
        }

        private static bool ValidateToken()
        {
            var ret = api.GetApiResponse<LineupResponse>(Method.GET, "lineups");
            if (ret == null) return false;
            return ret.Code == 0;
        }

        public static UserStatus GetUserStatus()
        {
            var ret = api.GetApiResponse<UserStatus>(Method.GET, "status");
            if (ret != null)
            {
                Logger.WriteVerbose($"Status request successful. account expires: {ret.Account.Expires:s}Z , lineups: {ret.Lineups.Count}/{ret.Account.MaxLineups} , lastDataUpdate: {ret.LastDataUpdate:s}Z");
                Logger.WriteVerbose($"System status: {ret.SystemStatus[0].Status} , message: {ret.SystemStatus[0].Message}");
                MaxLineups = ret.Account.MaxLineups;

                var expires = ret.Account.Expires - DateTime.UtcNow;
                if (expires >= TimeSpan.FromDays(7.0)) return ret;
                Logger.WriteWarning($"Your Schedules Direct account expires in {expires.Days:D2} days {expires.Hours:D2} hours {expires.Minutes:D2} minutes.");
                Logger.WriteInformation("*** Renew your Schedules Direct membership at https://schedulesdirect.org. ***");
            }
            else Logger.WriteError("Did not receive a response from Schedules Direct for a status request.");
            return ret;
        }
        #endregion

        #region ========== Lineup Management ==========
        public static LineupResponse GetSubscribedLineups()
        {
            var ret = api.GetApiResponse<LineupResponse>(Method.GET, "lineups");
            if (ret != null) Logger.WriteVerbose("Successfully requested listing of subscribed lineups from Schedules Direct.");
            else Logger.WriteError("Did not receive a response from Schedules Direct for list of subscribed lineups.");
            return ret;
        }

        public static Dictionary<string, List<Country>> GetAvailableCountries()
        {
            var ret = api.GetApiResponse<Dictionary<string, List<Country>>>(Method.GET, "available/countries");
            if (ret != null) Logger.WriteVerbose("Successfully retrieved list of available countries from Schedules Direct.");
            else Logger.WriteError("Did not receive a response from Schedules Direct for a list of available countries.");
            return ret;
        }

        public static Dictionary<string, string> GetTransmitters(string country)
        {
            var ret = api.GetApiResponse<Dictionary<string, string>>(Method.GET, $"transmitters/{country}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved list of available transmitters from Schedules Direct for country {country}.");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for a list of available transmitters for country {country}.");
            return ret;
        }

        public static dynamic GetAvailableSatellites()
        {
            var ret = api.GetApiResponse<dynamic>(Method.GET, "available/dvb-s");
            if (ret != null) Logger.WriteVerbose("Successfully retrieved list of available satellites from Schedules Direct.");
            else Logger.WriteError("Did not receive a response from Schedules Direct for a list of available satellites.");
            return ret;
        }

        public static List<Headend> GetHeadends(string country, string postalcode)
        {
            var ret = api.GetApiResponse<List<Headend>>(Method.GET, $"headends?country={country}&postalcode={postalcode}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved the headends for {country} and postal code {postalcode}.");
            else Logger.WriteError($"Failed to get a response from Schedules Direct for the headends of {country} and postal code {postalcode}.");
            return ret;
        }

        public static List<LineupPreviewChannel> GetLineupPreviewChannels(string lineup)
        {
            var ret = api.GetApiResponse<List<LineupPreviewChannel>>(Method.GET, $"lineups/preview/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved the channels in lineup {lineup} for preview.");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup} for preview.");
            return ret;
        }

        public static bool AddLineup(string lineup)
        {
            var ret = api.GetApiResponse<AddRemoveLineupResponse>(Method.PUT, $"lineups/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully added lineup {lineup} to account. serverID: {ret.ServerId} , message: {ret.Message} , changesRemaining: {ret.ChangesRemaining}");
            else Logger.WriteError($"Failed to get a response from Schedules Direct when trying to add lineup {lineup}.");
            return ret != null;
        }

        public static bool RemoveLineup(string lineup)
        {
            var ret = api.GetApiResponse<AddRemoveLineupResponse>(Method.DELETE, $"lineups/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully removed lineup {lineup} from account. serverID: {ret.ServerId} , message: {ret.Message} , changesRemaining: {ret.ChangesRemaining}");
            else Logger.WriteError($"Failed to get a response from Schedules Direct when trying to remove lineup {lineup}.");
            return ret != null;
        }
        #endregion

        #region ========== Program Metadata ==========
        public static StationChannelMap GetStationChannelMap(string lineup)
        {
            var ret = api.GetApiResponse<StationChannelMap>(Method.GET, $"lineups/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved the station mapping for lineup {lineup}. ({ret.Stations.Count} stations; {ret.Map.Count} channels)");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup}.");
            return ret;
        }

        public static Dictionary<string, Dictionary<string, ScheduleMd5Response>> GetScheduleMd5s(ScheduleRequest[] request)
        {
            var dtStart = DateTime.Now;
            var ret = api.GetApiResponse<Dictionary<string, Dictionary<string, ScheduleMd5Response>>>(Method.POST, "schedules/md5", request);
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved Md5s for {ret.Count}/{request.Length} stations' daily schedules. ({DateTime.Now - dtStart:G})");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for Md5s of {request.Length} stations' daily schedules. ({DateTime.Now - dtStart:G})");
            return ret;
        }

        public static List<ScheduleResponse> GetScheduleListings(ScheduleRequest[] request)
        {
            var dtStart = DateTime.Now;
            var ret = api.GetApiResponse<List<ScheduleResponse>>(Method.POST, "schedules", request);
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved {request.Length} stations' daily schedules. ({DateTime.Now - dtStart:G})");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length} stations' daily schedules. ({DateTime.Now - dtStart:G})");
            return ret;
        }

        public static List<Programme> GetPrograms(string[] request)
        {
            var dtStart = DateTime.Now;
            var ret = api.GetApiResponse<List<Programme>>(Method.POST, "programs", request);
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved {ret.Count}/{request.Length} program descriptions. ({DateTime.Now - dtStart:G})");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length} program descriptions. ({DateTime.Now - dtStart:G})");
            return ret;
        }

        public static Dictionary<string, GenericDescription> GetGenericDescriptions(string[] request)
        {
            var dtStart = DateTime.Now;
            var ret = api.GetApiResponse<Dictionary<string, GenericDescription>>(Method.POST, "metadata/description", request);
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved {ret.Count}/{request.Length} generic program descriptions. ({DateTime.Now - dtStart:G})");
            else Logger.WriteInformation($"Did not receive a response from Schedules Direct for {request.Length} generic program descriptions. ({DateTime.Now - dtStart:G})");
            return ret;
        }

        public static List<ProgramMetadata> GetArtwork(string[] request)
        {
            var dtStart = DateTime.Now;
            var ret = api.GetApiResponse<List<ProgramMetadata>>(Method.POST, "metadata/programs", request);
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved artwork info for {ret.Count}/{request.Length} programs. ({DateTime.Now - dtStart:G})");
            else Logger.WriteInformation($"Did not receive a response from Schedules Direct for artwork info of {request.Length} programs. ({DateTime.Now - dtStart:G})");
            return ret;
        }

        public static List<string> GetCustomLogosFromServer(string server)
        {
            return api.GetApiResponse<List<string>>(Method.GET, server);
        }
        #endregion
    }
}