using System.Xml.Serialization;

namespace MxfXml
{
    public class MxfSeason
    {
        [XmlIgnore]
        public int index;

        [XmlIgnore]
        public string zap2it;

        /// <summary>
        /// A season of a particular program.
        /// Example: Lost Season 1
        /// </summary>
        public MxfSeason() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as sn1, sn2, and sn3. Seasons are referenced by Program elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get
            {
                return ("sn" + index.ToString());
            }
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!Season!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get
            {
                return ("!Season!" + zap2it + "_" + SeasonNumber);
            }
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
        public string SeasonNumber { get; set; }

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
