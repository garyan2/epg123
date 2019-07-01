using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace epg123
{
    [XmlRoot("EPG123")]
    public class epgConfig
    {
        [XmlAttribute("version")]
        public string version { get; set; }

        [XmlElement("UserAccount")]
        public SdUserAccount UserAccount { get; set; }

        [XmlElement("RatingsOrigin")]
        public string RatingsOrigin { get; set; }

        [XmlElement("DaysToDownload")]
        public int DaysToDownload { get; set; }

        [XmlElement("TheTVDBNumbers")]
        public bool TheTVDBNumbers { get; set; }

        [XmlElement("PrefixEpisodeTitle")]
        public bool PrefixEpisodeTitle { get; set; }

        [XmlElement("PrefixEpisodeDescription")]
        public bool PrefixEpisodeDescription { get; set; }

        [XmlElement("AppendEpisodeDesc")]
        public bool AppendEpisodeDesc { get; set; }

        [XmlElement("OADOverride")]
        public bool OADOverride { get; set; }

        [XmlElement("SeriesPosterArt")]
        public bool SeriesPosterArt { get; set; }

        [XmlElement("TMDbCoverArt")]
        public bool TMDbCoverArt { get; set; }

        [XmlElement("IncludeSDLogos")]
        public bool IncludeSDLogos { get; set; }

        [XmlElement("AutoAddNew")]
        public bool AutoAddNew { get; set; }

        [XmlElement("AutoImport")]
        public bool AutoImport { get; set; }

        [XmlElement("Automatch")]
        public bool Automatch { get; set; }

        [XmlElement("CreateXmltv")]
        public bool CreateXmltv { get; set; }

        [XmlElement("XmltvIncludeChannelNumbers")]
        public bool XmltvIncludeChannelNumbers { get; set; }

        [XmlElement("XmltvIncludeChannelLogos")]
        public string XmltvIncludeChannelLogos { get; set; }

        [XmlElement("XmltvLogoSubtitutePath")]
        public string XmltvLogoSubstitutePath { get; set; }

        [XmlElement("ModernMediaUiPlusSupport")]
        public bool ModernMediaUiPlusSupport { get; set; }

        [XmlElement("ModernMediaUiPlusJsonFilepath")]
        public string ModernMediaUiPlusJsonFilepath { get; set; }

        [XmlAnyElement("BrandLogoImageComment")]
        public XmlComment BrandLogoImageComment { get { return new XmlDocument().CreateComment(" BrandLogoImage: Allowed values are 'none', 'light', and 'dark'. "); } set { } }
        [XmlElement("BrandLogoImage")]
        public string BrandLogoImage { get; set; }

        [XmlAnyElement("SuppressStationEmptyWarningsComment")]
        public XmlComment SuppressStationEmptyWarningsComment { get { return new XmlDocument().CreateComment(" SuppressStationEmptyWarnings: Enter specific station callsigns, comma delimited, to suppress warnings for no guide data, or use a wildcard (*) for a group of callsigns. A solitary wildcard means all station warnings will be suppressed."); } set { } }
        [XmlElement("SuppressStationEmptyWarnings")]
        public string SuppressStationEmptyWarnings { get; set; }

        [XmlElement("IncludedLineup")]
        public List<string> IncludedLineup { get; set; }

        [XmlElement("ExpectedServiceCount")]
        public int ExpectedServicecount { get; set; }

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
