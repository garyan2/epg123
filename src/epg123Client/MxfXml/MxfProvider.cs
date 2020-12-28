using System.Xml.Serialization;

namespace epg123Client.MxfXml
{
    public class MxfProvider
    {
        /// <summary>
        /// The name of the supplier of the listings.
        /// The maximum length is 255 characters.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}