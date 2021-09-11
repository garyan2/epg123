using System.Management;
using Microsoft.Win32;

namespace tokenServer
{
    public partial class Server
    {
        private ManagementEventWatcher _regWatcher;
        private const string RegQuery = "SELECT * FROM RegistryKeyChangeEvent " +
                                         "WHERE Hive='HKEY_LOCAL_MACHINE' " + 
                                        @"AND KeyPath='SOFTWARE\\GaRyan2\\epg123'";

        public void StartRegistryWatcher()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GaRyan2\epg123", false))
            {
                if (key != null)
                {
                    TokenService.Token = (string) key.GetValue("token", "");
                    TokenService.GoodToken = TokenService.Token != "";
                    TokenService.RefreshToken = (int) key.GetValue("autoRefreshToken", 1) > 0;
                    _cache.cacheImages = (int) key.GetValue("cacheImages", 0) > 0;
                    _cache.cacheRetention = (int) key.GetValue("cacheRetention", 30);

                    Helper.WriteLogEntry($"version {Helper.Epg123Version} : token={TokenService.Token} , autoRefreshToken={TokenService.RefreshToken} , cacheImages={_cache.cacheImages} , cacheRetention={_cache.cacheRetention}");
                }
                else
                {
                    Helper.WriteLogEntry("Cannot start registry watcher. Registry key does not exist.");
                    return;
                }
            }

            _regWatcher = new ManagementEventWatcher(RegQuery);
            _regWatcher.EventArrived += RegEventHandler;
            _regWatcher.Start();
        }

        private void RegEventHandler(object sender, EventArrivedEventArgs e)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GaRyan2\epg123", false))
            {
                if (key != null)
                {
                    var regToken = (string)key.GetValue("token", "");
                    if (regToken != TokenService.Token)
                    {
                        Helper.WriteLogEntry($"New token detected in registry. token={regToken}");
                        TokenService.Token = regToken;
                        _cache.Save(); // seems like a good time to save
                    }

                    var regRefresh = (int)key.GetValue("autoRefreshToken", 0) > 0;
                    if (regRefresh != TokenService.RefreshToken)
                    {
                        Helper.WriteLogEntry($"Auto token refresh setting changed. autoRefreshToken={regRefresh}");
                        TokenService.RefreshToken = regRefresh;
                    }

                    var regCache = (int)key.GetValue("cacheImages", 0) > 0;
                    if (regCache != _cache.cacheImages)
                    {
                        Helper.WriteLogEntry($"Cache images setting changed. cacheImages={regCache}");
                        _cache.cacheImages = regCache;
                    }

                    var regCacheRetent = (int) key.GetValue("cacheRetention", 30);
                    if (regCacheRetent != _cache.cacheRetention)
                    {
                        Helper.WriteLogEntry($"Cache retention setting changed. cacheRetention={regCacheRetent}");
                        _cache.cacheRetention = regCacheRetent;
                    }
                }
                else
                {
                    Helper.WriteLogEntry("Registry key no longer exists. Stopping watcher.");
                    _regWatcher.Stop();
                }
            }
        }
    }
}
