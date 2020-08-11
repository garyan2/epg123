using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using epg123.MxfXml;
using epg123.XmltvXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        public static System.ComponentModel.BackgroundWorker backgroundWorker;
        public static bool success = false;
        public static DateTime startTime = DateTime.UtcNow;

        private static epgConfig config;
        public static MXF sdMxf = new MXF();

        public static void Build(epgConfig configuration)
        {
            string errString = string.Empty;

            // initialize schedules direct API
            sdAPI.Initialize("EPG123", epg123Version);

            // copy configuration to local variable
            config = configuration;

            // initialize event buffer
            Logger.WriteInformation(string.Format("Beginning EPG123 update execution. {0:u}", DateTime.Now.ToUniversalTime()));
            Logger.WriteVerbose(string.Format("DaysToDownload: {0} , TheTVDBNumbers : {1} , PrefixEpisodeTitle: {2} , PrefixEpisodeDescription : {3} , AppendEpisodeDesc: {4} , OADOverride : {5} , TMDbCoverArt: {6} , IncludeSDLogos : {7} , AutoAddNew: {8} , CreateXmltv: {9} , ModernMediaUiPlusSupport: {10}",
                                config.DaysToDownload, config.TheTVDBNumbers, config.PrefixEpisodeTitle, config.PrefixEpisodeDescription, config.AppendEpisodeDesc, config.OADOverride, config.TMDbCoverArt, config.IncludeSDLogos, config.AutoAddNew, config.CreateXmltv, config.ModernMediaUiPlusSupport));

            // populate station prefixes to suppress
            suppressedPrefixes = new List<string>(config.SuppressStationEmptyWarnings.Split(','));

            // login to Schedules Direct and build the mxf file
            if (sdAPI.sdGetToken(config.UserAccount.LoginName, config.UserAccount.PasswordHash, ref errString))
            {
                // check server status
                sdUserStatusResponse susr = sdAPI.sdGetStatus();
                if (susr == null) return;
                else if (susr.SystemStatus[0].Status.ToLower().Equals("offline"))
                {
                    Logger.WriteError("Schedules Direct server is offline. Aborting update.");
                    return;
                }

                // check for latest version and update the display name that shows in About Guide
                sdClientVersionResponse scvr = sdAPI.sdCheckVersion();
                if ((scvr != null) && !string.IsNullOrEmpty(scvr.Version))
                {
                    sdMxf.Providers[0].DisplayName += " v" + epg123Version;
                    if (epg123Version != scvr.Version)
                    {
                        sdMxf.Providers[0].DisplayName += string.Format(" (v{0} Available)", scvr.Version);
                        BrandLogo.updateAvailable = true;
                    }
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
                    tmdbAPI.Initialize(false);
                }

                // prepopulate keyword groups
                initializeKeywordGroups();

                // read all image links archived in file
                getImageArchive();

                // read all included and excluded station from configuration
                populateIncludedExcludedStations(config.StationID);

                // if all components of the mxf file have been successfully created, save the file
                if (success = buildLineupServices() && serviceCountSafetyCheck() &&
                              getAllScheduleEntryMd5s(config.DaysToDownload) &&
                              buildAllProgramEntries() &&
                              buildAllGenericSeriesInfoDescriptions() && buildAllExtendedSeriesDataForUiPlus() &&
                              getAllMoviePosters() &&
                              getAllSeriesImages() &&
                              buildKeywords() &&
                              writeMxf())
                {
                    // create the xmltv file if desired
                    if (config.CreateXmltv && CreateXmltvFile())
                    {
                        writeXmltv();
                        ++processedObjects; reportProgress();
                    }

                    // remove the guide images xml file
                    if (File.Exists(Helper.Epg123GuideImagesXmlPath))
                    {
                        try
                        {
                            File.Delete(Helper.Epg123GuideImagesXmlPath);
                        }
                        catch { }
                    }

                    // create the ModernMedia UI+ json file if desired
                    if (config.ModernMediaUiPlusSupport)
                    {
                        ModernMediaUiPlus.WriteModernMediaUiPlusJson(config.ModernMediaUiPlusJsonFilepath ?? null);
                        ++processedObjects; reportProgress();
                    }

                    // clean the cache folder of stale data
                    cleanCacheFolder();
                    epgCache.WriteCache();

                    Logger.WriteVerbose(string.Format("Downloaded and processed {0} of data from Schedules Direct.", sdAPI.TotalDownloadBytes));
                    Logger.WriteVerbose(string.Format("Generated .mxf file contains {0} services, {1} series, {2} programs, and {3} people with {4} image links.",
                                        sdMxf.With[0].Services.Count, sdMxf.With[0].SeriesInfos.Count, sdMxf.With[0].Programs.Count, sdMxf.With[0].People.Count, sdMxf.With[0].GuideImages.Count));
                    Logger.WriteInformation("Completed EPG123 update execution. SUCCESS.");
                }
                else
                {
                    Logger.WriteError("Failed to create MXF file. Exiting.");
                }
            }
            else
            {
                Logger.WriteError(string.Format("Failed to retrieve token from Schedules Direct. message: {0}", errString));
            }
            Helper.SendPipeMessage("Download Complete");
        }
        private static void AddBrandLogoToMxf()
        {
            using (var ms = new MemoryStream())
            {
                BrandLogo.statusImage(config.BrandLogoImage).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                sdMxf.DeviceGroup.guideImage.Image = Convert.ToBase64String(ms.ToArray());
            }
        }

        private static bool serviceCountSafetyCheck()
        {
            if ((double)sdMxf.With[0].Services.Count < config.ExpectedServicecount * 0.95)
            {
                Logger.WriteError(string.Format("The expected number of stations to download is {0} but there are only {1} stations available from Schedules Direct. Aborting update for review by user.",
                                  config.ExpectedServicecount, sdMxf.With[0].Services.Count));
                return false;
            }
            return true;
        }

        private static bool writeMxf()
        {
            // add dummy lineup with dummy channel
            MxfService service = sdMxf.With[0].getService("DUMMY");
            service.CallSign = "DUMMY";
            service.Name = "DUMMY Station";

            sdMxf.With[0].Lineups.Add(new MxfLineup()
            {
                index = sdMxf.With[0].Lineups.Count + 1,
                Uid = "ZZZ-DUMMY-EPG123",
                Name = "ZZZ123 Dummy Lineup",
                channels = new List<MxfChannel>()
            });

            int lineupIndex = sdMxf.With[0].Lineups.Count - 1;
            sdMxf.With[0].Lineups[lineupIndex].channels.Add(new MxfChannel()
            {
                Lineup = sdMxf.With[0].Lineups[lineupIndex].Id,
                lineupUid = "ZZZ-DUMMY-EPG123",
                stationId = service.StationID,
                Service = service.Id
            });

            // make sure background worker to download station logos is complete
            int waits = 0;
            while (!stationLogosDownloadComplete)
            {
                ++waits;
                System.Threading.Thread.Sleep(100);
            }
            if (waits > 0)
            {
                Logger.WriteInformation(string.Format("Waited {0} seconds for the background worker to complete station logo downloads prior to saving files.", waits * 0.1));
            }

            // reset counters
            processedObjects = 0; totalObjects = 1 + (config.CreateXmltv ? 1 : 0) + (config.ModernMediaUiPlusSupport ? 1 : 0);
            ++processStage; reportProgress();

            AddBrandLogoToMxf();
            sdMxf.Providers[0].Status = Logger.eventID.ToString();
            try
            {
                using (StreamWriter stream = new StreamWriter(Helper.Epg123MxfPath, false, Encoding.UTF8))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MXF));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    TextWriter writer = stream;
                    serializer.Serialize(writer, sdMxf, ns);
                }

                Logger.WriteInformation(string.Format("Completed save of the MXF file to \"{0}\".", Helper.Epg123MxfPath));
                ++processedObjects; reportProgress();
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Failed to save the MXF file to \"{0}\". Message: {1}", Helper.Epg123MxfPath, ex.Message));
            }
            return false;
        }

        private static void cleanCacheFolder()
        {
            int delCnt = 0;
            string[] cacheFiles = Directory.GetFiles(Helper.Epg123CacheFolder, "*.*");

            // reset counters
            processedObjects = 0; totalObjects = cacheFiles.Length;
            ++processStage; reportProgress();

            foreach (string file in cacheFiles)
            {
                ++processedObjects; reportProgress();
                if (file.Equals(Helper.Epg123CacheJsonPath)) continue;
                if (file.Equals(Helper.Epg123CompressCachePath)) continue;

                //if (File.GetLastAccessTimeUtc(file) < startTime)
                {
                    try
                    {
                        File.Delete(file);
                        ++delCnt;
                    }
                    catch { }
                }
            }

            if (delCnt > 0)
            {
                Logger.WriteInformation(string.Format("{0} files deleted from the cache directory during cleanup.", delCnt));
            }
        }

        private static bool writeXmltv()
        {
            if (config.CreateXmltv)
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(config.XmltvOutputFile, false, Encoding.UTF8))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(XMLTV));
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        TextWriter writer = stream;
                        serializer.Serialize(writer, xmltv, ns);
                    }

                    Logger.WriteInformation(string.Format("Completed save of the XMLTV file to \"{0}\".", Helper.Epg123XmltvPath));
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteError(string.Format("Failed to save the XMLTV file to \"{0}\". Message: {1}", Helper.Epg123XmltvPath, ex.Message));
                }
            }

            return false;
        }
    }
}