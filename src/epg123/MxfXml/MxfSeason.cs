using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfSeason
    {
        [XmlIgnore]
        public int Index;

        [XmlIgnore]
        public string Zap2It;

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as sn1, sn2, and sn3. Seasons are referenced by Program elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"sn{Index}";
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!Season!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!Season!{Zap2It}_{SeasonNumber}";
            set { }
        }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("endAirdate")]
        public string EndAirdate { get; set; }

        /// <summary>
        /// An image to display for this season.
        /// This value contains the GuideImage id attribute.
        /// </summary>
        [XmlAttribute("guideImage")]
        public string GuideImage { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("seasonNumber")]
        public int SeasonNumber { get; set; }
        public bool ShouldSerializeSeasonNumber() { return SeasonNumber != 0; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("startAirdate")]
        public string StartAirdate { get; set; }

        /// <summary>
        /// The series ID to which this season belongs.
        /// This value should be specified because it doesn't make sense for a season to not be part of a series.
        /// </summary>
        [XmlAttribute("series")]
        public string Series { get; set; }

        /// <summary>
        /// The name of this season.
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        /// The name of the studio that created this season.
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("studio")]
        public string Studio { get; set; }

        /// <summary>
        /// The year this season was aired.
        /// </summary>
        [XmlAttribute("year")]
        public string Year { get; set; }
    }
}