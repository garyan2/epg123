using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using epg123.MxfXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static List<string> seriesImageQueue = new List<string>();
        private static NameValueCollection sportsSeries = new NameValueCollection();
        private static ConcurrentBag<sdArtworkResponse> seriesImageResponses = new ConcurrentBag<sdArtworkResponse>();

        private static bool getAllSeriesImages()
        {
            // reset counters
            processedObjects = 0;
            Logger.WriteMessage(string.Format("Entering getAllSeriesImages() for {0} series.",
                                totalObjects = sdMxf.With[0].SeriesInfos.Count));
            ++processStage; reportProgress();

            // scan through each series in the mxf
            foreach (MxfSeriesInfo series in sdMxf.With[0].SeriesInfos)
            {
                // if image for series already exists in archive file, use it
                // cycle images for a refresh based on day of month and seriesid
                var oldImage = oldImageLibrary.Images.Where(arg => arg.Zap2itId.Equals(series.tmsSeriesId)).SingleOrDefault();
                bool refresh = false;
                if (int.TryParse(series.tmsSeriesId, out int digits))
                {
                    refresh = ((digits * config.ExpectedServicecount) % DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)) == DateTime.Now.Day + 1;
                }

                if (oldImage != null && !string.IsNullOrEmpty(oldImage.Height))
                {
                    double oldAspect = double.Parse(oldImage.Width) / double.Parse(oldImage.Height);
                    if (config.SeriesPosterArt && oldAspect > 1.0) refresh = true;
                    else if (!config.SeriesPosterArt && oldAspect < 1.0) refresh = true;
                }

                if (oldImage != null && !refresh && !config.CreateXmltv)
                {
                    ++processedObjects; reportProgress();
                    series.GuideImage = sdMxf.With[0].getGuideImage(oldImage.Url).Id;
                    newImageLibrary.Images.Add(new archiveImage()
                    {
                        Title = series.Title,
                        Url = oldImage.Url,
                        Zap2itId = series.tmsSeriesId,
                        Height = oldImage.Height,
                        Width = oldImage.Width
                    });
                }
                else if (!series.tmsSeriesId.StartsWith("SP"))
                {
                    seriesImageQueue.Add("SH" + series.tmsSeriesId);
                }
                else
                {
                    seriesImageQueue.AddRange(sportsSeries.GetValues(series.tmsSeriesId));
                }
            }
            Logger.WriteVerbose(string.Format("Found {0} cached series image links.", processedObjects));
            totalObjects = processedObjects + seriesImageQueue.Count;
            reportProgress();

            // maximum 500 queries at a time
            if (seriesImageQueue.Count > 0)
            {
                Parallel.For(0, (seriesImageQueue.Count / MAXIMGQUERIES + 1), new ParallelOptions { MaxDegreeOfParallelism = MAXPARALLELDOWNLOADS }, i =>
                {
                    downloadSeriesImageResponses(i * MAXIMGQUERIES);
                });

                processSeriesImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning("Problem occurred during getAllSeriesImages(). Did not process all series image links.");
                }
            }
            Logger.WriteInformation(string.Format("Processed {0} series image links.", processedObjects));
            Logger.WriteMessage("Exiting getAllSeriesImages(). SUCCESS.");
            seriesImageQueue = null; sportsSeries = null; seriesImageResponses = null;
            return true;
        }

        private static void downloadSeriesImageResponses(int start = 0)
        {
            // build the array of series to request images for
            string[] series = new string[Math.Min(seriesImageQueue.Count - start, MAXIMGQUERIES)];
            for (int i = 0; i < series.Length; ++i)
            {
                series[i] = seriesImageQueue[start + i] + "0000";
            }

            // request images from Schedules Direct
            IList<sdArtworkResponse> responses = sdAPI.sdGetArtwork(series);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    seriesImageResponses.Add(response);
                });
            }
        }

        private static void processSeriesImageResponses()
        {
            // process request response
            foreach (sdArtworkResponse response in seriesImageResponses)
            {
                ++processedObjects; reportProgress();

                if (response.Data != null)
                {
                    MxfSeriesInfo mxfSeriesInfo = null;
                    if (response.ProgramID.StartsWith("SP"))
                    {
                        foreach (string key in sportsSeries.AllKeys)
                        {
                            if (sportsSeries.Get(key).Contains(response.ProgramID.Substring(0, 10)))
                            {
                                mxfSeriesInfo = sdMxf.With[0].getSeriesInfo(key);
                            }
                        }
                    }
                    else
                    {
                        mxfSeriesInfo = sdMxf.With[0].getSeriesInfo(response.ProgramID.Substring(2, 8));
                    }
                    if (mxfSeriesInfo == null || !string.IsNullOrEmpty(mxfSeriesInfo.GuideImage)) return;

                    // get series images
                    mxfSeriesInfo.seriesImages = getSeriesImages(response.Data);

                    if (mxfSeriesInfo.seriesImages.Count > 0)
                    {
                        string mxfAspect = config.SeriesPosterArt ? "2x3" : "4x3";
                        var image = mxfSeriesInfo.seriesImages.Where(arg => arg.Aspect.ToLower().Equals(mxfAspect)).SingleOrDefault();
                        if (image != null)
                        {
                            mxfSeriesInfo.GuideImage = sdMxf.With[0].getGuideImage(image.Uri).Id;

                            // add to archive for no other reason than to see it
                            newImageLibrary.Images.Add(new archiveImage()
                            {
                                Title = mxfSeriesInfo.Title,
                                Url = image.Uri,
                                Zap2itId = mxfSeriesInfo.tmsSeriesId,
                                Height = image.Height,
                                Width = image.Width
                            });
                        }
                    }
                }
            }
        }

        private static IList<sdImage> getSeriesImages(IList<sdImage> sdImages)
        {
            List<sdImage> ret = new List<sdImage>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                                 .Where(arg => !string.IsNullOrEmpty(arg.Aspect))
                                 .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md"))
                                 .Where(arg => !string.IsNullOrEmpty(arg.Uri))
                                 .Where(arg => string.IsNullOrEmpty(arg.Tier) ||
                                               arg.Tier.ToLower().Equals("series") ||
                                               arg.Tier.ToLower().Equals("sport") ||
                                               arg.Tier.ToLower().Equals("sport event"));
            if (images != null)
            {
                // get the aspect ratios available and fix the URI
                HashSet<string> aspects = new HashSet<string>();
                foreach (sdImage image in images)
                {
                    aspects.Add(image.Aspect);
                    if (!image.Uri.ToLower().StartsWith("http"))
                    {
                        image.Uri = string.Format("{0}{1}image/{2}", sdAPI.jsonBaseUrl, sdAPI.jsonApi, image.Uri.ToLower());
                    }
                }

                // determine which image to return with each aspect
                foreach (string aspect in aspects)
                {
                    var imgAspects = images.Where(arg => arg.Aspect.ToLower().Equals(aspect));

                    sdImage[] links = new sdImage[8];
                    foreach (sdImage image in imgAspects)
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
                            default:
                                break;
                        }

                    }

                    foreach (sdImage link in links)
                    {
                        if (link != null)
                        {
                            ret.Add(link);
                            break;
                        }
                    }
                }
            }
            return ret;
        }
    }
}