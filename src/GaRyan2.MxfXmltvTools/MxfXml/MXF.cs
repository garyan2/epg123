using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    [XmlRoot("MXF")]
    public partial class MXF
    {
        public enum TYPEMXF
        {
            EPG,
            SATELLITES,
            RECORDINGS
        };

        /// <summary>
        /// Definitions for MXF xml format can be located at
        /// https://msdn.microsoft.com/en-us/library/dd776338.aspx
        /// Note that this class is limited to a single provider
        /// </summary>
        /// <param name="generatorName">name of software used to generate mxf file</param>
        /// <param name="generatorDescription">description of software used to generate mxf file</param>
        /// <param name="author">author of software used to generate mxf file</param>
        /// <param name="dataSource">identify data source used to populate mxf file</param>
        /// <param name="mxfType">identify how to initialize mxf based on intended content</param>
        public MXF(string generatorName, string generatorDescription, string author, string dataSource, TYPEMXF mxfType = TYPEMXF.EPG)
        {
            string _key = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
            string _version = "6.0.6000.0"; // Vista=6.0.6000.0, Win7=6.1.0.0, Win8=6.2.0.0, Win8.1=6.3.0.0
            string _culture = string.Empty;

            // create mcepg and mcstore assembly entries
            Assembly = new List<MxfAssembly>();
            if (mxfType == TYPEMXF.EPG)
            {
                Assembly = new List<MxfAssembly>
                {
                    new MxfAssembly
                    {
                        Name = "mcepg",
                        Version = _version,
                        CultureInfo = _culture,
                        PublicKey = _key,
                        Namespace = new List<MxfNamespace>
                        {
                            new MxfNamespace
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
                        }
                    }
                };

                // initialize the devicegroup with everything except the image
                DeviceGroup = new MxfDeviceGroup
                {
                    Uid = "!DeviceGroup!All",
                    Name = "All",
                    LastConfigurationChange = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    Rank = "0",
                    PermitAnyDeviceType = "true",
                    IsEnabled = "true",
                    FirstRunProcessId = "0",
                    OnlyShowDynamicLineups = "false",
                    GuideImage = new MxfGuideImage(0, null, string.Empty)
                    {
                        Uid = $"!Image!{generatorName}",
                        Image = "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAoCAYAAABOzvzpAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAhSURBVGhD7cEBDQAAAMKg909tDwcEAAAAAAAAAAAAnKoBKCgAAWgZruEAAAAASUVORK5CYII="
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

                // initialize keywordgroups
                for (int i = 0; i < KeywordGroupsText.Length; ++i)
                {
                    _ = FindOrCreateKeywordGroup((KeywordGroups)i);
                }
            }
            else if (mxfType == TYPEMXF.SATELLITES)
            {
                Assembly = new List<MxfAssembly>
                {
                    new MxfAssembly
                    {
                        Name = "mcepg",
                        Version = _version,
                        CultureInfo = _culture,
                        PublicKey = _key,
                        Namespace = new List<MxfNamespace>
                        {
                            new MxfNamespace
                            {
                                Name = "Microsoft.MediaCenter.Satellites",
                                Type = new List<MxfType>
                                {
                                    new MxfType { Name = "DvbsDataSet" },
                                    new MxfType { Name = "DvbsSatellite" },
                                    new MxfType { Name = "DvbsRegion" },
                                    new MxfType { Name = "DvbsHeadend" },
                                    new MxfType { Name = "DvbsTransponder" },
                                    new MxfType { Name = "DvbsFootprint" },
                                    new MxfType { Name = "DvbsChannel" },
                                    new MxfType { Name = "DvbsService" }
                                }
                            }
                        }
                    }
                };

                // create dvbsdataset
                DvbsDataSet = new MxfDvbsDataSet
                {
                    Uid = "!DvbsDataSet",
                    FrequencyTolerance = 10,
                    SymbolRateTolerance = 1500,
                    MinimumSearchMatches = 3,
                    DataSetRevision = 59
                };
            }
            else if (mxfType == TYPEMXF.RECORDINGS)
            {
                Assembly = new List<MxfAssembly>
                {
                    new MxfAssembly
                    {
                        Name = "mcepg",
                        Version = _version,
                        CultureInfo = _culture,
                        PublicKey = _key,
                        Namespace = new List<MxfNamespace>
                        {
                            new MxfNamespace
                            {
                                Name = "Microsoft.MediaCenter.Guide",
                                Type = new List<MxfType>
                                {
                                    new MxfType { Name = "Progam" },
                                    new MxfType { Name = "Service" },
                                    new MxfType { Name = "MergedChannel" },
                                    new MxfType { Name = "SeriesInfo" }
                                }
                            }
                        }
                    }
                };
            }

            // add mcstore assembly
            Assembly.Add(new MxfAssembly
            {
                Name = "mcstore",
                Version = _version,
                CultureInfo = _culture,
                PublicKey = _key,
                Namespace = new List<MxfNamespace>
                {
                    new MxfNamespace
                    {
                        Name = "Microsoft.MediaCenter.Store",
                        Type = new List<MxfType>
                        {
                            new MxfType { Name = "Provider" },
                            new MxfType { Name = "UId", ParentFieldName = "target" }
                        }
                    }
                }
            });
        }
        private MXF() { }

        [XmlElement("Assembly")]
        public List<MxfAssembly> Assembly { get; set; }

        [XmlElement("DeviceGroup")]
        public MxfDeviceGroup DeviceGroup { get; set; }

        [XmlArrayItem("Provider")]
        public List<MxfProvider> Providers { get; set; }

        [XmlElement("With")]
        public MxfWith With { get; set; }

        [XmlElement("OneTimeRequest")]
        public List<MxfRequest> OneTimeRequest { get; set; }

        [XmlElement("ManualRequest")]
        public List<MxfRequest> ManualRequest { get; set; }

        [XmlElement("SeriesRequest")]
        public List<MxfRequest> SeriesRequest { get; set; }

        [XmlElement("WishListRequest")]
        public List<MxfRequest> WishListRequest { get; set; }

        [XmlElement("DvbsDataSet")]
        public MxfDvbsDataSet DvbsDataSet { get; set; }
    }
}