using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GaRyan2.SchedulesDirectAPI
{
    public class Programme : BaseResponse
    {
        [JsonProperty("programID")]
        public string ProgramId { get; set; }

        [JsonProperty("resourceID"), JsonIgnore]
        public string ResourceId { get; set; }

        [JsonProperty("titles")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramTitle>))]
        public List<ProgramTitle> Titles { get; set; }

        [JsonProperty("episodeTitle150")]
        public string EpisodeTitle150 { get; set; }

        [JsonProperty("descriptions")]
        public ProgramDescriptions Descriptions { get; set; }

        [JsonProperty("eventDetails")]
        public ProgramEventDetails EventDetails { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, ProgramMetadataProvider>[] Metadata { get; set; }

        [JsonProperty("originalAirDate")]
        public DateTime OriginalAirDate { get; set; }
        public bool ShouldSerializeOriginalAirDate() => OriginalAirDate.Ticks > 0;

        [JsonProperty("duration")]
        [DefaultValue(0)]
        public int Duration { get; set; }

        [JsonProperty("movie")]
        public ProgramMovie Movie { get; set; }

        [JsonProperty("genres")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Genres { get; set; }

        [JsonProperty("officialURL")]
        public string OfficialUrl { get; set; }

        [JsonProperty("contentRating")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramContentRating>))]
        public List<ProgramContentRating> ContentRating { get; set; }

        [JsonProperty("contentAdvisory")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] ContentAdvisory { get; set; }

        [JsonProperty("cast")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramPerson>))]
        public List<ProgramPerson> Cast { get; set; }

        [JsonProperty("crew")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramPerson>))]
        public List<ProgramPerson> Crew { get; set; }

        /// <summary>
        /// program type; one of following values;
        /// Show, Episode, Sports, Movie
        /// </summary>
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        /// <summary>
        /// program subtype; one of following values:
        /// Feature Film, Short Film, TV Movie, Miniseries, Series, Special, Sports event, Sports non-event, Paid Programming, Theatre Event, TBA, Off Air
        /// </summary>
        [JsonProperty("showType")]
        public string ShowType { get; set; }

        [JsonProperty("episodeImage")]
        public string EpisodeImage { get; set; }

        [JsonProperty("hasImageArtwork")]
        public bool HasImageArtwork { get; set; }

        [JsonProperty("hasSeriesArtwork")]
        public bool HasSeriesArtwork { get; set; }

        [JsonProperty("hasSeasonArtwork")]
        public bool HasSeasonArtwork { get; set; }

        [JsonProperty("hasEpisodeArtwork")]
        public bool HasEpisodeArtwork { get; set; }

        [JsonProperty("hasMovieArtwork")]
        public bool HasMovieArtwork { get; set; }

        [JsonProperty("hasSportsArtwork")]
        public bool HasSportsArtwork { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }

    public class ProgramTitle
    {
        [JsonProperty("title120")]
        public string Title120 { get; set; }

        [JsonProperty("titleLanguage")]
        public string TitleLanguage { get; set; }
    }

    public class ProgramEventDetails
    {
        [JsonProperty("venue100")]
        public string Venue100 { get; set; }

        [JsonProperty("teams")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramEventDetailsTeam>))]
        public List<ProgramEventDetailsTeam> Teams { get; set; }

        [JsonProperty("gameDate")]
        public DateTime GameDate { get; set; }
        public bool ShouldSerializeGameDate() => GameDate.Ticks > 0;
    }

    public class ProgramEventDetailsTeam
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isHome")]
        public bool IsHome { get; set; }
    }


    public class ProgramDescriptions
    {
        [JsonProperty("description100")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramDescription>))]
        public List<ProgramDescription> Description100 { get; set; }

        [JsonProperty("description1000")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramDescription>))]
        public List<ProgramDescription> Description1000 { get; set; }
    }

    public class ProgramDescription
    {
        [JsonProperty("descriptionLanguage")]
        public string DescriptionLanguage { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class ProgramKeyWords
    {
        [JsonProperty("Mood")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Mood { get; set; }

        [JsonProperty("Time Period")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] TimePeriod { get; set; }

        [JsonProperty("Theme")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Theme { get; set; }

        [JsonProperty("Character")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Character { get; set; }

        [JsonProperty("Setting")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Setting { get; set; }

        [JsonProperty("Subject")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Subject { get; set; }

        [JsonProperty("General")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] General { get; set; }
    }

    public class ProgramMetadataProvider
    {
        [JsonProperty("seriesID")]
        public uint SeriesId { get; set; }

        [JsonProperty("episodeID")]
        public int EpisodeId { get; set; }

        [JsonProperty("season")]
        public int SeasonNumber { get; set; }

        [JsonProperty("episode")]
        public int EpisodeNumber { get; set; }

        [JsonProperty("totalEpisodes")]
        public int TotalEpisodes { get; set; }

        [JsonProperty("totalSeasons")]
        public int TotalSeasons { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class ProgramContentRating
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("contentAdvisory")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] ContentAdvisory { get; set; }
    }

    public class ProgramMovie
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("qualityRating")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramQualityRating>))]
        public List<ProgramQualityRating> QualityRating { get; set; }
    }

    public class ProgramQualityRating
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

    public class ProgramPerson
    {
        [JsonProperty("billingOrder")]
        public string BillingOrder { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("nameId")]
        public string NameId { get; set; }

        [JsonProperty("personId")]
        public string PersonId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("characterName")]
        public string CharacterName { get; set; }
    }

    public class ProgramRecommendation
    {
        [JsonProperty("programID")]
        public string ProgramId { get; set; }

        [JsonProperty("title120")]
        public string Title120 { get; set; }
    }

    public class ProgramAward
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("awardName")]
        public string AwardName { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [JsonProperty("personId")]
        public string PersonId { get; set; }

        [JsonProperty("won")]
        public bool Won { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}