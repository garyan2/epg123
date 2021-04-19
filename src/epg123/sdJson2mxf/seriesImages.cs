using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using epg123.MxfXml;
using epg123.SchedulesDirect;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> seriesImageQueue = new List<string>();
        private static NameValueCollection sportsSeries = new NameValueCollection();
        private static ConcurrentBag<ProgramMetadata> seriesImageResponses = new ConcurrentBag<ProgramMetadata>();

        private static bool GetAllSeriesImages()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering GetAllSeriesImages() for {totalObjects = SdMxf.With.SeriesInfos.Count} series.");
            ++processStage; ReportProgress();
            var refreshing = 0;

            // scan through each series in the mxf
            foreach (var series in SdMxf.With.SeriesInfos)
            {
                var uid = "SH" + series.SeriesId + "0000";

                // if image for series already exists in archive file, use it
                // cycle images for a refresh based on day of month and seriesid
                var refresh = false;
                if (int.TryParse(series.SeriesId, out var digits))
                {
                    refresh = ((digits * config.ExpectedServicecount) % DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)) == DateTime.Now.Day + 1;
                }
                else
                {
                    uid = series.SeriesId;
                }

                if (!refresh && epgCache.JsonFiles.ContainsKey(uid) && epgCache.JsonFiles[uid]?.Images != null)
                {
                    ++processedObjects; ReportProgress();
                    if (epgCache.JsonFiles[uid].Images == string.Empty) continue;

                    List<ProgramArtwork> artwork;
                    using (var reader = new StringReader(epgCache.JsonFiles[uid].Images))
                    {
                        var serializer = new JsonSerializer();
                        series.extras.Add("artwork", artwork = (List<ProgramArtwork>)serializer.Deserialize(reader, typeof(List<ProgramArtwork>)));
                    }

                    ProgramArtwork image = null;
                    if (config.SeriesPosterArt || config.SeriesWsArt)
                    {
                        image = artwork.SingleOrDefault(arg =>
                            arg.Aspect.ToLower().Equals(config.SeriesPosterArt ? "2x3" : "16x9"));
                    }
                    if (image == null)
                    {
                        image = artwork.SingleOrDefault(arg => arg.Aspect.ToLower().Equals("4x3"));
                    }

                    if (image != null)
                    {
                        series.mxfGuideImage = SdMxf.GetGuideImage(image.Uri);
                    }
                }
                else if (int.TryParse(series.SeriesId, out var dummy))
                {
                    // only increment the refresh count if something exists already
                    if (refresh && epgCache.JsonFiles.ContainsKey(uid) && epgCache.JsonFiles[uid].Images != null)
                    {
                        ++refreshing;
                    }
                    seriesImageQueue.Add("SH" + series.SeriesId);
                }
                else
                {
                    seriesImageQueue.AddRange(sportsSeries.GetValues(series.SeriesId));
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable series image links.");
            if (refreshing > 0)
            {
                Logger.WriteVerbose($"Refreshing {refreshing} series image links.");
            }
            totalObjects = processedObjects + seriesImageQueue.Count;
            ReportProgress();

            // maximum 500 queries at a time
            if (seriesImageQueue.Count > 0)
            {
                Parallel.For(0, (seriesImageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadSeriesImageResponses(i * MaxImgQueries);
                });

                ProcessSeriesImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteInformation("Problem occurred during getAllSeriesImages(). Did not process all series image links.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} series image links.");
            Logger.WriteMessage("Exiting getAllSeriesImages(). SUCCESS.");
            seriesImageQueue = null; sportsSeries = null; seriesImageResponses = null;
            return true;
        }

        private static void DownloadSeriesImageResponses(int start = 0)
        {
            // reject 0 requests
            if (seriesImageQueue.Count - start < 1) return;

            // build the array of series to request images for
            var series = new string[Math.Min(seriesImageQueue.Count - start, MaxImgQueries)];
            for (var i = 0; i < series.Length; ++i)
            {
                series[i] = seriesImageQueue[start + i] + "0000";
            }

            // request images from Schedules Direct
            var responses = SdApi.GetArtwork(series);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    seriesImageResponses.Add(response);
                });
            }
        }

        private static void ProcessSeriesImageResponses()
        {
            // process request response
            foreach (var response in seriesImageResponses)
            {
                ++processedObjects; ReportProgress();
                var uid = response.ProgramId;

                if (response.Data == null) continue;
                MxfSeriesInfo series = null;
                if (response.ProgramId.StartsWith("SP"))
                {
                    foreach (var key in sportsSeries.AllKeys)
                    {
                        if (!sportsSeries.Get(key).Contains(response.ProgramId.Substring(0, 10))) continue;
                        series = SdMxf.GetSeriesInfo(key);
                        uid = key;
                    }
                }
                else
                {
                    series = SdMxf.GetSeriesInfo(response.ProgramId.Substring(2, 8));
                }
                if (series == null || !string.IsNullOrEmpty(series.GuideImage)) continue;

                // get series images
                List<ProgramArtwork> artwork;
                series.extras.Add("artwork", artwork = GetSeriesImages(response.Data));

                if (artwork.Count > 0)
                {
                    ProgramArtwork image = null;
                    if (config.SeriesPosterArt || config.SeriesWsArt)
                    {
                        image = artwork.SingleOrDefault(arg =>
                            arg.Aspect.ToLower().Equals(config.SeriesPosterArt ? "2x3" : "16x9"));
                    }
                    if (image == null)
                    {
                        image = artwork.SingleOrDefault(arg => arg.Aspect.ToLower().Equals("4x3"));
                    }

                    if (image != null)
                    {
                        series.mxfGuideImage = SdMxf.GetGuideImage(image.Uri);
                    }

                    using (var writer = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, artwork);

                        if (!epgCache.JsonFiles.ContainsKey(uid))
                        {
                            epgCache.AddAsset(uid, "{\"code\":0}");
                        }
                        epgCache.UpdateAssetImages(uid, writer.ToString());
                    }
                }
                else if (epgCache.JsonFiles.ContainsKey(uid))
                {
                    epgCache.UpdateAssetImages(uid, string.Empty);
                }
            }
        }

        private static List<ProgramArtwork> GetSeriesImages(List<ProgramArtwork> sdImages)
        {
            var ret = new List<ProgramArtwork>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                    .Where(arg => !string.IsNullOrEmpty(arg.Aspect))
                    .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md") || arg.Size.ToLower().Equals("sm"))
                    .Where(arg => !string.IsNullOrEmpty(arg.Uri))
                    .Where(arg => string.IsNullOrEmpty(arg.Tier) || arg.Tier.ToLower().Equals("series") || arg.Tier.ToLower().Equals("sport") || arg.Tier.ToLower().Equals("episode")).ToArray();

            // get the aspect ratios available and fix the URI
            var aspects = new HashSet<string>();
            foreach (var image in images)
            {
                if (image.Aspect.Equals("1x1")) continue;
                aspects.Add(image.Aspect);
                if (!image.Uri.ToLower().StartsWith("http"))
                {
                    image.Uri = $"{SdApi.JsonBaseUrl}{SdApi.JsonApi}image/{image.Uri.ToLower()}";
                }
            }

            // determine which image to return with each aspect
            foreach (var aspect in aspects)
            {
                var imgAspects = images.Where(arg => arg.Aspect.Equals(aspect) && arg.Size.ToLower().Equals(aspect.Equals("16x9") ? "sm" : "md"));

                var links = new ProgramArtwork[8];
                foreach (var image in imgAspects)
                {
                    switch (image.Category.ToLower())
                    {
                        case "banner":      // source-provided image, usually shows cast ensemble with source-provided text
                            if (links[0] == null) links[0] = image;
                            break;
                        case "banner-l1":   // same as Banner
                            if (links[1] == null) links[1] = image;
                            break;
                        case "banner-l2":   // source-provided image with plain text
                            if (links[2] == null) links[2] = image;
                            break;
                        case "banner-lo":   // banner with Logo Only
                            if (links[3] == null) links[3] = image;
                            break;
                        case "logo":        // official logo for program, sports organization, sports conference, or TV station
                            if (links[4] == null) links[4] = image;
                            break;
                        case "banner-l3":   // stock photo image with plain text
                            if (links[5] == null) links[5] = image;
                            break;
                        case "iconic":      // representative series/season/episode image, no text
                            if (links[6] == null) links[6] = image;
                            break;
                        case "staple":      // the staple image is intended to cover programs which do not have a unique banner image
                            if (links[7] == null) links[7] = image;
                            break;
                    }

                }

                foreach (var link in links)
                {
                    if (link == null) continue;
                    ret.Add(link);
                    break;
                }
            }
            return ret;
        }
    }
}