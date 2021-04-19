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
        private static List<string> sportsImageQueue = new List<string>();
        private static List<MxfProgram> sportEvents = new List<MxfProgram>();
        private static ConcurrentBag<ProgramMetadata> sportsImageResponses = new ConcurrentBag<ProgramMetadata>();

        private static bool GetAllSportsImages()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering GetAllSportsImages() for {totalObjects = sportEvents.Count} sports events.");
            ++processStage; ReportProgress();

            // scan through each series in the mxf
            foreach (var sportEvent in sportEvents)
            {
                if (epgCache.JsonFiles.ContainsKey(sportEvent.extras["md5"]) && epgCache.JsonFiles[sportEvent.extras["md5"]].Images != null)
                {
                    ++processedObjects; ReportProgress();
                    if (string.IsNullOrEmpty(epgCache.JsonFiles[sportEvent.extras["md5"]].Images)) continue;

                    List<ProgramArtwork> artwork;
                    using (var reader = new StringReader(epgCache.JsonFiles[sportEvent.extras["md5"]].Images))
                    {
                        var serializer = new JsonSerializer();
                        sportEvent.extras.Add("artwork", artwork = (List<ProgramArtwork>)serializer.Deserialize(reader, typeof(List<ProgramArtwork>)));
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
                        sportEvent.mxfGuideImage = SdMxf.GetGuideImage(image.Uri);
                    }
                }
                else
                {
                    sportsImageQueue.Add(sportEvent.ProgramId);
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable sport event image links.");
            totalObjects = processedObjects + sportsImageQueue.Count;
            ReportProgress();

            // maximum 500 queries at a time
            if (sportsImageQueue.Count > 0)
            {
                Parallel.For(0, (sportsImageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadSportsImageResponses(i * MaxImgQueries);
                });

                ProcessSportsImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteInformation("Problem occurred during getAllSportsImages(). Did not process all sports image links.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} sport event image links.");
            Logger.WriteMessage("Exiting getAllSportsImages(). SUCCESS.");
            sportsImageQueue = null; sportsImageResponses = null;
            return true;
        }

        private static void DownloadSportsImageResponses(int start = 0)
        {
            // reject 0 requests
            if (sportsImageQueue.Count - start < 1) return;

            // build the array of series to request images for
            var events = new string[Math.Min(sportsImageQueue.Count - start, MaxImgQueries)];
            for (var i = 0; i < events.Length; ++i)
            {
                events[i] = sportsImageQueue[start + i];
            }

            // request images from Schedules Direct
            var responses = SdApi.GetArtwork(events);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    sportsImageResponses.Add(response);
                });
            }
        }

        private static void ProcessSportsImageResponses()
        {
            // process request response
            foreach (var response in sportsImageResponses)
            {
                ++processedObjects; ReportProgress();
                if (response.Data == null) continue;

                var mxfProgram = sportEvents.SingleOrDefault(arg => arg.ProgramId == response.ProgramId);
                if (mxfProgram == null) continue;

                // get sports event images
                List<ProgramArtwork> artwork;
                mxfProgram.extras.Add("artwork", artwork = GetSportsImages(response.Data));

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
                        mxfProgram.mxfGuideImage = SdMxf.GetGuideImage(image.Uri);
                    }

                    using (var writer = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, artwork);
                        epgCache.UpdateAssetImages(mxfProgram.extras["md5"], writer.ToString());
                    }
                }
                else
                {
                    epgCache.UpdateAssetImages(mxfProgram.extras["md5"], string.Empty);
                }
            }
        }

        private static List<ProgramArtwork> GetSportsImages(List<ProgramArtwork> sdImages)
        {
            var ret = new List<ProgramArtwork>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                     .Where(arg => !string.IsNullOrEmpty(arg.Aspect))
                     .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md") || arg.Size.ToLower().Equals("sm"))
                     .Where(arg => !string.IsNullOrEmpty(arg.Uri))
                     .Where(arg => string.IsNullOrEmpty(arg.Tier) || arg.Tier.ToLower().Equals("team event") || arg.Tier.ToLower().Equals("sport event")).ToArray();

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