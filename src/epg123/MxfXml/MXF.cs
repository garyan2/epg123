using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    [XmlRoot("MXF")]
    public class Mxf
    {
        private string _progName = "EPG123";
        private string _progDesc = "Electronic Program Guide in 1-2-3";
        private string _key = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
        private string _version = "6.1.0.0";
        private string _culture = string.Empty;

        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// </summary>
        public Mxf()
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
                    Uid = "!Image!EPG123",
                    Image = string.Empty
                }
            };

            // create provider entry
            Providers = new List<MxfProvider>
            {
                new MxfProvider
                {
                    Index = 1,
                    Name = _progName,
                    DisplayName = _progDesc,
                    Copyright = $"© {DateTime.Now.Year} GaRyan2. Portions of content provided by Gracenote."
                }
            };

            // establish all other branches
            With = new List<MxfWith>
            {
                new MxfWith
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
                }
            };
        }

        [XmlElement("Assembly")]
        public List<MxfAssembly> Assembly { get; set; }

        [XmlElement("DeviceGroup")]
        public MxfDeviceGroup DeviceGroup { get; set; }

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
        private readonly Dictionary<string, MxfGuideImage> _guideImages = new Dictionary<string, MxfGuideImage>();
        public MxfGuideImage GetGuideImage(string pathname, string image = null)
        {
            if (_guideImages.TryGetValue(pathname, out var guideImage)) return guideImage;
            GuideImages.Add(guideImage = new MxfGuideImage
            {
                Index = GuideImages.Count + 1,
                ImageUrl = pathname,
                Image = image
            });
            _guideImages.Add(pathname, guideImage);
            return guideImage;
        }

        [XmlArrayItem("Person")]
        public List<MxfPerson> People { get; set; }
        private readonly Dictionary<string, MxfPerson> _people = new Dictionary<string, MxfPerson>();
        public string GetPersonId(string name)
        {
            if (_people.TryGetValue(name, out var person)) return person.Id;
            People.Add(person = new MxfPerson
            {
                Index = People.Count + 1,
                Name = name
            });
            _people.Add(name, person);
            return person.Id;
        }

        [XmlArrayItem("SeriesInfo")]
        public List<MxfSeriesInfo> SeriesInfos { get; set; }
        private readonly Dictionary<string, MxfSeriesInfo> _seriesInfos = new Dictionary<string, MxfSeriesInfo>();
        public MxfSeriesInfo GetSeriesInfo(string seriesId)
        {
            if (_seriesInfos.TryGetValue(seriesId, out var seriesInfo)) return seriesInfo;
            SeriesInfos.Add(seriesInfo = new MxfSeriesInfo
            {
                Index = SeriesInfos.Count + 1,
                TmsSeriesId = seriesId,
            });
            _seriesInfos.Add(seriesId, seriesInfo);
            return seriesInfo;
        }

        [XmlArrayItem("Season")]
        public List<MxfSeason> Seasons { get; set; }
        private readonly Dictionary<string, MxfSeason> _seasons = new Dictionary<string, MxfSeason>();
        public string GetSeasonId(string seriesId, int seasonNumber)
        {
            if (_seasons.TryGetValue($"{seriesId}_{seasonNumber}", out var season)) return season.Id;
            Seasons.Add(season = new MxfSeason
            {
                Index = Seasons.Count + 1,
                SeasonNumber = seasonNumber,
                Series = GetSeriesInfo(seriesId).Id,
                Zap2It = seriesId
            });
            _seasons.Add(seriesId + "_" + seasonNumber, season);
            return season.Id;
        }

        [XmlArrayItem("Program")]
        public List<MxfProgram> Programs { get; set; }
        private readonly Dictionary<string, MxfProgram> _programs = new Dictionary<string, MxfProgram>();
        public MxfProgram GetProgram(string tmsId, MxfProgram mxfProgram = null)
        {
            if (_programs.TryGetValue(tmsId, out var program)) return program;
            Programs.Add(program = mxfProgram);
            _programs.Add(tmsId, program);
            return program;
        }

        [XmlArrayItem("Affiliate")]
        public List<MxfAffiliate> Affiliates { get; set; }
        private readonly Dictionary<string, MxfAffiliate> _affiliates = new Dictionary<string, MxfAffiliate>();
        public string GetAffiliateId(string affiliateName)
        {
            if (_affiliates.TryGetValue(affiliateName, out var affiliate)) return affiliate.Uid;
            Affiliates.Add(affiliate = new MxfAffiliate
            {
                Name = affiliateName
            });
            _affiliates.Add(affiliateName, affiliate);
            return affiliate.Uid;
        }

        [XmlArrayItem("Service")]
        public List<MxfService> Services { get; set; }
        private readonly Dictionary<string, MxfService> _services = new Dictionary<string, MxfService>();
        public MxfService GetService(string stationId)
        {
            if (_services.TryGetValue(stationId, out var service)) return service;
            Services.Add(service = new MxfService
            {
                Index = Services.Count + 1,
                StationId = stationId
            });
            ScheduleEntries.Add(service.MxfScheduleEntries);
            _services.Add(stationId, service);
            return service;
        }

        [XmlElement("ScheduleEntries")]
        public List<MxfScheduleEntries> ScheduleEntries { get; set; }

        [XmlArrayItem("Lineup")]
        public List<MxfLineup> Lineups { get; set; }
    }
}