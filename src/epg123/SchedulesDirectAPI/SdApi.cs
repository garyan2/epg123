using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace epg123
{
    public static class sdAPI
    {
        enum METHODS
        {
            GET,
            GETVERBOSEMAP,
            POST,
            PUT,
            DELETE
        };

        private static string userAgent;
        private static string grabberVersion;
        public static string jsonBaseUrl = @"https://json.schedulesdirect.org";
        public static string jsonApi = @"/20141201/";
        private static string sdToken;
        public static string ErrorString { get; set; }
        public static int maxLineups { get; set; }

        private static long totalBytes_;
        public static string TotalDownloadBytes
        {
            get
            {
                return GetStringByteLength(totalBytes_);
            }
        }

        private static string GetStringTimeAndByteLength(TimeSpan span, long length = 0)
        {
            string ret = span.ToString("G");
            if (length == 0) return ret;

            Interlocked.Add(ref totalBytes_, length);
            return ret + " / " + GetStringByteLength(length);
        }
        private static string GetStringByteLength(long length)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            for (int i = 0; i < units.Length; ++i)
            {
                double calc;
                if ((calc = length / Math.Pow(1024, i)) < 1024)
                {
                    return string.Format("{0,9:N3} {1}", calc, units[i]);
                }
            }
            return string.Empty;
        }

        public static void Initialize(string agent, string version)
        {
            userAgent = agent;
            grabberVersion = version;
        }

        private static string sdGetRequestResponse(METHODS method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // clear errorstring
            ErrorString = string.Empty;

            // build url
            string url = string.Format("{0}{1}{2}", jsonBaseUrl, jsonApi, uri);

            // send request and get response
            int maxTries = 1;
            int cntTries = 0;
            int timeout = 300000;
            do
            {
                try
                {
                    // create the request with defaults
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.UserAgent = userAgent;
                    req.AutomaticDecompression = DecompressionMethods.Deflate;
                    req.Timeout = timeout; ++cntTries;

                    // add token if it is required
                    if (!string.IsNullOrEmpty(sdToken) && tkRequired)
                    {
                        req.Headers.Add("token", sdToken);
                    }

                    // setup request
                    switch (method)
                    {
                        case METHODS.GET:
                            req.Method = "GET";
                            break;
                        case METHODS.GETVERBOSEMAP:
                            req.Method = "GET";
                            req.Headers["verboseMap"] = "true";
                            break;
                        case METHODS.PUT:
                            req.Method = "PUT";
                            break;
                        case METHODS.DELETE:
                            req.Method = "DELETE";
                            break;
                        case METHODS.POST:
                            string send = JsonConvert.SerializeObject(jsonRequest);
                            byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonRequest));
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Accept = "application/json";
                            req.ContentLength = body.Length;

                            Stream reqStream = req.GetRequestStream();
                            reqStream.Write(body, 0, body.Length);
                            reqStream.Close();
                            break;
                        default:
                            break;
                    }

                    WebResponse resp = req.GetResponse();
                    return new StreamReader(resp.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                }
                catch (WebException wex)
                {
                    switch (wex.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            if ((wex.Status == WebExceptionStatus.Timeout) && (cntTries <= maxTries))
                            {
                                Logger.WriteVerbose(string.Format("SD API WebException Thrown. Message: {0} , Status: {1} . Trying again.", wex.Message, wex.Status));
                            }
                            break;
                        default:
                            Logger.WriteVerbose(string.Format("SD API WebException Thrown. Message: {0} , Status: {1}", wex.Message, wex.Status));
                            try
                            {
                                StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8);
                                sdError err = JsonConvert.DeserializeObject<sdError>(sr.ReadToEnd());
                                if (err != null)
                                {
                                    ErrorString = string.Format("Message: {0} Response: {1}", err.Message ?? string.Empty, err.Response ?? string.Empty);
                                    Logger.WriteVerbose(string.Format("SD responded with error code: {0} , message: {1} , serverID: {2} , datetime: {3}", err.Code, err.Message ?? err.Response, err.ServerID, err.Datetime));
                                }
                            }
                            catch { }
                            break; // try again until maxTries
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(string.Format("SD API Unknown exception thrown. Message: {0}", ex.Message));
                    return null;
                }
            } while (cntTries <= maxTries);

            // failed miserably
            Logger.WriteError("Failed to complete request. Exiting");
            return null;
        }
        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static bool sdGetToken(string username, string passwordHash, ref string errorString)
        {
            if (!string.IsNullOrEmpty(sdToken)) return true;

            var sr = sdGetRequestResponse(METHODS.POST, "token", new SdTokenRequest() { Username = username, Password_hash = passwordHash }, false);
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
                SdTokenResponse ret = JsonConvert.DeserializeObject<SdTokenResponse>(sr);
                switch (ret.Code)
                {
                    case 0:
                        Logger.WriteVerbose(string.Format("Token request successful. serverID: {0}", ret.ServerID));
                        sdToken = ret.Token;
                        return true;
                    default:
                        break;
                }
                errorString = string.Format("Failed token request. code: {0} , message: {1} , datetime: {2}", ret.Code, ret.Message, ret.DateTime);
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }
            Logger.WriteError(errorString);
            return false;
        }

        public static sdUserStatusResponse sdGetStatus()
        {
            var sr = sdGetRequestResponse(METHODS.GET, "status");
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a status request.");
                return null;
            }

            try
            {
                sdUserStatusResponse ret = JsonConvert.DeserializeObject<sdUserStatusResponse>(sr);
                switch (ret.Code)
                {
                    case 0:
                        Logger.WriteVerbose(string.Format("Status request successful. account expires: {0} , lineups: {1}/{2} , lastDataUpdate: {3}",
                                                          ret.Account.Expires, ret.Lineups.Count, ret.Account.MaxLineups, ret.LastDataUpdate));
                        Logger.WriteVerbose(string.Format("system status: {0} , message: {1}", ret.SystemStatus[0].Status, ret.SystemStatus[0].Message));
                        maxLineups = ret.Account.MaxLineups;

                        TimeSpan expires = DateTime.Parse(ret.Account.Expires) - DateTime.Now;
                        if (expires < TimeSpan.FromDays(7.0))
                        {
                            Logger.WriteWarning(string.Format("Your Schedules Direct account expires in {0:D2} days {1:D2} hours {2:D2} minutes.",
                                expires.Days, expires.Hours, expires.Minutes));
                        }
                        return ret;
                    default:
                        break;
                }
                Logger.WriteError(string.Format("Failed to get account status. code: {0} , message: {1}", ret.Code, sdErrorLookup(ret.Code)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdUserStatusResponse() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static sdClientVersionResponse sdCheckVersion()
        {
            var sr = sdGetRequestResponse(METHODS.GET, "version/" + userAgent, null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a version check.");
                return null;
            }

            try
            {
                sdClientVersionResponse ret = JsonConvert.DeserializeObject<sdClientVersionResponse>(sr);
                switch (ret.Code)
                {
                    case 0:
                        if (ret.Version != grabberVersion)
                        {
                            if (Logger.eventID == 0) Logger.eventID = 1;
                            Logger.WriteInformation(string.Format("epg123 is not up to date. Latest version is {0} and can be downloaded from http://epg123.garyan2.net.", ret.Version));
                        }
                        return ret;
                    case 1005:
                        Logger.WriteInformation(string.Format("epg123 is not recognized as an approved app from Schedules Direct. code: {0} , message: {1} , datetime: {2}", ret.Code, ret.Message, ret.Datetime));
                        break;
                    case 3000:
                    default:
                        Logger.WriteError(string.Format("Failed version check. code: {0} , message: {1} , datetime: {2}", ret.Code, ret.Message, ret.Datetime));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdClientVersionResponse() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static SdLineupResponse sdGetLineups()
        {
            var sr = sdGetRequestResponse(METHODS.GET, "lineups");
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a client lineup listings.");
                return null;
            }

            try
            {
                SdLineupResponse ret = JsonConvert.DeserializeObject<SdLineupResponse>(sr);
                switch (ret.Code)
                {
                    case 0:
                        Logger.WriteVerbose("Successfully requested listing of client lineups from Schedules Direct.");
                        return ret;
                    default:
                        break;
                }
                Logger.WriteError(string.Format("Failed request for listing of client lineups. code: {0} , message: {1}", ret.Code, sdErrorLookup(ret.Code)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetLineups() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static IList<sdLineupPreviewChannel> sdPreviewLineupChannels(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.GET, string.Format("lineups/preview/{0}", lineup));
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved the channels in lineup {0} for preview.", lineup));
                    return JsonConvert.DeserializeObject<IList<sdLineupPreviewChannel>>(sr.Replace("[],", string.Empty));
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for retrieval of lineup {0} to preview.", lineup));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdPreviewLineupChannels() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static SdStationMapResponse sdGetStationMaps(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.GETVERBOSEMAP, string.Format("lineups/{0}", lineup));
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved the station mapping for lineup {0}.", lineup));
                    return JsonConvert.DeserializeObject<SdStationMapResponse>(sr.Replace("[],", string.Empty));
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for retrieval of lineup {0}.", lineup));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetStationMaps() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static IList<sdScheduleResponse> sdGetScheduleListings(sdScheduleRequest[] request)
        {
            DateTime dtStart = DateTime.Now;
            var sr = sdGetRequestResponse(METHODS.POST, "schedules", request);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved {0,3} station's daily schedules.          ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)));
                    return JsonConvert.DeserializeObject<IList<sdScheduleResponse>>(sr);
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for {0,3} station's daily schedules. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetScheduleListings() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static Dictionary<string, Dictionary<string, sdScheduleMd5DateResponse>> sdGetScheduleMd5s(sdScheduleRequest[] request)
        {
            DateTime dtStart = DateTime.Now;
            var sr = sdGetRequestResponse(METHODS.POST, "schedules/md5", request);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved Md5s for {0,3} station's daily schedules. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)));
                    return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, sdScheduleMd5DateResponse>>>(sr.Replace("[]", "{}"));
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for Md5s of {0,3} station's daily schedules. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetScheduleMd5s() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static IList<sdProgram> sdGetPrograms(string[] request)
        {
            DateTime dtStart = DateTime.Now;
            var sr = sdGetRequestResponse(METHODS.POST, "programs", request);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved {0,4} program descriptions. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)));
                    return JsonConvert.DeserializeObject<IList<sdProgram>>(sr);
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for {0,4} program descriptions. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetPrograms() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static Dictionary<string, sdGenericDescriptions> sdGetProgramGenericDescription(string[] request)
        {
            DateTime dtStart = DateTime.Now;
            var sr = sdGetRequestResponse(METHODS.POST, "metadata/description", request);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved {0,3} generic program descriptions. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)));
                    return JsonConvert.DeserializeObject<Dictionary<string, sdGenericDescriptions>>(sr);
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for {0,3} generic program descriptions. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetProgramGenericDescription() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static IList<sdArtworkResponse> sdGetArtwork(string[] request)
        {
            DateTime dtStart = DateTime.Now;
            var sr = sdGetRequestResponse(METHODS.POST, "metadata/programs", request, false);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved artwork info for {0,3} programs. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)));
                    return JsonConvert.DeserializeObject<IList<sdArtworkResponse>>(sr);
                }
                Logger.WriteError(string.Format("Did not receive a response from Schedules Direct for artwork info of {0,3} programs. ({1})", request.Length, GetStringTimeAndByteLength(DateTime.Now - dtStart)));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("sdGetArtwork() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static Dictionary<string, IList<sdCountry>> getCountryAvailables()
        {
            var sr = sdGetRequestResponse(METHODS.GET, "available/countries", null, false);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose("Successfully retrieved list of available countries from Schedules Direct.");
                    return JsonConvert.DeserializeObject<Dictionary<string, IList<sdCountry>>>(sr);
                }
                Logger.WriteError("Did not receive a response from Schedules Direct for a list of available countries.");
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("getCountryAvailables() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static dynamic getSatelliteAvailables()
        {
            var sr = sdGetRequestResponse(METHODS.GET, "available/dvb-s", null, false);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose("Successfully retrieved list of available satellites from Schedules Direct.");
                    return JsonConvert.DeserializeObject<dynamic>(sr);
                }
                Logger.WriteError("Did not receive a response from Schedles Direct for a list of available satellites.");
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("getSatelliteAvailables() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static Dictionary<string, string> getTransmitters(string country)
        {
            var sr = sdGetRequestResponse(METHODS.GET, string.Format("transmitters/{0}", country), null, false);
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose("Successfully retrieved list of available transmitters from Schedules Direct.");
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(sr);
                }
                Logger.WriteError("Did not receive a response from Schedles Direct for a list of available transmitters.");
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("getTransmitters() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static IList<sdHeadendResponse> getHeadends(string country, string postalcode)
        {
            var sr = sdGetRequestResponse(METHODS.GET, string.Format("headends?country={0}&postalcode={1}", country, postalcode));
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose(string.Format("Successfully retrieved the headends for {0} and postal code {1}.", country, postalcode));
                    return JsonConvert.DeserializeObject<IList<sdHeadendResponse>>(sr);
                }
                Logger.WriteError(string.Format("Failed to get a response from Schedules Direct for the headends of {0} and postal code {1}.", country, postalcode));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("getHeadends() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return null;
        }

        public static bool addLineup(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.PUT, string.Format("lineups/{0}", lineup));
            if (sr == null)
            {
                Logger.WriteError(string.Format("Failed to get a response from Schedules Direct when trying to add lineup {0}.", lineup));
                return false;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject<dynamic>(sr);
                switch ((int)resp["code"])
                {
                    case 0:
                        Logger.WriteVerbose(string.Format("Successfully added lineup {0} to account. serverID: {1} , message: {2} , changesRemaining: {3}",
                            lineup, resp["serverID"], resp["message"], resp["changesRemaining"]));
                        return true;
                    default:
                        Logger.WriteError(string.Format("Failed to add lineup {0} to account. serverID: {1} , code: {2} , message: {3} , changesRemaining: {4}",
                            lineup, resp["serverID"], resp["code"], resp["message"], resp["changesRemaining"]));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("addLineup() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return false;
        }

        public static bool removeLineup(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.DELETE, string.Format("lineups/{0}", lineup));
            if (sr == null)
            {
                Logger.WriteError(string.Format("Failed to get a response from Schedules Direct when trying to remove lineup {0}.", lineup));
                return false;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject<dynamic>(sr);
                switch ((int)resp["code"])
                {
                    case 0:
                        Logger.WriteVerbose(string.Format("Successfully removed lineup {0} from account. serverID: {1} , message: {2} , changesRemaining: {3}",
                            lineup, resp["serverID"], resp["message"], resp["changesRemaining"]));
                        return true;
                    default:
                        Logger.WriteError(string.Format("Failed to remove lineup {0} from account. serverID: {1} , code: {2} , message: {3} , changesRemaining: {4}",
                            lineup, resp["serverID"], resp["code"], resp["message"], resp["changesRemaining"]));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("removeLineup() Unknown exception thrown. Message: {0}", ex.Message));
            }
            return false;
        }

        private static string sdErrorLookup(int code)
        {
            string ret = string.Empty;
            sdErrorCodes.TryGetValue(code, out ret);
            return ret;
        }
        private static Dictionary<int, string> sdErrorCodes = new Dictionary<int, string>()
        {
            { 0, "OK" },
            { 1001, "Unable to decode JSON." },
            { 1002, "Did not receive Accept-Encoding: deflate in request."},
            { 1004, "Token required but not provided in request header." },
            { 2000, "Unsupported command." },
            { 2001, "Request is missing an action to take." },
            { 2002, "Did not receive request." },
            { 2004, "In order to search for lineups, you must supply a 3-letter country parameter." },
            { 2005, "In order to search for lineups, you must supply a postal code parameter." },
            { 2006, "In order to delete a message you must supply the messageID." },
            { 2050, "The COUNTRY parameter must be ISO-3166-1 alpha 3. See http://en.wikipedia.org/wiki/ISO_3166-1_alpha-3." },
            { 2051, "The POSTALCODE parameter must be valid for the country you are searching. Post message to http://forums.schedulesdirect.org/viewforum.php?f=6 if you are having issues." },
            { 2052, "You didn't provide a fetchtype I know how to handle." },
            { 2100, "Lineup already in account." },
            { 2101, "Lineup not in account. Add lineup to account before requesting mapping." },
            { 2102, "Invalid lineup requested. Check your COUNTRY / POSTALCODE combination for validity." },
            { 2103, "Delete of lineup not in account." },
            { 2104, "Lineup must be formatted COUNTRY-LINEUP-DEVICE or COUNTRY-OTA-POSTALCODE." },
            { 2105, "The lineup you submitted doesn't exist." },
            { 2106, "The lineup you requested has been deleted from the server." },
            { 2107, "The lineup is being generated on the server.Please retry." },
            { 2108, "The country you requested is either mis-typed or does not have valid data." },
            { 2200, "The stationID you requested is not in any of your lineups." },
            { 3000, "Server offline for maintenance." },
            { 4001, "Account expired." },
            { 4002, "Password hash must be lowercase 40 character sha1_hex of password." },
            { 4003, "Invalid username or password." },
            { 4004, "Too many login failures. Locked for 15 minutes." },
            { 4005, "Account has been disabled. Please contact Schedules Direct support: admin@schedulesdirect.org for more information." },
            { 4006, "Token has expired. Request new token." },
            { 4100, "Exceeded maximum number of lineup changes for today." },
            { 4101, "Exceeded number of lineups for this account." },
            { 4102, "No lineups have been added to this account." },
            { 5000, "Could not find requested image. Post message to http://forums.schedulesdirect.org/viewforum.php?f=6 if you are having issues." },
            { 6000, "Could not find requested programID. Permanent failure." },
            { 6001, "ProgramID should exist at the server, but doesn't. The server will regenerate the JSON for the program, so your application should retry." },
            { 7000, "The schedule you requested should be available. Post message to http://forums.schedulesdirect.org/viewforum.php?f=6" },
            { 7010, "The server can't determine whether your schedule is valid or not. Open a support ticket." },
            { 7020, "The date that you've requested is outside of the range of the data for that stationID." },
            { 7030, "You have requested a schedule which is not in any of your configured lineups." },
            { 7100, "The schedule you requested has been queued for generation but is not yet ready for download. Retry." },
            { 9999, "Unknown error. Open support ticket." }
        };
    }
}