using Newtonsoft.Json;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoImage
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}