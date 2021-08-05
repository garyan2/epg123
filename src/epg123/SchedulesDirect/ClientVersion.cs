using System;
using System.Linq;
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
                var releases = github.GetAllReleasesInfo();
                
                // find my version in list
                var myVersion = releases.SingleOrDefault(arg => arg.TagName.Equals(Helper.Epg123Version));
                if (myVersion == null)
                {
                    return new ClientVersion
                    {
                        Client = "EPG123",
                        Datetime = DateTime.UtcNow.ToLocalTime(),
                        Version = Helper.Epg123Version
                    };
                }

                // find latest release and any betas afterwords
                var latestRelease = releases.First(arg => !arg.Prerelease);
                var latestBeta = releases.FirstOrDefault(arg => arg.PublishedAt > latestRelease.PublishedAt);
                if (myVersion.PublishedAt <= latestRelease.PublishedAt)
                {
                    if (myVersion.PublishedAt < latestRelease.PublishedAt)
                    {
                        Logger.WriteInformation($"epg123 is not up to date. Latest version is {latestRelease.TagName} and can be downloaded from {latestRelease.HtmlUrl}");
                    }
                    return new ClientVersion
                    {
                        Client = "EPG123",
                        Datetime = latestRelease.PublishedAt.ToLocalTime(),
                        Version = latestRelease.TagName.Replace("v", "")
                    };
                }
                
                // return latest beta version
                if (myVersion.PublishedAt < latestBeta.PublishedAt)
                {
                    Logger.WriteInformation($"epg123 is not up to date. Latest version is {latestBeta.TagName} and can be downloaded from {latestBeta.HtmlUrl}");
                }
                return new ClientVersion
                {
                    Client = "EPG123",
                    Datetime = latestBeta.PublishedAt.ToLocalTime(),
                    Version = latestBeta.TagName.Replace("v", "")
                };
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
