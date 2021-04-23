using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static List<Program> GetPrograms(string[] request)
        {
            var dtStart = DateTime.Now;
            var sr = GetRequestResponse(methods.POST, "programs", request);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for {request.Length,4} program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved {request.Length,4} program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<List<Program>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetPrograms() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class Program : BaseResponse
    {
        [JsonProperty("programID")]
        public string ProgramId { get; set; }

        [JsonProperty("resourceID")]
        public string ResourceId { get; set; }
        public bool ShouldSerializeResourceId() { return false; }

        [JsonProperty("titles")]
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
        public DateTime OriginalAirDate { get; set; } = DateTime.MinValue;
        public bool ShouldSerializeOriginalAirDate() { return OriginalAirDate != DateTime.MinValue; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
        public bool ShouldSerializeDuration() { return Duration > 0; }

        [JsonProperty("movie")]
        public ProgramMovie Movie { get; set; }

        [JsonProperty("genres")]
        public string[] Genres { get; set; }

        [JsonProperty("officialURL")]
        public string OfficialUrl { get; set; }
        public bool ShouldSerializeOfficialUrl() { return false; }

        [JsonProperty("keyWords")]
        public ProgramKeyWords KeyWords { get; set; }

        [JsonProperty("contentRating")]
        public List<ProgramContentRating> ContentRating { get; set; }

        [JsonProperty("contentAdvisory")]
        public string[] ContentAdvisory { get; set; }

        [JsonProperty("cast")]
        public List<sdProgramPerson> Cast { get; set; }

        [JsonProperty("crew")]
        public List<sdProgramPerson> Crew { get; set; }

        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        [JsonProperty("showType")]
        public string ShowType { get; set; }

        [JsonProperty("episodeImage")]
        public string EpisodeImage { get; set; }

        [JsonProperty("recommendations")]
        public List<sdProgramRecommendation> Recommendations { get; set; }
        public bool ShouldSerializeRecommendations() { return false; }

        [JsonProperty("hasImageArtwork")]
        public bool HasImageArtwork { get; set; }
        public bool ShouldSerializeHasImageArtwork() { return HasImageArtwork; }

        [JsonProperty("hasSeriesArtwork")]
        public bool HasSeriesArtwork { get; set; }
        public bool ShouldSerializeHasSeriesArtwork() { return HasSeriesArtwork; }

        [JsonProperty("hasSeasonArtwork")]
        public bool HasSeasonArtwork { get; set; }
        public bool ShouldSerializeHasSeasonArtwork() { return HasSeasonArtwork; }

        [JsonProperty("hasEpisodeArtwork")]
        public bool HasEpisodeArtwork { get; set; }
        public bool ShouldSerializeHasEpisodeArtwork() { return HasEpisodeArtwork; }

        [JsonProperty("hasMovieArtwork")]
        public bool HasMovieArtwork { get; set; }
        public bool ShouldSerializeHasMovieArtwork() { return HasMovieArtwork; }
        
        [JsonProperty("hasSportsArtwork")]
        public bool HasSportsArtwork { get; set; }
        public bool ShouldSerializeHasSportsArtwork() { return HasSportsArtwork; }

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
        public List<ProgramEventDetailsTeam> Teams { get; set; }

        [JsonProperty("gameDate")]
        public DateTime GameDate { get; set; } = DateTime.MinValue;
        public bool ShouldSerializeGameDate() { return GameDate != DateTime.MinValue; }
    }

    public class ProgramEventDetailsTeam
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isHome")]
        public bool IsHome { get; set; }
        public bool ShouldSerializeIsHome() { return IsHome; }
    }


    public class ProgramDescriptions
    {
        [JsonProperty("description100")]
        public List<ProgramDescription> Description100 { get; set; }

        [JsonProperty("description1000")]
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

    public class ProgramMetadataProvider
    {
        [JsonProperty("seriesID")]
        public uint SeriesId { get; set; }
        public bool ShouldSerializeSeriesId() { return SeriesId > 0; }

        [JsonProperty("episodeID")]
        public int EpisodeId { get; set; }
        public bool ShouldSerializeEpisodeId() { return EpisodeId > 0; }

        [JsonProperty("season")]
        public int SeasonNumber { get; set; }
        public bool ShouldSerializeSeasonNumber() { return SeasonNumber > 0; }

        [JsonProperty("episode")]
        public int EpisodeNumber { get; set; }
        public bool ShouldSerializeEpisodeNumber() { return EpisodeNumber > 0; }

        [JsonProperty("totalEpisodes")]
        public int TotalEpisodes { get; set; }
        public bool ShouldSerializeTotalEpisodes() { return TotalEpisodes > 0; }

        [JsonProperty("totalSeasons")]
        public int TotalSeasons { get; set; }
        public bool ShouldSerializeTotalSeasons() { return TotalSeasons > 0; }

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
        public string[] ContentAdvisory { get; set; }
    }

    public class ProgramMovie
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
        public bool ShouldSerializeDuration() { return Duration > 0; }

        [JsonProperty("qualityRating")]
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

    public class sdProgramPerson
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

    public class sdProgramRecommendation
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
        public bool ShouldSerializeWon() { return Won; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}
