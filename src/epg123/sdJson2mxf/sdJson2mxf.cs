using GaRyan2;
using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using api = GaRyan2.SchedulesDirect;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static string HostAddress = "192.168.7.6";
        private static epgConfig config;

        public static bool Success;
        public static MXF mxf;

        public static void Build()
        {
            // load configuration file
            config = Helper.ReadXmlFile(Helper.Epg123CfgPath, typeof(epgConfig));

            // initialize components
            var userAgent = $"EPG123/{Helper.Epg123Version}";
            Github.Initialize(userAgent, "epg123");
            Tmdb.Initialize(userAgent, config.ArtworkSize);
            api.Initialize(userAgent, config.BaseApiUrl, config.BaseArtworkUrl, config.UseDebug);

            var startTime = DateTime.UtcNow;
            Logger.WriteVerbose($"DaysToDownload: {config.DaysToDownload} , TheTVDBNumbers : {config.TheTvdbNumbers} , PrefixEpisodeTitle: {config.PrefixEpisodeTitle} , PrefixEpisodeDescription : {config.PrefixEpisodeDescription} , AppendEpisodeDesc: {config.AppendEpisodeDesc} , OADOverride : {config.OadOverride} , SeasonEventImages : {config.SeasonEventImages} , IncludeSDLogos : {config.IncludeSdLogos} , AutoAddNew: {config.AutoAddNew} , CreateXmltv: {config.CreateXmltv}");

            // login to Schedules Direct and check server status
            if (!api.GetToken(config.UserAccount.LoginName, config.UserAccount.PasswordHash))
            {
                Logger.WriteError("Failed to login to Schedules Direct. Aborting update.");
                return;
            }
            else
            {
                // check server status
                var susr = api.GetUserStatus();
                if (susr != null && susr.SystemStatus[0].Status.ToLower().Equals("offline"))
                {
                    Logger.WriteError("Schedules Direct server is offline. Aborting update.");
                    return;
                }
            }

            // initialize mxf file
            mxf = new MXF("EPG123", $"Electronic Program Guide in 1-2-3 v{Helper.Epg123Version}", "GaRyan2", "Schedules Direct");
            if (Github.UpdateAvailable())
            {
                mxf.Providers[0].DisplayName += $" (Update Available)";
                Logger.Status = 1;
            }

            // load cache file
            epgCache.LoadCache();

            // build mxf/xmltv/json files
            if (BuildLineupServices() &&
                ServiceCountSafetyCheck() &&
                GetAllScheduleEntryMd5S(config.DaysToDownload) &&
                BuildAllProgramEntries() &&
                BuildAllGenericSeriesInfoDescriptions() &&
                BuildAllExtendedSeriesDataForUiPlus() &&
                GetAllMoviePosters() &&
                GetAllSeriesImages() &&
                GetAllSeasonImages() &&
                GetAllSportsImages() &&
                BuildKeywords())
            {
                // save cache file and mxf file
                epgCache.WriteCache();
                AddBrandLogoToMxf();
                CreateDummyLineupChannel();
                WaitForLogoDownloadsToComplete();
                if (WriteMxf()) Success = true;

                // create the xmltv file if desired
                if (config.CreateXmltv && CreateXmltvFile())
                {
                    WriteXmltv();
                    IncrementProgress();
                }

                // create the ModernMedia UI+ json file if desired
                if (config.ModernMediaUiPlusSupport)
                {
                    ModernMediaUiPlus.WriteModernMediaUiPlusJson(config.ModernMediaUiPlusJsonFilepath ?? null);
                    IncrementProgress();
                }
                Logger.WriteInformation("Completed EPG123 update execution. SUCCESS.");
            }
            mxf = null; xmltv = null; StationLogosToDownload = null;
            Logger.WriteVerbose($"EPG123 update execution time was {DateTime.UtcNow - startTime}.");
        }

        private static bool ServiceCountSafetyCheck()
        {
            if (config.ExpectedServicecount < 20 || !(config.ExpectedServicecount - MissingStations < config.ExpectedServicecount * 0.95)) return true;
            Logger.WriteError($"Of the expected {config.ExpectedServicecount} stations to download, there are only {config.ExpectedServicecount - MissingStations} stations available from Schedules Direct. Aborting update for review by user.");
            Logger.WriteError("ACTION: Review log file to see what stations have been added and removed from your lineup(s) since you last saved your configuration.");
            Logger.WriteError("ACTION: Open configuration GUI and review lineup(s). If lineup channels and stations are accurate, click [Save] to rebaseline the expected number of stations to download.");
            return false;
        }

        private static void AddBrandLogoToMxf()
        {
            using (var ms = new MemoryStream())
            {
                BrandLogo.StatusImage(config.BrandLogoImage).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                mxf.DeviceGroup.GuideImage.Image = Convert.ToBase64String(ms.ToArray());
            }
        }

        private static void CreateDummyLineupChannel()
        {
            var mxfService = mxf.FindOrCreateService("DUMMY");
            mxfService.CallSign = "DUMMY";
            mxfService.Name = "DUMMY Station";

            var mxfLineup = mxf.FindOrCreateLineup("ZZZ-DUMMY-EPG123", "ZZZ123 Dummy Lineup");
            mxfLineup.channels.Add(new MxfChannel(mxfLineup, mxfService));
        }

        private static void WaitForLogoDownloadsToComplete()
        {
            // make sure background worker to download station logos is complete
            IncrementNextStage(1);
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
        }

        private static bool WriteMxf()
        {
            // reset counters
            IncrementNextStage(1 + (config.CreateXmltv ? 1 : 0) + (config.ModernMediaUiPlusSupport ? 1 : 0));
            mxf.Providers[0].Status = Logger.Status;

            if (!Helper.WriteXmlFile(mxf, Helper.Epg123MxfPath, true)) return false;
            
            var fi = new FileInfo(Helper.Epg123MxfPath);
            Logger.WriteInformation($"Completed save of the MXF file to \"{Helper.Epg123MxfPath}\". ({Helper.BytesToString(fi.Length)})");
            Logger.WriteVerbose($"Generated MXF file contains {mxf.With.Services.Count - 1} services, {mxf.With.SeriesInfos.Count} series, {mxf.With.Seasons.Count} seasons, {mxf.With.Programs.Count} programs, {mxf.With.ScheduleEntries.Sum(x => x.ScheduleEntry.Count)} schedule entries, and {mxf.With.People.Count} people with {mxf.With.GuideImages.Count} image links.");
            IncrementProgress();
            return true;
        }

        private static void WriteXmltv()
        {
            if (!config.CreateXmltv) return;
            if (!Helper.WriteXmlFile(xmltv, Helper.Epg123XmltvPath, true)) return;

            var fi = new FileInfo(Helper.Epg123XmltvPath);
            var imageCount = xmltv.Programs.SelectMany(program => program.Icons?.Select(icon => icon.Src) ?? new List<string>()).Distinct().Count();
            Logger.WriteInformation($"Completed save of the XMLTV file to \"{Helper.Epg123XmltvPath}\". ({Helper.BytesToString(fi.Length)})");
            Logger.WriteVerbose($"Generated XMLTV file contains {xmltv.Channels.Count} channels and {xmltv.Programs.Count} programs with {imageCount} distinct program image links.");
        }
    }
}