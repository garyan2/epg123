using System;
using System.Collections.Generic;
using System.Globalization;
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
            this.version = Helper.epg123Version;
            this.UserAccount = new SdUserAccount()
            {
                LoginName = other.UserAccount.LoginName,
                PasswordHash = other.UserAccount.PasswordHash
            };
            this.RatingsOrigin = other.RatingsOrigin;
            this.DaysToDownload = other.DaysToDownload;
            this.TheTVDBNumbers = other.TheTVDBNumbers;
            this.PrefixEpisodeTitle = other.PrefixEpisodeTitle;
            this.PrefixEpisodeDescription = other.PrefixEpisodeDescription;
            this.AlternateSEFormat = other.AlternateSEFormat;
            this.AppendEpisodeDesc = other.AppendEpisodeDesc;
            this.OADOverride = other.OADOverride;
            this.SeriesPosterArt = other.SeriesPosterArt;
            this.TMDbCoverArt = other.TMDbCoverArt;
            this.IncludeSDLogos = other.IncludeSDLogos;
            this.PreferredLogoStyle = other.PreferredLogoStyle;
            this.AlternateLogoStyle = other.AlternateLogoStyle;
            this.AutoAddNew = other.AutoAddNew;
            this.AutoImport = other.AutoImport;
            this.Automatch = other.Automatch;
            this.CreateXmltv = other.CreateXmltv;
            this.XmltvIncludeChannelNumbers = other.XmltvIncludeChannelNumbers;
            this.XmltvIncludeChannelLogos = other.XmltvIncludeChannelLogos;
            this.XmltvLogoSubstitutePath = other.XmltvLogoSubstitutePath;
            this.XmltvAddFillerData = other.XmltvAddFillerData;
            this.XmltvFillerProgramLength = other.XmltvFillerProgramLength;
            this.XmltvFillerProgramDescription = other.XmltvFillerProgramDescription;
            this.XmltvOutputFile = other.XmltvOutputFile;
            this.ModernMediaUiPlusSupport = other.ModernMediaUiPlusSupport;
            this.BrandLogoImage = other.BrandLogoImage;
            this.SuppressStationEmptyWarnings = other.SuppressStationEmptyWarnings;

            if (other.IncludedLineup != null)
            {
                this.IncludedLineup = new List<string>();
                foreach (string lineup in other.IncludedLineup)
                {
                    this.IncludedLineup.Add(lineup);
                }
            }

            this.ExpectedServicecount = other.ExpectedServicecount;
            if (other.StationID != null)
            {
                this.StationID = new List<SdChannelDownload>();
                foreach (SdChannelDownload station in other.StationID)
                {
                    this.StationID.Add(new SdChannelDownload()
                    {
                        CallSign = station.CallSign,
                        HDOverride = station.HDOverride,
                        SDOverride = station.SDOverride,
                        StationID = station.StationID
                    });
                }
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

            //if (!this.version.Equals(other.version)) return false;
            if (!this.UserAccount.LoginName.Equals(other.UserAccount?.LoginName)) return false;
            if (!this.UserAccount.PasswordHash.Equals(other.UserAccount?.PasswordHash)) return false;
            if (!this.RatingsOrigin.Equals(other.RatingsOrigin)) return false;
            if (!this.DaysToDownload.Equals(other.DaysToDownload)) return false;
            if (!this.TheTVDBNumbers.Equals(other.TheTVDBNumbers)) return false;
            if (!this.PrefixEpisodeTitle.Equals(other.PrefixEpisodeTitle)) return false;
            if (!this.PrefixEpisodeDescription.Equals(other.PrefixEpisodeDescription)) return false;
            if (!this.AlternateSEFormat.Equals(other.AlternateSEFormat)) return false;
            if (!this.AppendEpisodeDesc.Equals(other.AppendEpisodeDesc)) return false;
            if (!this.OADOverride.Equals(other.OADOverride)) return false;
            if (!this.SeriesPosterArt.Equals(other.SeriesPosterArt)) return false;
            if (!this.TMDbCoverArt.Equals(other.TMDbCoverArt)) return false;
            if (!this.IncludeSDLogos.Equals(other.IncludeSDLogos)) return false;
            if (!this.PreferredLogoStyle.Equals(other.PreferredLogoStyle)) return false;
            if (!this.AlternateLogoStyle.Equals(other.AlternateLogoStyle)) return false;
            if (!this.AutoAddNew.Equals(other.AutoAddNew)) return false;
            if (!this.AutoImport.Equals(other.AutoImport)) return false;
            if (!this.Automatch.Equals(other.Automatch)) return false;
            if (!this.CreateXmltv.Equals(other.CreateXmltv)) return false;
            if (!this.XmltvIncludeChannelNumbers.Equals(other.XmltvIncludeChannelNumbers)) return false;
            if (!this.XmltvIncludeChannelLogos.Equals(other.XmltvIncludeChannelLogos)) return false;
            if (!this.XmltvLogoSubstitutePath.Equals(other.XmltvLogoSubstitutePath)) return false;
            if (!this.XmltvAddFillerData.Equals(other.XmltvAddFillerData)) return false;
            if (!this.XmltvFillerProgramLength.Equals(other.XmltvFillerProgramLength)) return false;
            if (!this.XmltvFillerProgramDescription.Equals(other.XmltvFillerProgramDescription)) return false;
            if (!this.XmltvOutputFile.Equals(other.XmltvOutputFile)) return false;
            if (!this.ModernMediaUiPlusSupport.Equals(other.ModernMediaUiPlusSupport)) return false;
            if (!this.BrandLogoImage.Equals(other.BrandLogoImage)) return false;
            if (!this.SuppressStationEmptyWarnings.Equals(other.SuppressStationEmptyWarnings)) return false;
            if (!this.ExpectedServicecount.Equals(other.ExpectedServicecount)) return false;

            if (this.IncludedLineup != null && other.IncludedLineup != null)
            {
                foreach (string lineup in this.IncludedLineup)
                {
                    if (!other.IncludedLineup.Contains(lineup)) return false;
                }
                foreach (string lineup in other.IncludedLineup)
                {
                    if (!this.IncludedLineup.Contains(lineup)) return false;
                }
            }
            else if (this.IncludedLineup == null ^ other.IncludedLineup == null) return false;

            if (this.StationID != null && other.StationID != null)
            {
                List<string> thisStationId = new List<string>();
                foreach (SdChannelDownload stationId in this.StationID)
                {
                    thisStationId.Add(stationId.StationID);
                }
                List<string> otherStationId = new List<string>();
                foreach (SdChannelDownload stationId in other.StationID)
                {
                    otherStationId.Add(stationId.StationID);
                }

                foreach (string stationId in thisStationId)
                {
                    if (!otherStationId.Contains(stationId)) return false;
                }
                foreach (string stationId in otherStationId)
                {
                    if (!thisStationId.Contains(stationId)) return false;
                }
            }
            else if (this.StationID == null ^ other.StationID == null) return false;

            return true;
        }

        [XmlAttribute("version")]
        public string version { get; set; } = Helper.epg123Version;

        [XmlElement("UserAccount")]
        public SdUserAccount UserAccount { get; set; }

        [XmlElement("RatingsOrigin")]
        public string RatingsOrigin { get; set; } = RegionInfo.CurrentRegion.ThreeLetterISORegionName;

        [XmlElement("DaysToDownload")]
        public int DaysToDownload { get; set; } = 14;

        [XmlElement("TheTVDBNumbers")]
        public bool TheTVDBNumbers { get; set; } = true;

        [XmlElement("PrefixEpisodeTitle")]
        public bool PrefixEpisodeTitle { get; set; } = false;

        [XmlElement("PrefixEpisodeDescription")]
        public bool PrefixEpisodeDescription { get; set; } = false;

        [XmlElement("AlternateSEFormat")]
        public bool AlternateSEFormat { get; set; } = false;

        [XmlElement("AppendEpisodeDesc")]
        public bool AppendEpisodeDesc { get; set; } = false;

        [XmlElement("OADOverride")]
        public bool OADOverride { get; set; } = true;

        [XmlElement("SeriesPosterArt")]
        public bool SeriesPosterArt { get; set; } = false;

        [XmlElement("TMDbCoverArt")]
        public bool TMDbCoverArt { get; set; } = true;

        [XmlElement("IncludeSDLogos")]
        public bool IncludeSDLogos { get; set; } = true;

        [XmlElement("PreferredLogoStyle")]
        public string PreferredLogoStyle { get; set; } = "dark";

        [XmlElement("AlternateLogoStyle")]
        public string AlternateLogoStyle { get; set; } = "white";

        [XmlElement("AutoAddNew")]
        public bool AutoAddNew { get; set; } = true;

        [XmlElement("AutoImport")]
        public bool AutoImport { get; set; } = true;

        [XmlElement("Automatch")]
        public bool Automatch { get; set; } = true;

        [XmlElement("CreateXmltv")]
        public bool CreateXmltv { get; set; } = false;

        [XmlElement("XmltvIncludeChannelNumbers")]
        public bool XmltvIncludeChannelNumbers { get; set; } = true;

        [XmlElement("XmltvIncludeChannelLogos")]
        public string XmltvIncludeChannelLogos { get; set; } = "url";

        [XmlElement("XmltvLogoSubtitutePath")]
        public string XmltvLogoSubstitutePath { get; set; } = string.Empty;

        [XmlElement("XmltvAddFillerData")]
        public bool XmltvAddFillerData { get; set; } = true;

        [XmlElement("XmltvFillerProgramLength")]
        public int XmltvFillerProgramLength { get; set; } = 4;

        [XmlElement("XmltvFillerProgramDescription")]
        public string XmltvFillerProgramDescription { get; set; } = "This program was generated by EPG123 to provide filler data for stations that did not receive any guide listings from the upstream source.";

        [XmlElement("XmltvOutputFile")]
        public string XmltvOutputFile { get; set; } = Helper.Epg123XmltvPath;

        [XmlElement("ModernMediaUiPlusSupport")]
        public bool ModernMediaUiPlusSupport { get; set; } = false;

        [XmlElement("ModernMediaUiPlusJsonFilepath")]
        public string ModernMediaUiPlusJsonFilepath { get; set; }

        [XmlElement("BrandLogoImage")]
        public string BrandLogoImage { get; set; } = "none";

        [XmlAnyElement("SuppressStationEmptyWarningsComment")]
        public XmlComment SuppressStationEmptyWarningsComment { get { return new XmlDocument().CreateComment(" SuppressStationEmptyWarnings: Enter specific station callsigns, comma delimited, to suppress warnings for no guide data, or use a wildcard (*) for a group of callsigns. A solitary wildcard means all station warnings will be suppressed."); } set { } }
        [XmlElement("SuppressStationEmptyWarnings")]
        public string SuppressStationEmptyWarnings { get; set; } = "GOAC*,LOOR*,EDAC*,LEAC*,PEG*,LOAC*,PPV*,PUAC*,SPALT*,INFO*";

        [XmlElement("IncludedLineup")]
        public List<string> IncludedLineup { get; set; }

        [XmlElement("ExpectedServiceCount")]
        public int ExpectedServicecount { get; set; } = 0;

        [XmlElement("StationID")]
        public List<SdChannelDownload> StationID { get; set; }
    }

    public class SdUserAccount
    {
        [XmlIgnore]
        private string _passwordHash;

        private string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = SHA1.Create().ComputeHash(bytes);
            return HexStringFromBytes(hashBytes);
        }
        private string HexStringFromBytes(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
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
            get
            {
                return _passwordHash;
            }
            set
            {
                _passwordHash = value;
            }
        }

        [XmlIgnore]
        public string Password
        {
            set
            {
                _passwordHash = HashPassword(value);
            }
        }
    }

    public class sdLineupDownload
    {
        [XmlAttribute("Lineup")]
        public string Lineup { get; set; }

        [XmlText()]
        public string LineupID { get; set; }
    }

    public class SdChannelDownload
    {
        [XmlAttribute("CallSign")]
        public string CallSign { get; set; }

        [XmlAttribute("HDOverride")]
        public bool HDOverride { get; set; }

        [XmlAttribute("SDOverride")]
        public bool SDOverride { get; set; }

        [XmlText()]
        public string StationID { get; set; }
    }
}
