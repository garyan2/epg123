using System.Collections.Generic;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfAssembly
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("cultureinfo")]
        public string CultureInfo { get; set; }

        [XmlAttribute("publicKey")]
        public string PublicKey { get; set; }

        [XmlElement("NameSpace")]
        public List<MxfNamespace> Namespace { get; set; }
    }
}