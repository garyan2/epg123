using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using epg123.MxfXml;
using epg123.SchedulesDirectAPI;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static readonly Dictionary<string, string[]> ScheduleEntries = new Dictionary<string, string[]>();
        private static int cachedSchedules;
        private static int downloadedSchedules;

        private static bool GetAllScheduleEntryMd5S(int days = 1)
        {
            Logger.WriteMessage($"Entering getAllScheduleEntryMd5s() for {days} days on {SdMxf.With[0].Services.Count} stations.");
            if (days <= 0)
            {
                Logger.WriteError("Invalid number of days to download. Exiting.");
                return false;
            }

            // reset counter
            processedObjects = 0;
            totalObjects = SdMxf.With[0].Services.Count * days;
            ++processStage; ReportProgress();

            // build date array for requests
            var dt = DateTime.UtcNow;

            // adjust number of days if currently within 0000 - 0400 UTC
            if (dt.Hour < 4)
            {
                ++days;
                dt -= TimeSpan.FromDays(1.0);
                totalObjects += SdMxf.With[0].Services.Count;
            }

            // build the date array to request
            var dates = new string[days];
            for (var i = 0; i < dates.Length; ++i)
            {
                dates[i] = dt.ToString("yyyy-MM-dd");
                dt = dt.AddDays(1.0);
            }

            // maximum 5000 queries at a time
            for (var i = 0; i < SdMxf.With[0].Services.Count; i += MaxQueries / dates.Length)
            {
                if (GetMd5ScheduleEntries(dates, i)) continue;
                Logger.WriteError("Problem occurred during getMd5ScheduleEntries(). Exiting.");
                return false;
            }
            Logger.WriteVerbose($"Found {cachedSchedules} cached daily schedules.");
            Logger.WriteVerbose($"Downloaded {downloadedSchedules} daily schedules.");

            // reset counters again
            processedObjects = 0;
            totalObjects = ScheduleEntries.Count;
            ++processStage; ReportProgress();

            // process all schedules
            foreach (var md5 in ScheduleEntries.Keys)
            {
                ++processedObjects; ReportProgress();
                ProcessMd5ScheduleEntry(md5);
            }
            Logger.WriteInformation($"Processed {totalObjects} daily schedules for {SdMxf.With[0].Services.Count} stations.");
            Logger.WriteMessage("Exiting getAllScheduleEntryMd5s(). SUCCESS.");
            GC.Collect();
            return true;
        }

        private static bool GetMd5ScheduleEntries(string[] dates, int start)
        {
            // build request for station schedules
            var requests = new sdScheduleRequest[Math.Min(SdMxf.With[0].Services.Count - start, MaxQueries / dates.Length)];
            for (var i = 0; i < requests.Length; ++i)
            {
                requests[i] = new sdScheduleRequest()
                {
                    StationId = SdMxf.With[0].Services[start + i].StationId,
                    Date = dates
                };
            }

            // request schedule md5s from Schedules Direct
            var stationResponses = sdApi.SdGetScheduleMd5S(requests);
            if (stationResponses == null) return false;

            // build request of daily schedules not downloaded yet
            IList<sdScheduleRequest> newRequests = new List<sdScheduleRequest>();
            foreach (var request in requests)
            {
                var requestErrors = new Dictionary<int, string>();
                if (stationResponses.TryGetValue(request.StationId, out var stationResponse))
                {
                    // if the station return is empty, go to next station
                    if (stationResponse.Count == 0)
                    {
                        var mxfService = SdMxf.With[0].GetService(request.StationId);
                        var comment = $"Failed to parse the schedule Md5 return for stationId {mxfService.StationId} ({mxfService.CallSign}) on {dates[0]} and after.";
                        if (CheckSuppressWarnings(mxfService.CallSign))
                        {
                            Logger.WriteInformation(comment);
                        }
                        else
                        {
                            Logger.WriteWarning(comment);
                        }
                        processedObjects += dates.Length; ReportProgress();
                        continue;
                    }

                    // scan through all the dates returned for the station and request dates that are not cached
                    IList<string> newDateRequests = new List<string>();
                    foreach (var day in dates)
                    {
                        if (stationResponse.TryGetValue(day, out var dayResponse) && (dayResponse.Code == 0) && !string.IsNullOrEmpty(dayResponse.Md5))
                        {
                            var filepath = $"{Helper.Epg123CacheFolder}\\{SafeFilename(dayResponse.Md5)}";
                            var file = new FileInfo(filepath);
                            if (file.Exists && (file.Length > 0) && !epgCache.JsonFiles.ContainsKey(dayResponse.Md5))
                            {
                                using (var reader = File.OpenText(filepath))
                                {
                                    epgCache.AddAsset(dayResponse.Md5, reader.ReadToEnd());
                                }
                            }

                            if (epgCache.JsonFiles.ContainsKey(dayResponse.Md5))
                            {
                                ++processedObjects; ReportProgress();
                                ++cachedSchedules;
                            }
                            else
                            {
                                newDateRequests.Add(day);
                            }
                            ScheduleEntries.Add(dayResponse.Md5, new[] { request.StationId, day });
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
                            StationId = request.StationId,
                            Date = newDateRequests.ToArray()
                        });
                    }
                }
                else
                {
                    // requested station was not in response
                    var mxfService = SdMxf.With[0].GetService(request.StationId);
                    Logger.WriteWarning($"Requested stationId {mxfService.StationId} ({mxfService.CallSign}) was not present in schedule Md5 response.");
                    processedObjects += dates.Length; ReportProgress();
                    continue;
                }

                if (requestErrors.Count <= 0) continue;
                foreach (var keyValuePair in requestErrors)
                {
                    Logger.WriteError($"Requests for MD5 schedule entries of station {request.StationId} returned error code {keyValuePair.Key} , message: {keyValuePair.Value}");
                }
            }
            ReportProgress();

            // download the remaining daily schedules to the cache directory
            if (newRequests.Count > 0)
            {
                // request daily schedules from Schedules Direct
                var responses = sdApi.SdGetScheduleListings(newRequests.ToArray());
                if (responses == null) return false;

                // process the responses
                foreach (var response in responses)
                {
                    ++processedObjects; ReportProgress();
                    if (response?.Programs == null)
                    {
                        continue;
                    }
                    ++downloadedSchedules;

                    // serialize JSON directly to a file
                    if (ScheduleEntries.TryGetValue(response.Metadata.Md5, out var serviceDate))
                    {
                        using (var writer = new StringWriter())
                        {
                            try
                            {
                                var serializer = new JsonSerializer();
                                serializer.Serialize(writer, response);
                                epgCache.AddAsset(response.Metadata.Md5, writer.ToString());
                            }
                            catch
                            {
                                Logger.WriteError($"Failed to write station daily schedule file to cache directory. station: {serviceDate[0]} ; date: {serviceDate[1]}");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var compare = ScheduleEntries
                                .Where(arg => arg.Value[0].Equals(response.StationId))
                                .Single(arg => arg.Value[1].Equals(response.Metadata.StartDate));
                            Logger.WriteError($"Md5 mismatch for station {compare.Value[0]} on {compare.Value[1]}. Expected: {compare.Key} - Downloaded {response.Metadata.Md5}");
                        }
                        catch
                        {
                            Logger.WriteError($"Md5 mismatch for station {response.StationId} on {response.Metadata.StartDate}. Downloaded {response.Metadata.Md5}");
                        }
                    }
                }
            }
            ReportProgress();
            return true;
        }

        private static void ProcessMd5ScheduleEntry(string md5)
        {
            // ensure cached file exists
            if (!epgCache.JsonFiles.ContainsKey(md5))
            {
                return;
            }

            // read the cached file
            sdScheduleResponse schedule;
            try
            {
                using (var reader = new StringReader(epgCache.GetAsset(md5)))
                {
                    var serializer = new JsonSerializer();
                    schedule = (sdScheduleResponse)serializer.Deserialize(reader, typeof(sdScheduleResponse));
                    if (schedule == null)
                    {
                        Logger.WriteError("Failed to read Md5Schedule file in cache directory.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error occurred when trying to read Md5Schedule file in cache directory. Message: " + ex.Message);
                return;
            }

            // determine which service entry applies to
            var mxfService = SdMxf.With[0].GetService(schedule.StationId);

            // process each program schedule entry
            var firstProgram = true;
            var discontinuity = false;
            var lastProgramId = string.Empty;
            foreach (var program in schedule.Programs)
            {
                // determine whether to populate StartTime attribute
                var dtStart = program.AirDateTime;
                var startTime = dtStart.ToString("s");
                if (dtStart == mxfService.MxfScheduleEntries.EndTime)
                {
                    startTime = null;
                }
                else if (!firstProgram && !discontinuity)
                {
                    var msg = string.Format("Time discontinuity detected. Service {0} ({1}) {2} (entry {3}) ends at {4:u}, {5} (entry {6}) starts at {7:u}. " +
                                            "No further discontinuities on {1} will be reported for the date {8:yyyy-MM-dd}.",
                        schedule.StationId,
                        mxfService.CallSign,
                        lastProgramId,
                        mxfService.MxfScheduleEntries.ScheduleEntry.Count,
                        mxfService.MxfScheduleEntries.EndTime,
                        program.ProgramId,
                        mxfService.MxfScheduleEntries.ScheduleEntry.Count + 1,
                        dtStart, mxfService.MxfScheduleEntries.EndTime);
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
                lastProgramId = program.ProgramId;

                // prepopulate some of the program
                var prog = new MxfProgram()
                {
                    Index = SdMxf.With[0].Programs.Count + 1,
                    Md5 = program.Md5,
                    TmsId = program.ProgramId,
                    IsSeasonFinale = Helper.StringContains(program.IsPremiereOrFinale, "Season Finale"),
                    IsSeasonPremiere = Helper.StringContains(program.IsPremiereOrFinale, "Season Premiere"),
                    IsSeriesFinale = Helper.StringContains(program.IsPremiereOrFinale, "Series Finale"),
                    IsSeriesPremiere = Helper.StringContains(program.IsPremiereOrFinale, "Series Premiere"),
                    IsPremiere = program.Premiere || Helper.StringContains(program.IsPremiereOrFinale, "Premiere"),
                    Part = program.Multipart?.PartNumber ?? 0,
                    Parts = program.Multipart?.TotalParts ?? 0,
                    NewDate = (config.OadOverride && program.New) ? dtStart.ToLocalTime() : DateTime.MinValue
                };

                // grab any tvratings from desired countries
                var scheduleTvRatings = new Dictionary<string, string>();
                if (program.Ratings != null)
                {
                    var ratings = config.RatingsOrigin.Split(',');
                    foreach (var rating in program.Ratings)
                    {
                        if (string.IsNullOrEmpty(rating.Country) || Helper.TableContains(ratings, "ALL") || Helper.TableContains(ratings, rating.Country))
                        {
                            scheduleTvRatings.Add(rating.Body, rating.Code);
                        }
                    }
                }

                // populate the schedule entry and create program entry as required
                mxfService.MxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry()
                {
                    SchedTvRatings = scheduleTvRatings,
                    AudioFormat = EncodeAudioFormat(program.AudioProperties),
                    Duration = program.Duration,
                    Is3D = Helper.TableContains(program.VideoProperties, "3d"),
                    IsBlackout = program.SubjectToBlackout,
                    IsClassroom = program.CableInTheClassroom,
                    IsCc = Helper.TableContains(program.AudioProperties, "cc"),
                    IsDelay = Helper.StringContains(program.LiveTapeDelay, "delay"),
                    IsDvs = Helper.TableContains(program.AudioProperties, "dvs"),
                    IsEnhanced = Helper.TableContains(program.VideoProperties, "enhanced"),
                    IsFinale = Helper.StringContains(program.IsPremiereOrFinale, "finale"),
                    IsHdtv = CheckHdOverride(schedule.StationId) || !CheckSdOverride(schedule.StationId) && Helper.TableContains(program.VideoProperties, "hdtv"),
                    //IsHdtvSimulCast = null,
                    IsInProgress = program.JoinedInProgress,
                    IsLetterbox = Helper.TableContains(program.VideoProperties, "letterbox"),
                    IsLive = Helper.StringContains(program.LiveTapeDelay, "live"),
                    //IsLiveSports = null,
                    IsPremiere = prog.IsPremiere,
                    IsRepeat = !program.New,
                    IsSap = Helper.TableContains(program.AudioProperties, "sap"),
                    IsSubtitled = Helper.TableContains(program.AudioProperties, "subtitled"),
                    IsTape = Helper.StringContains(program.LiveTapeDelay, "tape"),
                    Part = program.Multipart?.PartNumber ?? 0,
                    Parts = program.Multipart?.TotalParts ?? 0,
                    Program = SdMxf.With[0].GetProgram(program.ProgramId, prog).Id,
                    StartTime = (startTime == null) ? DateTime.MinValue : DateTime.Parse(startTime),
                    //TvRating is determined in the class itself to combine with the program content ratings
                    IsSigned = program.Signed
                });
            }
        }

        private static int EncodeAudioFormat(string[] audioProperties)
        {
            var maxValue = 0;
            if (audioProperties == null) return maxValue;
            foreach (var property in audioProperties)
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
                }
            }
            return maxValue;
        }

        private static bool CheckHdOverride(string stationId)
        {
            return (from station in config.StationId ?? new List<SdChannelDownload>() where station.StationId == stationId select station.HdOverride).FirstOrDefault();
        }
        private static bool CheckSdOverride(string stationId)
        {
            return (from station in config.StationId ?? new List<SdChannelDownload>() where station.StationId == stationId select station.SdOverride).FirstOrDefault();
        }
        private static bool CheckSuppressWarnings(string callsign)
        {
            if (suppressedPrefixes.Contains("*")) return true;
            foreach (var prefix in suppressedPrefixes)
            {
                if (callsign.Equals(prefix)) return true;
                if (prefix.Contains("*") && callsign.StartsWith(prefix.Replace("*", ""))) return true;
            }
            return false;
        }

        private static string SafeFilename(string md5)
        {
            return md5 == null ? null : Regex.Replace(md5, @"[^\w\.@-]", "-");
        }
    }
}