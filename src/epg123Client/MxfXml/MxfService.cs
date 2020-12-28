using System.Xml.Serialization;

namespace epg123Client.MxfXml
{
    public class MxfService
    {
        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as s1, s2, s3, and so forth.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// An ID that uniquely identifies the service.
        /// Should be of the form "!Service!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// The call sign of the service.
        /// For example, "BBC1".
        /// </summary>
        [XmlAttribute("callSign")]
        public string CallSign { get; set; }
    }
}