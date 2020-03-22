using Newtonsoft.Json;

namespace epg123
{
    public class sdClientVersionResponse
    {
        //[JsonProperty("response")]
        //public string Response { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        //[JsonProperty("client")]
        //public string Client { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        //[JsonProperty("serverID")]
        //public string ServerID { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("datetime")]
        public string Datetime { get; set; }
    }
}
