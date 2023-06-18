using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDvbsSatellite
    {
        private string _uid;

        public MxfDvbsTransponder GetOrCreateTransponder(int freq, int pol, int sr, int onid, int tsid)
        {
            var transponder = _transponders.SingleOrDefault(arg => arg.CarrierFrequency == freq && arg.Polarization == pol && arg.SymbolRate == sr);
            if (transponder != null) return transponder;

            transponder = new MxfDvbsTransponder
            {
                _satellite = this,
                CarrierFrequency = freq,
                Polarization = pol,
                SymbolRate = sr,
                OriginalNetworkId = onid,
                TransportStreamId = tsid,
                _services = new List<MxfDvbsService>()
            };
            _transponders.Add(transponder);
            return transponder;
        }

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"!DvbsDataSet!DvbsSatellite[{PositionEast}]";
            set { _uid = value; }
        }

        [XmlAttribute("_name")]
        public string Name { get; set; }

        [XmlAttribute("_positionEast")]
        [DefaultValue(0)]
        public int PositionEast { get; set; }

        [XmlArrayItem("DvbsTransponder")]
        public List<MxfDvbsTransponder> _transponders { get; set; }
    }
}
