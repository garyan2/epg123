using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using epg123.SchedulesDirectAPI;
using epg123.TheMovieDbAPI;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> movieImageQueue;
        private static ConcurrentBag<sdArtworkResponse> movieImageResponses = new ConcurrentBag<sdArtworkResponse>();
        private static sdImage movieStaple;

        private static bool GetAllMoviePosters()
        {
            var moviePrograms = SdMxf.With[0].Programs.Where(arg => arg.IsMovie)
                                                                .Where(arg => !arg.IsAdultOnly).ToArray();

            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering getAllMoviePosters() for {totalObjects = moviePrograms.Count()} movies.");
            ++processStage; ReportProgress();

            // query all movies from Schedules Direct
            movieImageQueue = new List<string>();
            foreach (var mxfProgram in moviePrograms)
            {
                try
                {
                    if (epgCache.JsonFiles[mxfProgram.Md5].Images != null)
                    {
                        ++processedObjects; ReportProgress();
                        if (string.IsNullOrEmpty(epgCache.JsonFiles[mxfProgram.Md5].Images)) continue;

                        using (var reader = new StringReader(epgCache.JsonFiles[mxfProgram.Md5].Images))
                        {
                            var serializer = new JsonSerializer();
                            mxfProgram.ProgramImages = (List<sdImage>)serializer.Deserialize(reader, typeof(List<sdImage>));
                        }

                        if (mxfProgram.ProgramImages.Count > 0)
                        {
                            mxfProgram.GuideImage = SdMxf.With[0].GetGuideImage(mxfProgram.ProgramImages[0].Uri).Id;
                        }
                    }
                    else
                    {
                        movieImageQueue.Add(mxfProgram.TmsId);
                    }
                }
                catch
                {
                    Logger.WriteInformation($"Could not find expected program with MD5 hash {mxfProgram.Md5}. Continuing.");
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached movie poster links.");

            // maximum 500 queries at a time
            if (movieImageQueue.Count > 0)
            {
                Parallel.For(0, (movieImageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadMovieImageResponses(i * MaxImgQueries);
                });

                ProcessMovieImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteInformation("Problem occurred during getAllMoviePosters(). Did not process all movie image responses.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} movie poster links.");
            Logger.WriteMessage("Exiting getAllMoviePosters(). SUCCESS.");
            movieImageQueue = null; movieImageResponses = null;
            return true;
        }

        private static void DownloadMovieImageResponses(int start = 0)
        {
            // reject 0 requests
            if (movieImageQueue.Count - start < 1) return;

            // build the array of movies to request images for
            var movies = new string[Math.Min(movieImageQueue.Count - start, MaxImgQueries)];
            for (var i = 0; i < movies.Length; ++i)
            {
                movies[i] = movieImageQueue[start + i];
            }

            // request images from Schedules Direct
            var responses = sdApi.SdGetArtwork(movies);
            if (responses != null)
            {
                Parallel.ForEach(responses, (response) =>
                {
                    movieImageResponses.Add(response);
                });
            }
        }

        private static void ProcessMovieImageResponses()
        {
            // process request response
            foreach (var response in movieImageResponses)
            {
                ++processedObjects; ReportProgress();
                movieStaple = null;

                // determine which program this belongs to
                var mxfProgram = SdMxf.With[0].GetProgram(response.ProgramId);

                // first choice is return from Schedules Direct
                if (response.Data != null)
                {
                    mxfProgram.ProgramImages = GetMovieImages(response.Data);
                }

                // second choice is from TMDb if allowed and available
                if (mxfProgram.ProgramImages.Count == 0 && config.TMDbCoverArt && tmdbApi.IsAlive)
                {
                    mxfProgram.ProgramImages = GetMoviePosterId(mxfProgram.Title, mxfProgram.Year, mxfProgram.Language);
                }

                // last choice is the staple image
                if (mxfProgram.ProgramImages.Count == 0 && movieStaple != null)
                {
                    mxfProgram.ProgramImages.Add(movieStaple);
                }

                // regardless if image is found or not, store the final result in xml file
                // this avoids hitting the tmdb server every update for every movie missing cover art
                if (mxfProgram.ProgramImages.Count > 0)
                {
                    using (var writer = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, mxfProgram.ProgramImages);
                        epgCache.UpdateAssetImages(mxfProgram.Md5, writer.ToString());
                    }

                    mxfProgram.GuideImage = SdMxf.With[0].GetGuideImage(mxfProgram.ProgramImages[0].Uri).Id;
                }
                else if (config.TMDbCoverArt && tmdbApi.IsAlive)
                {
                    epgCache.UpdateAssetImages(mxfProgram.Md5, string.Empty);
                }
            }
        }

        private static IList<sdImage> GetMovieImages(IList<sdImage> sdImages)
        {
            var ret = new List<sdImage>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                     .Where(arg => !string.IsNullOrEmpty(arg.Aspect)).Where(arg => arg.Aspect.ToLower().Equals("2x3"))
                     .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md"))
                     .Where(arg => !string.IsNullOrEmpty(arg.Uri));

            // get the aspect ratios available and fix the URI
            var links = new sdImage[3];
            foreach (var image in images)
            {
                if (!image.Uri.ToLower().StartsWith("http"))
                {
                    image.Uri = $"{sdApi.JsonBaseUrl}{sdApi.JsonApi}image/{image.Uri.ToLower()}";
                }

                switch (image.Category.ToLower())
                {
                    case "poster art":  // theatrical movie poster, standard sizes
                        if (links[0] == null) links[0] = image;
                        break;
                    case "box art":     // DVD box art, for movies only
                        if (links[1] == null) links[1] = image;
                        break;
                    case "vod art":     // undocumented, but looks like box art for video on demand
                        if (links[2] == null) links[2] = image;
                        break;
                    case "staple":      // the staple image is intended to cover programs which do not have a unique banner image
                        if (movieStaple == null) movieStaple = image;
                        break;
                }
            }

            foreach (var link in links)
            {
                if (link == null) continue;
                ret.Add(link);
                break;
            }
            return ret;
        }

        private static IList<sdImage> GetMoviePosterId(string title, int year, string language)
        {
            language = !string.IsNullOrEmpty(language) ? language.Substring(0, 2) : "en";

            if (!tmdbApi.IsAlive) return new List<sdImage>();

            // if year is empty, use last year as starting point
            if (year == 0)
            {
                year = DateTime.Now.Year - 1;
            }

            // return first finding
            var years = new[] { year, year + 1, year - 1 };
            return years.Any(y => year <= DateTime.Now.Year && tmdbApi.SearchCatalog(title, y, language) > 0) ? tmdbApi.SdImages : new List<sdImage>();
        }
    }
}