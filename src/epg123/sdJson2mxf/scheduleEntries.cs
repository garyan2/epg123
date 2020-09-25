using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using epg123.MxfXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static Dictionary<string, string[]> scheduleEntries = new Dictionary<string, string[]>();
        private static int cachedSchedules = 0;
        private static int downloadedSchedules = 0;

        private static bool getAllScheduleEntryMd5s(int days = 1)
        {
            Logger.WriteMessage(string.Format("Entering getAllScheduleEntryMd5s() for {0} days on {1} stations.", days, sdMxf.With[0].Services.Count));

            // reset counter
            processedObjects = 0;
            totalObjects = sdMxf.With[0].Services.Count * days;
            ++processStage; reportProgress();

            // build date array for requests
            string[] dates = null;
            if (days > 0)
            {
                DateTime dt = DateTime.UtcNow;

                // adjust number of days if currently within 0000 - 0400 UTC
                if (dt.Hour < 4)
                {
                    ++days;
                    dt -= TimeSpan.FromDays(1.0);
                    totalObjects += sdMxf.With[0].Services.Count;
                }

                // build the date array to request
                dates = new string[days];
                for (int i = 0; i < dates.Length; ++i)
                {
                    dates[i] = dt.ToString("yyyy-MM-dd");
                    dt = dt.AddDays(1.0);
                }
            }

            // maximum 5000 queries at a time
            for (int i = 0; i < sdMxf.With[0].Services.Count; i += MAXQUERIES / dates.Length)
            {
                if (!getMd5ScheduleEntries(dates, i))
                {
                    Logger.WriteError("Problem occurred during getMd5ScheduleEntries(). Exiting.");
                    return false;
                }
            }
            Logger.WriteVerbose(string.Format("Found {0} cached daily schedules.", cachedSchedules));
            Logger.WriteVerbose(string.Format("Downloaded {0} daily schedules.", downloadedSchedules));

            // reset counters again
            processedObjects = 0;
            totalObjects = scheduleEntries.Count;
            ++processStage; reportProgress();

            // process all schedules
            foreach (string md5 in scheduleEntries.Keys)
            {
                ++processedObjects; reportProgress();
                if (!processMd5ScheduleEntry(md5))
                {
                    string filepath = string.Format("{0}\\{1}", Helper.Epg123CacheFolder, safeFilename(md5));
                    if (File.Exists(filepath))
                    {
                        try
                        {
                            File.Delete(filepath);
                            Logger.WriteError(string.Format("Deleted bad/corrupted file \"{0}\".", filepath));
                        }
                        catch { }
                    }
                    //return false;
                }
            }
            Logger.WriteInformation(string.Format("Processed {0} daily schedules for {1} stations.", totalObjects, sdMxf.With[0].Services.Count));
            Logger.WriteMessage("Exiting getAllScheduleEntryMd5s(). SUCCESS.");
            GC.Collect();
            return true;
        }

        private static bool getMd5ScheduleEntries(string[] dates, int start)
        {
            // build request for station schedules
            sdScheduleRequest[] requests = new sdScheduleRequest[Math.Min(sdMxf.With[0].Services.Count - start, MAXQUERIES / dates.Length)];
            for (int i = 0; i < requests.Length; ++i)
            {
                requests[i] = new sdScheduleRequest()
                {
                    StationID = sdMxf.With[0].Services[start + i].StationID,
                    Date = dates
                };
            }

            // request schedule md5s from Schedules Direct
            Dictionary<string, Dictionary<string, sdScheduleMd5DateResponse>> stationResponses = sdAPI.sdGetScheduleMd5s(requests);
            if (stationResponses == null) return false;

            // build request of daily schedules not downloaded yet
            IList<sdScheduleRequest> newRequests = new List<sdScheduleRequest>();
            foreach (sdScheduleRequest request in requests)
            {
                Dictionary<int, string> requestErrors = new Dictionary<int, string>();
                Dictionary<string, sdScheduleMd5DateResponse> stationResponse;
                if (stationResponses.TryGetValue(request.StationID, out stationResponse))
                {
                    // if the station return is empty, go to next station
                    if (stationResponse.Count == 0)
                    {
                        MxfService mxfService = sdMxf.With[0].getService(request.StationID);
                        string comment = string.Format("Failed to parse the schedule Md5 return for stationId {0} ({1}) on {2} and after.", mxfService.StationID, mxfService.CallSign, dates[0]);
                        if (checkSuppressWarnings(mxfService.CallSign))
                        {
                            Logger.WriteInformation(comment);
                        }
                        else
                        {
                            Logger.WriteWarning(comment);
                        }
                        processedObjects += dates.Length; reportProgress();
                        continue;
                    }

                    // scan through all the dates returned for the station and request dates that are not cached
                    IList<string> newDateRequests = new List<string>();
                    foreach (string day in dates)
                    {
                        sdScheduleMd5DateResponse dayResponse;
                        if (stationResponse.TryGetValue(day, out dayResponse) && (dayResponse.Code == 0) && !string.IsNullOrEmpty(dayResponse.Md5))
                        {
                            string filepath = string.Format("{0}\\{1}", Helper.Epg123CacheFolder, safeFilename(dayResponse.Md5));
                            FileInfo file = new FileInfo(filepath);
                            if (file.Exists && (file.Length > 0) && !epgCache.JsonFiles.ContainsKey(dayResponse.Md5))
                            {
                                using (StreamReader reader = File.OpenText(filepath))
                                {
                                    epgCache.AddAsset(dayResponse.Md5, reader.ReadToEnd());
                                }
                            }

                            if (epgCache.JsonFiles.ContainsKey(dayResponse.Md5))
                            {
                                ++processedObjects; reportProgress();
                                ++cachedSchedules;
                            }
                            else
                            {
                                newDateRequests.Add(day);
                            }
                            scheduleEntries.Add(dayResponse.Md5, new string[] { request.StationID, day });
                        }
                        else if ((dayResponse != null) && (dayResponse.Code != 0) && !requestErrors.ContainsKey(dayResponse.Code))
                        {
                            requestErrors.Add(dayResponse.Code, dayResponse.Message);
                        }
                    }

                    // create the new request for the station
                    if (newDateRequests.Count > 0)
                    {
                        newRequests.Add(new sdScheduleRequest()
                        {
                            StationID = request.StationID,
                            Date = newDateRequests.ToArray()
                        });
                    }
                }
                else
                {
                    // requested station was not in response
                    MxfService mxfService = sdMxf.With[0].getService(request.StationID);
                    Logger.WriteWarning(string.Format("Requested stationId {0} ({1}) was not present in schedule Md5 response.", mxfService.StationID, mxfService.CallSign));
                    processedObjects += dates.Length; reportProgress();
                    continue;
                }

                if (requestErrors.Count > 0)
                {
                    foreach (KeyValuePair<int, string> keyValuePair in requestErrors)
                    {
                        Logger.WriteError(string.Format("Requests for MD5 schedule entries of station {0} returned error code {1} , message: {2}",
                            request.StationID, keyValuePair.Key, keyValuePair.Value));
                    }
                }
            }
            reportProgress();

            // download the remaining daily schedules to the cache directory
            if (newRequests.Count > 0)
            {
                // request daily schedules from Schedules Direct
                IList<sdScheduleResponse> responses = sdAPI.sdGetScheduleListings(newRequests.ToArray());
                if (responses == null) return false;

                // process the responses
                foreach (sdScheduleResponse response in responses)
                {
                    ++processedObjects; reportProgress();
                    if (response == null || response.Programs == null)
                    {
                        continue;
                    }
                    ++downloadedSchedules;

                    // serialize JSON directly to a file
                    if (scheduleEntries.TryGetValue(response.Metadata.Md5, out string[] serviceDate))
                    {
                        using (StringWriter writer = new StringWriter())
                        {
                            try
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                serializer.Serialize(writer, response);
                                epgCache.AddAsset(response.Metadata.Md5, writer.ToString());
                            }
                            catch
                            {
                                Logger.WriteError(string.Format("Failed to write station daily schedule file to cache directory. station: {0} ; date: {1}", serviceDate[0], serviceDate[1]));
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var compare = scheduleEntries.Where(arg => arg.Value[0].Equals(response.StationID))
                                                         .Where(arg => arg.Value[1].Equals(response.Metadata.StartDate))
                                                         .Single();
                            Logger.WriteError(string.Format("Md5 mismatch for station {0} on {1}. Expected: {2} - Downloaded {3}", compare.Value[0], compare.Value[1], compare.Key, response.Metadata.Md5));
                        }
                        catch
                        {
                            Logger.WriteError(string.Format("Md5 mismatch for station {0} on {1}. Downloaded {2}", response.StationID, response.Metadata.StartDate, response.Metadata.Md5));
                        }
                    }
                }
            }
            reportProgress();
            return true;
        }

        private static bool processMd5ScheduleEntry(string md5)
        {
            // ensure cached file exists
            if (!epgCache.JsonFiles.ContainsKey(md5))
            {
                return false;
            }

            // read the cached file
            sdScheduleResponse schedule;
            try
            {
                using (StringReader reader = new StringReader(epgCache.GetAsset(md5)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    schedule = (sdScheduleResponse)serializer.Deserialize(reader, typeof(sdScheduleResponse));
                    if (schedule == null)
                    {
                        Logger.WriteError("Failed to read Md5Schedule file in cache directory.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error occurred when trying to read Md5Schedule file in cache directory. Message: " + ex.Message);
                return false;
            }

            // determine which service entry applies to
            MxfService mxfService = sdMxf.With[0].getService(schedule.StationID);

            // process each program schedule entry
            bool firstProgram = true;
            bool discontinuity = false;
            string lastProgramID = string.Empty;
            foreach (sdSchedProgram program in schedule.Programs)
            {
                // determine whether to populate StartTime attribute
                DateTime dtStart = program.AirDateTime;
                string startTime = dtStart.ToString("s");
                if (dtStart == mxfService.mxfScheduleEntries.endTime)
                {
                    startTime = null;
                }
                else if (!firstProgram && !discontinuity)
                {
                    string msg = string.Format("Time discontinuity detected. Service {0} ({1}) {2} (entry {3}) ends at {4:u}, {5} (entry {6}) starts at {7:u}. " +
                                               "No further discontinuities on {1} will be reported for the date {8}.",
                        schedule.StationID,
                        mxfService.CallSign,
                        lastProgramID,
                        mxfService.mxfScheduleEntries.ScheduleEntry.Count,
                        mxfService.mxfScheduleEntries.endTime,
                        program.ProgramID,
                        mxfService.mxfScheduleEntries.ScheduleEntry.Count + 1,
                        dtStart,
                        mxfService.mxfScheduleEntries.endTime.ToString("yyyy-MM-dd"));
                    if (dtStart < DateTime.UtcNow + TimeSpan.FromDays(2))
                    {
                        Logger.WriteWarning(msg);
                    }
                    else
                    {
                        Logger.WriteInformation(msg);
                    }
                    discontinuity = true;
                }
                firstProgram = false;
                lastProgramID = program.ProgramID;

                // prepopulate some of the program
                MxfProgram prog = new MxfProgram()
                {
                    index = sdMxf.With[0].Programs.Count + 1,
                    md5 = program.Md5,
                    tmsId = program.ProgramID,
                    IsSeasonFinale = Helper.stringContains(program.IsPremiereOrFinale, "Season Finale"),
                    IsSeasonPremiere = Helper.stringContains(program.IsPremiereOrFinale, "Season Premiere"),
                    IsSeriesFinale = Helper.stringContains(program.IsPremiereOrFinale, "Series Finale"),
                    IsSeriesPremiere = Helper.stringContains(program.IsPremiereOrFinale, "Series Premiere"),
                    IsPremiere = program.Premiere || Helper.stringContains(program.IsPremiereOrFinale, "Premiere"),
                    _part = (program.Multipart != null) ? program.Multipart.PartNumber : 0,
                    _parts = (program.Multipart != null) ? program.Multipart.TotalParts : 0,
                    _newDate = (config.OADOverride && program.New) ? dtStart.ToLocalTime() : DateTime.MinValue
                };

                // grab any tvratings from desired countries
                Dictionary<string, string> scheduleTvRatings = new Dictionary<string, string>();
                if (program.Ratings != null)
                {
                    string[] ratings = config.RatingsOrigin.Split(',');
                    foreach (sdSchedTvRating rating in program.Ratings)
                    {
                        if (string.IsNullOrEmpty(rating.Country) || Helper.tableContains(ratings, "ALL") || Helper.tableContains(ratings, rating.Country))
                        {
                            scheduleTvRatings.Add(rating.Body, rating.Code);
                        }
                    }
                }

                // populate the schedule entry and create program entry as required
                mxfService.mxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                {
                    schedTvRatings = scheduleTvRatings,
                    AudioFormat = encodeAudioFormat(program.AudioProperties),
                    Duration = program.Duration,
                    Is3D = Helper.tableContains(program.VideoProperties, "3d"),
                    IsBlackout = program.SubjectToBlackout,
                    IsClassroom = program.CableInTheClassroom,
                    IsCC = Helper.tableContains(program.AudioProperties, "cc"),
                    IsDelay = Helper.stringContains(program.LiveTapeDelay, "delay"),
                    IsDvs = Helper.tableContains(program.AudioProperties, "dvs"),
                    IsEnhanced = Helper.tableContains(program.VideoProperties, "enhanced"),
                    IsFinale = Helper.stringContains(program.IsPremiereOrFinale, "finale"),
                    IsHdtv = checkHdOverride(schedule.StationID) ? true : checkSdOverride(schedule.StationID) ? false : Helper.tableContains(program.VideoProperties, "hdtv"),
                    //IsHdtvSimulCast = null,
                    IsInProgress = program.JoinedInProgress,
                    IsLetterbox = Helper.tableContains(program.VideoProperties, "letterbox"),
                    IsLive = Helper.stringContains(program.LiveTapeDelay, "live"),
                    //IsLiveSports = null,
                    IsPremiere = prog.IsPremiere,
                    IsRepeat = !program.New,
                    IsSap = Helper.tableContains(program.AudioProperties, "sap"),
                    IsSubtitled = Helper.tableContains(program.AudioProperties, "subtitled"),
                    IsTape = Helper.stringContains(program.LiveTapeDelay, "tape"),
                    Part = (program.Multipart != null) ? program.Multipart.PartNumber : 0,
                    Parts = (program.Multipart != null) ? program.Multipart.TotalParts : 0,
                    Program = sdMxf.With[0].getProgram(program.ProgramID, prog).Id,
                    StartTime = (startTime == null) ? DateTime.MinValue : DateTime.Parse(startTime),
                    //TvRating is determined in the class itself to combine with the program content ratings
                    IsSigned = program.Signed
                });
            }
            return true;
        }

        private static int encodeAudioFormat(string[] audioProperties)
        {
            int maxValue = 0;
            if (audioProperties != null)
            {
                foreach (string property in audioProperties)
                {
                    switch (property.ToLower())
                    {
                        case "stereo":
                            maxValue = Math.Max(maxValue, 2);
                            break;
                        case "dolby":
                        case "surround":
                            maxValue = Math.Max(maxValue, 3);
                            break;
                        case "dd":
                        case "dd 5.1":
                            maxValue = Math.Max(maxValue, 4);
                            break;
                        default:
                            break;
                    }
                }
            }
            return maxValue;
        }

        private static bool checkHdOverride(string stationId)
        {
            foreach (SdChannelDownload station in config.StationID ?? new List<SdChannelDownload>())
            {
                if (station.StationID == stationId) return station.HDOverride;
            }
            return false;
        }
        private static bool checkSdOverride(string stationId)
        {
            foreach (SdChannelDownload station in config.StationID ?? new List<SdChannelDownload>())
            {
                if (station.StationID == stationId) return station.SDOverride;
            }
            return false;
        }
        private static bool checkSuppressWarnings(string callsign)
        {
            if (sdJson2mxf.suppressedPrefixes.Contains("*")) return true;
            foreach (string prefix in sdJson2mxf.suppressedPrefixes)
            {
                if (callsign.Equals(prefix)) return true;
                if (prefix.Contains("*") && callsign.StartsWith(prefix.Replace("*", ""))) return true;
            }
            return false;
        }

        private static string safeFilename(string md5)
        {
            if (md5 == null) return null;
            return Regex.Replace(md5, @"[^\w\.@-]", "-");
        }
    }
}