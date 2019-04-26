using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace epg123Transfer
{
    public class tvdbTokenRequest
    {
        [JsonProperty("apikey")]
        public string ApiKey { get; set; }

        [JsonProperty("userkey")]
        public string UserKey { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }
    }

    public class tvdbTokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }


    public class tvdbSearchSeriesResponse
    {
        [JsonProperty("data")]
        public tvdbSeriesSearchData[] Data { get; set; }
    }

    public class tvdbSeriesSearchData
    {
        [JsonProperty("aliases")]
        public string[] Aliases { get; set; }

        [JsonProperty("banner")]
        public string Banner { get; set; }

        [JsonProperty("firstAired")]
        public string FirstAired { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("network")]
        public string Network { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("seriesName")]
        public string SeriesName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class tvdbSeriesDataResponse
    {
        [JsonProperty("data")]
        public tvdbSeries Data { get; set; }

        [JsonProperty("errors")]
        public tvdbJsonErrors errors { get; set; }
    }

    public class tvdbSeries
    {
        public override string ToString()
        {
            return SeriesName;
        }

        public Image SeriesImage { get; set; }

        [JsonProperty("added")]
        public string Added { get; set; }

        [JsonProperty("airsDayOfWeek")]
        public string AirsDayOfWeek { get; set; }

        [JsonProperty("airsTime")]
        public string AirsTime { get; set; }

        [JsonProperty("aliases")]
        public string[] Aliases { get; set; }

        [JsonProperty("banner")]
        public string Banner { get; set; }

        [JsonProperty("firstAired")]
        public string FirstAired { get; set; }

        [JsonProperty("genre")]
        public string[] Genre { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("imdbId")]
        public string ImdbId { get; set; }

        [JsonProperty("lastUpdated")]
        public int LastUpdated { get; set; }

        [JsonProperty("network")]
        public string Network { get; set; }

        [JsonProperty("networkId")]
        public string NetworkId { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("runtime")]
        public string Runtime { get; set; }

        [JsonProperty("seriesId")]
        public string SeriesId { get; set; }

        [JsonProperty("seriesName")]
        public string SeriesName { get; set; }

        [JsonProperty("siteRating")]
        public decimal SiteRating { get; set; }

        [JsonProperty("siteRatingCount")]
        public int SiteRatingCount { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("zap2itId")]
        public string Zap2itId { get; set; }
    }

    public class tvdbJsonErrors
    {
        [JsonProperty("invalidFilters")]
        public string[] InvalidFilters { get; set; }

        [JsonProperty("invalidLanguage")]
        public string InvalidLanguage { get; set; }

        [JsonProperty("invalidQueryParams")]
        public string InvalidQueryParams { get; set; }
    }

    public class tvdbSeriesImagesCounts
    {
        [JsonProperty("data")]
        public tvdbSeriesImagesCount Data { get; set; }
    }

    public class tvdbSeriesImagesCount
    {
        [JsonProperty("fanart")]
        public int Fanart { get; set; }

        [JsonProperty("poster")]
        public int Poster { get; set; }

        [JsonProperty("season")]
        public int Season { get; set; }

        [JsonProperty("seasonwide")]
        public int Seasonwide { get; set; }

        [JsonProperty("series")]
        public int Series { get; set; }
    }

    public class tvdbSeriesImageQueryResults
    {
        [JsonProperty("data")]
        public IList<tvdbSeriesImageQueryResult> Data { get; set; }

        [JsonProperty("errors")]
        public tvdbJsonErrors Errors { get; set; }
    }

    public class tvdbSeriesImageQueryResult
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("keyType")]
        public string KeyType { get; set; }

        [JsonProperty("languageId")]
        public string LanguageId { get; set; }

        [JsonProperty("ratingsInfo")]
        public tvdbInlineModel RatingsInfo { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("subKey")]
        public string SubKey { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
    }

    public class tvdbInlineModel
    {
        [JsonProperty("average")]
        public decimal Average { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
