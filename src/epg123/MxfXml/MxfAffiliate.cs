using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public partial class Mxf
    {
        private readonly Dictionary<string, MxfAffiliate> _affiliates = new Dictionary<string, MxfAffiliate>();
        public MxfAffiliate GetAffiliate(string affiliateName)
        {
            if (_affiliates.TryGetValue(affiliateName, out var affiliate)) return affiliate;
            With.Affiliates.Add(affiliate = new MxfAffiliate
            {
                Name = affiliateName
            });
            _affiliates.Add(affiliateName, affiliate);
            return affiliate;
        }
    }

    public class MxfAffiliate
    {
        public override string ToString() { return Uid; }

        [XmlIgnore] public MxfGuideImage mxfGuideImage;

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
        public string LogoImage
        {
            get => mxfGuideImage?.ToString();
            set { }
        }
    }
}