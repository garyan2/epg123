using epg123;
using GaRyan2;
using GaRyan2.Utilities;
using System.IO;

namespace epg123Server
{
    public partial class Server
    {
        private FileSystemWatcher watcher;
        public void StartConfigFileWatcher()
        {
            watcher = new FileSystemWatcher(Helper.Epg123ProgramDataFolder)
            {
                Filter = "epg123.cfg",
                NotifyFilter = NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            watcher.Changed += new FileSystemEventHandler(OnConfigChanged);
        }

        private void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            while (!IsFileReady(Helper.Epg123CfgPath)) ;
            var config = (epgConfig)Helper.ReadXmlFile(Helper.Epg123CfgPath, typeof(epgConfig));

            // determine actionable changes
            if (config.UserAccount?.LoginName != SchedulesDirect.Username ||
                config.UserAccount?.PasswordHash != SchedulesDirect.PasswordHash ||
                config.BaseApiUrl != SchedulesDirect.ApiBaseAddress ||
                config.BaseArtworkUrl != SchedulesDirect.ApiBaseArtwork ||
                config.UseDebug != SchedulesDirect.ApiDebug)
            {
                Logger.WriteInformation("Configuration reloaded due to account or base URL changes in file.");
                SchedulesDirect.RefreshConfiguration();
            }
            Helper.DeleteFile(Helper.Epg123MxfPath);
            Helper.DeleteFile(Helper.Epg123XmltvPath);
            JsonImageCache.cacheRetention = config.CacheRetention;
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    return fs.Length > 0;
            }
            catch { return false; }
        }

        public void StopConfigFileWatcher()
        {
            watcher.Dispose();
        }
    }
}