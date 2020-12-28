using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace epg123.SchedulesDirectAPI
{
    public static class sdApi
    {
        enum methods
        {
            GET,
            GETVERBOSEMAP,
            POST,
            PUT,
            DELETE
        };

        private static string userAgent;
        private static string grabberVersion;
        public static string JsonBaseUrl = @"https://json.schedulesdirect.org";
        public static string JsonApi = @"/20141201/";
        private static string sdToken;
        public static string ErrorString { get; set; }
        public static int MaxLineups { get; set; }

        private static long totalBytes;
        public static string TotalDownloadBytes => GetStringByteLength(totalBytes);

        private static string GetStringTimeAndByteLength(TimeSpan span, long length = 0)
        {
            var ret = span.ToString("G");
            if (length == 0) return ret;

            Interlocked.Add(ref totalBytes, length);
            return $"{ret} / {GetStringByteLength(length)}";
        }
        private static string GetStringByteLength(long length)
        {
            string[] units = { "", "K", "M", "G", "T" };
            for (var i = 0; i < units.Length; ++i)
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

        private static string SdGetRequestResponse(methods method, string uri, object jsonRequest = null, bool tkRequired = true)
        {
            // clear errorstring
            ErrorString = string.Empty;

            // build url
            var url = $"{JsonBaseUrl}{JsonApi}{uri}";

            // send request and get response
            var maxTries = uri.Equals("token") ? 1 : 2;
            var cntTries = 0;
            var timeout = uri.Equals("token") ? 3000 : 300000;
            do
            {
                try
                {
                    // create the request with defaults
                    var req = (HttpWebRequest)WebRequest.Create(url);
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
                        case methods.GET:
                            req.Method = "GET";
                            break;
                        case methods.GETVERBOSEMAP:
                            req.Method = "GET";
                            req.Headers["verboseMap"] = "true";
                            break;
                        case methods.PUT:
                            req.Method = "PUT";
                            break;
                        case methods.DELETE:
                            req.Method = "DELETE";
                            break;
                        case methods.POST:
                            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonRequest));
                            req.Method = "POST";
                            req.ContentType = "application/json";
                            req.Accept = "application/json";
                            req.ContentLength = body.Length;

                            var reqStream = req.GetRequestStream();
                            reqStream.Write(body, 0, body.Length);
                            reqStream.Close();
                            break;
                    }

                    var resp = req.GetResponse();
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
                                var sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8);
                                var err = JsonConvert.DeserializeObject<sdError>(sr.ReadToEnd());
                                if (err != null)
                                {
                                    ErrorString = $"Message: {err.Message ?? string.Empty} Response: {err.Response ?? string.Empty}";
                                    Logger.WriteVerbose($"SD responded with error code: {err.Code} , message: {err.Message ?? err.Response} , serverID: {err.ServerId} , datetime: {err.Datetime:s}Z");
                                }
                            }
                            catch
                            {
                                // ignored
                            }

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

        public static bool SdGetToken(string username, string passwordHash, ref string errorString)
        {
            if (!string.IsNullOrEmpty(sdToken)) return true;

            var sr = SdGetRequestResponse(methods.POST, "token", new SdTokenRequest() { Username = username, PasswordHash = passwordHash }, false);
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
                var ret = JsonConvert.DeserializeObject<SdTokenResponse>(sr);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose($"Token request successful. serverID: {ret.ServerId}");
                    sdToken = ret.Token;
                    return true;
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

        public static sdUserStatusResponse SdGetStatus()
        {
            var sr = SdGetRequestResponse(methods.GET, "status");
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a status request.");
                return null;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<sdUserStatusResponse>(sr);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose($"Status request successful. account expires: {ret.Account.Expires:s}Z , lineups: {ret.Lineups.Count}/{ret.Account.MaxLineups} , lastDataUpdate: {ret.LastDataUpdate:s}Z");
                    Logger.WriteVerbose($"system status: {ret.SystemStatus[0].Status} , message: {ret.SystemStatus[0].Message}");
                    MaxLineups = ret.Account.MaxLineups;

                    var expires = ret.Account.Expires - DateTime.UtcNow;
                    if (expires < TimeSpan.FromDays(7.0))
                    {
                        Logger.WriteWarning($"Your Schedules Direct account expires in {expires.Days:D2} days {expires.Hours:D2} hours {expires.Minutes:D2} minutes.");
                    }
                    return ret;
                }
                Logger.WriteError($"Failed to get account status. code: {ret.Code} , message: {SdErrorLookup(ret.Code)}");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdUserStatusResponse() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static sdClientVersionResponse SdCheckVersion()
        {
            var sr = SdGetRequestResponse(methods.GET, $"version/{userAgent}", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a version check.");
                return null;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<sdClientVersionResponse>(sr);
                switch (ret.Code)
                {
                    case 0:
                        if (ret.Version == grabberVersion) return ret;
                        if (Logger.EventId == 0) Logger.EventId = 1;
                        Logger.WriteInformation($"epg123 is not up to date. Latest version is {ret.Version} and can be downloaded from http://epg123.garyan2.net.");
                        return ret;
                    case 1005:
                        Logger.WriteInformation($"epg123 is not recognized as an approved app from Schedules Direct. code: {ret.Code} , message: {ret.Message} , datetime: {ret.Datetime:s}Z");
                        break;
                    default:
                        Logger.WriteError($"Failed version check. code: {ret.Code} , message: {ret.Message} , datetime: {ret.Datetime:s}Z");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdClientVersionResponse() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static SdLineupResponse SdGetLineups()
        {
            var sr = SdGetRequestResponse(methods.GET, "lineups");
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a client lineup listings.");
                return null;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<SdLineupResponse>(sr);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose("Successfully requested listing of client lineups from Schedules Direct.");
                    return ret;
                }
                Logger.WriteError($"Failed request for listing of client lineups. code: {ret.Code} , message: {SdErrorLookup(ret.Code)}");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetLineups() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static IList<sdLineupPreviewChannel> SdPreviewLineupChannels(string lineup)
        {
            var sr = SdGetRequestResponse(methods.GET, $"lineups/preview/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup} to preview.");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved the channels in lineup {lineup} for preview.");
                return JsonConvert.DeserializeObject<IList<sdLineupPreviewChannel>>(sr.Replace("[],", string.Empty));
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdPreviewLineupChannels() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static SdStationMapResponse SdGetStationMaps(string lineup)
        {
            var sr = SdGetRequestResponse(methods.GETVERBOSEMAP, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup}.");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved the station mapping for lineup {lineup}.");
                return JsonConvert.DeserializeObject<SdStationMapResponse>(sr.Replace("[],", string.Empty));
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetStationMaps() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static IList<sdScheduleResponse> SdGetScheduleListings(sdScheduleRequest[] request)
        {
            var dtStart = DateTime.Now;
            var sr = SdGetRequestResponse(methods.POST, "schedules", request);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,3} station's daily schedules. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved {request.Length,3} station's daily schedules.          ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<IList<sdScheduleResponse>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetScheduleListings() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static Dictionary<string, Dictionary<string, sdScheduleMd5DateResponse>> SdGetScheduleMd5S(sdScheduleRequest[] request)
        {
            var dtStart = DateTime.Now;
            var sr = SdGetRequestResponse(methods.POST, "schedules/md5", request);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for Md5s of {request.Length,3} station's daily schedules. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved Md5s for {request.Length,3} station's daily schedules. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, sdScheduleMd5DateResponse>>>(sr.Replace("[]", "{}"));
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetScheduleMd5s() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static IList<sdProgram> SdGetPrograms(string[] request)
        {
            var dtStart = DateTime.Now;
            var sr = SdGetRequestResponse(methods.POST, "programs", request);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,4} program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved {request.Length,4} program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<IList<sdProgram>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetPrograms() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static Dictionary<string, sdGenericDescriptions> SdGetProgramGenericDescription(string[] request)
        {
            var dtStart = DateTime.Now;
            var sr = SdGetRequestResponse(methods.POST, "metadata/description", request);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,3} generic program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved {request.Length,3} generic program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<Dictionary<string, sdGenericDescriptions>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetProgramGenericDescription() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static IList<sdArtworkResponse> SdGetArtwork(string[] request)
        {
            DateTime dtStart = DateTime.Now;
            var sr = SdGetRequestResponse(methods.POST, "metadata/programs", request, false);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for artwork info of {request.Length,3} programs. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved artwork info for {request.Length,3} programs. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<IList<sdArtworkResponse>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdGetArtwork() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static Dictionary<string, IList<sdCountry>> GetAvailableCountries()
        {
            var sr = SdGetRequestResponse(methods.GET, "available/countries", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a list of available countries.");
                return null;
            }

            try
            {
                Logger.WriteVerbose("Successfully retrieved list of available countries from Schedules Direct.");
                return JsonConvert.DeserializeObject<Dictionary<string, IList<sdCountry>>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetAvailableCountries() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static dynamic GetAvailableSatellites()
        {
            var sr = SdGetRequestResponse(methods.GET, "available/dvb-s", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a list of available satellites.");
                return null;
            }

            try
            {
                Logger.WriteVerbose("Successfully retrieved list of available satellites from Schedules Direct.");
                return JsonConvert.DeserializeObject<dynamic>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetAvailableSatellites() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static Dictionary<string, string> GetTransmitters(string country)
        {
            var sr = SdGetRequestResponse(methods.GET, $"transmitters/{country}", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a list of available transmitters.");
                return null;
            }

            try
            {
                Logger.WriteVerbose("Successfully retrieved list of available transmitters from Schedules Direct.");
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetTransmitters() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static IList<sdHeadendResponse> GetHeadends(string country, string postalcode)
        {
            var sr = SdGetRequestResponse(methods.GET, $"headends?country={country}&postalcode={postalcode}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct for the headends of {country} and postal code {postalcode}.");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved the headends for {country} and postal code {postalcode}.");
                return JsonConvert.DeserializeObject<IList<sdHeadendResponse>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetHeadends() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }

        public static bool AddLineup(string lineup)
        {
            var sr = SdGetRequestResponse(methods.PUT, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct when trying to add lineup {lineup}.");
                return false;
            }

            try
            {
                var resp = JsonConvert.DeserializeObject<dynamic>(sr);
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
                Logger.WriteError($"AddLineup() Unknown exception thrown. Message: {ex.Message}");
            }
            return false;
        }

        public static bool RemoveLineup(string lineup)
        {
            var sr = SdGetRequestResponse(methods.DELETE, $"lineups/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct when trying to remove lineup {lineup}.");
                return false;
            }

            try
            {
                var resp = JsonConvert.DeserializeObject<dynamic>(sr);
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
                Logger.WriteError($"RemoveLineup() Unknown exception thrown. Message: {ex.Message}");
            }
            return false;
        }

        private static string SdErrorLookup(int code)
        {
            SdErrorCodes.TryGetValue(code, out var ret);
            return ret;
        }
        private static readonly Dictionary<int, string> SdErrorCodes = new Dictionary<int, string>()
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