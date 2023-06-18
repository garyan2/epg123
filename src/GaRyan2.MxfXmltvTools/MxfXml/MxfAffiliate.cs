using System.Collections.Generic;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
    {
        private readonly Dictionary<string, MxfAffiliate> _affiliates = new Dictionary<string, MxfAffiliate>();
        public MxfAffiliate FindOrCreateAffiliate(string affiliateName)
        {
            if (_affiliates.TryGetValue(affiliateName, out var affiliate)) return affiliate;
            With.Affiliates.Add(affiliate = new MxfAffiliate(affiliateName));
            _affiliates.Add(affiliateName, affiliate);
            return affiliate;
        }
    }

    public class MxfAffiliate
    {
        private string _name;
        private string _uid;
        private string _logoImage;

        [XmlIgnore] public MxfGuideImage mxfGuideImage;

        public MxfAffiliate(string name)
        {
            _name = name;
        }
        private MxfAffiliate() { }

        /// <summary>
        /// The display name of the network.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get => _name;
            set { _name = value; }
        }

        /// <summary>
        /// An ID that uniquely identifies the affiliate.
        /// This value should take the form "!Affiliate!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"!Affiliate!{_name}";
            set { _uid = value; }
        }

        /// <summary>
        /// Specifies a network logo to display.
        /// This value contains a GuideImage id attribute.
        /// </summary>
        [XmlAttribute("logoImage")]
        public string LogoImage
        {
            get => _logoImage ?? mxfGuideImage?.Id;
            set { _logoImage = value; }
        }
    }
}