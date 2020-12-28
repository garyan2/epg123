using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfAffiliate
    {
        /// <summary>
        /// The display name of the network.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// An ID that uniquely identifies the affiliate.
        /// This value should take the form "!Affiliate!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!Affiliate!{Name}";
            set { }
        }

        /// <summary>
        /// Specifies a network logo to display.
        /// This value contains a GuideImage id attribute.
        /// </summary>
        [XmlAttribute("logoImage")]
        public string LogoImage { get; set; }
    }
}