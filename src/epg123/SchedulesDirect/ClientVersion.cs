using System;
using epg123.Github;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static ClientVersion GetClientVersion()
        {
            try
            {
                var github = new GithubApi();
                var release = github.GetLatestReleaseInfo();
                if (release != null)
                {
                    if (Helper.Epg123Version != release.TagName.Replace("v", ""))
                    {
                        Logger.WriteInformation($"epg123 is not up to date. Latest version is {release.TagName} and can be downloaded from {release.HtmlUrl}");
                    }
                    return new ClientVersion
                    {
                        Client = "EPG123",
                        Datetime = release.PublishedAt.ToLocalTime(),
                        Version = release.TagName.Replace("v", "")
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"GetClientVersion() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class ClientVersion : BaseResponse
    {
        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
