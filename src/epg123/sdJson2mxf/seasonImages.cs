using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static List<MxfSeason> seasons = new List<MxfSeason>();

        private static bool GetAllSeasonImages()
        {
            // reset counters
            imageQueue = new List<string>();
            imageResponses = new ConcurrentBag<ProgramMetadata>();
            processedObjects = 0;
            ++processStage; ReportProgress();
            if (!config.SeasonEventImages) return true;

            // scan through each series in the mxf
            Logger.WriteMessage($"Entering GetAllSeasonImages() for {totalObjects = SdMxf.With.Seasons.Count} seasons.");
            foreach (var season in SdMxf.With.Seasons)
            {
                var uid = $"{season.mxfSeriesInfo.SeriesId}_{season.SeasonNumber}";
                if (epgCache.JsonFiles.ContainsKey(uid) && !string.IsNullOrEmpty(epgCache.JsonFiles[uid].Images))
                {
                    epgCache.JsonFiles[uid].Current = true;
                    ++processedObjects; ReportProgress();
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
                    ++processedObjects; ReportProgress();
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable season image links.");
            totalObjects = processedObjects + imageQueue.Count;
            ReportProgress();

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
                    Logger.WriteInformation($"Failed to download and process {SdMxf.With.Seasons.Count - processedObjects} season image links.");
                }
            }
            Logger.WriteMessage("Exiting GetAllSeasonImages(). SUCCESS.");
            imageQueue = null; imageResponses = null;
            return true;
        }

        private static void ProcessSeasonImageResponses()
        {
            // process request response
            foreach (var response in imageResponses)
            {
                ++processedObjects; ReportProgress();
                if (response.Data == null) continue;
                
                var season = seasons.SingleOrDefault(arg => arg.ProtoTypicalProgram == response.ProgramId);
                if (season == null) continue;

                // get season images
                List<ProgramArtwork> artwork;
                season.extras.Add("artwork", artwork = GetTieredImages(response.Data, new List<string> { "season" }));

                // create a season entry in cache
                var uid = $"{season.mxfSeriesInfo.SeriesId}_{season.SeasonNumber}";
                if (!epgCache.JsonFiles.ContainsKey(uid))
                {
                    epgCache.AddAsset(uid, null);
                }

                season.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Season, uid);
            }
        }
    }
}