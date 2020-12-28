using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123Transfer.SchedulesDirectAPI
{
    public class SdTokenRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string PasswordHash { get; set; }
    }

    public class SdTokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class sdProgram
    {
        [JsonProperty("titles")]
        public IList<sdProgramTitle> Titles { get; set; }

        [JsonProperty("descriptions")]
        public sdProgramDescriptions Descriptions { get; set; }

    }

    public class sdProgramTitle
    {
        [JsonProperty("title120")]
        public string Title120 { get; set; }
    }

    public class sdProgramDescriptions
    {
        [JsonProperty("description100")]
        public IList<sdProgramDescription> Description100 { get; set; }

        [JsonProperty("description1000")]
        public IList<sdProgramDescription> Description1000 { get; set; }
    }

    public class sdProgramDescription
    {
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class sdImage
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("aspect")]
        public string Aspect { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }
    }
}
