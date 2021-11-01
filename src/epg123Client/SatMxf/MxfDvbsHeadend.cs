using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsHeadend
    {
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!DvbsDataSet!DvbsHeadend[{CsiId}]";
            set { }
        }
        public bool ShouldSerializeUid() => string.IsNullOrEmpty(IdRef);

        [XmlAttribute("_csiId")]
        public int CsiId { get; set; }

        [XmlAttribute("_languageIso639")]
        public string LanguageIso639 { get; set; }

        [XmlAttribute("idref")]
        public string IdRef { get; set; }

        [XmlArrayItem("DvbsChannel")]
        public List<MxfDvbsChannel> _channels { get; set; }
    }
}
