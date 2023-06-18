using System.Collections.Generic;
using System.Linq;
using GaRyan2.Utilities;

namespace GaRyan2.GithubApi
{
    internal class API : BaseAPI
    {
        internal string repo { get; set; }

        public List<Release> GetAllReleases()
        {
            var ret = GetApiResponse<List<Release>>(Method.GET, $"{repo}/releases")?.OrderByDescending(arg => arg.PublishedAt).ToList();
            if (ret == null) Logger.WriteInformation("Failed to get list of released version information from Github.");
            return ret;
        }

        public Release GetLatestRelease()
        {
            var ret = GetApiResponse<Release>(Method.GET, $"{repo}/releases/latest");
            if (ret == null) Logger.WriteInformation("Failed to get latest release information from Github.");
            return ret;
        }
    }
}