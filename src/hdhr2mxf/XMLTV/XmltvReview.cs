using System.Xml.Serialization;

namespace XmltvXml
{
    /// <summary>
    /// <!ELEMENT review(#PCDATA)>
    /// <!ATTLIST review type     (text | url) #REQUIRED
    ///                  source   CDATA        #IMPLIED
    ///                  reviewer CDATA        #IMPLIED
    ///                  lang     CDATA        #IMPLIED>
    /// </summary>
    public class XmltvReview
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlAttribute("reviewer")]
        public string Reviewer { get; set; }

        [XmlAttribute("lang")]
        public string Lang { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
