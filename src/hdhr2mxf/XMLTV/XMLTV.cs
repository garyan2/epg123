using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace XmltvXml
{
    /// <summary>
    /// <!ELEMENT tv(channel*, programme*)>
    /// <!ATTLIST tv date CDATA #IMPLIED
    ///              source-info-url CDATA #IMPLIED
    ///              source-info-name CDATA #IMPLIED
    ///              source-data-url CDATA #IMPLIED
    ///              generator-info-name CDATA #IMPLIED
    ///              generator-info-url CDATA #IMPLIED >
    /// </summary>
    [XmlRoot("tv")]
    public class XMLTV
    {
        [XmlAttribute("date")]
        public string Date { get; set; }

        [XmlAttribute("source-info-url")]
        public string SourceInfoUrl { get; set; }

        [XmlAttribute("source-info-name")]
        public string SourceInfoName { get; set; }

        [XmlAttribute("source-data-url")]
        public string SourceDataUrl { get; set; }

        [XmlAttribute("generator-info-name")]
        public string GeneratorInfoName { get; set; }

        [XmlAttribute("generator-info-url")]
        public string GeneratorInfoUrl { get; set; }

        [XmlElement("channel")]
        public List<XmltvChannel> Channels { get; set; }

        [XmlElement("programme")]
        public List<XmltvProgramme> Programs { get; set; }
    }
}
