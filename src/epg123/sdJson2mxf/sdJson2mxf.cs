using System;
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
        private static DateTime startTime = DateTime.UtcNow - TimeSpan.FromMinutes(1.0);

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

                // initialize tmdb api
                tmdbAPI.Initialize(false);

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

                    // save the image links
                    writeImageArchive();

                    // create the ModernMedia UI+ json file if desired
                    if (config.ModernMediaUiPlusSupport)
                    {
                        ModernMediaUiPlus.WriteModernMediaUiPlusJson(config.ModernMediaUiPlusJsonFilepath ?? null);
                        ++processedObjects; reportProgress();
                    }

                    // clean the cache folder of stale data
                    cleanCacheFolder();

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
                if (File.GetLastAccessTimeUtc(file) < startTime)
                {
                    try
                    {
                        File.Delete(file);
                        ++delCnt;
                    }
                    catch { }
                }
            }
            Logger.WriteInformation(string.Format("{0} files deleted from the cache directory during cleanup.", delCnt));
        }

        private static bool writeXmltv()
        {
            if (config.CreateXmltv)
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(Helper.Epg123XmltvPath, false, Encoding.UTF8))
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