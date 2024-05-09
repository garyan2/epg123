﻿using GaRyan2.MxfXml;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using api = GaRyan2.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static readonly Dictionary<string, string[]> ScheduleEntries = new Dictionary<string, string[]>();
        private static int cachedSchedules;
        private static int downloadedSchedules;
        private static List<string> suppressedPrefixes = new List<string>();
        private static int missingGuide;

        private static bool GetAllScheduleEntryMd5S(int days = 1)
        {
            Logger.WriteMessage($"Entering GetAllScheduleEntryMd5s() for {days} days on {mxf.With.Services.Count} stations.");
            if (days <= 0)
            {
                Logger.WriteError("Invalid number of days to download. Exiting.");
                return false;
            }

            // populate station prefixes to suppress
            suppressedPrefixes = new List<string>(config.SuppressStationEmptyWarnings.Split(','));

            // reset counter
            processedObjects = 0;
            totalObjects = mxf.With.Services.Count * days;
            ++processStage; ReportProgress();

            // build date array for requests
            var dt = DateTime.UtcNow;

            // build the date array to request
            var dates = new string[days];
            for (var i = 0; i < dates.Length; ++i)
            {
                dates[i] = dt.ToString("yyyy-MM-dd");
                dt = dt.AddDays(1.0);
            }

            // maximum 5000 queries at a time
            for (var i = 0; i < mxf.With.Services.Count; i += MaxQueries / dates.Length)
            {
                if (GetMd5ScheduleEntries(dates, i)) continue;
                Logger.WriteError("Problem occurred during GetMd5ScheduleEntries(). Exiting.");
                return false;
            }
            Logger.WriteVerbose($"Found {cachedSchedules} cached daily schedules.");
            Logger.WriteVerbose($"Downloaded {downloadedSchedules} daily schedules.");

            var missing = (double)missingGuide / mxf.With.Services.Count;
            if (missing > 0.1 && config.CreateXmltv)
            {
                Logger.WriteError($"{100 * missing:N1}% of all stations are missing guide data. Aborting update.");
                return false;
            }

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
            Logger.WriteInformation($"Processed {totalObjects} daily schedules for {mxf.With.Services.Count} stations for average of {(double)totalObjects / mxf.With.Services.Count:N1} days per station.");
            Logger.WriteMessage("Exiting GetAllScheduleEntryMd5s(). SUCCESS.");
            return true;
        }

        private static bool GetMd5ScheduleEntries(string[] dates, int start)
        {
            // reject 0 requests
            if (mxf.With.Services.Count - start < 1) return true;

            // build request for station schedules
            var requests = new ScheduleRequest[Math.Min(mxf.With.Services.Count - start, MaxQueries / dates.Length)];
            for (var i = 0; i < requests.Length; ++i)
            {
                requests[i] = new ScheduleRequest()
                {
                    StationId = mxf.With.Services[start + i].StationId,
                    Date = dates
                };
            }

            // request schedule md5s from Schedules Direct
            var stationResponses = api.GetScheduleMd5s(requests);
            if (stationResponses == null) return false;

            // build request of daily schedules not downloaded yet
            var newRequests = new List<ScheduleRequest>();
            foreach (var request in requests)
            {
                var requestErrors = new Dictionary<int, string>();
                var mxfService = mxf.FindOrCreateService(request.StationId);
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
                            ++missingGuide;
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
                    Logger.WriteWarning($"Requests for MD5 schedule entries of station {request.StationId} returned error code {keyValuePair.Key} , message: {keyValuePair.Value}");
                    if (keyValuePair.Key == 7030)
                    {
                        Logger.WriteWarning("ACTION: Login and report a lineup problem with Schedules Direct at https://schedulesdirect.org with the above error.");
                    }
                }
            }
            ReportProgress();

            // download the remaining daily schedules to the cache directory
            if (newRequests.Count > 0)
            {
                // request daily schedules from Schedules Direct
                var responses = api.GetScheduleListings(newRequests.ToArray());
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
                            Logger.WriteWarning($"Md5 mismatch for station {compare.Value[0]} on {compare.Value[1]}. Expected: {compare.Key} - Downloaded: {response.Metadata.Md5}");
                        }
                        catch
                        {
                            Logger.WriteWarning($"Md5 mismatch for station {response.StationId} on {response.Metadata.StartDate}. Downloaded: {response.Metadata.Md5}");
                        }
                        Logger.WriteWarning("ACTION: This type of warning is typically temporary and will probably clear itself within 24 hours.");
                        Logger.WriteWarning("ACTION: If the issue persists for the same station over multiple days, submit a ticket with Schedules Direct at https://schedulesdirect.org.");
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
                        Logger.WriteError("Failed to read Md5Schedule entry in cache file.");
                        Logger.WriteError("ACTION: Use the configuration GUI to clear the cache and run another update.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Error occurred when trying to read Md5Schedule entry in cache file. Exception:{Helper.ReportExceptionMessages(ex)}");
                Logger.WriteError("ACTION: Use the configuration GUI to clear the cache and run another update.");
                return;
            }

            // determine which service entry applies to
            var mxfService = mxf.FindOrCreateService(schedule.StationId);

            // process each program schedule entry
            foreach (var scheduleProgram in schedule.Programs)
            {
                // limit requests to airing programs now or in the future
                if (scheduleProgram.AirDateTime + TimeSpan.FromSeconds(scheduleProgram.Duration) < DateTime.UtcNow) continue;

                // prepopulate some of the program
                var mxfProgram = mxf.FindOrCreateProgram(scheduleProgram.ProgramId);
                if (mxfProgram.extras.Count == 0)
                {
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
                if (scheduleProgram.Premiere) mxfProgram.extras["premiere"] = true; // used only for movie and miniseries premieres

                // grab any tvratings from desired countries
                var scheduleTvRatings = new Dictionary<string, string>();
                if (scheduleProgram.Ratings != null)
                {
                    var origins = !string.IsNullOrEmpty(config.RatingsOrigin) ? config.RatingsOrigin.Split(',') : new[] { RegionInfo.CurrentRegion.ThreeLetterISORegionName };
                    if (Helper.TableContains(origins, "ALL"))
                    {
                        foreach (var rating in scheduleProgram.Ratings)
                        {
                            scheduleTvRatings.Add(rating.Body, rating.Code);
                        }
                    }
                    else
                    {
                        foreach (var origin in origins)
                        {
                            foreach (var rating in scheduleProgram.Ratings.Where(arg => arg.Country?.Equals(origin) ?? false))
                            {
                                scheduleTvRatings.Add(rating.Body, rating.Code);
                            }
                            if (scheduleTvRatings.Count > 0) break;
                        }
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
                    IsHdtv = CheckHdOverride(schedule.StationId) || !CheckSdOverride(schedule.StationId) && Helper.TableContains(scheduleProgram.VideoProperties, "hd"),
                    //IsHdtvSimulCast = null,
                    IsInProgress = scheduleProgram.JoinedInProgress,
                    IsLetterbox = Helper.TableContains(scheduleProgram.VideoProperties, "letterbox"),
                    IsLive = Helper.StringContains(scheduleProgram.LiveTapeDelay, "live"),
                    //IsLiveSports = null,
                    IsPremiere = scheduleProgram.Premiere || Helper.StringContains(scheduleProgram.IsPremiereOrFinale, "premiere"),
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