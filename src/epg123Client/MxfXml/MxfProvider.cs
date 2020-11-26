using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfProvider
    {
        /// <summary>
        /// Provides information and copyright about who provided the listing data.
        /// </summary>
        public MxfProvider() { }

        /// <summary>
        /// The name of the supplier of the listings.
        /// The maximum length is 255 characters.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
