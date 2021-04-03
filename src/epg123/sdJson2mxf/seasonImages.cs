using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using epg123.MxfXml;
using epg123.SchedulesDirectAPI;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> seasonImageQueue = new List<string>();
        private static List<MxfSeason> seasons = new List<MxfSeason>();
        private static ConcurrentBag<sdArtworkResponse> seasonImageResponses = new ConcurrentBag<sdArtworkResponse>();

        private static bool GetAllSeasonImages()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering getAllSeasonImages() for {totalObjects = SdMxf.With[0].Seasons.Count} seasons.");
            ++processStage; ReportProgress();

            // scan through each series in the mxf
            foreach (var season in SdMxf.With[0].Seasons)
            {
                var uid = $"{season.Zap2It}_{season.SeasonNumber}";
                if (epgCache.JsonFiles.ContainsKey(uid) && epgCache.JsonFiles[uid].Images != null)
                {
                    ++processedObjects; ReportProgress();
                    if (string.IsNullOrEmpty(epgCache.JsonFiles[uid].Images)) continue;

                    using (var reader = new StringReader(epgCache.JsonFiles[uid].Images))
                    {
                        var serializer = new JsonSerializer();
                        season.seasonImages = (List<sdImage>)serializer.Deserialize(reader, typeof(List<sdImage>));
                    }

                    sdImage image = null;
                    if (config.SeriesPosterArt || config.SeriesWsArt)
                    {
                        image = season.seasonImages.SingleOrDefault(arg =>
                            arg.Aspect.ToLower().Equals(config.SeriesPosterArt ? "2x3" : "16x9"));
                    }
                    if (image == null)
                    {
                        image = season.seasonImages.SingleOrDefault(arg => arg.Aspect.ToLower().Equals("4x3"));
                    }
                    if (image != null)
                    {
                        season.GuideImage = SdMxf.With[0].GetGuideImage(image.Uri).Id;
                    }
                }
                else if (!string.IsNullOrEmpty(season.ProtoTypicalProgram))
                {
                    seasons.Add(season);
                    seasonImageQueue.Add(season.ProtoTypicalProgram);
                }
                else
                {
                    ++processedObjects; ReportProgress();
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached season image links.");
            totalObjects = processedObjects + seasonImageQueue.Count;
            ReportProgress();

            // maximum 500 queries at a time
            if (seasonImageQueue.Count > 0)
            {
                Parallel.For(0, (seasonImageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadSeasonImageResponses(i * MaxImgQueries);
                });

                ProcessSeasonImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteInformation("Problem occurred during getAllSeasonImages(). Did not process all season image links.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} season image links.");
            Logger.WriteMessage("Exiting getAllSeasonImages(). SUCCESS.");
            seasonImageQueue = null; seasonImageResponses = null;
            return true;
        }

        private static void DownloadSeasonImageResponses(int start = 0)
        {
            // reject 0 requests
            if (seasonImageQueue.Count - start < 1) return;

            // build the array of series to request images for
            var programs = new string[Math.Min(seasonImageQueue.Count - start, MaxImgQueries)];
            for (var i = 0; i < programs.Length; ++i)
            {
                programs[i] = seasonImageQueue[start + i];
            }

            // request images from Schedules Direct
            var responses = sdApi.SdGetArtwork(programs);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    seasonImageResponses.Add(response);
                });
            }
        }

        private static void ProcessSeasonImageResponses()
        {
            // process request response
            foreach (var response in seasonImageResponses)
            {
                ++processedObjects; ReportProgress();
                if (response.Data == null) continue;
                
                var mxfSeason = seasons.SingleOrDefault(arg => arg.ProtoTypicalProgram == response.ProgramId);
                if (mxfSeason == null) continue;

                // get sports event images
                mxfSeason.seasonImages = GetSeasonImages(response.Data);

                var uid = $"{mxfSeason.Zap2It}_{mxfSeason.SeasonNumber}";
                if (mxfSeason.seasonImages.Count > 0)
                {
                    sdImage image = null;
                    if (config.SeriesPosterArt || config.SeriesWsArt)
                    {
                        image = mxfSeason.seasonImages.SingleOrDefault(arg =>
                            arg.Aspect.ToLower().Equals(config.SeriesPosterArt ? "2x3" : "16x9"));
                    }
                    if (image == null)
                    {
                        image = mxfSeason.seasonImages.SingleOrDefault(arg => arg.Aspect.ToLower().Equals("4x3"));
                    }
                    if (image != null)
                    {
                        mxfSeason.GuideImage = SdMxf.With[0].GetGuideImage(image.Uri).Id;
                    }

                    using (var writer = new StringWriter())
                    {
                        try
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(writer, mxfSeason.seasonImages);

                            if (!epgCache.JsonFiles.ContainsKey(uid))
                            {
                                epgCache.AddAsset(uid, string.Empty);
                            }
                            epgCache.JsonFiles[uid].Images = writer.ToString();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else if (epgCache.JsonFiles.ContainsKey(uid))
                {
                    epgCache.UpdateAssetImages(uid, string.Empty);
                }
            }
        }

        private static IList<sdImage> GetSeasonImages(IList<sdImage> sdImages)
        {
            var ret = new List<sdImage>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                     .Where(arg => !string.IsNullOrEmpty(arg.Aspect))
                     .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md") || arg.Size.ToLower().Equals("sm"))
                     .Where(arg => !string.IsNullOrEmpty(arg.Uri))
                     .Where(arg => string.IsNullOrEmpty(arg.Tier) || arg.Tier.ToLower().Equals("season")).ToArray();

            // get the aspect ratios available and fix the URI
            var aspects = new HashSet<string>();
            foreach (var image in images)
            {
                aspects.Add(image.Aspect);
                if (!image.Uri.ToLower().StartsWith("http"))
                {
                    image.Uri = $"{sdApi.JsonBaseUrl}{sdApi.JsonApi}image/{image.Uri.ToLower()}";
                }
            }

            // determine which image to return with each aspect
            foreach (var aspect in aspects)
            {
                var imgAspects = images.Where(arg => arg.Aspect.Equals(aspect) && arg.Size.ToLower().Equals(aspect.Equals("16x9") ? "sm" : "md"));

                var links = new sdImage[6];
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
                        case "banner-lot":  // banner with logo only + text indicating season number
                            if (links[2] == null) links[3] = image;
                            break;
                        case "banner-l2":   // source-provided image with plain text
                            if (links[3] == null) links[2] = image;
                            break;
                        case "banner-lo":   // banner with Logo Only
                            if (links[4] == null) links[3] = image;
                            break;
                        case "banner-l3":   // stock photo image with plain text
                            if (links[5] == null) links[5] = image;
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