using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static List<Headend> GetHeadends(string country, string postalcode)
        {
            var ret = GetSdApiResponse<List<Headend>>("GET", $"headends?country={country}&postalcode={postalcode}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved the headends for {country} and postal code {postalcode}.");
            else Logger.WriteError($"Failed to get a response from Schedules Direct for the headends of {country} and postal code {postalcode}.");
            return ret;
        }
    }

    public class Headend
    {
        [JsonProperty("headend")]
        public string HeadendId { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("lineups")]
        [JsonConverter(typeof(SingleOrListConverter<HeadendLineup>))]
        public List<HeadendLineup> Lineups { get; set; }
    }

    public class HeadendLineup
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
