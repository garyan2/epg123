using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace epg123Transfer.MxfXml
{
    [XmlRoot("MXF")]
    public class MXF
    {
        [XmlElement("Assembly")]
        public List<MxfAssembly> Assembly { get; set; }

        [XmlElement("OneTimeRequest")]
        public List<MxfRequest> OneTimeRequest { get; set; }

        [XmlElement("ManualRequest")]
        public List<MxfRequest> ManualRequest { get; set; }

        [XmlElement("SeriesRequest")]
        public List<MxfRequest> SeriesRequest { get; set; }

        [XmlElement("WishListRequest")]
        public List<MxfRequest> WishListRequest { get; set; }
    }

    public class MxfAssembly
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("cultureInfo")]
        public string CultureInfo { get; set; }

        [XmlAttribute("publicKey")]
        public string PublicKey { get; set; }

        [XmlElement("NameSpace")]
        public List<MxfNamespace> Namespace { get; set; }
    }

    public class MxfNamespace
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Type")]
        public List<MxfType> Type { get; set; }
    }

    public class MxfType
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("groupName")]
        public string GroupName { get; set; }

        [XmlAttribute("parentFieldName")]
        public string ParentFieldName { get; set; }
    }

    public class MxfRequest
    {
        [XmlAttribute("prototypicalProgram")]
        public string PrototypicalProgram { get; set; }

        [XmlAttribute("prototypicalService")]
        public string PrototypicalService { get; set; }

        [XmlAttribute("channel")]
        public string Channel { get; set; }

        [XmlAttribute("series")]
        public string SeriesAttribute { get; set; }

        [XmlAttribute("creationTime")]
        public string CreationTime { get; set; }

        [XmlAttribute("sourceName")]
        public string SourceName { get; set; }

        [XmlAttribute("complete")]
        public bool Complete { get; set; }

        [XmlAttribute("prototypicalStartTime")]
        public DateTime PrototypicalStartTime { get; set; }

        [XmlAttribute("prototypicalDuration")]
        public string PrototypicalDuration { get; set; }

        [XmlAttribute("prototypicalLanguage")]
        public string PrototypicalLanguage { get; set; }

        [XmlAttribute("prototypicalTitle")]
        public string PrototypicalTitle { get; set; }

        [XmlAttribute("prototypicalIsHdtv")]
        public string PrototypicalIsHdtv { get; set; }

        [XmlAttribute("prototypicalChannelNumber")]
        public string PrototypicalChannelNumber { get; set; }

        [XmlAttribute("anyChannel")]
        public string AnyChannel { get; set; }

        [XmlAttribute("anyLanguage")]
        public string AnyLanguage
        {
            get
            {
                return "true";
            }
            set { }
        }

        [XmlAttribute("contentQualityPreference")]
        public string ContentQualityPreference { get; set; }

        [XmlAttribute("scheduleLimit")]
        public string ScheduleLimit { get; set; }

        [XmlAttribute("tooManyScheduled")]
        public string TooManyScheduled { get; set; }

        [XmlAttribute("sourceTypeFilter")]
        public string SourceTypeFilter { get; set; }

        [XmlAttribute("prePaddingRequired")]
        public string PrePaddingRequired { get; set; }

        [XmlAttribute("prePaddingRequested")]
        public string PrePaddingRequested { get; set; }

        [XmlAttribute("postPaddingRequired")]
        public string PostPaddingRequired { get; set; }

        [XmlAttribute("postPaddingRequested")]
        public string PostPaddingRequested { get; set; }

        [XmlAttribute("keepLength")]
        public string KeepLength { get; set; }

        [XmlAttribute("quality")]
        public string Quality { get; set; }

        [XmlAttribute("isRecurring")]
        public string IsRecurring { get; set; }

        [XmlAttribute("recordingLimit")]
        public string RecordingLimit { get; set; }

        [XmlAttribute("runType")]
        public string RunType { get; set; }

        [XmlAttribute("anyTime")]
        public string AnyTime { get; set; }

        [XmlAttribute("dayOfWeekMask")]
        public string DayOfWeekMask { get; set; }

        [XmlAttribute("airtime")]
        public string Airtime { get; set; }

        [XmlAttribute("airtimeValid")]
        public string AirtimeValid { get; set; }

        [XmlAttribute("keywords")]
        public string Keywords { get; set; }

        [XmlAttribute("exactKeywordMatch")]
        public string ExactKeywordMatch { get; set; }

        [XmlAttribute("keywordType")]
        public string KeywordType { get; set; }

        [XmlAttribute("episodeTitle")]
        public string EpisodeTitle { get; set; }

        [XmlAttribute("year")]
        public string Year { get; set; }

        [XmlAttribute("earliestToSchedule")]
        public string EarliestToSchedule { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("titleTemplate")]
        public string TitleTemplate { get; set; }

        [XmlAttribute("episodeTitleTemplate")]
        public string EpisodeTitleTemplate { get; set; }

        [XmlAttribute("descriptionTemplate")]
        public string DescriptionTemplate { get; set; }

        [XmlAttribute("cultureId")]
        public string CultureId { get; set; }

        [XmlAttribute("lastScheduled")]
        public string LastScheduled { get; set; }

        [XmlElement("priorityToken")]
        public MxfPriorityToken PriorityToken { get; set; }

        [XmlElement("series")]
        public MxfSeries SeriesElement { get; set; }

        [XmlElement("prototypicalProgram")]
        public MxfProgram PrototypicalProgramElement { get; set; }
    }

    public class MxfPriorityToken
    {
        [XmlAttribute("priority")]
        public string Priority { get; set; }
    }

    public class MxfProgram
    {
        /// <summary>
        /// The title of the program (for example, Lost). 
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        /// The description of this program.
        /// The maximum length is 2048 characters.
        /// </summary>
        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        /// A shorter form of the description attribute, if available.
        /// The maximum length is 512 characters. If a short description is not available, do not specify a value.
        /// </summary>
        [XmlAttribute("shortDescription")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// The episode title of the program (for example, The others attack).
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("episodeTitle")]
        public string EpisodeTitle { get; set; }

        [XmlAttribute("movieId")]
        public string MovieId { get; set; }

        /// <summary>
        /// The language of the program.
        /// </summary>
        [XmlAttribute("language")]
        public string Language { get; set; }

        [XmlAttribute("movieIdLookupHash")]
        public string MovieIdLookupHash { get; set; }

        /// <summary>
        /// The year the program was created.
        /// If unknown, this value is 0.
        /// </summary>
        [XmlAttribute("year")]
        public string Year { get; set; }

        /// <summary>
        /// The season number of the program (for example, 1). 
        /// If unknown, this value is 0.
        /// </summary>
        [XmlAttribute("seasonNumber")]
        public string SeasonNumber { get; set; }

        /// <summary>
        /// The episode number of the program in the season.
        /// If unknown, this value is 0.
        /// </summary>
        [XmlAttribute("episodeNumber")]
        public string EpisodeNumber { get; set; }

        /// <summary>
        /// The original air date (in local time) of the program. Use this value to determine whether this program is a repeat.
        /// </summary>
        [XmlAttribute("originalAirdate")]
        public string OriginalAirdate { get; set; }

        [XmlAttribute("wdsTimestamp")]
        public string WdsTimestamp { get; set; }

        /// <summary>
        /// The star rating of the program. 
        /// Each star equals two points. For example, a value of "3" is equal to 1.5 stars.
        /// </summary>
        [XmlAttribute("halfStars")]
        public string HalfStars { get; set; }

        /// <summary>
        /// The MPAA movie rating.
        /// Possible values are:
        /// 0 = Unknown
        /// 1 = G
        /// 2 = PG
        /// 3 = PG13
        /// 4 = R
        /// 5 = NC17
        /// 6 = X
        /// 7 = NR
        /// 8 = AO
        /// </summary>
        [XmlAttribute("mpaaRating")]
        public string MpaaRating { get; set; }

        [XmlAttribute("isBroadbandAvailable")]
        public string IsBroadbandAvailable { get; set; }

        /// <summary>
        /// Indicates whether the program is a movie.
        /// This value determines whether the program appears in the Movies category of the Guide grid and in other movie-related locations.
        /// </summary>
        [XmlAttribute("isMovie")]
        public string IsMovie { get; set; }

        /// <summary>
        /// Indicates whether the program is a miniseries.
        /// </summary>
        [XmlAttribute("isMiniseries")]
        public string IsMiniseries { get; set; }

        /// <summary>
        /// Indicates whether the program is a limited series.
        /// </summary>
        [XmlAttribute("isLimitedSeries")]
        public string IsLimitedSeries { get; set; }

        /// <summary>
        /// Indicates whether the program is paid programming.
        /// </summary>
        [XmlAttribute("isPaidProgramming")]
        public string IsPaidProgramming { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isProgramEpisodic")]
        public string IsProgramEpisodic { get; set; }

        /// <summary>
        /// Indicates program is soft/contains generic description.
        /// Use for series episodes that are not known yet.
        /// </summary>
        [XmlAttribute("isGeneric")]
        public string IsGeneric { get; set; }

        /// <summary>
        /// Indicates whether the program is a serial.
        /// </summary>
        [XmlAttribute("isSerial")]
        public string IsSerial { get; set; }

        /// <summary>
        /// Indicates whether the program is part of a series.
        /// </summary>
        [XmlAttribute("isSeries")]
        public string IsSeries { get; set; }

        /// <summary>
        /// Indicates whether the program is a short film.
        /// </summary>
        [XmlAttribute("isShortFilm")]
        public string IsShortFilm { get; set; }

        /// <summary>
        /// Indicates whether the program is a special.
        /// This value is used in the Guide's grid view categories.
        /// </summary>
        [XmlAttribute("isSpecial")]
        public string IsSpecial { get; set; }

        /// <summary>
        /// Indicates whether the program is a sports program.
        /// This value is used in the Guide's grid view categories.
        /// </summary>
        [XmlAttribute("isSports")]
        public string IsSports { get; set; }

        /// <summary>
        /// Indicates whether the program is a news show.
        /// This value is used in the Guide's grid view categories.
        /// </summary>
        [XmlAttribute("isNews")]
        public string IsNews { get; set; }

        /// <summary>
        /// Indicates whether the program is for children.
        /// This value is used in the Guide's grid view categories.
        /// </summary>
        [XmlAttribute("isKids")]
        public string IsKids { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isAction")]
        public string IsAction { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isHorror")]
        public string IsHorror { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isMusic")]
        public string IsMusic { get; set; }

        /// <summary>
        /// Indicates whether the program is a reality show.
        /// </summary>
        [XmlAttribute("isReality")]
        public string IsReality { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isRomance")]
        public string IsRomance { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isScienceFiction")]
        public string IsScienceFiction { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isSoap")]
        public string IsSoap { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isThriller")]
        public string IsThriller { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isComedy")]
        public string IsComedy { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isDocumentary")]
        public string IsDocumentary { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isDrama")]
        public string IsDrama { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isEducational")]
        public string IsEducational { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isIndy")]
        public string IsIndy { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isSeriesPremiere")]
        public string IsSeriesPremiere { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isSeriesFinale")]
        public string IsSeriesFinale { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isSeasonPremiere")]
        public string IsSeasonPremiere { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isSeasonFinale")]
        public string IsSeasonFinale { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasAdult")]
        public string HasAdult { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasBriefNudity")]
        public string HasBriefNudity { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasGraphicLanguage")]
        public string HasGraphicLanguage { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasGraphicViolence")]
        public string HasGraphicViolence { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasLanguage")]
        public string HasLanguage { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasMildViolence")]
        public string HasMildViolence { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasNudity")]
        public string HasNudity { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasRape")]
        public string HasRape { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasStrongSexualContent")]
        public string HasStrongSexualContent { get; set; }

        /// <summary>
        /// This value indicates the reason for the MPAA rating.
        /// </summary>
        [XmlAttribute("hasViolence")]
        public string HasViolence { get; set; }

        /// <summary>
        /// This value is for movies to show the extended cast and crew information.
        /// </summary>
        [XmlAttribute("hasExtendedCastAndCrew")]
        public string HasExtendedCastAndCrew { get; set; }

        /// <summary>
        /// This value is for movies to show any review available.
        /// </summary>
        [XmlAttribute("hasReview")]
        public string HasReview { get; set; }

        /// <summary>
        /// This value is for movies to show any similar movies.
        /// </summary>
        [XmlAttribute("hasSimilarPrograms")]
        public string HasSimilarPrograms { get; set; }

        /// <summary>
        /// This value is for movies to show any extended synopsis.
        /// </summary>
        [XmlAttribute("hasExtendedSynopsis")]
        public string HasExtendedSynopsis { get; set; }

        [XmlAttribute("hasOnDemand")]
        public string HasOnDemand { get; set; }
    }

    public class MxfSeries
    {
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("shortTitle")]
        public string ShortTitle { get; set; }

        [XmlAttribute("description")]
        public string DescriptionAttribute { get; set; }

        [XmlAttribute("shortDescription")]
        public string ShortDescriptionAttribute { get; set; }

        [XmlAttribute("studio")]
        public string Studio { get; set; }

        [XmlAttribute("startAirdate")]
        public string StartAirdate { get; set; }

        [XmlAttribute("endAirdate")]
        public string EndAirdate { get; set; }

        [XmlAttribute("year")]
        public string Year { get; set; }

        [XmlElement("description")]
        public string DescriptionElement { get; set; }

        [XmlElement("shortDescription")]
        public string ShortDescriptionElement { get; set; }
    }
}