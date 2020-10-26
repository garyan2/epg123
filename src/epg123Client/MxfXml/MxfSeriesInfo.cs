using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace epg123.MxfXml
{
    public class MxfSeriesInfo
    {
        /// <summary>
        /// A series of a particular program.
        /// Example: Lost
        /// </summary>
        public MxfSeriesInfo() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as si1, si2, and si3. SeriesInfo is referenced by the Program and Season elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This value starts with "!Series!". Using the format "!Series!seriesName" is somewhat unique, but won't handle cases such as a remake of a series (for example, Knight Rider or Battlestar Galactica).
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid { get; set; }

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
        public string StartAirdate { get; set; }

        /// <summary>
        /// The date the series ended.
        /// </summary>
        //[XmlAttribute("endAirdate")]
        //public string EndAirdate { get; set; }

        /// <summary>
        /// An image to show for the series.
        /// This value contains the GuideImage id attribute. 
        /// </summary>
        [XmlAttribute("guideImage")]
        public string GuideImage { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        //[XmlAttribute("studio")]
        //public string Studio { get; set; }
    }
}