using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using epg123.SchedulesDirect;
using epg123.TheMovieDbAPI;
using Newtonsoft.Json;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static List<string> movieImageQueue;
        private static ConcurrentBag<ProgramMetadata> movieImageResponses = new ConcurrentBag<ProgramMetadata>();
        private static ProgramArtwork movieStaple;

        private static bool GetAllMoviePosters()
        {
            var moviePrograms = SdMxf.With.Programs.Where(arg => arg.IsMovie)
                                                                .Where(arg => !arg.IsAdultOnly).ToArray();

            // reset counters
            processedObjects = 0;
            Logger.WriteMessage($"Entering GetAllMoviePosters() for {totalObjects = moviePrograms.Count()} movies.");
            ++processStage; ReportProgress();

            // query all movies from Schedules Direct
            movieImageQueue = new List<string>();
            foreach (var mxfProgram in moviePrograms)
            {
                if (epgCache.JsonFiles.ContainsKey(mxfProgram.extras["md5"]) && epgCache.JsonFiles[mxfProgram.extras["md5"]].Images != null)
                {
                    ++processedObjects; ReportProgress();
                    if (string.IsNullOrEmpty(epgCache.JsonFiles[mxfProgram.extras["md5"]].Images)) continue;

                    List<ProgramArtwork> artwork;
                    using (var reader = new StringReader(epgCache.JsonFiles[mxfProgram.extras["md5"]].Images))
                    {
                        var serializer = new JsonSerializer();
                        mxfProgram.extras.Add("artwork", artwork = (List<ProgramArtwork>)serializer.Deserialize(reader, typeof(List<ProgramArtwork>)));
                    }

                    if (artwork.Count > 0)
                    {
                        mxfProgram.mxfGuideImage = SdMxf.GetGuideImage(artwork[0].Uri);
                    }
                }
                else
                {
                    movieImageQueue.Add(mxfProgram.ProgramId);
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable movie poster links.");

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
            var responses = SdApi.GetArtwork(movies);
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
                if (response.Data == null) continue;

                movieStaple = null;

                // determine which program this belongs to
                var mxfProgram = SdMxf.GetProgram(response.ProgramId);

                // first choice is return from Schedules Direct
                List<ProgramArtwork> artwork; 
                artwork = GetMovieImages(response.Data);

                // second choice is from TMDb if allowed and available
                if (artwork.Count == 0 && config.TMDbCoverArt && tmdbApi.IsAlive)
                {
                    artwork = GetMoviePosterId(mxfProgram.Title, mxfProgram.Year, mxfProgram.Language);
                }

                // last choice is the staple image
                if (artwork.Count == 0 && movieStaple != null)
                {
                    artwork.Add(movieStaple);
                }

                // regardless if image is found or not, store the final result in xml file
                // this avoids hitting the tmdb server every update for every movie missing cover art
                mxfProgram.extras.Add("artwork", artwork);
                if (artwork.Count > 0)
                {
                    using (var writer = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, artwork);
                        epgCache.UpdateAssetImages(mxfProgram.extras["md5"], writer.ToString());
                    }

                    mxfProgram.mxfGuideImage = SdMxf.GetGuideImage(artwork[0].Uri);
                }
                else if (config.TMDbCoverArt && tmdbApi.IsAlive)
                {
                    epgCache.UpdateAssetImages(mxfProgram.extras["md5"], string.Empty);
                }
            }
        }

        private static List<ProgramArtwork> GetMovieImages(List<ProgramArtwork> sdImages)
        {
            var ret = new List<ProgramArtwork>();
            var images = sdImages.Where(arg => !string.IsNullOrEmpty(arg.Category))
                     .Where(arg => !string.IsNullOrEmpty(arg.Aspect)).Where(arg => arg.Aspect.ToLower().Equals("2x3"))
                     .Where(arg => !string.IsNullOrEmpty(arg.Size)).Where(arg => arg.Size.ToLower().Equals("md"))
                     .Where(arg => !string.IsNullOrEmpty(arg.Uri));

            // get the aspect ratios available and fix the URI
            var links = new ProgramArtwork[4];
            foreach (var image in images)
            {
                if (!image.Uri.ToLower().StartsWith("http"))
                {
                    image.Uri = $"{SdApi.JsonBaseUrl}{SdApi.JsonApi}image/{image.Uri.ToLower()}";
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
                    case "banner-l2":
                        if (links[3] == null) links[3] = image;
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

        private static List<ProgramArtwork> GetMoviePosterId(string title, int year, string language)
        {
            language = !string.IsNullOrEmpty(language) ? language.Substring(0, 2) : "en";

            if (!tmdbApi.IsAlive) return new List<ProgramArtwork>();

            // if year is empty, use last year as starting point
            if (year == 0)
            {
                year = DateTime.Now.Year - 1;
            }

            // return first finding
            var years = new[] { year, year + 1, year - 1 };
            return years.Any(y => year <= DateTime.Now.Year && tmdbApi.SearchCatalog(title, y, language) > 0) ? tmdbApi.SdImages : new List<ProgramArtwork>();
        }
    }
}