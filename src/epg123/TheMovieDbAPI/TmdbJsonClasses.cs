using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.TheMovieDbAPI
{
    internal class TmdbMovieListResponse
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("results")]
        public IList<TmdbMovieResults> Results { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("total_results")]
        public int TotalResults { get; set; }
    }

    internal class TmdbMovieResults
    {
        [JsonProperty("adult")]
        public bool Adult { get; set; }

        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("original_title")]
        public string OriginalTitle { get; set; }

        [JsonProperty("popularity")]
        public double Popularity { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("vote_average")]
        public double VoteAverage { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }
    }

    internal class TmdbConfiguration
    {
        [JsonProperty("images")]
        public TmdbImagesConfiguration Images { get; set; }

        [JsonProperty("change_keys")]
        public IList<string> ChangeKeys { get; set; }
    }

    internal class TmdbImagesConfiguration
    {
        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        [JsonProperty("secure_base_url")]
        public string SecureBaseUrl { get; set; }

        [JsonProperty("backdrop_sizes")]
        public IList<string> BackdropSizes { get; set; }

        [JsonProperty("logo_sizes")]
        public IList<string> LogoSizes { get; set; }

        [JsonProperty("poster_sizes")]
        public IList<string> PosterSizes { get; set; }

        [JsonProperty("profile_sizes")]
        public IList<string> ProfileSizes { get; set; }

        [JsonProperty("still_sizes")]
        public IList<string> StillSizes { get; set; }
    }
}
