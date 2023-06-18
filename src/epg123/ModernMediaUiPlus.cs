using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace epg123
{
    public static class ModernMediaUiPlus
    {
        public static Dictionary<string, ModernMediaUiPlusPrograms> Programs = new Dictionary<string, ModernMediaUiPlusPrograms>();

        public static void WriteModernMediaUiPlusJson(string filepath = null)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = Helper.Epg123MmuiplusJsonPath;
            }

            using (StreamWriter writer = File.CreateText(filepath))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                    serializer.Serialize(writer, Programs);

                    Logger.WriteInformation($"Completed save of ModernMedia UI+ JSON support file to \"{filepath}\".");
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"Failed to save the ModernMedia UI+ JSON support file to \"{filepath}\". Message: {ex}");
                }
            }
        }
    }

    public class ModernMediaUiPlusPrograms
    {
        [JsonProperty("eventDetails")]
        public ProgramEventDetails EventDetails { get; set; }

        [JsonProperty("keyWords")]
        public ProgramKeyWords KeyWords { get; set; }

        [JsonProperty("contentRating")]
        public IList<ProgramContentRating> ContentRating { get; set; }

        [JsonProperty("movie")]
        public ProgramMovie Movie { get; set; }

        [JsonProperty("showType")]
        public string ShowType { get; set; }

        [JsonProperty("originalAirDate")]
        public string OriginalAirDate { get; set; }
    }
}