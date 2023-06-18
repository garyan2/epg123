using Newtonsoft.Json;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoClip
    {
        [JsonProperty("originalReleaseDate")]
        public string OriginalReleaseDate { get; set; }
    }
}