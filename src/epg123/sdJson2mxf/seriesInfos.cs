using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using epg123.SchedulesDirectAPI;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> seriesDescriptionQueue = new List<string>();
        private static ConcurrentDictionary<string, sdGenericDescriptions> seriesDescriptionResponses = new ConcurrentDictionary<string, sdGenericDescriptions>();

        private static bool BuildAllGenericSeriesInfoDescriptions()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering buildAllGenericSeriesInfoDescriptions() for {totalObjects = SdMxf.With[0].SeriesInfos.Count} series.");
            ++processStage; ReportProgress();

            // fill mxf programs with cached values and queue the rest
            foreach (var series in SdMxf.With[0].SeriesInfos)
            {
                // sports events will not have a generic description
                if (series.TmsSeriesId.StartsWith("SP"))
                {
                    ++processedObjects; ReportProgress();
                    continue;
                }

                // import the cached description if exists, otherwise queue it up
                var seriesId = $"SH{series.TmsSeriesId}0000";
                if (epgCache.JsonFiles.ContainsKey(seriesId) && epgCache.JsonFiles[seriesId].JsonEntry != "")
                {
                    try
                    {
                        using (var reader = new StringReader(epgCache.GetAsset(seriesId)))
                        {
                            var serializer = new JsonSerializer();
                            var cached = (sdGenericDescriptions)serializer.Deserialize(reader, typeof(sdGenericDescriptions));
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
                        ++processedObjects; ReportProgress();
                    }
                    catch
                    {
                        if (int.TryParse(series.TmsSeriesId, out var dummy))
                        {
                            // must use EP to query generic series description
                            seriesDescriptionQueue.Add($"EP{series.TmsSeriesId}0000");
                        }
                        else
                        {
                            ++processedObjects; ReportProgress();
                        }
                    }
                }
                else if (!int.TryParse(series.TmsSeriesId, out var dummy))
                {
                    ++processedObjects; ReportProgress();
                }
                else
                {
                    // must use EP to query generic series description
                    seriesDescriptionQueue.Add($"EP{series.TmsSeriesId}0000");
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached series descriptions.");

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
                    Logger.WriteInformation("Problem occurred during buildGenericSeriesInfoDescriptions(). Did not process all series descriptions.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} series descriptions.");
            Logger.WriteMessage("Exiting buildAllGenericSeriesInfoDescriptions(). SUCCESS.");
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
            var responses = sdApi.SdGetProgramGenericDescription(series);
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
                ++processedObjects; ReportProgress();

                var seriesId = response.Key.Replace("EP", "SH");
                var description = response.Value;

                // determine which seriesInfo this belongs to
                var mxfSeriesInfo = SdMxf.With[0].GetSeriesInfo(seriesId.Substring(2, 8));

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
            processedObjects = 0;
            ++processStage; ReportProgress();
            seriesDescriptionQueue = new List<string>();

            if (!config.ModernMediaUiPlusSupport) return true;
            Logger.WriteMessage($"Entering buildAllExtendedSeriesDataForUiPlus() for {totalObjects = SdMxf.With[0].SeriesInfos.Count} series.");

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
            foreach (var series in SdMxf.With[0].SeriesInfos)
            {
                var seriesId = $"SH{series.TmsSeriesId}0000";

                // sports events will not have a generic description
                if (series.TmsSeriesId.StartsWith("SP"))
                {
                    ++processedObjects; ReportProgress();
                    continue;
                }

                // generic series information already in support file array
                if (ModernMediaUiPlus.Programs.ContainsKey(seriesId))
                {
                    ++processedObjects; ReportProgress();
                    if (ModernMediaUiPlus.Programs.TryGetValue(seriesId, out var program) && program.OriginalAirDate != null)
                    {
                        UpdateSeriesAirdate(seriesId, program.OriginalAirDate);
                    }
                    continue;
                }

                // extended information in current json file
                if (oldPrograms.ContainsKey(seriesId))
                {
                    ++processedObjects; ReportProgress();
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
                    Logger.WriteInformation("Problem occurred during buildAllExtendedSeriesDataForUiPlus(). Exiting.");
                    return true;
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} extended series descriptions.");
            Logger.WriteMessage("Exiting buildAllExtendedSeriesDataForUiPlus(). SUCCESS.");
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
            var responses = sdApi.SdGetPrograms(programs);
            if (responses == null) return false;

            // process request response
            var idx = 0;
            foreach (var response in responses)
            {
                if (response == null)
                {
                    Logger.WriteInformation($"Did not receive data for program {programs[idx++]}.");
                    continue;
                }
                ++idx; ++processedObjects; ReportProgress();

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
            SdMxf.With[0].GetSeriesInfo(seriesId.Substring(2, 8)).StartAirdate = airdate.ToString("yyyy-MM-dd");

            // update cache if needed
            try
            {
                using (var reader = new StringReader(epgCache.GetAsset(seriesId)))
                {
                    var serializer = new JsonSerializer();
                    var cached = (sdGenericDescriptions)serializer.Deserialize(reader, typeof(sdGenericDescriptions));
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