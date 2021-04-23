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
        private static bool GetAllMoviePosters()
        {
            var moviePrograms = SdMxf.With.Programs.Where(arg => arg.IsMovie)
                .Where(arg => !arg.IsAdultOnly).ToList();

            // reset counters
            imageQueue = new List<string>();
            imageResponses = new ConcurrentBag<ProgramMetadata>();
            processedObjects = 0;
            Logger.WriteMessage($"Entering GetAllMoviePosters() for {totalObjects = moviePrograms.Count} movies.");
            ++processStage; ReportProgress();

            // query all movies from Schedules Direct
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
                    mxfProgram.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Movie);
                }
                else
                {
                    imageQueue.Add(mxfProgram.ProgramId);
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable movie poster links.");

            // maximum 500 queries at a time
            if (imageQueue.Count > 0)
            {
                Parallel.For(0, (imageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadImageResponses(i * MaxImgQueries);
                });

                ProcessMovieImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteInformation("Problem occurred during GetAllMoviePosters(). Did not process all movie image responses.");
                }
            }
            Logger.WriteInformation($"Processed {processedObjects} movie poster links.");
            Logger.WriteMessage("Exiting GetAllMoviePosters(). SUCCESS.");
            imageQueue = null; imageResponses = null;
            return true;
        }

        private static void ProcessMovieImageResponses()
        {
            // process request response
            foreach (var response in imageResponses)
            {
                ++processedObjects; ReportProgress();
                if (response.Data == null) continue;

                // determine which program this belongs to
                var mxfProgram = SdMxf.GetProgram(response.ProgramId);

                // first choice is return from Schedules Direct
                List<ProgramArtwork> artwork;
                artwork = GetTieredImages(response.Data, new List<string> { "episode" }).Where(arg => arg.Aspect.Equals("2x3")).ToList();

                // second choice is from TMDb if allowed and available
                if (artwork.Count == 0 || artwork[0].Category.Equals("Staple") && config.TMDbCoverArt && tmdbApi.IsAlive)
                {
                    var tmdb = GetTmdbMoviePoster(mxfProgram.Title, mxfProgram.Year, mxfProgram.Language);
                    if (tmdb.Count > 0) artwork = tmdb;
                }

                // regardless if image is found or not, store the final result in xml file
                // this avoids hitting the tmdb server every update for every movie missing cover art
                mxfProgram.extras.Add("artwork", artwork);
                mxfProgram.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Movie, mxfProgram.extras["md5"]);
            }
        }

        private static List<ProgramArtwork> GetTmdbMoviePoster(string title, int year, string language)
        {
            if (!tmdbApi.IsAlive) return new List<ProgramArtwork>();
            language = !string.IsNullOrEmpty(language) ? language.Substring(0, 2) : "en";

            // if year is empty, use last year as starting point
            if (year == 0) { year = DateTime.Now.Year - 1; }

            // return first finding
            var years = new[] { year, year + 1, year - 1 };
            if (!years.Any(y => year <= DateTime.Now.Year && tmdbApi.SearchCatalog(title, y, language) > 0) ||
                string.IsNullOrEmpty(tmdbApi.SearchResults.Results[0]?.PosterPath)) return new List<ProgramArtwork>();

            // grab first result
            int sizeIndex;
            var width = 0;
            for (sizeIndex = 0; sizeIndex < tmdbApi.Config.Images.PosterSizes.Count; ++sizeIndex)
            {
                if ((width = int.Parse(tmdbApi.Config.Images.PosterSizes[sizeIndex].Substring(1))) < 300) continue;
                break;
            }
            return new List<ProgramArtwork>
            {
                new ProgramArtwork
                {
                    Aspect = "2x3",
                    Category = "Box Art",
                    Height = (int)(width * 1.5),
                    Size = "Md",
                    Uri = $"{tmdbApi.Config.Images.BaseUrl}w{width}{tmdbApi.SearchResults.Results[0].PosterPath}",
                    Width = width
                }
            };
        }
    }
}