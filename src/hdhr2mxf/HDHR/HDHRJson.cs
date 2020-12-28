using System;
using Newtonsoft.Json;

namespace hdhr2mxf.HDHR
{
    public class hdhrDiscover
    {
        [JsonProperty("DeviceID")]
        public string DeviceId { get; set; }

        [JsonProperty("LocalIP")]
        public string LocalIp { get; set; }

        [JsonProperty("BaseURL")]
        public string BaseUrl { get; set; }

        [JsonProperty("DiscoverURL")]
        public string DiscoverUrl { get; set; }

        [JsonProperty("LineupURL")]
        public string LineupUrl { get; set; }
    }

    public class hdhrDevice
    {
        [JsonProperty("FriendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("ModelNumber")]
        public string ModelNumber { get; set; }

        [JsonProperty("FirmwareName")]
        public string FirmwareName { get; set; }

        [JsonProperty("FirmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("DeviceID")]
        public string DeviceId { get; set; }

        [JsonProperty("DeviceAuth")]
        public string DeviceAuth { get; set; }

        [JsonProperty("TunerCount")]
        public int TunerCount { get; set; }

        [JsonProperty("BaseURL")]
        public string BaseUrl { get; set; }

        [JsonProperty("LineupURL")]
        public string LineupUrl { get; set; }
    }

    public class hdhrChannel
    {
        [JsonProperty("GuideNumber")]
        public string GuideNumber { get; set; }

        [JsonProperty("GuideName")]
        public string GuideName { get; set; }

        [JsonProperty("VideoCodec")]
        public string VideoCodec { get; set; }

        [JsonProperty("AudioCodec")]
        public string AudioCodec { get; set; }

        [JsonProperty("HD")]
        public bool Hd { get; set; }

        [JsonProperty("Favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("URL")]
        public string Url { get; set; }

        // the below items are returned with the ?tuning option in the request
        [JsonProperty("TransportStreamID")]
        public string TransportStreamId { get; set; }

        [JsonProperty("Modulation")]
        public string Modulation { get; set; }

        [JsonProperty("Frequency")]
        public string Frequency { get; set; }

        [JsonProperty("ProgramNumber")]
        public string ProgramNumber { get; set; }

        [JsonProperty("OriginalNetworkID")]
        public string OriginalNetworkId { get; set; }
    }

    /// <summary>
    /// This is similar to MXF Service entry
    /// </summary>
    public class hdhrChannelGuide
    {
        [JsonProperty("GuideNumber")]
        public string GuideNumber { get; set; }

        [JsonProperty("GuideName")]
        public string GuideName { get; set; }

        [JsonProperty("Affiliate")]
        public string Affiliate { get; set; }

        [JsonProperty("ImageURL")]
        public string ImageUrl { get; set; }

        [JsonProperty("Guide")]
        public hdhrProgram[] Guide { get; set; }
    }

    public class hdhrAccount
    {
        [JsonProperty("AccountEmail")]
        public string AccountEmail { get; set; }

        [JsonProperty("AccountDeviceIDs")]
        public string[] AccountDeviceIDs { get; set; }

        [JsonProperty("DvrActive")]
        public bool DvrActive { get; set; }

        [JsonProperty("AccountState")]
        public string AccountState { get; set; }
    }
    public class hdhrProgram
    {
        public override int GetHashCode()
        {
            var ret = (Title != null) ? Title.GetHashCode() : 0;
            ret = (ret * 397) ^ (EpisodeNumber != null ? EpisodeNumber.GetHashCode() : 0);
            ret = (ret * 397) ^ (EpisodeTitle != null ? EpisodeTitle.GetHashCode() : 0);
            ret = (ret * 397) ^ (Synopsis != null ? Synopsis.GetHashCode() : 0);
            ret = (ret * 397) ^ (Team1 != null ? Team1.GetHashCode() : 0);
            ret = (ret * 397) ^ (Team2 != null ? Team2.GetHashCode() : 0);
            ret = (ret * 397) ^ SeriesId.GetHashCode();
            ret &= 0x7fffffff;

            return ret;
        }

        [JsonProperty("StartTime")]
        public int StartTime { get; set; }
        public DateTime StartDateTime => (new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(StartTime));

        [JsonProperty("EndTime")]
        public int EndTime { get; set; }
        //public DateTime EndDateTime => (new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(EndTime));

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("EpisodeNumber")]
        public string EpisodeNumber { get; set; }

        [JsonProperty("EpisodeTitle")]
        public string EpisodeTitle { get; set; }

        [JsonProperty("Synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("Team1")]
        public string Team1 { get; set; }

        [JsonProperty("Team2")]
        public string Team2 { get; set; }

        [JsonProperty("OriginalAirdate")]
        public int? OriginalAirdate { get; set; }
        public DateTime OriginalAirDateTime
        {
            get
            {
                if (OriginalAirdate != null)
                    return (new DateTime(1970, 1, 1) + TimeSpan.FromSeconds((double)OriginalAirdate));
                return new DateTime(1970, 1, 1);
            }
        }

        [JsonProperty("ImageURL")]
        public string ImageUrl { get; set; }

        [JsonProperty("PosterURL")]
        public string PosterUrl { get; set; }

        [JsonProperty("SeriesID")]
        public string SeriesId { get; set; }

        [JsonProperty("Filter")]
        public string[] Filters { get; set; }
    }
}
