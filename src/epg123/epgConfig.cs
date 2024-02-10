using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace epg123
{
    [XmlRoot("EPG123")]
    public class epgConfig : IEquatable<epgConfig>
    {
        public epgConfig() { }

        protected epgConfig(epgConfig other)
        {
            Version = Helper.Epg123Version;
            BaseApiUrl = other.BaseApiUrl;
            BaseArtworkUrl = other.BaseArtworkUrl;
            UseDebug = other.UseDebug;
            CacheRetention = other.CacheRetention;
            UserAccount = new SdUserAccount
            {
                LoginName = other.UserAccount?.LoginName,
                PasswordHash = other.UserAccount?.PasswordHash
            };
            RatingsOrigin = other.RatingsOrigin;
            DaysToDownload = other.DaysToDownload;
            TheTvdbNumbers = other.TheTvdbNumbers;
            PrefixEpisodeTitle = other.PrefixEpisodeTitle;
            PrefixEpisodeDescription = other.PrefixEpisodeDescription;
            AlternateSEFormat = other.AlternateSEFormat;
            AppendEpisodeDesc = other.AppendEpisodeDesc;
            OadOverride = other.OadOverride;
            SeasonEventImages = other.SeasonEventImages;
            SeriesPosterAspect = other.SeriesPosterAspect;
            ArtworkSize = other.ArtworkSize;
            IncludeSdLogos = other.IncludeSdLogos;
            PreferredLogoStyle = other.PreferredLogoStyle;
            AlternateLogoStyle = other.AlternateLogoStyle;
            AutoAddNew = other.AutoAddNew;
            ExcludeCastAndCrew = other.ExcludeCastAndCrew;
            CreateXmltv = other.CreateXmltv;
            XmltvIncludeChannelNumbers = other.XmltvIncludeChannelNumbers;
            XmltvIncludeChannelLogos = other.XmltvIncludeChannelLogos;
            XmltvAddFillerData = other.XmltvAddFillerData;
            XmltvFillerProgramLength = other.XmltvFillerProgramLength;
            XmltvFillerProgramDescription = other.XmltvFillerProgramDescription;
            XmltvExtendedInfoInTitleDescriptions = other.XmltvExtendedInfoInTitleDescriptions;
            XmltvSingleImage = other.XmltvSingleImage;
            UseIpAddress = other.UseIpAddress;
            ModernMediaUiPlusSupport = other.ModernMediaUiPlusSupport;
            BrandLogoImage = other.BrandLogoImage;
            SuppressStationEmptyWarnings = other.SuppressStationEmptyWarnings;

            if (other.IncludedLineup != null)
            {
                IncludedLineup = new List<string>();
                foreach (var lineup in other.IncludedLineup)
                {
                    IncludedLineup.Add(lineup);
                }
            }
            if (other.DiscardChanNumbers != null)
            {
                DiscardChanNumbers = new List<string>();
                foreach (var lineup in other.DiscardChanNumbers)
                {
                    DiscardChanNumbers.Add(lineup);
                }
            }

            ExpectedServicecount = other.ExpectedServicecount;
            if (other.StationId == null) return;
            StationId = new List<SdChannelDownload>();
            foreach (var station in other.StationId)
            {
                StationId.Add(new SdChannelDownload
                {
                    CallSign = station.CallSign,
                    CustomCallSign = station.CustomCallSign,
                    CustomServiceName = station.CustomServiceName,
                    HdOverride = station.HdOverride,
                    SdOverride = station.SdOverride,
                    StationId = station.StationId
                });
            }
        }

        public epgConfig Clone()
        {
            return new epgConfig(this);
        }

        public bool Equals(epgConfig other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            //if (!Version.Equals(other.Version)) return false;
            if (!BaseApiUrl.Equals(other.BaseApiUrl)) return false;
            if (!BaseArtworkUrl.Equals(other.BaseArtworkUrl)) return false;
            if (!UseDebug.Equals(other.UseDebug)) return false;
            if (!CacheRetention.Equals(other.CacheRetention)) return false;
            if (!UserAccount.LoginName.Equals(other.UserAccount?.LoginName)) return false;
            if (!UserAccount.PasswordHash.Equals(other.UserAccount?.PasswordHash)) return false;
            if (!RatingsOrigin.Equals(other.RatingsOrigin)) return false;
            if (!DaysToDownload.Equals(other.DaysToDownload)) return false;
            if (!TheTvdbNumbers.Equals(other.TheTvdbNumbers)) return false;
            if (!PrefixEpisodeTitle.Equals(other.PrefixEpisodeTitle)) return false;
            if (!PrefixEpisodeDescription.Equals(other.PrefixEpisodeDescription)) return false;
            if (!AlternateSEFormat.Equals(other.AlternateSEFormat)) return false;
            if (!AppendEpisodeDesc.Equals(other.AppendEpisodeDesc)) return false;
            if (!OadOverride.Equals(other.OadOverride)) return false;
            if (!SeasonEventImages.Equals(other.SeasonEventImages)) return false;
            if (!SeriesPosterAspect.Equals(other.SeriesPosterAspect)) return false;
            if (!ArtworkSize.Equals(other.ArtworkSize)) return false;
            if (!IncludeSdLogos.Equals(other.IncludeSdLogos)) return false;
            if (!PreferredLogoStyle.Equals(other.PreferredLogoStyle)) return false;
            if (!AlternateLogoStyle.Equals(other.AlternateLogoStyle)) return false;
            if (!AutoAddNew.Equals(other.AutoAddNew)) return false;
            if (!ExcludeCastAndCrew.Equals(other.ExcludeCastAndCrew)) return false;
            if (!CreateXmltv.Equals(other.CreateXmltv)) return false;
            if (!XmltvIncludeChannelNumbers.Equals(other.XmltvIncludeChannelNumbers)) return false;
            if (!XmltvIncludeChannelLogos.Equals(other.XmltvIncludeChannelLogos)) return false;
            if (!XmltvAddFillerData.Equals(other.XmltvAddFillerData)) return false;
            if (!XmltvFillerProgramLength.Equals(other.XmltvFillerProgramLength)) return false;
            if (!XmltvFillerProgramDescription.Equals(other.XmltvFillerProgramDescription)) return false;
            if (!XmltvExtendedInfoInTitleDescriptions.Equals(other.XmltvExtendedInfoInTitleDescriptions)) return false;
            if (!XmltvSingleImage.Equals(other.XmltvSingleImage)) return false;
            if (!(UseIpAddress ?? "").Equals(other.UseIpAddress ?? "")) return false;
            if (!ModernMediaUiPlusSupport.Equals(other.ModernMediaUiPlusSupport)) return false;
            if (!BrandLogoImage.Equals(other.BrandLogoImage)) return false;
            if (!SuppressStationEmptyWarnings.Equals(other.SuppressStationEmptyWarnings)) return false;
            if (!ExpectedServicecount.Equals(other.ExpectedServicecount)) return false;

            if (IncludedLineup != null && other.IncludedLineup != null)
            {
                if (IncludedLineup.Any(lineup => !other.IncludedLineup.Contains(lineup))) return false;
                if (other.IncludedLineup.Any(lineup => !IncludedLineup.Contains(lineup))) return false;
            }
            else if (IncludedLineup == null ^ other.IncludedLineup == null) return false;

            if (DiscardChanNumbers != null && other.DiscardChanNumbers != null)
            {
                if (DiscardChanNumbers.Any(lineup => !other.DiscardChanNumbers.Contains(lineup))) return false;
                if (other.DiscardChanNumbers.Any(lineup => !DiscardChanNumbers.Contains(lineup))) return false;
            }
            else if (DiscardChanNumbers == null ^ other.DiscardChanNumbers == null) return false;

            if (StationId == null || other.StationId == null) return !(StationId == null ^ other.StationId == null);
            var thisStationId = StationId.Select(stationId => stationId.StationId).ToList();
            var otherStationId = other.StationId.Select(stationId => stationId.StationId).ToList();

            return thisStationId.All(stationId => otherStationId.Contains(stationId)) && otherStationId.All(stationId => thisStationId.Contains(stationId));
        }

        [XmlAttribute("version")]
        public string Version { get; set; } = Helper.Epg123Version;

        [XmlElement("BaseApiUrl")]
        public string BaseApiUrl { get; set; } = "https://json.schedulesdirect.org/20141201/";

        [XmlElement("BaseArtworkUrl")]
        public string BaseArtworkUrl { get; set; } = "https://json.schedulesdirect.org/20141201/";

        [XmlElement("UseDebug")]
        public bool UseDebug { get; set; }

        [XmlElement("CacheRetention")]
        public int CacheRetention { get; set; } = 30;

        [XmlElement("UserAccount")]
        public SdUserAccount UserAccount { get; set; }

        [XmlElement("RatingsOrigin")]
        public string RatingsOrigin { get; set; } = RegionInfo.CurrentRegion.ThreeLetterISORegionName;

        [XmlElement("DaysToDownload")]
        public int DaysToDownload { get; set; } = 14;

        [XmlElement("TheTVDBNumbers")]
        public bool TheTvdbNumbers { get; set; } = true;

        [XmlElement("PrefixEpisodeTitle")]
        public bool PrefixEpisodeTitle { get; set; }

        [XmlElement("PrefixEpisodeDescription")]
        public bool PrefixEpisodeDescription { get; set; }

        [XmlElement("AlternateSEFormat")]
        public bool AlternateSEFormat { get; set; }

        [XmlElement("AppendEpisodeDesc")]
        public bool AppendEpisodeDesc { get; set; }

        [XmlElement("OADOverride")]
        public bool OadOverride { get; set; } = true;

        [XmlElement("SeasonEventImages")]
        public bool SeasonEventImages { get; set; } = true;

        [XmlElement("SeriesPosterArt")] // deprecated
        public bool SeriesPosterArt { get; set; }
        public bool ShouldSerializeSeriesPosterArt() => false;

        [XmlElement("SeriesWsArt")] // deprecated
        public bool SeriesWsArt { get; set; }
        public bool ShouldSerializeSeriesWsArt() => false;

        [XmlAnyElement("SeriesPosterAspectComment")]
        public XmlComment SeriesPosterAspectComment
        {
            get => new XmlDocument().CreateComment(" SeriesPostAspect: Set aspect of series artwork to link. \"2x3\", \"3x4\", \"4x3\", \"16x9\" ");
            set { }
        }
        [XmlElement("SeriesPosterAspect")]
        public string SeriesPosterAspect { get; set; } = "4x3";

        [XmlAnyElement("ArtworkSizeComment")]
        public XmlComment ArtworkSizeComment
        {
            get => new XmlDocument().CreateComment(" ArtworkSize: Set size of artwork to link. \"Sm\" Small, \"Md\" Medium, \"Lg\" Large ");
            set { }
        }
        [XmlElement("ArtworkSize")]
        public string ArtworkSize { get; set; } = "Md";

        [XmlElement("IncludeSDLogos")]
        public bool IncludeSdLogos { get; set; } = true;

        [XmlElement("PreferredLogoStyle")]
        public string PreferredLogoStyle { get; set; } = "DARK";

        [XmlElement("AlternateLogoStyle")]
        public string AlternateLogoStyle { get; set; } = "WHITE";

        [XmlElement("AutoAddNew")]
        public bool AutoAddNew { get; set; } = true;

        [XmlElement("ExcludeCastAndCrew")]
        public bool ExcludeCastAndCrew { get; set; }

        [XmlElement("CreateXmltv")]
        public bool CreateXmltv { get; set; }

        [XmlElement("XmltvIncludeChannelNumbers")]
        public bool XmltvIncludeChannelNumbers { get; set; } = true;

        [XmlElement("XmltvIncludeChannelLogos")]
        public string XmltvIncludeChannelLogos { get; set; } = "local";

        [XmlElement("XmltvAddFillerData")]
        public bool XmltvAddFillerData { get; set; } = true;

        [XmlElement("XmltvFillerProgramLength")]
        public int XmltvFillerProgramLength { get; set; } = 4;

        [XmlElement("XmltvFillerProgramDescription")]
        public string XmltvFillerProgramDescription { get; set; } = "This program was generated by EPG123 to provide filler data for stations that did not receive any guide listings from the upstream source.";

        [XmlElement("XmltvExtendedInfoInTitleDescriptions")]
        public bool XmltvExtendedInfoInTitleDescriptions { get; set; }

        [XmlElement("XmltvSingleImage")]
        public bool XmltvSingleImage { get; set; }

        [XmlElement("UseIpAddress")]
        public string UseIpAddress { get; set; }

        [XmlElement("ModernMediaUiPlusSupport")]
        public bool ModernMediaUiPlusSupport { get; set; }

        [XmlElement("ModernMediaUiPlusJsonFilepath")]
        public string ModernMediaUiPlusJsonFilepath { get; set; }

        [XmlAnyElement("BrandLogoImageComment")]
        public XmlComment BrandLogoImageComment
        {
            get => new XmlDocument().CreateComment(" BrandLogoImage: Add status image to guide view in WMC. Options are \"none\", \"light\", and \"dark\". ");
            set { }
        }
        [XmlElement("BrandLogoImage")]
        public string BrandLogoImage { get; set; } = "none";

        [XmlAnyElement("SuppressStationEmptyWarningsComment")]
        public XmlComment SuppressStationEmptyWarningsComment
        {
            get => new XmlDocument().CreateComment(" SuppressStationEmptyWarnings: Enter specific station callsigns, comma delimited, to suppress warnings for no guide data, or use a wildcard (*) for a group of callsigns. A solitary wildcard means all station warnings will be suppressed. ");
            set { }
        }
        [XmlElement("SuppressStationEmptyWarnings")]
        public string SuppressStationEmptyWarnings { get; set; } = "GOAC*,LOOR*,EDAC*,LEAC*,PEG*,LOAC*,PPV*,PUAC*,SPALT*,INFO*";

        [XmlElement("IncludedLineup")]
        public List<string> IncludedLineup { get; set; }

        [XmlElement("DiscardChanNumbers")]
        public List<string> DiscardChanNumbers { get; set; }

        [XmlElement("ExpectedServiceCount")]
        public int ExpectedServicecount { get; set; }

        [XmlAnyElement("StationIDComment")]
        public XmlComment StationIdComment
        {
            get =>
                new XmlDocument().CreateComment(" StationID attributes: You can add the following attributes to any station to customize your guide further.\n" +
                                                "      HDOverride=\"true\" - flags all programs on this station to be HD\n" +
                                                "      SDOverride=\"true\" - flags all programs on this station to be SD\n" +
                                                "      customCallSign=\"H and I\" - will replace the call sign provided by Schedules Direct with \"H and I\"\n" +
                                                "      customServiceName=\"Heroes &amp; Icons\" - will replace the station name provided by Schedules Direct with \"Heroes & Icons\"\n" +
                                                "      Note: special characters in XML will need to be escaped.\n" +
                                                "            ampersand = &amp;\n" +
                                                "            less-than = &lt;\n" +
                                                "            greater-than = &gt;\n" +
                                                "            quotation = &quot;\n" +
                                                "            apostrophe = &apos; ");
            set { }
        }
        [XmlElement("StationID")]
        public List<SdChannelDownload> StationId { get; set; } = new List<SdChannelDownload>();
    }

    public class SdUserAccount
    {
        [XmlIgnore]
        private string _passwordHash;

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = SHA1.Create().ComputeHash(bytes);
            return HexStringFromBytes(hashBytes);
        }
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        [XmlElement("LoginName")]
        public string LoginName { get; set; }

        [XmlElement("PasswordHash")]
        public string PasswordHash
        {
            get => _passwordHash;
            set => _passwordHash = value;
        }

        [XmlIgnore]
        public string Password
        {
            set => _passwordHash = HashPassword(value);
        }
    }

    public class SdChannelDownload
    {
        [XmlAttribute("CallSign")]
        public string CallSign { get; set; }

        [XmlAttribute("HDOverride")]
        public bool HdOverride { get; set; }
        public bool ShouldSerializeHdOverride() { return HdOverride; }

        [XmlAttribute("SDOverride")]
        public bool SdOverride { get; set; }
        public bool ShouldSerializeSdOverride() { return SdOverride; }

        [XmlAttribute("customCallSign")]
        public string CustomCallSign { get; set; }

        [XmlAttribute("customServiceName")]
        public string CustomServiceName { get; set; }

        [XmlText]
        public string StationId { get; set; }
    }
}
