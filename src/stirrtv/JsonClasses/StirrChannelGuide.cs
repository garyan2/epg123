using Newtonsoft.Json;

namespace GaRyan2.StirrTvApi
{
    public class StirrChannelGuide
    {
        [JsonProperty("programme")]
        public StirrProgramme[] Programs { get; set; }
    }

    public class StirrProgramme
    {
        [JsonProperty("title")]
        public StirrValue Title { get; set; }

        [JsonProperty("desc")]
        public StirrValue Description { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("stop")]
        public string End { get; set; }
    }

    public class StirrValue
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}