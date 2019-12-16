using System;
using Newtonsoft.Json;

namespace HDHomeRunTV
{
    public class HDHRDiscover
    {
        [JsonProperty("DeviceID")]
        public string DeviceID { get; set; }

        [JsonProperty("LocalIP")]
        public string LocalIP { get; set; }

        [JsonProperty("BaseURL")]
        public string BaseURL { get; set; }

        [JsonProperty("DiscoverURL")]
        public string DiscoverURL { get; set; }

        [JsonProperty("LineupURL")]
        public string LineupURL { get; set; }
    }

    public class HDHRDevice
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
        public string DeviceID { get; set; }

        [JsonProperty("DeviceAuth")]
        public string DeviceAuth { get; set; }

        [JsonProperty("TunerCount")]
        public int TunerCount { get; set; }

        [JsonProperty("BaseURL")]
        public string BaseURL { get; set; }

        [JsonProperty("LineupURL")]
        public string LineupURL { get; set; }
    }

    public class HDHRChannel
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
        public bool HD { get; set; }

        [JsonProperty("Favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("URL")]
        public string URL { get; set; }

        // the below items are returned with the ?tuning option in the request
        [JsonProperty("TransportStreamID")]
        public string TransportStreamID { get; set; }

        [JsonProperty("Modulation")]
        public string Modulation { get; set; }

        [JsonProperty("Frequency")]
        public string Frequency { get; set; }

        [JsonProperty("ProgramNumber")]
        public string ProgramNumber { get; set; }

        [JsonProperty("OriginalNetworkID")]
        public string OriginalNetworkID { get; set; }
    }

    /// <summary>
    /// This is similar to MXF Service entry
    /// </summary>
    public class HDHRChannelGuide
    {
        [JsonProperty("GuideNumber")]
        public string GuideNumber { get; set; }

        [JsonProperty("GuideName")]
        public string GuideName { get; set; }

        [JsonProperty("Affiliate")]
        public string Affiliate { get; set; }

        [JsonProperty("ImageURL")]
        public string ImageURL { get; set; }

        [JsonProperty("Guide")]
        public HDHRProgram[] Guide { get; set; }
    }

    public class HDHRAccount
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
    public class HDHRProgram
    {
        public override int GetHashCode()
        {
            int ret = (Title != null) ? Title.GetHashCode() : 0;
            ret = (ret * 397) ^ ((EpisodeNumber != null) ? EpisodeNumber.GetHashCode() : 0);
            ret = (ret * 397) ^ ((EpisodeTitle != null) ? EpisodeTitle.GetHashCode() : 0);
            ret = (ret * 397) ^ ((Synopsis != null) ? Synopsis.GetHashCode() : 0);
            ret = (ret * 397) ^ ((Team1 != null) ? Team1.GetHashCode() : 0);
            ret = (ret * 397) ^ ((Team2 != null) ? Team2.GetHashCode() : 0);
            ret = (ret * 397) ^ SeriesID.GetHashCode();
            ret &= 0x7fffffff;

            return ret;
        }

        [JsonProperty("StartTime")]
        public int StartTime { get; set; }
        public DateTime StartDateTime
        {
            get
            {
                return (new DateTime(1970, 1, 1) + TimeSpan.FromSeconds((double)StartTime));
            }
        }

        [JsonProperty("EndTime")]
        public int EndTime { get; set; }
        public DateTime EndDateTime
        {
            get
            {
                return (new DateTime(1970, 1, 1) + TimeSpan.FromSeconds((double)EndTime));
            }
        }

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
                else
                    return new DateTime(1970, 1, 1);
            }
        }

        [JsonProperty("ImageURL")]
        public string ImageURL { get; set; }

        [JsonProperty("PosterURL")]
        public string PosterURL { get; set; }

        [JsonProperty("SeriesID")]
        public string SeriesID { get; set; }

        [JsonProperty("Filter")]
        public string[] Filters { get; set; }
    }
}
