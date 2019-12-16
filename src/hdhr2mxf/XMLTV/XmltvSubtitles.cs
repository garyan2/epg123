using System.Xml.Serialization;

namespace XmltvXml
{
    /// <summary>
    /// <!ELEMENT subtitles(language?)>
    /// <!ATTLIST subtitles type(teletext | onscreen | deaf-signed) #IMPLIED>
    /// </summary>
    public class XmltvSubtitles
    {
        [XmlElement("language")]
        public string Language { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
