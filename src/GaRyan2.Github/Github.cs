using GaRyan2.GithubApi;
using GaRyan2.Utilities;
using System.Linq;

namespace GaRyan2
{
    public static class Github
    {
        private static readonly API api = new API() { BaseAddress = "https://api.github.com/repos/garyan2/" };

        public static void Initialize(string userAgent, string repo)
        {
            api.UserAgent = userAgent;
            api.repo = repo;
            api.Initialize();
        }

        public static bool UpdateAvailable()
        {
            var releases = api.GetAllReleases()?.OrderByDescending(arg => arg.PublishedAt);
            var thisRelease = releases?.SingleOrDefault(arg => arg.TagName.Equals(Helper.Epg123Version));
            if (thisRelease == null) return false;

            var latestRelease = releases?.FirstOrDefault(arg => !arg.Prerelease);
            if (latestRelease != null && latestRelease.PublishedAt > thisRelease.PublishedAt)
            {
                Logger.WriteInformation($"{api.repo} is not up to date. Latest released version is {latestRelease.TagName} and can be downloaded from {latestRelease.HtmlUrl}.");
                return true;
            }

            var latestBeta = releases?.FirstOrDefault(arg => arg.Prerelease);
            if (latestBeta != null && thisRelease.Prerelease && latestBeta.PublishedAt > thisRelease.PublishedAt)
            {
                Logger.WriteInformation($"{api.repo} is not up to date. Latest beta version is {latestBeta.TagName} and can be downloaded from {latestBeta.HtmlUrl}.");
                return true;
            }
            return false;
        }
    }
}