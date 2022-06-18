using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    [XmlRoot("MXF")]
    public partial class Mxf
    {
        [XmlIgnore] public string generatorName;
        [XmlIgnore] public string generatorDescription;
        [XmlIgnore] public string author;
        [XmlIgnore] public string dataSource;

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
                        Name = "Microsoft.MediaCenter.Guide",
                        Type = new List<MxfType>
                        {
                            new MxfType { Name = "DeviceGroup" },
                            new MxfType { Name = "Lineup" },
                            new MxfType { Name = "Channel", ParentFieldName = "lineup" },
                            new MxfType { Name = "Service" },
                            new MxfType { Name = "ScheduleEntry", GroupName = "ScheduleEntries" },
                            new MxfType { Name = "Program" },
                            new MxfType { Name = "Keyword" },
                            new MxfType { Name = "KeywordGroup" },
                            new MxfType { Name = "Person", GroupName = "People" },
                            new MxfType { Name = "ActorRole", ParentFieldName = "program" },
                            new MxfType { Name = "DirectorRole", ParentFieldName = "program" },
                            new MxfType { Name = "WriterRole", ParentFieldName = "program" },
                            new MxfType { Name = "HostRole", ParentFieldName = "program" },
                            new MxfType { Name = "GuestActorRole", ParentFieldName = "program" },
                            new MxfType { Name = "ProducerRole", ParentFieldName = "program" },
                            new MxfType { Name = "GuideImage" },
                            new MxfType { Name = "Affiliate" },
                            new MxfType { Name = "SeriesInfo" },
                            new MxfType { Name = "Season" }
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

            // initialize the devicegroup with everything except the image
            DeviceGroup = new MxfDeviceGroup
            {
                Uid = "!DeviceGroup!All",
                Name = "All",
                LastConfigurationChange = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Rank = "0",
                PermitAnyDeviceType = "true",
                IsEnabled = "true",
                FirstRunProcessId = "0",
                OnlyShowDynamicLineups = "false",
                GuideImage = new MxfGuideImage
                {
                    Uid = $"!Image!{generatorName}",
                    Image = string.Empty
                }
            };

            // create provider entry
            Providers = new List<MxfProvider>
            {
                new MxfProvider
                {
                    Index = 1,
                    Name = generatorName,
                    DisplayName = generatorDescription,
                    Copyright = $"© {DateTime.Now.Year} {author}. Powered by {dataSource}."
                }
            };

            // establish all other branches
            With = new MxfWith
            {
                Provider = Providers[0].Id,
                Keywords = new List<MxfKeyword>(),
                KeywordGroups = new List<MxfKeywordGroup>(),
                GuideImages = new List<MxfGuideImage>(),
                People = new List<MxfPerson>(),
                SeriesInfos = new List<MxfSeriesInfo>(),
                Seasons = new List<MxfSeason>(),
                Programs = new List<MxfProgram>(),
                Affiliates = new List<MxfAffiliate>(),
                Services = new List<MxfService>(),
                ScheduleEntries = new List<MxfScheduleEntries>(),
                Lineups = new List<MxfLineup>()
            };
        }

        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// </summary>
        public Mxf()
        {
        }

        [XmlElement("Assembly")]
        public List<MxfAssembly> Assembly { get; set; }

        [XmlElement("DeviceGroup")]
        public MxfDeviceGroup DeviceGroup { get; set; }

        [XmlArrayItem("Provider")]
        public List<MxfProvider> Providers { get; set; }

        [XmlElement("With")]
        public MxfWith With { get; set; }
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

        [XmlElement("guideImage")]
        public MxfGuideImage GuideImage { get; set; }
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
        [XmlAttribute("provider")]
        public string Provider { get; set; }

        [XmlArrayItem("Keyword")]
        public List<MxfKeyword> Keywords { get; set; }

        [XmlArrayItem("KeywordGroup")]
        public List<MxfKeywordGroup> KeywordGroups { get; set; }

        [XmlArrayItem("GuideImage")]
        public List<MxfGuideImage> GuideImages { get; set; }

        [XmlArrayItem("Person")]
        public List<MxfPerson> People { get; set; }

        [XmlArrayItem("SeriesInfo")]
        public List<MxfSeriesInfo> SeriesInfos { get; set; }

        [XmlArrayItem("Season")]
        public List<MxfSeason> Seasons { get; set; }

        [XmlArrayItem("Program")]
        public List<MxfProgram> Programs { get; set; }

        [XmlArrayItem("Affiliate")]
        public List<MxfAffiliate> Affiliates { get; set; }

        [XmlArrayItem("Service")]
        public List<MxfService> Services { get; set; }

        [XmlElement("ScheduleEntries")]
        public List<MxfScheduleEntries> ScheduleEntries { get; set; }

        [XmlArrayItem("Lineup")]
        public List<MxfLineup> Lineups { get; set; }
    }
}