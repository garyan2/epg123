using System.ComponentModel;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDvbsChannel
    {
        private string _uid;
        private string _service;

        [XmlIgnore] public MxfDvbsHeadend _mxfHeadend;
        [XmlIgnore] public MxfDvbsService _mxfService;

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"{_mxfHeadend.Uid}!DvbsChannel[{_mxfService._transponder._satellite.PositionEast}:{_mxfService._transponder.CarrierFrequency}:{(_mxfService._transponder.OriginalNetworkIdValid ? $"{_mxfService._transponder._originalNetworkId}" : "")}:{(_mxfService._transponder.TransportStreamIdValid ? $"{_mxfService._transponder._transportStreamId}" : "")}:{_mxfService._serviceId}]";
            set { _uid = value; }
        }

        [XmlAttribute("_service")]
        public string Service
        {
            get => _service ?? $"{_mxfService.Uid}";
            set { _service = value; }
        }

        [XmlAttribute("_csiChannelValid")]
        [DefaultValue(false)]
        public bool CsiChannelValid
        {
            get => CsiChannel != 0;
            set { }
        }

        [XmlAttribute("_csiChannel")]
        [DefaultValue(0)]
        public int CsiChannel { get; set; }

        [XmlAttribute("_presetValid")]
        [DefaultValue(false)]
        public bool PresetValid
        {
            get => Preset != 0;
            set { }
        }

        [XmlAttribute("_preset")]
        [DefaultValue(0)]
        public int Preset { get; set; }
    }
}
