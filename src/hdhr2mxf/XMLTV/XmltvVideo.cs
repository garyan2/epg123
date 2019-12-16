using System;
using System.Xml.Serialization;

namespace XmltvXml
{
    /// <summary>
    /// <!ELEMENT video(present?, colour?, aspect?, quality?)>
    /// <!ELEMENT present(#PCDATA)>
    /// <!ELEMENT colour (#PCDATA)>
    /// <!ELEMENT aspect (#PCDATA)>
    /// <!ELEMENT quality (#PCDATA)>
    /// </summary>
    public class XmltvVideo
    {
        [XmlElement("present")]
        public string Present { get; set; }

        [XmlElement("colour")]
        public string Colour { get; set; }

        [XmlElement("aspect")]
        public string Aspect { get; set; }

        [XmlElement("quality")]
        public string Quality { get; set; }
    }
}
