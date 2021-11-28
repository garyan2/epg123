using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public partial class Mxf
    {
        public MxfDvbsHeadend GetOrCreateHeadend(int csiId)
        {
            var headend = DvbsDataSet._allHeadends.SingleOrDefault(arg => arg.CsiId == csiId);
            if (headend != null) return headend;

            headend = new MxfDvbsHeadend
            {
                CsiId = csiId,
                _channels = new List<MxfDvbsChannel>()
            };
            DvbsDataSet._allHeadends.Add(headend);
            return headend;
        }

        public void AddReferenceHeadend(MxfDvbsHeadend headend)
        {
            var refHead = DvbsDataSet._allHeadends.SingleOrDefault(arg => arg.IdRef?.Equals(headend.Uid) ?? false);
            if (refHead != null) return;

            DvbsDataSet._allHeadends.Add(new MxfDvbsHeadend
            {
                IdRef = headend.Uid
            });
        }
    }

    public class MxfDvbsHeadend
    {
        public void AddChannel(MxfDvbsService service, int preset)
        {
            var channel = _channels.SingleOrDefault(arg => arg._service.Equals(service) && arg.Preset == preset);
            if (channel != null) return;

            channel = new MxfDvbsChannel
            {
                _headend = this,
                _service = service,
                Preset = preset
            };
            _channels.Add(channel);
        }

        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!DvbsDataSet!DvbsHeadend[{CsiId}]";
            set { }
        }
        public bool ShouldSerializeUid() => string.IsNullOrEmpty(IdRef);

        [XmlAttribute("_csiId")]
        public int CsiId { get; set; }
        public bool ShouldSerializeCsiId() => string.IsNullOrEmpty(IdRef);

        [XmlAttribute("_languageIso639")]
        public string LanguageIso639 { get; set; }

        [XmlAttribute("idref")]
        public string IdRef { get; set; }

        [XmlArrayItem("DvbsChannel")]
        public List<MxfDvbsChannel> _channels { get; set; }
    }
}
