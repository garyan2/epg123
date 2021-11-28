using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.MediaCenter.Satellites;

namespace epg123Client.SatMxf
{
    public partial class Mxf
    {
        public MxfDvbsSatellite GetOrCreateSatellite(int position)
        {
            var satellite = DvbsDataSet._allSatellites.SingleOrDefault(arg => arg.PositionEast == position);
            if (satellite != null) return satellite;

            var dvbSatellites = new DvbsSatellites(WmcStore.WmcObjectStore);
            var dvbSatellite = dvbSatellites.Single(arg => arg.PositionEast == position);
            satellite = new MxfDvbsSatellite
            {
                PositionEast = position,
                Name = dvbSatellite.Name,
                _transponders = new List<MxfDvbsTransponder>()
            };
            DvbsDataSet._allSatellites.Add(satellite);
            return satellite;
        }
    }

    public class MxfDvbsSatellite
    {
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
            get => $"!DvbsDataSet!DvbsSatellite[{PositionEast}]";
            set { }
        }

        [XmlAttribute("_name")]
        public string Name { get; set; }

        [XmlAttribute("_positionEast")]
        public int PositionEast { get; set; }

        [XmlArrayItem("DvbsTransponder")]
        public List<MxfDvbsTransponder> _transponders { get; set; }
    }
}
