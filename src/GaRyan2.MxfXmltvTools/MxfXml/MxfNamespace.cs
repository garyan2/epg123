using System.Collections.Generic;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfNamespace
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Type")]
        public List<MxfType> Type { get; set; }
    }
}