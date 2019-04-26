using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfProgram
    {
        private string _oad;

        [XmlIgnore]
        public int index;

        [XmlIgnore]
        public sdProgram jsonProgramData;

        [XmlIgnore]
        public string md5;

        [XmlIgnore]
        public string IsPremiere;

        [XmlIgnore]
        public string tmsId { get; set; }

        [XmlIgnore]
        public int _part { get; set; }

        [XmlIgnore]
        public int _parts { get; set; }

        [XmlIgnore]
        public string _newDate { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> contentRatings { get; set; }

        [XmlIgnore]
        public string[] contentAdvisories { get; set; }

        [XmlIgnore]
        public IList<sdImage> programImages;

        /// <summary>
        /// A movie or episode.
        /// Example: An episode of Lost, titled "The others strike".
        /// </summary>
        public MxfProgram() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// This value should be an integer without a letter prefix.
        /// Program elements are referenced by ScheduleEntry elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get
            {
                return index.ToString();
            }
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!Program!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get
            {
                return ("!Program!" + tmsId.Substring(0, 10) + "_" + tmsId.Substring(10));
            }
            set { }
        }

        /// <summary>
        /// The title of the program (for example, Lost). 
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        /// The episode title of the program (for example, The others attack).
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("episodeTitle")]
        public string EpisodeTitle { get; set; }

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
        /// The language of the program.
        /// </summary>
        [XmlAttribute("language")]
        public string Language { get; set; }

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
        public string OriginalAirdate
        {
            get
            {
                if (!string.IsNullOrEmpty(_newDate) && !string.IsNullOrEmpty(_oad) && string.IsNullOrEmpty(IsGeneric))
                {
                    return _newDate;
                }
                else return _oad;
            }
            set
            {
                _oad = value;
            }
        }

        /// <summary>
        /// A comma-delimited list of keyword IDs. This value specifies the Keyword attributes that this program has.
        /// </summary>
        [XmlAttribute("keywords")]
        public string Keywords { get; set; }

        /// <summary>
        /// The ID of the season that this program belongs to, if any.
        /// If this value is not known, do not specify a value.
        /// </summary>
        [XmlAttribute("season")]
        public string Season { get; set; }

        /// <summary>
        /// The ID of the series that this program belongs to, if any.
        /// If this value is not known, do not specify a value.
        /// </summary>
        [XmlAttribute("series")]
        public string Series { get; set; }

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

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isAction")]
        public string IsAction { get; set; }

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
        [XmlAttribute("isHorror")]
        public string IsHorror { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isIndy")]
        public string IsIndy { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("isMusic")]
        public string IsMusic { get; set; }

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
        [XmlAttribute("isProgramEpisodic")]
        public string IsProgramEpisodic { get; set; }

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
        /// Indicates whether the program is a reality show.
        /// </summary>
        [XmlAttribute("isReality")]
        public string IsReality { get; set; }

        /// <summary>
        /// Indicates program is soft/contains generic description.
        /// Use for series episodes that are not known yet.
        /// </summary>
        [XmlAttribute("isGeneric")]
        public string IsGeneric { get; set; }

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
        /// This value is for movies to show any extended synopsis.
        /// </summary>
        [XmlAttribute("hasExtendedSynopsis")]
        public string HasExtendedSynopsis { get; set; }

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
        /// This value contains an image to display for the program.
        /// Contains the value of a GuideImage id attribute. When a program is selected in the UI, the Guide searches for an image to display.The search order is first the program, its season, then its series.
        /// </summary>
        [XmlAttribute("guideImage")]
        public string GuideImage { get; set; }

        [XmlElement("ActorRole")]
        public List<MxfPersonRank> ActorRole { get; set; }

        [XmlElement("WriterRole")]
        public List<MxfPersonRank> WriterRole { get; set; }

        [XmlElement("GuestActorRole")]
        public List<MxfPersonRank> GuestActorRole { get; set; }

        [XmlElement("HostRole")]
        public List<MxfPersonRank> HostRole { get; set; }

        [XmlElement("ProducerRole")]
        public List<MxfPersonRank> ProducerRole { get; set; }

        [XmlElement("DirectorRole")]
        public List<MxfPersonRank> DirectorRole { get; set; }
    }
}
