using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
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
        [DefaultValue(false)]
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
            get => "true";
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
        [DefaultValue(0)]
        public int KeywordType { get; set; }

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

        [XmlArrayItem("Keyword")]
        public List<MxfKeyword> categories { get; set; }
    }

    public class MxfPriorityToken
    {
        [XmlAttribute("priority")]
        public string Priority { get; set; }
    }
}