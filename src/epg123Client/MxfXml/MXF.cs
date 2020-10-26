using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    [XmlRoot("MXF")]
    public class MXF
    {
        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// </summary>
        public MXF()
        {
        }

        [XmlElement("Assembly")]
        public List<MxfAssembly> Assembly { get; set; }

        //[XmlElement("DeviceGroup")]
        //public MxfDeviceGroup DeviceGroup { get; set; }

        [XmlArrayItem("Provider")]
        public List<MxfProvider> Providers { get; set; }

        [XmlElement("With")]
        public List<MxfWith> With { get; set; }
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

    public class MxfDeviceGroup
    {
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("lastConfigurationChange")]
        public string LastConfigurationChange { get; set; }

        [XmlAttribute("rank")]
        public string Rank { get; set; }

        [XmlAttribute("permitAnyDeviceType")]
        public string PermitAnyDeviceType { get; set; }

        [XmlAttribute("isEnabled")]
        public string IsEnabled { get; set; }

        [XmlAttribute("firstRunProcessId")]
        public string FirstRunProcessId { get; set; }

        [XmlAttribute("onlyShowDynamicLineups")]
        public string OnlyShowDynamicLineups { get; set; }

        public MxfGuideImage guideImage { get; set; }
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

    public class MxfWith
    {
        [XmlIgnore]
        public string progName { get; set; }

        [XmlAttribute("provider")]
        public string Provider { get; set; }

        //[XmlArrayItem("Keyword")]
        //public List<MxfKeyword> Keywords { get; set; }

        //[XmlArrayItem("KeywordGroup")]
        //public List<MxfKeywordGroup> KeywordGroups { get; set; }

        //[XmlArrayItem("GuideImage")]
        //public List<MxfGuideImage> GuideImages { get; set; }

        //[XmlArrayItem("Person")]
        //public List<MxfPerson> People { get; set; }

        //[XmlArrayItem("SeriesInfo")]
        //public List<MxfSeriesInfo> SeriesInfos { get; set; }

        //[XmlArrayItem("Season")]
        //public List<MxfSeason> Seasons { get; set; }

        [XmlArrayItem("Program")]
        public List<MxfProgram> Programs { get; set; }

        //[XmlArrayItem("Affiliate")]
        //public List<MxfAffiliate> Affiliates { get; set; }

        [XmlArrayItem("Service")]
        public List<MxfService> Services { get; set; }

        [XmlElement("ScheduleEntries")]
        public List<MxfScheduleEntries> ScheduleEntries { get; set; }

        //[XmlArrayItem("Lineup")]
        //public List<MxfLineup> Lineups { get; set; }
    }
}