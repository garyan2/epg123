using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmltvXml
{
    /// <summary>
    /// <!ELEMENT channel(display-name+, icon*, url*) >
    /// <!ATTLIST channel id CDATA #REQUIRED >
    /// </summary>
    public class XmltvChannel
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("display-name")]
        public List<XmltvText> DisplayNames { get; set; }

        [XmlElement("lcn")]
        public List<XmltvText> Lcn { get; set; }

        [XmlElement("icon")]
        public List<XmltvIcon> Icons { get; set; }

        [XmlElement("url")]
        public List<string> Urls { get; set; }
    }
}
