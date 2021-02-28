using System.Collections.Generic;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private const int MaxQueries = 5000;
        private const int MaxImgQueries = 500;
        private const int MaxParallelDownloads = 4;

        private static List<string> suppressedPrefixes = new List<string>();

        private static int processedObjects;
        private static int totalObjects;
        private static int processStage;
        private static readonly string[] Stages = { "TASK: Process subscribed lineups and stations ...",
                                                    "TASK: Build schedules - Stage 1 ...",
                                                    "TASK: Build schedules - Stage 2 ...",
                                                    "TASK: Build programs ...",
                                                    "TASK: Build series descriptions ...",
                                                    "TASK: Build extended series data for MMUI+ ...",
                                                    "TASK: Build movie posters ...",
                                                    "TASK: Build series images ...",
                                                    "TASK: Build sport event images ...",
                                                    "TASK: Waiting for download of channel logos to complete ...",
                                                    "TASK: Saving files ...",
                                                    "TASK: Clean and save cache file ..." };

        private static void ReportProgress()
        {
            if (processedObjects == 0)
            {
                Helper.SendPipeMessage($"Downloading|{processStage + 1}/{Stages.Length} {Stages[processStage].Substring(6)}");
            }

            // if the progress form is not shown, nothing to update
            if (BackgroundWorker == null) return;

            var numerator = processedObjects * 100;
            var denominator = totalObjects;
            if (denominator == 0)
            {
                numerator = 0;
                denominator = 1;
            }
            string[] textObjects = { Stages[processStage],
                $"{processStage + 1}/{Stages.Length}",
                $"{processedObjects}/{totalObjects}"
            };
            BackgroundWorker.ReportProgress(numerator / denominator + (processStage + 1) * 10000, textObjects);
        }
    }
}
