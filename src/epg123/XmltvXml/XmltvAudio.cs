using System;
using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    /// <summary>
    /// <!ELEMENT audio(present?, stereo?)>
    /// <!ELEMENT stereo(#PCDATA)>
    /// </summary>
    public class XmltvAudio
    {
        [XmlElement("present")]
        public string Present { get; set; }

        [XmlElement("stereo")]
        public string Stereo { get; set; }
    }
}
