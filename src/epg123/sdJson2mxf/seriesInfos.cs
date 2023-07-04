using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using api = GaRyan2.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> seriesDescriptionQueue;
        private static ConcurrentDictionary<string, GenericDescription> seriesDescriptionResponses;

        private static bool BuildAllGenericSeriesInfoDescriptions()
        {
            // reset counters
            seriesDescriptionQueue = new List<string>();
            seriesDescriptionResponses = new ConcurrentDictionary<string, GenericDescription>();
            IncrementNextStage(mxf.SeriesInfosToProcess.Count);
            Logger.WriteMessage($"Entering BuildAllGenericSeriesInfoDescriptions() for {totalObjects} series.");

            // fill mxf programs with cached values and queue the rest
            foreach (var series in mxf.SeriesInfosToProcess)
            {
                // sports events will not have a generic description
                if (series.SeriesId.StartsWith("SP"))
                {
                    IncrementProgress();
                    continue;
                }

                // import the cached description if exists, otherwise queue it up
                var seriesId = $"SH{series.SeriesId}0000";
                if (epgCache.JsonFiles.ContainsKey(seriesId) && epgCache.JsonFiles[seriesId].JsonEntry != null)
                {
                    try
                    {
                        using (var reader = new StringReader(epgCache.GetAsset(seriesId)))
                        {
                            var serializer = new JsonSerializer();
                            var cached = (GenericDescription)serializer.Deserialize(reader, typeof(GenericDescription));
                            if (cached.Code == 0)
                            {
                                series.ShortDescription = cached.Description100;
                                series.Description = cached.Description1000;
                                if (!string.IsNullOrEmpty(cached.StartAirdate))
                                {
                                    series.StartAirdate = cached.StartAirdate;
                                }
                            }
                        }
                        IncrementProgress();
                    }
                    catch
                    {
                        if (int.TryParse(series.SeriesId, out var dummy))
                        {
                            // must use EP to query generic series description
                            seriesDescriptionQueue.Add($"{series.ProtoTypicalProgram}");
                        }
                        else
                        {
                            IncrementProgress();
                        }
                    }
                }
                else if (!int.TryParse(series.SeriesId, out var dummy) || series.ProtoTypicalProgram.StartsWith("SH"))
                {
                    IncrementProgress();
                }
                else
                {
                    // must use EP to query generic series description
                    seriesDescriptionQueue.Add($"{series.ProtoTypicalProgram}");
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable series descriptions.");

            // maximum 500 queries at a time
            if (seriesDescriptionQueue.Count > 0)
            {
                Parallel.For(0, (seriesDescriptionQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadGenericSeriesDescriptions(i * MaxImgQueries);
                });

                ProcessSeriesDescriptionsResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning($"Failed to download and process {mxf.SeriesInfosToProcess.Count - processedObjects} series descriptions.");
                }
            }
            Logger.WriteMessage("Exiting BuildAllGenericSeriesInfoDescriptions(). SUCCESS.");
            seriesDescriptionQueue = null; seriesDescriptionResponses = null;
            return true;
        }

        private static void DownloadGenericSeriesDescriptions(int start = 0)
        {
            // reject 0 requests
            if (seriesDescriptionQueue.Count - start < 1) return;

            // build the array of series to request descriptions for
            var series = new string[Math.Min(seriesDescriptionQueue.Count - start, MaxImgQueries)];
            for (var i = 0; i < series.Length; ++i)
            {
                series[i] = seriesDescriptionQueue[start + i];
            }

            // request descriptions from Schedules Direct
            var responses = api.GetGenericDescriptions(series);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    seriesDescriptionResponses.TryAdd(response.Key, response.Value);
                });
            }
        }

        private static void ProcessSeriesDescriptionsResponses()
        {
            // process request response
            foreach (var response in seriesDescriptionResponses)
            {
                IncrementProgress();

                var seriesId = response.Key;
                var description = response.Value;

                // determine which seriesInfo this belongs to
                var mxfSeriesInfo = mxf.FindOrCreateSeriesInfo(seriesId.Substring(2, 8));

                // populate descriptions
                mxfSeriesInfo.ShortDescription = description.Description100;
                mxfSeriesInfo.Description = description.Description1000;

                // serialize JSON directly to a file
                using (var writer = new StringWriter())
                {
                    try
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, description);
                        epgCache.AddAsset(seriesId, writer.ToString());
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private static bool BuildAllExtendedSeriesDataForUiPlus()
        {
            // reset counters
            IncrementNextStage(mxf.With.SeriesInfos.Count);
            seriesDescriptionQueue = new List<string>();

            if (!config.ModernMediaUiPlusSupport) return true;
            Logger.WriteMessage($"Entering BuildAllExtendedSeriesDataForUiPlus() for {totalObjects} series.");

            // read in cached ui+ extended information
            var oldPrograms = new Dictionary<string, ModernMediaUiPlusPrograms>();
            if (File.Exists(Helper.Epg123MmuiplusJsonPath))
            {
                using (var sr = new StreamReader(Helper.Epg123MmuiplusJsonPath))
                {
                    oldPrograms = JsonConvert.DeserializeObject<Dictionary<string, ModernMediaUiPlusPrograms>>(sr.ReadToEnd());
                }
            }

            // fill mxf programs with cached values and queue the rest
            foreach (var series in mxf.With.SeriesInfos)
            {
                // sports events will not have a generic description
                if (!series.SeriesId.StartsWith("SH"))
                {
                    IncrementProgress();
                    continue;
                }

                var seriesId = $"SH{series.SeriesId}0000";

                // generic series information already in support file array
                if (ModernMediaUiPlus.Programs.ContainsKey(seriesId))
                {
                    IncrementProgress();
                    if (ModernMediaUiPlus.Programs.TryGetValue(seriesId, out var program) && program.OriginalAirDate != null)
                    {
                        UpdateSeriesAirdate(seriesId, program.OriginalAirDate);
                    }
                    continue;
                }

                // extended information in current json file
                if (oldPrograms.ContainsKey(seriesId))
                {
                    IncrementProgress();
                    if (oldPrograms.TryGetValue(seriesId, out var program) && !string.IsNullOrEmpty(program.OriginalAirDate))
                    {
                        ModernMediaUiPlus.Programs.Add(seriesId, program);
                        UpdateSeriesAirdate(seriesId, program.OriginalAirDate);
                    }
                    continue;
                }

                // add to queue
                seriesDescriptionQueue.Add(seriesId);
            }
            Logger.WriteVerbose($"Found {processedObjects} cached extended series descriptions.");

            // maximum 5000 queries at a time
            if (seriesDescriptionQueue.Count > 0)
            {
                for (var i = 0; i < seriesDescriptionQueue.Count; i += MaxQueries)
                {
                    if (GetExtendedSeriesDataForUiPlus(i)) continue;
                    Logger.WriteWarning($"Failed to download and process {mxf.With.SeriesInfos.Count - processedObjects} extended series descriptions.");
                    return true;
                }
            }
            Logger.WriteMessage("Exiting BuildAllExtendedSeriesDataForUiPlus(). SUCCESS.");
            seriesDescriptionQueue = null; seriesDescriptionResponses = null;
            return true;
        }
        private static bool GetExtendedSeriesDataForUiPlus(int start = 0)
        {
            // build the array of programs to request for
            var programs = new string[Math.Min(seriesDescriptionQueue.Count - start, MaxQueries)];
            for (var i = 0; i < programs.Length; ++i)
            {
                programs[i] = seriesDescriptionQueue[start + i];
            }

            // request programs from Schedules Direct
            var responses = api.GetPrograms(programs);
            if (responses == null) return false;

            // process request response
            var idx = 0;
            foreach (var response in responses)
            {
                if (response == null)
                {
                    Logger.WriteWarning($"Did not receive data for program {programs[idx++]}.");
                    continue;
                }
                ++idx; IncrementProgress();

                // add the series extended data to the file if not already present from program builds
                if (!ModernMediaUiPlus.Programs.ContainsKey(response.ProgramId))
                {
                    AddModernMediaUiPlusProgram(response);
                }

                // add the series start air date if available
                UpdateSeriesAirdate(response.ProgramId, response.OriginalAirDate);
            }
            return true;
        }
        private static void UpdateSeriesAirdate(string seriesId, string originalAirdate)
        {
            UpdateSeriesAirdate(seriesId, DateTime.Parse(originalAirdate));
        }
        private static void UpdateSeriesAirdate(string seriesId, DateTime airdate)
        {
            // write the mxf entry
            mxf.FindOrCreateSeriesInfo(seriesId.Substring(2, 8)).StartAirdate = airdate.ToString("yyyy-MM-dd");

            // update cache if needed
            try
            {
                using (var reader = new StringReader(epgCache.GetAsset(seriesId)))
                {
                    var serializer = new JsonSerializer();
                    var cached = (GenericDescription)serializer.Deserialize(reader, typeof(GenericDescription));
                    if (!string.IsNullOrEmpty(cached.StartAirdate)) return;
                    cached.StartAirdate = airdate.Equals(DateTime.MinValue) ? "" : airdate.ToString("yyyy-MM-dd");
                    using (var writer = new StringWriter())
                    {
                        serializer.Serialize(writer, cached);
                        epgCache.UpdateAssetJsonEntry(seriesId, writer.ToString());
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}