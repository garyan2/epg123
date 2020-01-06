using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using epg123.MxfXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static List<string> movieImageQueue;
        private static ConcurrentBag<sdArtworkResponse> movieImageResponses = new ConcurrentBag<sdArtworkResponse>();
        private static sdImage movieStaple;

        private static bool getAllMoviePosters()
        {
            var moviePrograms = sdMxf.With[0].Programs.Where(arg => !string.IsNullOrEmpty(arg.IsMovie))
                                                      .Where(arg => string.IsNullOrEmpty(Helper.tableContains(arg.jsonProgramData.Genres, "Adults Only")));

            // reset counters
            processedObjects = 0;
            Logger.WriteMessage(string.Format("Entering getAllMoviePosters() for {0} movies.", totalObjects = moviePrograms.Count()));
            ++processStage; reportProgress();

            // query all movies from Schedules Direct
            movieImageQueue = new List<string>();
            if (moviePrograms != null)
            {
                foreach (MxfProgram mxfProgram in moviePrograms)
                {
                    var oldImage = oldImageLibrary.Images.Where(arg => arg.Zap2itId.Equals(mxfProgram.tmsId.Substring(0, 10))).SingleOrDefault();
                    if (oldImage != null)
                    {
                        ++processedObjects; reportProgress();
                        if (!string.IsNullOrEmpty(oldImage.Url))
                        {
                            mxfProgram.GuideImage = sdMxf.With[0].getGuideImage(oldImage.Url).Id;
                            mxfProgram.programImages = new List<sdImage>
                            {
                                new sdImage()
                                {
                                    Uri = oldImage.Url,
                                    Height = oldImage.Height,
                                    Width = oldImage.Width
                                }
                            };
                        }

                        newImageLibrary.Images.Add(new archiveImage()
                        {
                            Title = oldImage.Title,
                            Url = oldImage.Url,
                            Zap2itId = oldImage.Zap2itId,
                            Height = oldImage.Height,
                            Width = oldImage.Width
                        });
                    }
                    else
                    {
                        movieImageQueue.Add(mxfProgram.tmsId);
                    }
                }
            }

            // maximum 500 queries at a time
            if (movieImageQueue.Count > 0)
            {
                Parallel.For(0, (movieImageQueue.Count / MAXIMGQUERIES + 1), new ParallelOptions { MaxDegreeOfParallelism = MAXPARALLELDOWNLOADS }, i =>
                {
                    downloadMovieImageResponses(i * MAXIMGQUERIES);
                });

                processMovieImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning("Problem occurred during getAllMoviePosters(). Did not process all movie image responses.");
                }
            }
            Logger.WriteInformation(string.Format("Processed {0} movie poster links.", processedObjects));
            Logger.WriteMessage("Exiting getAllMoviePosters(). SUCCESS.");
            movieImageQueue = null; movieImageResponses = null;
            return true;
        }

        private static void downloadMovieImageResponses(int start = 0)
        {
            // build the array of movies to request images for
            string[] movies = new string[Math.Min(movieImageQueue.Count - start, MAXIMGQUERIES)];
            for (int i = 0; i < movies.Length; ++i)
            {
                movies[i] = movieImageQueue[start + i];
            }

            // request images from Schedules Direct
            IList<sdArtworkResponse> responses = sdAPI.sdGetArtwork(movies);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    movieImageResponses.Add(response);
                });
            }
        }

        private static void processMovieImageResponses()
        {
            // process request response
            foreach (sdArtworkResponse response in movieImageResponses)
            {
                ++processedObjects; reportProgress();
                movieStaple = null;

                // determine which program this belongs to
                MxfProgram mxfProgram = sdMxf.With[0].getProgram(response.ProgramID);

                // first choice is return from Schedules Direct
                if (response.Data != null)
                {
                    mxfProgram.programImages = getMovieImages(response.Data);
                }

                // second choice is from TMDb if allowed and available
                if (mxfProgram.programImages.Count == 0 && config.TMDbCoverArt && tmdbAPI.isAlive)
                {
                    mxfProgram.programImages = getMoviePosterId(mxfProgram.Title, mxfProgram.Year, mxfProgram.Language);
                }

                // regardless if image is found or not, store the final result in xml file
                // this avoids hitting the tmdb server every update for every movie missing cover art
                // do not include the staple image
                sdImage sdImage = (mxfProgram.programImages.Count > 0) ? mxfProgram.programImages[0] : new sdImage() { Uri = string.Empty };
                newImageLibrary.Images.Add(new archiveImage()
                {
                    Title = mxfProgram.Title,
                    Url = sdImage.Uri,
                    Zap2itId = mxfProgram.tmsId.Substring(0, 10),
                    Height = sdImage.Height,
                    Width = sdImage.Width
                });

                // final choice is the staple return from Schedules Direct
                if ((mxfProgram.programImages.Count == 0) && (movieStaple != null))
                {
                    mxfProgram.programImages.Add(movieStaple);
                }

                // if an image is found, insert in the mxf file
                if (mxfProgram.programImages.Count > 0)
                {
                    mxfProgram.GuideImage = sdMxf.With[0].getGuideImage(mxfProgram.programImages[0].Uri).Id;
                }
            }
        }

        private static IList<sdImage> getMovieImages(IList<sdImage> sdImages)
        {
            List<sdImage> ret = new List<sdImage>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                                 .Where(arg => !string.IsNullOrEmpty(arg.Aspect)).Where(arg => arg.Aspect.ToLower().Equals("2x3"))
                                 .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md"))
                                 .Where(arg => !string.IsNullOrEmpty(arg.Uri));
            if (images != null)
            {
                // get the aspect ratios available and fix the URI
                sdImage[] links = new sdImage[3];
                foreach (sdImage image in images)
                {
                    if (!image.Uri.ToLower().StartsWith("http"))
                    {
                        image.Uri = string.Format("{0}{1}image/{2}", sdAPI.jsonBaseUrl, sdAPI.jsonApi, image.Uri.ToLower());
                    }

                    switch (image.Category.ToLower())
                    {
                        case "box art":     // DVD box art, for movies only
                            if (links[0] == null) links[0] = image;
                            break;
                        case "vod art":     // undocumented, but looks like box art for video on demand
                            if (links[1] == null) links[1] = image;
                            break;
                        case "poster art":  // theatrical movie poster, standard sizes
                            if (links[2] == null) links[2] = image;
                            break;
                        case "staple":      // the staple image is intended to cover programs which do not have a unique banner image
                            if (movieStaple == null) movieStaple = image;
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
            return ret;
        }

        private static IList<sdImage> getMoviePosterId(string title, string year, string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                language = language.Substring(0, 2);
            }
            else
            {
                language = "en";
            }

            if (tmdbAPI.isAlive)
            {
                // if year is empty, use last year as starting point
                if (string.IsNullOrEmpty(year))
                {
                    year = (DateTime.Now.Year - 1).ToString();
                }

                // return first finding
                int yyyy = int.Parse(year);
                int[] years = new int[] { yyyy, yyyy - 1, yyyy + 1 };
                foreach (int y in years)
                {
                    if (tmdbAPI.SearchCatalog(title, y, language) > 0)
                    {
                        return tmdbAPI.sdImages;
                    }
                }
            }
            return new List<sdImage>();
        }
    }
}