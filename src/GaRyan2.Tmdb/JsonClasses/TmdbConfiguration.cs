using Newtonsoft.Json;
using System.Collections.Generic;

namespace GaRyan2.TmdbApi
{
    public class TmdbConfiguration
    {
        [JsonProperty("images")]
        public ImagesConfiguration Images { get; set; }

        [JsonProperty("change_keys")]
        public List<string> ChangeKeys { get; set; }
    }
}
