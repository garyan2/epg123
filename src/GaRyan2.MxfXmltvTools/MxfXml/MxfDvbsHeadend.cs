using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
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
        private string _uid;

        public void AddChannel(MxfDvbsService service, int preset)
        {
            var channel = _channels.SingleOrDefault(arg => arg._mxfService.Equals(service) && arg.Preset == preset);
            if (channel != null) return;

            channel = new MxfDvbsChannel
            {
                _mxfHeadend = this,
                _mxfService = service,
                Preset = preset
            };
            _channels.Add(channel);
        }

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"!DvbsDataSet!DvbsHeadend[{CsiId}]";
            set { _uid = value; }
        }

        [XmlAttribute("_csiId")]
        [DefaultValue(0)]
        public int CsiId { get; set; }

        [XmlAttribute("_languageIso639")]
        public string LanguageIso639 { get; set; }

        [XmlAttribute("idref")]
        public string IdRef { get; set; }

        [XmlArrayItem("DvbsChannel")]
        public List<MxfDvbsChannel> _channels { get; set; }
    }
}
