using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using epg123.SchedulesDirectAPI;

namespace epg123.MxfXml
{
    public class MxfSeriesInfo
    {
        private DateTime _seriesStartDate = DateTime.MinValue;
        private DateTime _seriesEndDate = DateTime.MinValue;

        [XmlIgnore]
        public int Index { get; set; }

        [XmlIgnore]
        public IList<sdImage> SeriesImages { get; set; }

        /// <summary>
        /// 8 digit number unique to the series
        /// </summary>
        [XmlIgnore]
        public string TmsSeriesId { get; set; }

        /// <summary>
        /// The seried ID value for this series from thetvdb.com
        /// </summary>
        [XmlIgnore]
        public string TvdbSeriesId { get; set; }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as si1, si2, and si3. SeriesInfo is referenced by the Program and Season elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"si{Index}";
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This value starts with "!Series!". Using the format "!Series!seriesName" is somewhat unique, but won't handle cases such as a remake of a series (for example, Knight Rider or Battlestar Galactica).
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!Series!{TmsSeriesId}";
            set { }
        }

        /// <summary>
        /// The title name of the series.
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        /// A shorter form of the title attribute (if available).
        /// The maximum length is 512 characters. If this value is not available, use the same value as the title attribute.
        /// </summary>
        [XmlAttribute("shortTitle")]
        public string ShortTitle { get; set; }

        /// <summary>
        /// A description of the series.
        /// The maximum length is 512 characters.
        /// </summary>
        //private string _description;
        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        /// A shorter form of the description attribute, if available.
        /// The maximum length is 512 characters. If this value is not available, use the same value as the description attribute.
        /// </summary>
        [XmlAttribute("shortDescription")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// The date the series was first aired.
        /// </summary>
        [XmlAttribute("startAirdate")]
        public string StartAirdate
        {
            get => _seriesStartDate != DateTime.MinValue ? _seriesStartDate.ToString("yyyy-MM-dd") : null;
            set => _seriesStartDate = DateTime.Parse(value);
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("endAirdate")]
        public string EndAirdate
        {
            get => _seriesEndDate != DateTime.MinValue? _seriesEndDate.ToString("yyyy-MM-dd") : null;
            set => _seriesEndDate = DateTime.Parse(value);
        }

        /// <summary>
        /// An image to show for the series.
        /// This value contains the GuideImage id attribute. 
        /// </summary>
        [XmlAttribute("guideImage")]
        public string GuideImage { get; set; }

        /// <summary>
        /// The name of the studio that created this season.
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("studio")]
        public string Studio { get; set; }
    }
}