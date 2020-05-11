using System.Reflection;
using System.Collections.Generic;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private const int MAXQUERIES = 5000;
        private const int MAXIMGQUERIES = 500;
        private const int MAXPARALLELDOWNLOADS = 4;
        private const int MAXPARALLELCPUTASKS = 8;

        private static string epg123Version
        {
            get
            {
                string[] vers = Assembly.GetEntryAssembly().GetName().Version.ToString().Split('.');
                return string.Format("{0}.{1}.{2}", vers[0], vers[1], vers[2]);
            }
        }
        
        private static List<string> suppressedPrefixes = new List<string>();

        private static int processedObjects = 0;
        private static int totalObjects = 0;
        private static int processStage = 0;
        private static string[] stages = { "TASK: Process subscribed lineups and stations ...",
                                           "TASK: Build schedules - Stage 1 ...",
                                           "TASK: Build schedules - Stage 2 ...",
                                           "TASK: Build programs ...",
                                           "TASK: Build series descriptions ...",
                                           "TASK: Build extended series data for MMUI+ ...",
                                           "TASK: Build movie posters ...",
                                           "TASK: Build series images ...",
                                           "TASK: Saving files ...",
                                           "TASK: Clean and save cache file ..." };

        private static void reportProgress()
        {
            // if the progress form is not shown, nothing to update
            if (backgroundWorker == null) return;

            int numerator = processedObjects * 100;
            int denominator = totalObjects;
            if (denominator == 0)
            {
                numerator = 0;
                denominator = 1;
            }
            string[] textObjects = { stages[processStage],
                                     string.Format("{0}/{1}", processStage + 1, stages.Length),
                                     string.Format("{0}/{1}", processedObjects, totalObjects) };
            backgroundWorker.ReportProgress(numerator / denominator + (processStage + 1) * 10000, textObjects);
        }
    }
}
