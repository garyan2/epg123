using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123
{
    public class sdProgram
    {
        [JsonProperty("programID")]
        public string ProgramID { get; set; }

        //[JsonProperty("resourceID")]
        //public string ResourceID { get; set; }

        [JsonProperty("titles")]
        public IList<sdProgramTitle> Titles { get; set; }

        [JsonProperty("eventDetails")]
        public sdProgramEventDetails EventDetails { get; set; }

        [JsonProperty("descriptions")]
        public sdProgramDescriptions Descriptions { get; set; }

        [JsonProperty("originalAirDate")]
        public string OriginalAirDate { get; set; }

        [JsonProperty("genres")]
        public string[] Genres { get; set; }

        //[JsonProperty("officialURL")]
        //public string OfficialURL { get; set; }

        [JsonProperty("keyWords")]
        public sdProgramKeyWords KeyWords { get; set; }

        [JsonProperty("episodeTitle150")]
        public string EpisodeTitle150 { get; set; }

        [JsonProperty("metadata")]
        public IList<Dictionary<string, sdProgramMetadataProvider>> Metadata { get; set; }

        [JsonProperty("contentRating")]
        public IList<sdProgramContentRating> ContentRating { get; set; }

        [JsonProperty("contentAdvisory")]
        public string[] ContentAdvisory { get; set; }

        [JsonProperty("movie")]
        public sdProgramMovie Movie { get; set; }

        [JsonProperty("cast")]
        public IList<sdProgramPerson> Cast { get; set; }

        [JsonProperty("crew")]
        public IList<sdProgramPerson> Crew { get; set; }

        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        [JsonProperty("showType")]
        public string ShowType { get; set; }

        //[JsonProperty("recommendations")]
        //public IList<sdProgramRecommendation> Recommendations { get; set; }

        //[JsonProperty("hasImageArtwork")]
        //public bool HasImageArtwork { get; set; }

        //[JsonProperty("hasSeriesArtwork")]
        //public bool HasSeriesArtwork { get; set; }

        //[JsonProperty("hasEpisodeArtwork")]
        //public bool HasEpisodeArtwork { get; set; }

        //[JsonProperty("hasMovieArtwork")]
        //public bool HasMovieArtwork { get; set; }

        //[JsonProperty("hasSportsArtwork")]
        //public bool HasSportsArtwork { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }

    public class sdProgramTitle
    {
        [JsonProperty("title120")]
        public string Title120 { get; set; }
    }

    public class sdProgramEventDetails
    {
        [JsonProperty("venue100")]
        public string Venue100 { get; set; }

        [JsonProperty("teams")]
        public IList<sdProgramEventDetailsTeam> Teams { get; set; }

        [JsonProperty("gameDate")]
        public string GameDate { get; set; }
    }

    public class sdProgramEventDetailsTeam
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isHome")]
        public bool IsHome { get; set; }
    }

    public class sdGenericDescriptions
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("description100")]
        public string Description100 { get; set; }

        [JsonProperty("description1000")]
        public string Description1000 { get; set; }
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
        [JsonProperty("descriptionLanguage")]
        public string DescriptionLanguage { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class sdProgramKeyWords
    {
        [JsonProperty("Mood")]
        public string[] Mood { get; set; }

        [JsonProperty("Time Period")]
        public string[] TimePeriod { get; set; }

        [JsonProperty("Theme")]
        public string[] Theme { get; set; }

        [JsonProperty("Character")]
        public string[] Character { get; set; }

        [JsonProperty("Setting")]
        public string[] Setting { get; set; }

        [JsonProperty("Subject")]
        public string[] Subject { get; set; }

        [JsonProperty("General")]
        public string[] General { get; set; }
    }

    public class sdProgramMetadataProvider
    {
        // TheTVDB only
        [JsonProperty("seriesID")]
        public uint SeriesID { get; set; }

        // TheTVDB only
        [JsonProperty("episodeID")]
        public int EpisodeID { get; set; }

        [JsonProperty("season")]
        public int SeasonNumber { get; set; }

        [JsonProperty("episode")]
        public int EpisodeNumber { get; set; }

        // Gracenote only
        //[JsonProperty("totalEpisodes")]
        //public int TotalEpisodes { get; set; }
    }

    public class sdProgramContentRating
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("contentAdvisory")]
        public string[] ContentAdvisory { get; set; }
    }

    public class sdProgramMovie
    {
        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("qualityRating")]
        public IList<sdProgramQualityRating> QualityRating { get; set; }
    }

    public class sdProgramQualityRating
    {
        [JsonProperty("ratingsBody")]
        public string RatingsBody { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("minRating")]
        public string MinRating { get; set; }

        [JsonProperty("maxRating")]
        public string MaxRating { get; set; }

        [JsonProperty("increment")]
        public string Increment { get; set; }
    }

    public class sdProgramPerson
    {
        [JsonProperty("billingOrder")]
        public string BillingOrder { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        //[JsonProperty("nameId")]
        //public string NameId { get; set; }

        //[JsonProperty("personId")]
        //public string PersonId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("characterName")]
        public string CharacterName { get; set; }
    }

    public class sdProgramRecommendation
    {
        //[JsonProperty("programID")]
        //public string ProgramID { get; set; }

        //[JsonProperty("title120")]
        //public string Title120 { get; set; }
    }
}
