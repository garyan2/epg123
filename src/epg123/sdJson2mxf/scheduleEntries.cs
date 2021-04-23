using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using epg123.MxfXml;
using epg123.SchedulesDirect;
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
            Logger.WriteMessage($"Entering GetAllScheduleEntryMd5s() for {days} days on {SdMxf.With.Services.Count} stations.");
            if (days <= 0)
            {
                Logger.WriteError("Invalid number of days to download. Exiting.");
                return false;
            }

            // reset counter
            processedObjects = 0;
            totalObjects = SdMxf.With.Services.Count * days;
            ++processStage; ReportProgress();

            // build date array for requests
            var dt = DateTime.UtcNow;

            // adjust number of days if currently within 0000 - 0400 UTC
            if (dt.Hour < 4)
            {
                ++days;
                dt -= TimeSpan.FromDays(1.0);
                totalObjects += SdMxf.With.Services.Count;
            }

            // build the date array to request
            var dates = new string[days];
            for (var i = 0; i < dates.Length; ++i)
            {
                dates[i] = dt.ToString("yyyy-MM-dd");
                dt = dt.AddDays(1.0);
            }

            // maximum 5000 queries at a time
            for (var i = 0; i < SdMxf.With.Services.Count; i += MaxQueries / dates.Length)
            {
                if (GetMd5ScheduleEntries(dates, i)) continue;
                Logger.WriteError("Problem occurred during GetMd5ScheduleEntries(). Exiting.");
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
            Logger.WriteInformation($"Processed {totalObjects} daily schedules for {SdMxf.With.Services.Count} stations.");
            Logger.WriteMessage("Exiting GetAllScheduleEntryMd5s(). SUCCESS.");
            GC.Collect();
            return true;
        }

        private static bool GetMd5ScheduleEntries(string[] dates, int start)
        {
            // reject 0 requests
            if (SdMxf.With.Services.Count - start < 1) return true;

            // build request for station schedules
            var requests = new ScheduleRequest[Math.Min(SdMxf.With.Services.Count - start, MaxQueries / dates.Length)];
            for (var i = 0; i < requests.Length; ++i)
            {
                requests[i] = new ScheduleRequest()
                {
                    StationId = SdMxf.With.Services[start + i].StationId,
                    Date = dates
                };
            }

            // request schedule md5s from Schedules Direct
            var stationResponses = SdApi.GetScheduleMd5s(requests);
            if (stationResponses == null) return false;

            // build request of daily schedules not downloaded yet
            var newRequests = new List<ScheduleRequest>();
            foreach (var request in requests)
            {
                var requestErrors = new Dictionary<int, string>();
                var mxfService = SdMxf.GetService(request.StationId);
                if (stationResponses.TryGetValue(request.StationId, out var stationResponse))
                {
                    // if the station return is empty, go to next station
                    if (stationResponse.Count == 0)
                    {
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
                    var newDateRequests = new List<string>();
                    var dupeMd5s = new HashSet<string>();
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

                            if (!ScheduleEntries.ContainsKey(dayResponse.Md5))
                            {
                                ScheduleEntries.Add(dayResponse.Md5, new[] { request.StationId, day });
                            }
                            else
                            {
                                var previous = ScheduleEntries[dayResponse.Md5][1];
                                var comment = $"Duplicate schedule Md5 return for stationId {mxfService.StationId} ({mxfService.CallSign}) on {day} with {previous}.";
                                Logger.WriteWarning(comment);
                                dupeMd5s.Add(dayResponse.Md5);
                            }
                        }
                        else if ((dayResponse != null) && (dayResponse.Code != 0) && !requestErrors.ContainsKey(dayResponse.Code))
                        {
                            requestErrors.Add(dayResponse.Code, dayResponse.Message);
                        }
                    }

                    // clear out dupe entries
                    foreach (var dupe in dupeMd5s)
                    {
                        var previous = ScheduleEntries[dupe][1];
                        var comment = $"Removing duplicate Md5 schedule entry for stationId {mxfService.StationId} ({mxfService.CallSign}) on {previous}.";
                        Logger.WriteWarning(comment);
                        ScheduleEntries.Remove(dupe);
                    }

                    // create the new request for the station
                    if (newDateRequests.Count > 0)
                    {
                        newRequests.Add(new ScheduleRequest()
                        {
                            StationId = request.StationId,
                            Date = newDateRequests.ToArray()
                        });
                    }
                }
                else
                {
                    // requested station was not in response
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
                var responses = SdApi.GetScheduleListings(newRequests.ToArray());
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
                                Logger.WriteInformation($"Failed to write station daily schedule file to cache file. station: {serviceDate[0]} ; date: {serviceDate[1]}");
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
                            Logger.WriteWarning($"Md5 mismatch for station {compare.Value[0]} on {compare.Value[1]}. Expected: {compare.Key} - Downloaded {response.Metadata.Md5}");
                        }
                        catch
                        {
                            Logger.WriteWarning($"Md5 mismatch for station {response.StationId} on {response.Metadata.StartDate}. Downloaded {response.Metadata.Md5}");
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
            ScheduleResponse schedule;
            try
            {
                using (var reader = new StringReader(epgCache.GetAsset(md5)))
                {
                    var serializer = new JsonSerializer();
                    schedule = (ScheduleResponse)serializer.Deserialize(reader, typeof(ScheduleResponse));
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
            var mxfService = SdMxf.GetService(schedule.StationId);

            // process each program schedule entry
            foreach (var scheduleProgram in schedule.Programs)
            {
                // prepopulate some of the program
                var mxfProgram = SdMxf.GetProgram(scheduleProgram.ProgramId);
                if (mxfProgram.extras.Count == 0)
                {
                    mxfProgram.ProgramId = scheduleProgram.ProgramId;
                    mxfProgram.UidOverride = $"{scheduleProgram.ProgramId.Substring(0, 10)}_{scheduleProgram.ProgramId.Substring(10)}";
                    mxfProgram.extras.Add("md5", scheduleProgram.Md5);
                    if (scheduleProgram.Multipart?.PartNumber > 0)
                    {
                        mxfProgram.extras.Add("multipart", $"{scheduleProgram.Multipart.PartNumber}/{scheduleProgram.Multipart.TotalParts}");
                    }
                    if (config.OadOverride && scheduleProgram.New)
                    {
                        mxfProgram.extras.Add("newAirDate", scheduleProgram.AirDateTime.ToLocalTime());
                    }
                }
                mxfProgram.IsSeasonFinale |= Helper.StringContains(scheduleProgram.IsPremiereOrFinale, "Season Finale");
                mxfProgram.IsSeasonPremiere |= Helper.StringContains(scheduleProgram.IsPremiereOrFinale, "Season Premiere");
                mxfProgram.IsSeriesFinale |= Helper.StringContains(scheduleProgram.IsPremiereOrFinale, "Series Finale");
                mxfProgram.IsSeriesPremiere |= Helper.StringContains(scheduleProgram.IsPremiereOrFinale, "Series Premiere");
                if (!mxfProgram.extras.ContainsKey("premiere")) mxfProgram.extras.Add("premiere", false);
                else mxfProgram.extras["premiere"] |= scheduleProgram.Premiere;

                // grab any tvratings from desired countries
                var scheduleTvRatings = new Dictionary<string, string>();
                if (scheduleProgram.Ratings != null)
                {
                    var ratings = config.RatingsOrigin.Split(',');
                    foreach (var rating in scheduleProgram.Ratings.Where(rating => string.IsNullOrEmpty(rating.Country) || Helper.TableContains(ratings, "ALL") || Helper.TableContains(ratings, rating.Country)))
                    {
                        scheduleTvRatings.Add(rating.Body, rating.Code);
                    }
                }

                // populate the schedule entry and create program entry as required
                mxfService.MxfScheduleEntries.ScheduleEntry.Add(new MxfScheduleEntry
                {
                    AudioFormat = EncodeAudioFormat(scheduleProgram.AudioProperties),
                    Duration = scheduleProgram.Duration,
                    Is3D = Helper.TableContains(scheduleProgram.VideoProperties, "3d"),
                    IsBlackout = scheduleProgram.SubjectToBlackout,
                    IsClassroom = scheduleProgram.CableInTheClassroom,
                    IsCc = Helper.TableContains(scheduleProgram.AudioProperties, "cc"),
                    IsDelay = Helper.StringContains(scheduleProgram.LiveTapeDelay, "delay"),
                    IsDvs = Helper.TableContains(scheduleProgram.AudioProperties, "dvs"),
                    IsEnhanced = Helper.TableContains(scheduleProgram.VideoProperties, "enhanced"),
                    IsFinale = Helper.StringContains(scheduleProgram.IsPremiereOrFinale, "finale"),
                    IsHdtv = CheckHdOverride(schedule.StationId) || !CheckSdOverride(schedule.StationId) && Helper.TableContains(scheduleProgram.VideoProperties, "hdtv"),
                    //IsHdtvSimulCast = null,
                    IsInProgress = scheduleProgram.JoinedInProgress,
                    IsLetterbox = Helper.TableContains(scheduleProgram.VideoProperties, "letterbox"),
                    IsLive = Helper.StringContains(scheduleProgram.LiveTapeDelay, "live"),
                    //IsLiveSports = null,
                    IsRepeat = !scheduleProgram.New,
                    IsSap = Helper.TableContains(scheduleProgram.AudioProperties, "sap"),
                    IsSubtitled = Helper.TableContains(scheduleProgram.AudioProperties, "subtitled"),
                    IsTape = Helper.StringContains(scheduleProgram.LiveTapeDelay, "tape"),
                    Part = scheduleProgram.Multipart?.PartNumber ?? 0,
                    Parts = scheduleProgram.Multipart?.TotalParts ?? 0,
                    mxfProgram = mxfProgram,
                    StartTime = scheduleProgram.AirDateTime,
                    //TvRating is determined in the class itself to combine with the program content ratings
                    IsSigned = scheduleProgram.Signed
                });
                mxfService.MxfScheduleEntries.ScheduleEntry.Last().extras.Add("ratings", scheduleTvRatings);
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