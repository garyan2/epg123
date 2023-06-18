using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GaRyan2.SchedulesDirectAPI
{
    public class StationChannelMap
    {
        [JsonProperty("map")]
        [JsonConverter(typeof(SingleOrListConverter<LineupChannel>))]
        public List<LineupChannel> Map { get; set; }

        [JsonProperty("stations")]
        [JsonConverter(typeof(SingleOrListConverter<LineupStation>))]
        public List<LineupStation> Stations { get; set; }

        [JsonProperty("metadata")]
        public LineupMetadata Metadata { get; set; }
    }

    public class LineupChannel
    {
        public string ChannelNumber => $"{myChannelNumber}{(myChannelSubnumber > 0 ? $".{myChannelSubnumber}" : "")}";
        public int myChannelNumber
        {
            get
            {
                if (string.IsNullOrEmpty(Channel)) return ChannelMajor ?? AtscMajor ?? UhfVhf ?? -1;
                if (Regex.Match(Channel, @"[A-Za-z]{1}[\d]{4}").Length > 0) return int.Parse(Channel.Substring(2));
                if (Regex.Match(Channel, @"[A-Za-z0-9.]\.[A-Za-z]{2}").Length > 0) return -1;
                if (int.TryParse(Regex.Replace(Channel, "[^0-9.]", ""), out int number)) return number;
                else
                {
                    // if channel number is not a whole number, must be a decimal number
                    var numbers = Regex.Replace(Channel, "[^0-9.]", "").Replace('_', '.').Replace('-', '.').Split('.');
                    if (numbers.Length == 2)
                    {
                        return int.Parse(numbers[0]);
                    }
                }
                return -1;
            }
        }

        public int myChannelSubnumber
        {
            get
            {
                if (string.IsNullOrEmpty(Channel)) return ChannelMinor ?? AtscMinor ?? 0;
                if (!int.TryParse(Regex.Replace(Channel, "[^0-9.]", ""), out _))
                {
                    // if channel number is not a whole number, must be a decimal number
                    var numbers = Regex.Replace(Channel, "[^0-9.]", "").Replace('_', '.').Replace('-', '.').Split('.');
                    if (numbers.Length == 2)
                    {
                        return int.Parse(numbers[1]);
                    }
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string MatchName { get; set; }

        [JsonProperty("stationID")]
        public string StationId { get; set; }

        [JsonProperty("uhfVhf")]
        public int? UhfVhf { get; set; }

        [JsonProperty("atscMajor")]
        public int? AtscMajor { get; set; }

        [JsonProperty("atscMinor")]
        public int? AtscMinor { get; set; }

        [JsonProperty("frequencyHz")]
        public long? FrequencyHz { get; set; }

        [JsonProperty("polarization")]
        public string Polarization { get; set; }

        [JsonProperty("deliverySystem")]
        public string DeliverySystem { get; set; }

        [JsonProperty("modulationSystem")]
        public string ModulationSystem { get; set; }

        [JsonProperty("symbolrate")]
        public int? Symbolrate { get; set; }

        [JsonProperty("fec")]
        public string Fec { get; set; }

        [JsonProperty("serviceID")]
        public int? ServiceId { get; set; }

        [JsonProperty("networkID")]
        public int? NetworkId { get; set; }

        [JsonProperty("transportID")]
        public int? TransportId { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("virtualChannel")]
        public string VirtualChannel { get; set; }

        [JsonProperty("channelMajor")]
        public int? ChannelMajor { get; set; }

        [JsonProperty("channelMinor")]
        public int? ChannelMinor { get; set; }

        [JsonProperty("providerChannel")]
        public string ProviderChannel { get; set; }

        [JsonProperty("providerCallsign")]
        public string ProviderCallsign { get; set; }

        [JsonProperty("logicalChannelNumber")]
        public string LogicalChannelNumber { get; set; }

        [JsonProperty("matchType")]
        public string MatchType { get; set; }
    }

    public class LineupStation
    {
        [JsonProperty("stationID")]
        public string StationId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("callsign")]
        public string Callsign { get; set; }

        [JsonProperty("affiliate")]
        public string Affiliate { get; set; }

        [JsonProperty("broadcastLanguage")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] BroadcastLanguage { get; set; }

        [JsonProperty("descriptionLanguage")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] DescriptionLanguage { get; set; }

        [JsonProperty("broadcaster")]
        public StationBroadcaster Broadcaster { get; set; }

        [JsonProperty("isCommercialFree")]
        public bool? IsCommercialFree { get; set; }

        [JsonProperty("stationLogo")]
        [JsonConverter(typeof(SingleOrListConverter<StationImage>))]
        public List<StationImage> StationLogos { get; set; }

        [JsonProperty("logo")]
        public StationImage Logo { get; set; }
    }

    public class StationImage
    {
        [JsonProperty("URL")]
        public string Url { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }

    public class StationBroadcaster
    {
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postalcode")]
        public string Postalcode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class LineupMetadata
    {
        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("modulation")]
        public string Modulation { get; set; }
    }
}