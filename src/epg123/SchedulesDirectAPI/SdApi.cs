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
            return $"{ret} / {GetStringByteLength(length)}";
        }
        private static string GetStringByteLength(long length)
        {
            string[] units = { "", "K", "M", "G", "T" };
            for (int i = 0; i < units.Length; ++i)
            {
                double calc;
                if ((calc = length / Math.Pow(1024, i)) < 1024)
                {
                    return $"{calc,9:N3} {units[i]}B";
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
            int maxTries = uri.Equals("token") ? 1 : 2;
            int cntTries = 0;
            int timeout = uri.Equals("token") ? 3000 : 300000;
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
                                Logger.WriteVerbose($"SD API WebException Thrown. Message: {wex.Message} , Status: {wex.Status} . Trying again.");
                            }
                            break;
                        default:
                            Logger.WriteVerbose($"SD API WebException Thrown. Message: {wex.Message} , Status: {wex.Status}");
                            try
                            {
                                StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8);
                                sdError err = JsonConvert.DeserializeObject<sdError>(sr.ReadToEnd());
                                if (err != null)
                                {
                                    ErrorString = $"Message: {err.Message ?? string.Empty} Response: {err.Response ?? string.Empty}";
                                    Logger.WriteVerbose($"SD responded with error code: {err.Code} , message: {err.Message ?? err.Response} , serverID: {err.ServerID} , datetime: {err.Datetime:s}Z");
                                }
                            }
                            catch { }
                            break; // try again until maxTries
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteVerbose($"SD API Unknown exception thrown. Message: {ex.Message}");
                }
            } while (cntTries < maxTries);

            // failed miserably
            Logger.WriteError("Failed to complete request. Exiting");
            return null;
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
                        Logger.WriteVerbose($"Token request successful. serverID: {ret.ServerID}");
                        sdToken = ret.Token;
                        return true;
                    default:
                        break;
                }
                errorString = $"Failed token request. code: {ret.Code} , message: {ret.Message} , datetime: {ret.DateTime:s}Z";
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
                        Logger.WriteVerbose($"Status request successful. account expires: {ret.Account.Expires:s}Z , lineups: {ret.Lineups.Count}/{ret.Account.MaxLineups} , lastDataUpdate: {ret.LastDataUpdate:s}Z");
                        Logger.WriteVerbose($"system status: {ret.SystemStatus[0].Status} , message: {ret.SystemStatus[0].Message}");
                        maxLineups = ret.Account.MaxLineups;

                        TimeSpan expires = ret.Account.Expires - DateTime.UtcNow;
                        if (expires < TimeSpan.FromDays(7.0))
                        {
                            Logger.WriteWarning($"Your Schedules Direct account expires in {expires.Days:D2} days {expires.Hours:D2} hours {expires.Minutes:D2} minutes.");
                        }
                        return ret;
                    default:
                        break;
                }
                Logger.WriteError($"Failed to get account status. code: {ret.Code} , message: {sdErrorLookup(ret.Code)}");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdUserStatusResponse() Unknown exception thrown. Message: {ex.Message}");
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
                            Logger.WriteInformation($"epg123 is not up to date. Latest version is {ret.Version} and can be downloaded from http://epg123.garyan2.net.");
                        }
                        return ret;
                    case 1005:
                        Logger.WriteInformation($"epg123 is not recognized as an approved app from Schedules Direct. code: {ret.Code} , message: {ret.Message} , datetime: {ret.Datetime:s}Z");
                        break;
                    case 3000:
                    default:
                        Logger.WriteError($"Failed version check. code: {ret.Code} , message: {ret.Message} , datetime: {ret.Datetime:s}Z");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdClientVersionResponse() Unknown exception thrown. Message: {ex.Message}");
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
                Logger.WriteError($"Failed request for listing of client lineups. code: {ret.Code} , message: {sdErrorLookup(ret.Code)}");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetLineups() Unknown exception thrown. Message: {ex.Message}");
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
                    Logger.WriteVerbose($"Successfully retrieved the channels in lineup {lineup} for preview.");
                    return JsonConvert.DeserializeObject<IList<sdLineupPreviewChannel>>(sr.Replace("[],", string.Empty));
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup} to preview.");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdPreviewLineupChannels() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static SdStationMapResponse sdGetStationMaps(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.GETVERBOSEMAP, $"lineups/{lineup}");
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose($"Successfully retrieved the station mapping for lineup {lineup}.");
                    return JsonConvert.DeserializeObject<SdStationMapResponse>(sr.Replace("[],", string.Empty));
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup}.");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetStationMaps() Unknown exception thrown. Message: {ex.Message}");
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
                    Logger.WriteVerbose($"Successfully retrieved {request.Length,3} station's daily schedules.          ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                    return JsonConvert.DeserializeObject<IList<sdScheduleResponse>>(sr);
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,3} station's daily schedules. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetScheduleListings() Unknown exception thrown. Message: {ex.Message}");
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
                    Logger.WriteVerbose($"Successfully retrieved Md5s for {request.Length,3} station's daily schedules. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                    return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, sdScheduleMd5DateResponse>>>(sr.Replace("[]", "{}"));
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for Md5s of {request.Length,3} station's daily schedules. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetScheduleMd5s() Unknown exception thrown. Message: {ex.Message}");
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
                    Logger.WriteVerbose($"Successfully retrieved {request.Length,4} program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                    return JsonConvert.DeserializeObject<IList<sdProgram>>(sr);
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,4} program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetPrograms() Unknown exception thrown. Message: {ex.Message}");
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
                    Logger.WriteVerbose($"Successfully retrieved {request.Length,3} generic program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                    return JsonConvert.DeserializeObject<Dictionary<string, sdGenericDescriptions>>(sr);
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,3} generic program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetProgramGenericDescription() Unknown exception thrown. Message: {ex.Message}");
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
                    Logger.WriteVerbose($"Successfully retrieved artwork info for {request.Length,3} programs. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                    return JsonConvert.DeserializeObject<IList<sdArtworkResponse>>(sr);
                }
                Logger.WriteError($"Did not receive a response from Schedules Direct for artwork info of {request.Length,3} programs. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"sdGetArtwork() Unknown exception thrown. Message: {ex.Message}");
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
                Logger.WriteError($"getCountryAvailables() Unknown exception thrown. Message: {ex.Message}");
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
                Logger.WriteError($"getSatelliteAvailables() Unknown exception thrown. Message: {ex.Message}");
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
                Logger.WriteError($"getTransmitters() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static IList<sdHeadendResponse> getHeadends(string country, string postalcode)
        {
            var sr = sdGetRequestResponse(METHODS.GET, $"headends?country={country}&postalcode={postalcode}");
            try
            {
                if (sr != null)
                {
                    Logger.WriteVerbose($"Successfully retrieved the headends for {country} and postal code {postalcode}.");
                    return JsonConvert.DeserializeObject<IList<sdHeadendResponse>>(sr);
                }
                Logger.WriteError($"Failed to get a response from Schedules Direct for the headends of {country} and postal code {postalcode}.");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"getHeadends() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static bool addLineup(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.PUT, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct when trying to add lineup {lineup}.");
                return false;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject<dynamic>(sr);
                switch ((int)resp["code"])
                {
                    case 0:
                        Logger.WriteVerbose($"Successfully added lineup {lineup} to account. serverID: {resp["serverID"]} , message: {resp["message"]} , changesRemaining: {resp["changesRemaining"]}");
                        return true;
                    default:
                        Logger.WriteError($"Failed to add lineup {lineup} to account. serverID: {resp["serverID"]} , message: {resp["message"]} , changesRemaining: {resp["changesRemaining"]}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"addLineup() Unknown exception thrown. Message: {ex.Message}");
            }
            return false;
        }

        public static bool removeLineup(string lineup)
        {
            var sr = sdGetRequestResponse(METHODS.DELETE, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct when trying to remove lineup {lineup}.");
                return false;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject<dynamic>(sr);
                switch ((int)resp["code"])
                {
                    case 0:
                        Logger.WriteVerbose($"Successfully removed lineup {lineup} from account. serverID: {resp["serverID"]} , message: {resp["message"]} , changesRemaining: {resp["changesRemaining"]}");
                        return true;
                    default:
                        Logger.WriteError($"Failed to remove lineup {lineup} from account. serverID: {resp["serverID"]} , code: {resp["code"]} , message: {resp["message"]} , changesRemaining: {resp["changesRemaining"]}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"removeLineup() Unknown exception thrown. Message: {ex.Message}");
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