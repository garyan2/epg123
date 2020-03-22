using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123
{
    public class SdStationMapResponse
    {
        [JsonProperty("map")]
        public IList<SdLineupMap> Map { get; set; }

        [JsonProperty("stations")]
        public IList<SdLineupStation> Stations { get; set; }

        [JsonProperty("metadata")]
        public sdMetadata Metadata { get; set; }
    }

    public class SdLineupMap
    {
        [JsonProperty("stationID")]
        public string StationID { get; set; }

        [JsonProperty("uhfVhf")]
        public int UhfVhf { get; set; }

        [JsonProperty("atscMajor")]
        public int AtscMajor { get; set; }

        [JsonProperty("atscMinor")]
        public int AtscMinor { get; set; }

        [JsonProperty("frequencyHz")]
        public long FrequencyHz { get; set; }

        //[JsonProperty("polarization")]
        //public string Polarization { get; set; }

        //[JsonProperty("deliverySystem")]
        //public string DeliverySystem { get; set; }

        //[JsonProperty("modulationSystem")]
        //public string ModulationSystem { get; set; }

        //[JsonProperty("symbolrate")]
        //public int Symbolrate { get; set; }

        //[JsonProperty("fec")]
        //public string Fec { get; set; }

        [JsonProperty("serviceID")]
        public int ServiceID { get; set; }

        [JsonProperty("networkID")]
        public int NetworkID { get; set; }

        [JsonProperty("transportID")]
        public int TransportID { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        //[JsonProperty("virtualChannel")]
        //public string VirtualChannel { get; set; }

        [JsonProperty("channelMajor")]
        public int ChannelMajor { get; set; }

        [JsonProperty("channelMinor")]
        public int ChannelMinor { get; set; }

        //[JsonProperty("providerChannel")]
        //public string ProvideChannel { get; set; }

        //[JsonProperty("providerCallsign")]
        //public string ProviderCallsign { get; set; }

        //[JsonProperty("logicalChannelNumber")]
        //public string LogicalChannelNumber { get; set; }

        //[JsonProperty("matchType")]
        //public string MatchType { get; set; }
    }

    public class SdLineupStation
    {
        [JsonProperty("stationID")]
        public string StationID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("callsign")]
        public string Callsign { get; set; }

        [JsonProperty("affiliate")]
        public string Affiliate { get; set; }

        [JsonProperty("broadcastLanguage")]
        public string[] BroadcastLanguage { get; set; }

        //[JsonProperty("descriptionLanguage")]
        //public string[] DescriptionLanguage { get; set; }

        //[JsonProperty("broadcaster")]
        //public sdBroadcaster Broadcaster { get; set; }

        //[JsonProperty("isCommercialFree")]
        //public bool IsCommercialFree { get; set; }

        [JsonProperty("stationLogo")]
        public List<SdStationImage> StationLogos { get; set; }

        [JsonProperty("logo")]
        public SdStationImage Logo { get; set; }
    }

    public class SdStationImage
    {
        [JsonProperty("URL")]
        public string URL { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        //[JsonProperty("md5")]
        //public string Md5 { get; set; }

        //[JsonProperty("source")]
        //public string Source { get; set; }
    }

    public class sdBroadcaster
    {
        //[JsonProperty("city")]
        //public string City { get; set; }

        //[JsonProperty("state")]
        //public string State { get; set; }

        //[JsonProperty("postalcode")]
        //public string Postalcode { get; set; }

        //[JsonProperty("country")]
        //public string Country { get; set; }
    }

    public class sdMetadata
    {
        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        //[JsonProperty("modified")]
        //public string Modified { get; set; }

        //[JsonProperty("transport")]
        //public string Transport { get; set; }

        //[JsonProperty("modulation")]
        //public string Modulation { get; set; }
    }

    public class sdLineupPreviewChannel
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("callsign")]
        public string Callsign { get; set; }

        //[JsonProperty("affiliate")]
        //public string Affiliate { get; set; }
    }
}
