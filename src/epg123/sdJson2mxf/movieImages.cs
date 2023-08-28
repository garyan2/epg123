using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tmdbApi = GaRyan2.Tmdb;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static bool GetAllMoviePosters()
        {
            var moviePrograms = mxf.ProgramsToProcess.Where(arg => arg.IsMovie).Where(arg => !arg.IsAdultOnly).ToList();

            // reset counters
            imageQueue = new List<string>();
            imageResponses = new ConcurrentBag<ProgramMetadata>();
            IncrementNextStage(moviePrograms.Count);
            if (Helper.Standalone) return true;
            Logger.WriteMessage($"Entering GetAllMoviePosters() for {totalObjects} movies.");

            // query all movies from Schedules Direct
            foreach (var mxfProgram in moviePrograms)
            {
                if (epgCache.JsonFiles.ContainsKey(mxfProgram.extras["md5"]) && epgCache.JsonFiles[mxfProgram.extras["md5"]].Images != null)
                {
                    IncrementProgress();
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
                    Logger.WriteWarning($"Failed to download and process {moviePrograms.Count - processedObjects} movie poster links.");
                }
            }
            Logger.WriteMessage("Exiting GetAllMoviePosters(). SUCCESS.");
            imageQueue = null; imageResponses = null;
            return true;
        }

        private static void ProcessMovieImageResponses()
        {
            // process request response
            foreach (var response in imageResponses)
            {
                IncrementProgress();
                if (response.Data == null) continue;

                // determine which program this belongs to
                var mxfProgram = mxf.FindOrCreateProgram(response.ProgramId);

                // first choice is return from Schedules Direct
                List<ProgramArtwork> artwork;
                artwork = GetTieredImages(response.Data, new List<string> { "episode" }).Where(arg => arg.Aspect.Equals("2x3")).ToList();

                // second choice is from TMDb if allowed and available
                if (artwork.Count == 0 || artwork[0].Category.Equals("Staple"))
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
            var poster = tmdbApi.FindPosterArtwork(title, year, language);
            if (poster == null) return new List<ProgramArtwork>();
            return new List<ProgramArtwork>
            {
                new ProgramArtwork
                {
                    Aspect = "2x3",
                    Category = "Box Art",
                    Height = (int)(tmdbApi.PosterWidth * 1.5),
                    Size = config.ArtworkSize,
                    Uri = poster,
                    Width = tmdbApi.PosterWidth
                }
            };
        }
    }
}