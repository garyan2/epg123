using GaRyan2.Utilities;
using System.ComponentModel;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        public static BackgroundWorker BackgroundWorker;
        private const int MaxQueries = 1250;
        private const int MaxImgQueries = 125;
        private const int MaxParallelDownloads = 4;

        private static int processedObjects;
        private static int totalObjects;
        private static int processStage;
        public static readonly string[] Stages = { "TASK: Process subscribed lineups and stations ...",
                                                   "TASK: Build schedules - Stage 1 ...",
                                                   "TASK: Build schedules - Stage 2 ...",
                                                   "TASK: Build programs ...",
                                                   "TASK: Build series descriptions ...",
                                                   "TASK: Build extended series data for MMUI+ ...",
                                                   "TASK: Build movie posters ...",
                                                   "TASK: Build series images ...",
                                                   "TASK: Build season images ...",
                                                   "TASK: Build sport event images ...",
                                                   "TASK: Waiting for download of station logos to complete ...",
                                                   "TASK: Saving files ...",
                                                   "TASK: Clean and save cache file ..." };

        private static void ResetStages(int objects)
        {
            processStage = 0;
            processedObjects = 0;
            totalObjects = objects;
        }
        private static void IncrementNextStage(int objects)
        {
            processStage++;
            processedObjects = 0;
            totalObjects = objects;
            ReportProgress();
        }

        private static void IncrementProgress(int add = 1)
        {
            processedObjects += add;
            ReportProgress();
        }

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