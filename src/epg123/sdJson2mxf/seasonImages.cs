using GaRyan2.MxfXml;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static readonly List<MxfSeason> seasons = new List<MxfSeason>();

        private static bool GetAllSeasonImages()
        {
            // reset counters
            imageQueue = new List<string>();
            imageResponses = new ConcurrentBag<ProgramMetadata>();
            IncrementNextStage(mxf.SeasonsToProcess.Count);
            if (!config.SeasonEventImages) return true;
            if (Helper.Standalone) return true;

            // scan through each series in the mxf
            Logger.WriteMessage($"Entering GetAllSeasonImages() for {totalObjects} seasons.");
            foreach (var season in mxf.SeasonsToProcess)
            {
                var uid = $"{season.SeriesId}_{season.SeasonNumber}";
                if (epgCache.JsonFiles.ContainsKey(uid) && !string.IsNullOrEmpty(epgCache.JsonFiles[uid].Images))
                {
                    epgCache.JsonFiles[uid].Current = true;
                    IncrementProgress();
                    if (string.IsNullOrEmpty(epgCache.JsonFiles[uid].Images)) continue;

                    List<ProgramArtwork> artwork;
                    using (var reader = new StringReader(epgCache.JsonFiles[uid].Images))
                    {
                        var serializer = new JsonSerializer();
                        season.extras.Add("artwork", artwork = (List<ProgramArtwork>)serializer.Deserialize(reader, typeof(List<ProgramArtwork>)));
                    }
                    season.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Season);
                }
                else if (!string.IsNullOrEmpty(season.ProtoTypicalProgram))
                {
                    seasons.Add(season);
                    imageQueue.Add(season.ProtoTypicalProgram);
                }
                else
                {
                    IncrementProgress();
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable season image links.");

            // maximum 500 queries at a time
            if (imageQueue.Count > 0)
            {
                Parallel.For(0, (imageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadImageResponses(i * MaxImgQueries);
                });

                ProcessSeasonImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning($"Failed to download and process {seasons.Count - processedObjects} season image links.");
                }
            }
            Logger.WriteMessage("Exiting GetAllSeasonImages(). SUCCESS.");
            imageQueue = null; imageResponses = null; seasons.Clear();
            return true;
        }

        private static void ProcessSeasonImageResponses()
        {
            // process request response
            foreach (var response in imageResponses)
            {
                IncrementProgress();
                if (response.Data == null) continue;

                var season = seasons.SingleOrDefault(arg => arg.ProtoTypicalProgram == response.ProgramId);
                if (season == null) continue;

                // get season images
                List<ProgramArtwork> artwork;
                season.extras.Add("artwork", artwork = GetTieredImages(response.Data, new List<string> { "season" }));

                // create a season entry in cache
                var uid = $"{season.SeriesId}_{season.SeasonNumber}";
                if (!epgCache.JsonFiles.ContainsKey(uid))
                {
                    epgCache.AddAsset(uid, null);
                }

                season.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Season, uid);
            }
        }
    }
}