using Newtonsoft.Json;

namespace epg123
{
    public class sdError
    {
        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("serverID")]
        public string ServerID { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("datetime")]
        public string Datetime { get; set; }
    }
}
