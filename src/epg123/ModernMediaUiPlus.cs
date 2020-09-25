using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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

                    Logger.WriteInformation(string.Format("Completed save of ModernMedia UI+ JSON support file to \"{0}\".", filepath));
                }
                catch (Exception ex)
                {
                    Logger.WriteError(string.Format("Failed to save the ModernMedia UI+ JSON support file to \"{0}\". Message: {1}", filepath, ex.Message));
                }
            }
        }
    }

    public class ModernMediaUiPlusPrograms
    {
        [JsonProperty("eventDetails")]
        public sdProgramEventDetails EventDetails { get; set; }

        [JsonProperty("keyWords")]
        public sdProgramKeyWords KeyWords { get; set; }

        [JsonProperty("contentRating")]
        public IList<sdProgramContentRating> ContentRating { get; set; }

        [JsonProperty("movie")]
        public sdProgramMovie Movie { get; set; }

        [JsonProperty("showType")]
        public string ShowType { get; set; }

        [JsonProperty("originalAirDate")]
        public string OriginalAirDate { get; set; }
    }
}
