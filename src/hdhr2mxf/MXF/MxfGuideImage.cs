﻿using System.Xml.Serialization;

namespace hdhr2mxf.MXF
{
    public class MxfGuideImage
    {
        [XmlIgnore]
        public int Index;

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as i1, i2, i3, and so forth. GuideImage elements are referenced by the Series, SeriesInfo, Program, Affiliate, and Channel elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => ("i" + Index.ToString());
            set { }
        }

        [XmlAttribute("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// The URL of the image.
        /// This value can be in the form of file://URL.
        /// </summary>
        [XmlAttribute("imageUrl")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("format")]
        public string Format { get; set; }

        /// <summary>
        /// The string encoded image
        /// </summary>
        [XmlElement("image")]
        public string Image { get; set; }
    }
}
