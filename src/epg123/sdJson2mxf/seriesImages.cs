﻿using GaRyan2.MxfXml;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using api = GaRyan2.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> imageQueue;
        private static NameValueCollection sportsSeries = new NameValueCollection();
        private static ConcurrentBag<ProgramMetadata> imageResponses;

        private enum ImageType
        {
            Program,
            Movie,
            Season,
            Series
        }

        private static bool GetAllSeriesImages()
        {
            // reset counters
            imageQueue = new List<string>();
            imageResponses = new ConcurrentBag<ProgramMetadata>();
            IncrementNextStage(mxf.SeriesInfosToProcess.Count);
            if (Helper.Standalone) return true;
            Logger.WriteMessage($"Entering GetAllSeriesImages() for {totalObjects} series.");
            var refreshing = 0;

            // scan through each series in the mxf
            foreach (var series in mxf.SeriesInfosToProcess)
            {
                string seriesId;

                // if image for series already exists in archive file, use it
                // cycle images for a refresh based on day of month and seriesid
                var refresh = false;
                if (int.TryParse(series.SeriesId, out var digits))
                {
                    refresh = digits * config.ExpectedServicecount % DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + 1 == DateTime.Now.Day;
                    seriesId = $"SH{series.SeriesId}0000";
                }
                else
                {
                    seriesId = series.SeriesId;
                }

                if (!refresh && epgCache.JsonFiles.ContainsKey(seriesId) && !string.IsNullOrEmpty(epgCache.JsonFiles[seriesId].Images))
                {
                    IncrementProgress();
                    if (epgCache.JsonFiles[seriesId].Images == string.Empty) continue;

                    List<ProgramArtwork> artwork;
                    using (var reader = new StringReader(epgCache.JsonFiles[seriesId].Images))
                    {
                        var serializer = new JsonSerializer();
                        series.extras.Add("artwork", artwork = (List<ProgramArtwork>)serializer.Deserialize(reader, typeof(List<ProgramArtwork>)));
                    }
                    series.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Series);
                }
                else if (int.TryParse(series.SeriesId, out var dummy))
                {
                    // only increment the refresh count if something exists already
                    if (refresh && epgCache.JsonFiles.ContainsKey(seriesId) && epgCache.JsonFiles[seriesId].Images != null)
                    {
                        ++refreshing;
                    }
                    imageQueue.Add($"SH{series.SeriesId}0000");
                }
                else
                {
                    imageQueue.AddRange(sportsSeries.GetValues(series.SeriesId));
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable series image links.");
            if (refreshing > 0)
            {
                Logger.WriteVerbose($"Refreshing {refreshing} series image links.");
            }

            // maximum 500 queries at a time
            if (imageQueue.Count > 0)
            {
                Parallel.For(0, (imageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadImageResponses(i * MaxImgQueries);
                });

                ProcessSeriesImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning($"Failed to download and process {mxf.SeriesInfosToProcess.Count - processedObjects} series image links.");
                }
            }
            Logger.WriteMessage("Exiting GetAllSeriesImages(). SUCCESS.");
            imageQueue = null; sportsSeries = null; imageResponses = null;
            return true;
        }

        private static void DownloadImageResponses(int start = 0)
        {
            // reject 0 requests
            if (imageQueue.Count - start < 1) return;

            // build the array of series to request images for
            var series = new string[Math.Min(imageQueue.Count - start, MaxImgQueries)];
            for (var i = 0; i < series.Length; ++i)
            {
                series[i] = imageQueue[start + i];
            }

            // request images from Schedules Direct
            var responses = api.GetArtwork(series);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    imageResponses.Add(response);
                });
            }
        }

        private static void ProcessSeriesImageResponses()
        {
            // process request response
            foreach (var response in imageResponses)
            {
                IncrementProgress();
                var uid = response.ProgramId;

                if (response.Data == null || response.Code != 0) continue;
                MxfSeriesInfo series = null;
                if (response.ProgramId.StartsWith("SP"))
                {
                    foreach (var key in sportsSeries.AllKeys)
                    {
                        if (!sportsSeries.Get(key).Contains(response.ProgramId)) continue;
                        series = mxf.FindOrCreateSeriesInfo(key);
                        uid = key;
                    }
                }
                else
                {
                    series = mxf.FindOrCreateSeriesInfo(response.ProgramId.Substring(2, 8));
                }
                if (series == null || !string.IsNullOrEmpty(series.GuideImage) || series.extras.ContainsKey("artwork")) continue;

                // get series images
                var artwork = GetTieredImages(response.Data, new List<string> { "series", "sport", "episode" });
                if (response.ProgramId.StartsWith("SP") && artwork.Count <= 0) continue;
                series.extras.Add("artwork", artwork);
                series.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Series, uid);
            }
        }

        private static List<ProgramArtwork> GetTieredImages(List<ProgramArtwork> sdImages, List<string> tiers)
        {
            var ret = new List<ProgramArtwork>();
            var images = sdImages.Where(arg =>
                !string.IsNullOrEmpty(arg.Category) && !string.IsNullOrEmpty(arg.Aspect) && !string.IsNullOrEmpty(arg.Uri) &&
                (string.IsNullOrEmpty(arg.Tier) || tiers.Contains(arg.Tier.ToLower())) &&
                !string.IsNullOrEmpty(arg.Size) && arg.Size.Equals(config.ArtworkSize));

            // get the aspect ratios available and fix the URI
            var aspects = new HashSet<string>();
            foreach (var image in images)
            {
                aspects.Add(image.Aspect);
                if (!image.Uri.ToLower().StartsWith("http"))
                {
                    image.Uri = $"{api.BaseArtworkAddress}image/{image.Uri.ToLower()}";
                }
            }

            // determine which image to return with each aspect
            foreach (var aspect in aspects)
            {
                var imgAspects = images.Where(arg => arg.Aspect.Equals(aspect));

                var links = new ProgramArtwork[11];
                foreach (var image in imgAspects)
                {
                    switch (image.Category.ToLower())
                    {
                        case "box art":     // DVD box art, for movies only
                            if (links[0] == null) links[0] = image;
                            break;
                        case "vod art":
                            if (links[1] == null) links[1] = image;
                            break;
                        case "poster art":  // theatrical movie poster, standard sizes
                            if (links[2] == null) links[2] = image;
                            break;
                        case "banner":      // source-provided image, usually shows cast ensemble with source-provided text
                            if (links[3] == null) links[3] = image;
                            break;
                        case "banner-l1":   // same as Banner
                            if (links[4] == null) links[4] = image;
                            break;
                        case "banner-l2":   // source-provided image with plain text
                            if (links[5] == null) links[5] = image;
                            break;
                        case "banner-lo":   // banner with Logo Only
                            if (links[6] == null) links[6] = image;
                            break;
                        case "logo":        // official logo for program, sports organization, sports conference, or TV station
                            if (links[7] == null) links[7] = image;
                            break;
                        case "banner-l3":   // stock photo image with plain text
                            if (links[8] == null) links[8] = image;
                            break;
                        case "iconic":      // representative series/season/episode image, no text
                            if (tiers.Contains("series") && links[9] == null) links[9] = image;
                            break;
                        case "staple":      // the staple image is intended to cover programs which do not have a unique banner image
                            if (links[10] == null) links[10] = image;
                            break;
                        case "banner-l1t":
                        case "banner-lot":  // banner with Logo Only + Text indicating season number
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

            if (ret.Count > 1)
            {
                ret = ret.OrderBy(arg => arg.Width).ToList();
            }
            return ret;
        }

        private static MxfGuideImage GetGuideImageAndUpdateCache(List<ProgramArtwork> artwork, ImageType type, string cacheKey = null)
        {
            if (artwork.Count == 0)
            {
                if (cacheKey != null) epgCache.UpdateAssetImages(cacheKey, string.Empty);
                return null;
            }
            if (cacheKey != null)
            {
                using (var writer = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, artwork);
                    epgCache.UpdateAssetImages(cacheKey, writer.ToString());
                }
            }

            ProgramArtwork image = null;
            if (type == ImageType.Movie)
            {
                image = artwork.FirstOrDefault();
            }
            else
            {
                var aspect = config.SeriesPosterArt ? "2x3" : config.SeriesWsArt ? "16x9" : config.SeriesPosterAspect;
                image = artwork.SingleOrDefault(arg => arg.Aspect.ToLower().Equals(aspect));
            }

            if (image == null && type == ImageType.Series)
            {
                image = artwork.SingleOrDefault(arg => arg.Aspect.ToLower().Equals("4x3"));
            }
            return image != null ? mxf.FindOrCreateGuideImage(Helper.Standalone ? image.Uri : image.Uri.Replace($"{api.BaseArtworkAddress}", $"http://{HostAddress}:{Helper.TcpUdpPort}/")) : null;
        }
    }
}