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
        private static readonly List<MxfProgram> sportEvents = new List<MxfProgram>();

        private static bool GetAllSportsImages()
        {
            // reset counters
            imageQueue = new List<string>();
            imageResponses = new ConcurrentBag<ProgramMetadata>();
            IncrementNextStage(sportEvents.Count);
            if (!config.SeasonEventImages) return true;
            if (Helper.Standalone) return true;

            // scan through each series in the mxf
            Logger.WriteMessage($"Entering GetAllSportsImages() for {totalObjects} sports events.");
            foreach (var sportEvent in sportEvents)
            {
                string md5 = sportEvent.extras["md5"];
                if (epgCache.JsonFiles.ContainsKey(md5) && !string.IsNullOrEmpty(epgCache.JsonFiles[md5].Images))
                {
                    IncrementProgress();
                    List<ProgramArtwork> artwork;
                    using (var reader = new StringReader(epgCache.JsonFiles[md5].Images))
                    {
                        var serializer = new JsonSerializer();
                        sportEvent.extras.Add("artwork", artwork = (List<ProgramArtwork>)serializer.Deserialize(reader, typeof(List<ProgramArtwork>)));
                    }
                    sportEvent.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Program);
                }
                else
                {
                    imageQueue.Add(sportEvent.ProgramId);
                }
            }
            Logger.WriteVerbose($"Found {processedObjects} cached/unavailable sport event image links.");

            // maximum 500 queries at a time
            if (imageQueue.Count > 0)
            {
                Parallel.For(0, (imageQueue.Count / MaxImgQueries + 1), new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads }, i =>
                {
                    DownloadImageResponses(i * MaxImgQueries);
                });

                ProcessSportsImageResponses();
                if (processedObjects != totalObjects)
                {
                    Logger.WriteWarning($"Failed to download and process {sportEvents.Count - processedObjects} sport event image links.");
                }
            }
            Logger.WriteMessage("Exiting GetAllSportsImages(). SUCCESS.");
            imageQueue = null; imageResponses = null; sportEvents.Clear();
            return true;
        }

        private static void ProcessSportsImageResponses()
        {
            // process request response
            if (imageResponses == null) return;
            foreach (var response in imageResponses)
            {
                IncrementProgress();
                if (response.Data == null) continue;

                var mxfProgram = sportEvents.SingleOrDefault(arg => arg.ProgramId == response.ProgramId);
                if (mxfProgram == null) continue;

                // get sports event images
                List<ProgramArtwork> artwork;
                mxfProgram.extras.Add("artwork", artwork = GetTieredImages(response.Data, new List<string> { "team event", "sport event" }));
                mxfProgram.mxfGuideImage = GetGuideImageAndUpdateCache(artwork, ImageType.Program, mxfProgram.extras["md5"]);
            }
        }
    }
}