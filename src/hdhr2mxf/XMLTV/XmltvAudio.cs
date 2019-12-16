using System;
using System.Xml.Serialization;

namespace XmltvXml
{
    /// <summary>
    /// <!ELEMENT audio(present?, stereo?)>
    /// <!ELEMENT stereo(#PCDATA)>
    /// </summary>
    public class XmltvAudio
    {
        [XmlElement("present")]
        public String Present { get; set; }

        [XmlElement("stereo")]
        public String Stereo { get; set; }
    }
}
