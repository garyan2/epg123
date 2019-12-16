using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MxfXml
{
    [XmlRoot("MXF")]
    public class MXF
    {
        private string progName = "HDHR2MXF";
        private string progDesc = "EPG123 SiliconDust HDHR to MXF";
        private string key = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
        private string version = "6.1.0.0";
        private string culture = string.Empty;

        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// </summary>
        public MXF()
        {
            // create mcepg and mcstore assembly entries
            Assembly = new List<MxfAssembly>()
            {
                new MxfAssembly()
                {
                    Name = "mcepg",
                    Version = version,
                    CultureInfo = culture,
                    PublicKey = key,
                    Namespace = new MxfNamespace()
                    {
                        Name = "Microsoft.MediaCenter.Guide",
                        Type = new List<MxfType>()
                        {
                            //new MxfType() { Name = "DeviceGroup" },
                            new MxfType() { Name = "Lineup" },
                            new MxfType() { Name = "Channel", ParentFieldName = "lineup" },
                            new MxfType() { Name = "Service" },
                            new MxfType() { Name = "ScheduleEntry", GroupName = "ScheduleEntries" },
                            new MxfType() { Name = "Program" },
                            new MxfType() { Name = "Keyword" },
                            new MxfType() { Name = "KeywordGroup" },
                            new MxfType() { Name = "Person", GroupName = "People" },
                            new MxfType() { Name = "ActorRole", ParentFieldName = "program" },
                            new MxfType() { Name = "DirectorRole", ParentFieldName = "program" },
                            new MxfType() { Name = "WriterRole", ParentFieldName = "program" },
                            new MxfType() { Name = "HostRole", ParentFieldName = "program" },
                            new MxfType() { Name = "GuestActorRole", ParentFieldName = "program" },
                            new MxfType() { Name = "ProducerRole", ParentFieldName = "program" },
                            new MxfType() { Name = "GuideImage" },
                            new MxfType() { Name = "Affiliate" },
                            new MxfType() { Name = "SeriesInfo" },
                            new MxfType() { Name = "Season" }
                        }
                    }
                },
                new MxfAssembly()
                {
                    Name = "mcstore",
                    Version = version,
                    CultureInfo = culture,
                    PublicKey = key,
                    Namespace = new MxfNamespace()
                    {
                        Name = "Microsoft.MediaCenter.Store",
                        Type = new List<MxfType>()
                        {
                            new MxfType() { Name = "Provider" },
                            new MxfType() { Name = "UId", ParentFieldName = "target" }
                        }
                    }
                }
            };

            //// initialize the devicegroup with everything except the image
            //DeviceGroup = new MxfDeviceGroup()
            //{
            //    Uid = "!DeviceGroup!All",
            //    Name = "All",
            //    LastConfigurationChange = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            //    Rank = "0",
            //    PermitAnyDeviceType = "true",
            //    IsEnabled = "true",
            //    FirstRunProcessId = "0",
            //    OnlyShowDynamicLineups = "false",
            //    guideImage = new MxfGuideImage()
            //    {
            //        Uid = "!Image!HDHR",
            //        Image = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAYAAACM/rhtAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAABCxJREFUWEfNl2loXFUYhl8jlCLWapImmUwyiVnMOpM9zd60aYyxrS1ttVpLTRGKtVqqtVgzCe4LImLSWndFERcQFHEBURFBUFQQBEFcQJAEQRBEELRyfE8zn++5KQV/pffH8+c57733u3POd84dZP9G3tQJZKdPYH76H7hYwFp8TVOsDZN/IUtcTMniyJ+YJy6mzOPwH3BxBod+h4szOPgbXJzBgV/h4gz2/wIXZ7BvDi7OYO/PcHEG1/4EF7LzC7jqy+CW58MtW7E0+GdVbVp49uJ6sOdHOGPie7jCNCXODP7ZEz+oHg92fwdnbHj91IuWmo1vqB4Pdn0LZ4w8q2DqYrixVxeouVw+JL1fmaIO+aa98id5BW7983D9Dy2M5TdG7xMy8pzq8eCqb+CMtU8rWHulfPoG+ZCe+5VJrpXvvlP+dIy+CHd+bfR+nnXPRHPY8TWcseZJBWt2yDdfLx+y+l5lSoflO2+XH5iBa7kJrv02FvWyvGfbZ3AFmeg9h5+KZrD9Kzhj8HEFq6+Qb9wnH9J1tzKJNfId0/Jlo9Frirq4zt7X+PjbcHnLND70hMY82PolnDFwXMEqrjvzDdfJh3TepUxiSL5tSj65PnqNZ+VFcFs+VaZ8XGMDj8l7sPlzOKPvmIIXbpev58I2H9J+hzIlg/Ktk/KlI9Fr/svwJSzTziVhvu9ReQ828U2MnlkFK7fJj73HxfvaqYx/qEzxgK7NHJFPrJMPSbCpLNMfrP2eo/IeXPoJnNH9iIIVW+X/D0VBgelb5UuC7g65gM1hmUFuQeZ9DeY9uORjOKPzYQVTWwLP/auSTbOYXr65ZYr6dW3TYfmSoLtDirkkLNPDdWe+izWY92D0I7Z/jnYWYsHyzfLVe+RDmv3Wkcus6pNvPCRfHHR3SP1BZZqz8h2swbwHIx9w987R9qCCZfxgMF91jXxII6fSMoW98vU3yxcF3W2cU8b97h1lEmMa8zWY92CYDWC08GSwYJJfF+Yrd8uH1HMqLVPQI1/HX8f8qqC7PedWc0pf0HgfN++zl2u85QGNeTD0LjfHHGmeDBZMbJSv2CUfUneLMgWr5WsPyNdxupNsuNROrk3um4PcmG1s4E0ed63Re6bv07gH/W+xzXM08WSwYMkG+dTV8iG1nErL5AcF1twofzraeCisqI/ez9N0TzSHXr6F0cA3tGAh104ru8tTwl8zvImR4tq0zMrgTC3nC5k/CYvJcPto4IZcMZHLnqV8SCN/pLAmdPMb0MgEJ8mZIsOTJKwJnTwRQvKD/Wyp8c9eXA/a+EEZ0voSFzWn7rx2rhFOxVLgn5XkTuGfvbgeZNjmcQbN/LKNM2jgQR1nUMc/SnEGNfwPEGdQxf8AcQYVxzFHXEyZQ/lRTBIXUyaRnEVecgbZ0hnMl/K8jAWsxddUNou8fwG8hfn2f6N66AAAAABJRU5ErkJggg=="
            //    }
            //};

            // create provider entry
            Providers = new List<MxfProvider>()
            {
                new MxfProvider()
                {
                    index = 1,
                    Name = progName,
                    DisplayName = progDesc,
                    Copyright = string.Format("© {0} GaRyan2. Portions of content provided by SiliconDust.", DateTime.Now.Year)
                }
            };

            // establish all other branches
            With = new List<MxfWith>()
            {
                new MxfWith()
                {
                    progName = progName,
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

        [XmlArrayItem("Keyword")]
        public List<MxfKeyword> Keywords { get; set; }

        [XmlArrayItem("KeywordGroup")]
        public List<MxfKeywordGroup> KeywordGroups { get; set; }

        [XmlArrayItem("GuideImage")]
        public List<MxfGuideImage> GuideImages { get; set; }
        private Dictionary<string, MxfGuideImage> _guideImages = new Dictionary<string, MxfGuideImage>();
        public MxfGuideImage getGuideImage(string pathname, string image = null)
        {
            if (string.IsNullOrEmpty(pathname)) return null;
            if (!_guideImages.TryGetValue(pathname, out MxfGuideImage guideImage))
            {
                GuideImages.Add(guideImage = new MxfGuideImage()
                {
                    index = GuideImages.Count + 1,
                    ImageUrl = pathname,
                    Image = image
                });
                _guideImages.Add(pathname, guideImage);
            }
            return guideImage;
        }

        [XmlArrayItem("Person")]
        public List<MxfPerson> People { get; set; }
        private Dictionary<string, MxfPerson> _people = new Dictionary<string, MxfPerson>();
        public string getPersonId(string name)
        {
            if (!_people.TryGetValue(name, out MxfPerson person))
            {
                People.Add(person = new MxfPerson()
                {
                    index = People.Count + 1,
                    Name = name
                });
                _people.Add(name, person);
            }
            return person.Id;
        }

        [XmlArrayItem("SeriesInfo")]
        public List<MxfSeriesInfo> SeriesInfos { get; set; }
        private Dictionary<string, MxfSeriesInfo> _seriesInfos = new Dictionary<string, MxfSeriesInfo>();
        public MxfSeriesInfo getSeriesInfo(string seriesId)
        {
            if (!_seriesInfos.TryGetValue(seriesId, out MxfSeriesInfo seriesInfo))
            {
                SeriesInfos.Add(seriesInfo = new MxfSeriesInfo()
                {
                    index = SeriesInfos.Count + 1,
                    tmsSeriesId = seriesId,
                });
                _seriesInfos.Add(seriesId, seriesInfo);
            }
            return seriesInfo;
        }

        [XmlArrayItem("Season")]
        public List<MxfSeason> Seasons { get; set; }
        private Dictionary<string, MxfSeason> _seasons = new Dictionary<string, MxfSeason>();
        public string getSeasonId(string seriesId, string seasonNumber)
        {
            if (!_seasons.TryGetValue(seriesId + "_" + seasonNumber, out MxfSeason season))
            {
                Seasons.Add(season = new MxfSeason()
                {
                    index = Seasons.Count + 1,
                    SeasonNumber = seasonNumber,
                    Series = getSeriesInfo(seriesId).Id,
                    Title = string.Format("{0}, Season {1}", getSeriesInfo(seriesId).Title, seasonNumber),
                    zap2it = seriesId
                });
                _seasons.Add(seriesId + "_" + seasonNumber, season);
            }
            return season.Id;
        }

        [XmlArrayItem("Program")]
        public List<MxfProgram> Programs { get; set; }
        private Dictionary<string, MxfProgram> _programs = new Dictionary<string, MxfProgram>();
        public MxfProgram getProgram(string tmsId, MxfProgram mxfProgram = null)
        {
            if (!_programs.TryGetValue(tmsId, out MxfProgram program))
            {
                Programs.Add(program = mxfProgram);
                _programs.Add(tmsId, program);
            }
            return program;
        }

        [XmlArrayItem("Affiliate")]
        public List<MxfAffiliate> Affiliates { get; set; }
        private Dictionary<string, MxfAffiliate> _affiliates = new Dictionary<string, MxfAffiliate>();
        public string getAffiliateId(string affiliateName)
        {
            if (!_affiliates.TryGetValue(affiliateName, out MxfAffiliate affiliate))
            {
                Affiliates.Add(affiliate = new MxfAffiliate()
                {
                    Name = affiliateName
                });
                _affiliates.Add(affiliateName, affiliate);
            }
            return affiliate.Uid;
        }

        [XmlArrayItem("Service")]
        public List<MxfService> Services { get; set; }
        private Dictionary<string, MxfService> _services = new Dictionary<string, MxfService>();
        public MxfService getService(string stationId)
        {
            if (!_services.TryGetValue(stationId, out MxfService service))
            {
                Services.Add(service = new MxfService()
                {
                    index = Services.Count + 1,
                    StationID = stationId
                });
                ScheduleEntries.Add(service.mxfScheduleEntries);
                _services.Add(stationId, service);
            }
            return service;
        }

        [XmlElement("ScheduleEntries")]
        public List<MxfScheduleEntries> ScheduleEntries { get; set; }

        [XmlArrayItem("Lineup")]
        public List<MxfLineup> Lineups { get; set; }
        private Dictionary<string, MxfLineup> _lineups = new Dictionary<string, MxfLineup>();
        public MxfLineup getLineup(string hdhrType)
        {
            if (!_lineups.TryGetValue(hdhrType, out MxfLineup lineup))
            {
                Lineups.Add(lineup = new MxfLineup()
                {
                    index = Lineups.Count + 1,
                    Uid = "EPG123-HDHR2MXF-" + hdhrType,
                    Name = string.Format("EPG123 HDHR-{0} to MXF Converter", hdhrType),
                    channels = new List<MxfChannel>()
                });
                _lineups.Add(hdhrType.ToUpper(), lineup);
            }
            return lineup;
        }
    }
}