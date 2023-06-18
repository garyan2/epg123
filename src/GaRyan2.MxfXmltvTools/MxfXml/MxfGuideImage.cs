using System.Collections.Generic;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
    {
        private readonly Dictionary<string, MxfGuideImage> _guideImages = new Dictionary<string, MxfGuideImage>();
        public MxfGuideImage FindOrCreateGuideImage(string pathname, string image = null)
        {
            if (_guideImages.TryGetValue(pathname, out var guideImage)) return guideImage;
            With.GuideImages.Add(guideImage = new MxfGuideImage(With.GuideImages.Count + 1, pathname, image));
            _guideImages.Add(pathname, guideImage);
            return guideImage;
        }
    }

    public class MxfGuideImage
    {
        private int _index;
        private string _imageUrl;
        private string _encodedImage;

        public MxfGuideImage(int index, string pathName, string encodedImage)
        {
            _index = index;
            _imageUrl = pathName;
            _encodedImage = encodedImage;
        }
        private MxfGuideImage() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as i1, i2, i3, and so forth. GuideImage elements are referenced by the Series, SeriesInfo, Program, Affiliate, and Channel elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"i{_index}";
            set { _index = int.Parse(value.Substring(1)); }
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
        public string ImageUrl
        {
            get => _imageUrl;
            set { _imageUrl = value; }
        }

        /// <summary>
        /// Undocumented
        /// </summary>
        [XmlAttribute("format")]
        public string Format { get; set; }

        /// <summary>
        /// The string encoded image
        /// </summary>
        [XmlElement("image")]
        public string Image
        {
            get => _encodedImage;
            set { _encodedImage = value; }
        }
    }
}