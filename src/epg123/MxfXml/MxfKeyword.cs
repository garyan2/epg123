using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfKeyword
    {
        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// The top level keywords use IDs such as k1, k2, k3, and so forth. The second-level keywords have IDs in the corresponding 100s, such as k100 and k101. Keywords are referenced by Program and KeywordGroup elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// The keyword name that is displayed.
        /// The maximum length is 64 characters.
        /// </summary>
        [XmlAttribute("word")]
        public string Word { get; set; }
    }
}
