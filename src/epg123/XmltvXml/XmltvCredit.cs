using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    /// <summary>
    /// <!ELEMENT credits(director*, actor*, writer*, adapter*, producer*,
    ///                   composer*, editor*, presenter*, commentator*,
    ///                   guest* )>
    /// <!ELEMENT director    (#PCDATA)>
    /// <!ELEMENT actor       (#PCDATA)>
    /// <!ATTLIST actor role  CDATA #IMPLIED>
    /// <!ELEMENT writer      (#PCDATA)>
    /// <!ELEMENT adapter     (#PCDATA)>
    /// <!ELEMENT producer    (#PCDATA)>
    /// <!ELEMENT composer    (#PCDATA)>
    /// <!ELEMENT editor      (#PCDATA)>
    /// <!ELEMENT presenter   (#PCDATA)>
    /// <!ELEMENT commentator (#PCDATA)>
    /// <!ELEMENT guest       (#PCDATA)>
    /// </summary>
    public class XmltvCredit
    {
        [XmlElement("director")]
        public List<string> Directors { get; set; }

        [XmlElement("actor")]
        public List<XmltvActor> Actors { get; set; }

        [XmlElement("writer")]
        public List<string> Writers { get; set; }

        [XmlElement("adapter")]
        public List<string> Adapters { get; set; }

        [XmlElement("producer")]
        public List<string> Producers { get; set; }

        [XmlElement("composer")]
        public List<string> Composers { get; set; }

        [XmlElement("editor")]
        public List<string> Editors { get; set; }

        [XmlElement("presenter")]
        public List<string> Presenters { get; set; }

        [XmlElement("commentator")]
        public List<string> Commentators { get; set; }

        [XmlElement("guest")]
        public List<string> Guests { get; set; }
    }
}
