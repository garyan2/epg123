using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123
{
    public class sdCountry
    {
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("postalCodeExample")]
        public string PostalCodeExample { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("onePostalCode")]
        public bool OnePostalCode { get; set; }
    }

    public class sdHeadendResponse
    {
        [JsonProperty("headend")]
        public string Headend { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("lineups")]
        public IList<sdHeadendLineup> Lineups { get; set; }
    }

    public class sdHeadendLineup
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
