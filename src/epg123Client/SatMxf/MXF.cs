using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.TV.Tuning;

namespace epg123Client.SatMxf
{
    [XmlRoot("MXF")]
    public partial class Mxf
    {
        private readonly string _key = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
        private readonly string _version = "6.1.0.0";
        private readonly string _culture = string.Empty;

        public void InitializeMxf()
        {
            // create mcepg and mcstore assembly entries
            Assembly = new List<MxfAssembly>
            {
                new MxfAssembly
                {
                    Name = "mcepg",
                    Version = _version,
                    CultureInfo = _culture,
                    PublicKey = _key,
                    Namespace = new MxfNamespace
                    {
                        Name = "Microsoft.MediaCenter.Satellites",
                        Type = new List<MxfType>
                        {
                            new MxfType { Name = "DvbsDataSet" },
                            new MxfType { Name = "DvbsSatellite" },
                            new MxfType { Name = "DvbsRegion" },
                            new MxfType { Name = "DvbsHeadend" },
                            new MxfType { Name = "DvbsTransponder" },
                            new MxfType { Name = "DvbsFootprint" },
                            new MxfType { Name = "DvbsChannel" },
                            new MxfType { Name = "DvbsService" }
                        }
                    }
                },
                new MxfAssembly
                {
                    Name = "mcstore",
                    Version = _version,
                    CultureInfo = _culture,
                    PublicKey = _key,
                    Namespace = new MxfNamespace
                    {
                        Name = "Microsoft.MediaCenter.Store",
                        Type = new List<MxfType>
                        {
                            new MxfType { Name = "Provider" },
                            new MxfType { Name = "UId", ParentFieldName = "target" }
                        }
                    }
                }
            };

            DvbsDataSet = new MxfDvbsDataSet
            {
                Uid = "!DvbsDataSet",
                FrequencyTolerance = 10,
                SymbolRateTolerance = 1500,
                MinimumSearchMatches = 3,
                DataSetRevision = 59
            };
        }

        public void AddChannel(MergedChannel mergedChannel, bool includeEncrypted)
        {
            foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos)
            {
                // make sure it is DVBS
                if (!(tuningInfo is DvbTuningInfo dvbTuningInfo) || !dvbTuningInfo.TuningSpace.Equals("DVB-S")) continue;
                var locator = dvbTuningInfo.TuneRequest.Locator as DVBSLocator;

                // filter on options
                if ((dvbTuningInfo.IsEncrypted || dvbTuningInfo.IsSuggestedBlocked) && !includeEncrypted) continue;

                // determine satellite, transponder, and service for channel
                var satellite = GetOrCreateSatellite(locator.OrbitalPosition);
                var transponder = satellite.GetOrCreateTransponder(dvbTuningInfo.Frequency / 1000,
                    (int) locator.SignalPolarisation - 1, locator.SymbolRate / 1000, dvbTuningInfo.Onid,
                    dvbTuningInfo.Tsid);
                var service = transponder.GetOrCreateService(mergedChannel.CallSign, dvbTuningInfo.Sid,
                    mergedChannel.Service.ServiceType == 2 ? 1 : mergedChannel.Service.ServiceType == 3 ? 2 : 0,
                    dvbTuningInfo.IsEncrypted || dvbTuningInfo.IsSuggestedBlocked);

                // add channel with callsign and channel number
                var keyValues = new KeyValues(WmcStore.WmcObjectStore);
                var region = GetOrCreateRegion(keyValues.Single(arg => arg.Key == "ClientCountryCode").Value);
                var footprint = region.GetOrCreateFootprint(satellite);
                var headend = footprint.GetOrCreateHeadend(satellite.PositionEast);
                headend.AddChannel(service, int.Parse(mergedChannel.ChannelNumber.ToString()));
                AddReferenceHeadend(headend);
            }
        }

        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// </summary>
        public Mxf()
        {
            InitializeMxf();
        }

        [XmlElement("Assembly")]
        public List<MxfAssembly> Assembly { get; set; }

        [XmlElement("DvbsDataSet")]
        public MxfDvbsDataSet DvbsDataSet { get; set; }
    }

    public class MxfAssembly
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("cultureinfo")]
        public string CultureInfo { get; set; }

        [XmlAttribute("publicKey")]
        public string PublicKey { get; set; }

        [XmlElement("NameSpace")]
        public MxfNamespace Namespace { get; set; }
    }

    public class MxfNamespace
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Type")]
        public List<MxfType> Type { get; set; }
    }

    public class MxfType
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("groupName")]
        public string GroupName { get; set; }

        [XmlAttribute("parentFieldName")]
        public string ParentFieldName { get; set; }
    }

}