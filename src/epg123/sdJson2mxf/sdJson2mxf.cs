using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using epg123.MxfXml;
using epg123.SchedulesDirect;
using epg123.TheMovieDbAPI;
using epg123.XmltvXml;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        public static System.ComponentModel.BackgroundWorker BackgroundWorker;
        public static bool Success;

        private static epgConfig config;
        public static Mxf SdMxf = new Mxf
        {
            generatorName = "EPG123",
            generatorDescription = $"Electronic Program Guide in 1-2-3 v{Helper.Epg123Version}",
            author = "GaRyan2",
            dataSource = "Schedules Direct"
        };
        private static string HostAddress => string.IsNullOrEmpty(config.UseIpAddress) ? Environment.MachineName : config.UseIpAddress;

        public static void Build(epgConfig configuration)
        {
            // initialize schedules direct API
            SdApi.Initialize($"EPG123/{Helper.Epg123Version}");
            SdMxf.InitializeMxf();

            // copy configuration to local variable
            config = configuration;

            // initialize event buffer
            Logger.WriteInformation($"Beginning EPG123 update execution. {DateTime.Now.ToUniversalTime():u}");
            Logger.WriteVerbose($"DaysToDownload: {config.DaysToDownload} , TheTVDBNumbers : {config.TheTvdbNumbers} , PrefixEpisodeTitle: {config.PrefixEpisodeTitle} , PrefixEpisodeDescription : {config.PrefixEpisodeDescription} , AppendEpisodeDesc: {config.AppendEpisodeDesc} , OADOverride : {config.OadOverride} , SeasonEventImages : {config.SeasonEventImages} , TMDbCoverArt: {config.TMDbCoverArt} , IncludeSDLogos : {config.IncludeSdLogos} , AutoAddNew: {config.AutoAddNew} , CreateXmltv: {config.CreateXmltv} , ModernMediaUiPlusSupport: {config.ModernMediaUiPlusSupport}");

            // populate station prefixes to suppress
            suppressedPrefixes = new List<string>(config.SuppressStationEmptyWarnings.Split(','));

            // login to Schedules Direct and build the mxf file
            if (SdApi.GetToken(config.UserAccount.LoginName, config.UserAccount.PasswordHash))
            {
                // check server status
                var susr = SdApi.GetUserStatus();
                if (susr != null && susr.SystemStatus[0].Status.ToLower().Equals("offline"))
                {
                    Logger.WriteError("Schedules Direct server is offline. Aborting update.");
                    return;
                }

                // check for latest version and update the display name that shows in About Guide
                var scvr = SdApi.GetClientVersion();
                if (scvr != null && scvr.Version != Helper.Epg123Version)
                {
                    SdMxf.Providers[0].DisplayName += $" (v{scvr.Version} Available)";
                    BrandLogo.UpdateAvailable = true;
                }

                // make sure cache directory exists
                if (!Directory.Exists(Helper.Epg123CacheFolder))
                {
                    Directory.CreateDirectory(Helper.Epg123CacheFolder);
                }
                epgCache.LoadCache();

                // initialize tmdb api
                if (config.TMDbCoverArt)
                {
                    tmdbApi.Initialize(false);
                }

                // prepopulate keyword groups
                InitializeKeywordGroups();

                // read all included and excluded station from configuration
                PopulateIncludedExcludedStations(config.StationId);

                // if all components of the mxf file have been successfully created, save the file
                if (BuildLineupServices() && ServiceCountSafetyCheck() &&
                      GetAllScheduleEntryMd5S(config.DaysToDownload) &&
                      BuildAllProgramEntries() &&
                      BuildAllGenericSeriesInfoDescriptions() && 
                      BuildAllExtendedSeriesDataForUiPlus() &&
                      GetAllMoviePosters() &&
                      GetAllSeriesImages() &&
                      GetAllSeasonImages() &&
                      GetAllSportsImages() &&
                      BuildKeywords() &&
                      WriteMxf())
                {
                    Success = true;

                    // create the xmltv file if desired
                    if (config.CreateXmltv && CreateXmltvFile())
                    {
                        WriteXmltv();
                        ++processedObjects; ReportProgress();
                    }

                    // remove the guide images xml file
                    Helper.DeleteFile(Helper.Epg123GuideImagesXmlPath);

                    // create the ModernMedia UI+ json file if desired
                    if (config.ModernMediaUiPlusSupport)
                    {
                        ModernMediaUiPlus.WriteModernMediaUiPlusJson(config.ModernMediaUiPlusJsonFilepath ?? null);
                        ++processedObjects; ReportProgress();
                    }

                    // clean the cache folder of stale data
                    CleanCacheFolder();
                    epgCache.WriteCache();

                    Logger.WriteInformation("Completed EPG123 update execution. SUCCESS.");
                }
            }
            SdMxf = null;
        }

        private static void AddBrandLogoToMxf()
        {
            using (var ms = new MemoryStream())
            {
                BrandLogo.StatusImage(config.BrandLogoImage).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                SdMxf.DeviceGroup.GuideImage.Image = Convert.ToBase64String(ms.ToArray());
            }
        }

        private static bool ServiceCountSafetyCheck()
        {
            if (config.ExpectedServicecount < 20 || !(config.ExpectedServicecount - MissingStations < config.ExpectedServicecount * 0.95)) return true;
            Logger.WriteError($"Of the expected {config.ExpectedServicecount} stations to download, there are only {SdMxf.With.Services.Count - AddedStations} stations available from Schedules Direct. Aborting update for review by user.");
            return false;
        }

        private static bool WriteMxf()
        {
            Logger.WriteVerbose($"Downloaded and processed {SdApi.DownloadedBytes} of data from Schedules Direct.");

            // add dummy lineup with dummy channel
            var service = SdMxf.GetService("DUMMY");
            service.CallSign = "DUMMY";
            service.Name = "DUMMY Station";

            SdMxf.With.Lineups.Add(new MxfLineup
            {
                Index = SdMxf.With.Lineups.Count + 1,
                LineupId = "ZZZ-DUMMY-EPG123",
                Name = "ZZZ123 Dummy Lineup",
                channels = new List<MxfChannel>()
            });

            var lineupIndex = SdMxf.With.Lineups.Count - 1;
            SdMxf.With.Lineups[lineupIndex].channels.Add(new MxfChannel
            {
                mxfService = service,
                mxfLineup = SdMxf.With.Lineups[lineupIndex],
            });

            // make sure background worker to download station logos is complete
            processedObjects = 0; totalObjects = 1;
            ++processStage; ReportProgress();
            var waits = 0;
            while (!StationLogosDownloadComplete)
            {
                ++waits;
                System.Threading.Thread.Sleep(100);
            }
            if (waits > 0)
            {
                Logger.WriteInformation($"Waited {waits * 0.1} seconds for the background worker to complete station logo downloads prior to saving files.");
            }

            // reset counters
            processedObjects = 0; totalObjects = 1 + (config.CreateXmltv ? 1 : 0) + (config.ModernMediaUiPlusSupport ? 1 : 0);
            ++processStage; ReportProgress();

            AddBrandLogoToMxf();
            SdMxf.Providers[0].Status = Logger.EventId.ToString();
            try
            {
                using (var stream = new StreamWriter(Helper.Epg123MxfPath, false, Encoding.UTF8))
                {
                    using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
                    {
                        var serializer = new XmlSerializer(typeof(Mxf));
                        var ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        serializer.Serialize(writer, SdMxf, ns);
                    }
                }

                var fi = new FileInfo(Helper.Epg123MxfPath);
                Logger.WriteInformation($"Completed save of the MXF file to \"{Helper.Epg123MxfPath}\". ({Helper.BytesToString(fi.Length)})");
                Logger.WriteVerbose($"Generated .mxf file contains {SdMxf.With.Services.Count - 1} services, {SdMxf.With.SeriesInfos.Count} series, {SdMxf.With.Seasons.Count} seasons, {SdMxf.With.Programs.Count} programs, {SdMxf.With.ScheduleEntries.Sum(x => x.ScheduleEntry.Count)} schedule entries, and {SdMxf.With.People.Count} people with {SdMxf.With.GuideImages.Count} image links.");
                ++processedObjects; ReportProgress();
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to save the MXF file to \"{Helper.Epg123MxfPath}\". Message: {ex.Message}");
            }
            return false;
        }

        private static void CleanCacheFolder()
        {
            var delCnt = 0;
            var cacheFiles = Directory.GetFiles(Helper.Epg123CacheFolder, "*.*");

            // reset counters
            processedObjects = 0; totalObjects = cacheFiles.Length;
            ++processStage; ReportProgress();

            foreach (var file in cacheFiles)
            {
                ++processedObjects; ReportProgress();
                if (file.Equals(Helper.Epg123CacheJsonPath)) continue;
                if (file.Equals(Helper.Epg123CompressCachePath)) continue;
                if (Helper.DeleteFile(file)) ++delCnt;
            }

            if (delCnt > 0)
            {
                Logger.WriteInformation($"{delCnt} files deleted from the cache directory during cleanup.");
            }
        }

        private static void WriteXmltv()
        {
            if (!config.CreateXmltv) return;
            try
            {
                using (var stream = new StreamWriter(config.XmltvOutputFile, false, Encoding.UTF8))
                {
                    using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
                    {
                        var serializer = new XmlSerializer(typeof(xmltv));
                        var ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        serializer.Serialize(writer, xmltv, ns);
                    }
                }

                var fi = new FileInfo(config.XmltvOutputFile);
                Logger.WriteInformation($"Completed save of the XMLTV file to \"{config.XmltvOutputFile}\". ({Helper.BytesToString(fi.Length)})");
                Logger.WriteVerbose($"Generated .xmltv file contains {xmltv.Channels.Count} channels and {xmltv.Programs.Count} programs.");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to save the XMLTV file to \"{config.XmltvOutputFile}\". Message: {ex.Message}");
            }
        }
    }
}