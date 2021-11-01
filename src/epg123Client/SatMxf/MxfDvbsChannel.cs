using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsChannel
    {
        [XmlIgnore] public MxfDvbsHeadend _headend;
        [XmlIgnore] public MxfDvbsService _service;

        [XmlAttribute("uid")]
        public string Uid
        {
            get =>
                $"{_headend.Uid}!DvbsChannel[{_service._transponder._satellite.PositionEast}:{_service._transponder.CarrierFrequency}:{_service._transponder._originalNetworkId}:{_service._transponder._transportStreamId}:{_service._serviceId}]";
            set { }
        }

        [XmlAttribute("_service")]
        public string Service
        {
            get => $"{_service.Uid}";
            set { }
        }

        [XmlAttribute("_csiChannelValid")]
        public bool CsiChannelValid { get; set; }
        public bool ShouldSerializeCsiChannelValid() => CsiChannel != 0;

        [XmlAttribute("_csiChannel")]
        public int CsiChannel { get; set; }
        public bool ShouldSerializeCsiChannel() => CsiChannel != 0;

        [XmlAttribute("_presetValid")]
        public bool PresetValid { get; set; }
        public bool ShouldSerializePresetValid() => Preset != 0;

        [XmlAttribute("_preset")]
        public int Preset { get; set; }
        public bool ShouldSerializePreset() => Preset != 0;
    }
}
