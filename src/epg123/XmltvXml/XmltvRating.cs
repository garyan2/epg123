using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    /// <summary>
    /// <!ELEMENT rating(value, icon*)>
    /// <!ATTLIST rating system CDATA #IMPLIED>
    /// </summary>
    public class XmltvRating
    {
        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("icon")]
        public List<XmltvIcon> Icons { get; set; }

        [XmlAttribute("system")]
        public string System { get; set; }
    }
}
