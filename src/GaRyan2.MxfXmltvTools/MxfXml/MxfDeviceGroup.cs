using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDeviceGroup
    {
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("lastConfigurationChange")]
        public string LastConfigurationChange { get; set; }

        [XmlAttribute("rank")]
        public string Rank { get; set; }

        [XmlAttribute("permitAnyDeviceType")]
        public string PermitAnyDeviceType { get; set; }

        [XmlAttribute("isEnabled")]
        public string IsEnabled { get; set; }

        [XmlAttribute("firstRunProcessId")]
        public string FirstRunProcessId { get; set; }

        [XmlAttribute("onlyShowDynamicLineups")]
        public string OnlyShowDynamicLineups { get; set; }

        [XmlElement("guideImage")]
        public MxfGuideImage GuideImage { get; set; }
    }
}