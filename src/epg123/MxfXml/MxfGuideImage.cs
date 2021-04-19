using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public partial class Mxf
    {
        private readonly Dictionary<string, MxfGuideImage> _guideImages = new Dictionary<string, MxfGuideImage>();
        public MxfGuideImage GetGuideImage(string pathname, string image = null)
        {
            if (_guideImages.TryGetValue(pathname, out var guideImage)) return guideImage;
            With.GuideImages.Add(guideImage = new MxfGuideImage
            {
                Index = With.GuideImages.Count + 1,
                ImageUrl = pathname,
                Image = image
            });
            _guideImages.Add(pathname, guideImage);
            return guideImage;
        }
    }

    public class MxfGuideImage
    {
        public override string ToString() { return Id; }

        [XmlIgnore] public int Index;

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as i1, i2, i3, and so forth. GuideImage elements are referenced by the Series, SeriesInfo, Program, Affiliate, and Channel elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"i{Index}";
            set { }
        }

        /// <summary>
        /// Used for device group image only
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// The URL of the image.
        /// This value can be in the form of file://URL.
        /// </summary>
        [XmlAttribute("imageUrl")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Undocumented
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