using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace epg123.Github
{
    public class GithubApi
    { 
        private const string apiBase = @"https://api.github.com/repos/garyan2/epg123";

        public Release GetLatestReleaseInfo()
        {
            var url = $"{apiBase}/releases/latest";
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = $"EPG123/{Helper.Epg123Version}";
            req.Accept = "application/vnd.github.v3+json";
            req.Timeout = 1000;
            req.Method = "GET";

            var sr = new StreamReader(req.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return JsonConvert.DeserializeObject<Release>(sr);
        }

        public List<Release> GetAllReleasesInfo()
        {
            var url = $"{apiBase}/releases";
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = $"EPG123/{Helper.Epg123Version}";
            req.Accept = "application/vnd.github.v3+json";
            req.Timeout = 1000;
            req.Method = "GET";

            var sr = new StreamReader(req.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return JsonConvert.DeserializeObject<List<Release>>(sr);
        }
    }

    public class Release
    {
        public override string ToString()
        {
            return $"{TagName}/{Name}";
        }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("assets_url")]
        public string AssetsUrl { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("author")]
        public Person Author { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("target_commitish")]
        public string TargetCommitish { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonProperty("assets")]
        public Asset[] Assets { get; set; }

        [JsonProperty("tarball_url")]
        public string TarballUrl { get; set; }

        [JsonProperty("zipball_url")]
        public string ZipballUrl { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("reactions")]
        public Reaction Reactions { get; set; }
    }

    public class Person
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; }

        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }

        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }

        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }

        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }

        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; }

        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }

        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }

        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class Asset
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("uploader")]
        public Person Uploader { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    public class Reaction
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("+1")]
        public int Plus1 { get; set; }

        [JsonProperty("-1")]
        public int Minus1 { get; set; }

        [JsonProperty("laugh")]
        public int Laugh { get; set; }

        [JsonProperty("hooray")]
        public int Hooray { get; set; }

        [JsonProperty("confused")]
        public int Confused { get; set; }

        [JsonProperty("heart")]
        public int Heart { get; set; }

        [JsonProperty("rocket")]
        public int Rocket { get; set; }

        [JsonProperty("eyes")]
        public int Eyes { get; set; }
    }
}
