using System;
using System.IO;
using Microsoft.Win32;
using epg123;

namespace epg123Client
{
    static class WmcRegistries
    {
        private static string HKLM_PROGRAMGUIDE = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings\ProgramGuide";

        public static bool ActivateGuide()
        {
            bool ret = false;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(HKLM_PROGRAMGUIDE, true))
                {
                    if ((int)key.GetValue("fAgreeTOS", 0) != 1) key.SetValue("fAgreeTOS", 1);
                    if ((string)key.GetValue("strAgreedTOSVersion", "") != "1.0") key.SetValue("strAgreedTOSVersion", "1.0");
                }
                ret = true;
            }
            catch
            {
                Logger.WriteInformation("Could not write/verify the registry settings to activate the guide in WMC.");
            }
            return ret;
        }

        public static bool SetBackgroundScanning(bool enable)
        {
            bool ret = false;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\BackgroundScanner", true))
                {
                    if ((int)key.GetValue("PeriodicScanEnabled", -1) != (enable ? 1 : 0)) key.SetValue("PeriodicScanEnabled", enable ? 1 : 0);
                    ret = true;
                }
            }
            catch
            {
                Logger.WriteInformation(string.Format("Could not write/verify the registry settings to {0}able background scanning.", enable ? "en" : "dis"));
            }
            return ret;
        }

        public static string GetStoreFilename()
        {
            string ret = null;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Epg", false))
                {
                    int version = 2; // version 2 is Win7, version 3 is Win8/8.1/10
                    int instance = (int)key.GetValue("EPG.instance", 0);
                    string pattern = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Microsoft\\eHome\\mcepg{0}-{1}.db";
                    if (File.Exists(string.Format(pattern, version, instance)) || File.Exists(string.Format(pattern, ++version, instance)))
                        ret = string.Format(pattern, version, instance);
                }
            }
            catch
            {
                Logger.WriteInformation("Could not construct the store filename from the registry. Using <<null>> to open store.");
            }
            return ret;
        }

        public static DateTime NextScheduledRecording()
        {
            DateTime ret = DateTime.MaxValue;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", false))
                {
                    ret = DateTime.Parse((string)key.GetValue("NextRecordingAt"));
                    if (ret < DateTime.Now) ret = DateTime.MaxValue;
                }
            }
            catch
            {
                Logger.WriteInformation("Could not determine when next recording is to start.");
            }
            return ret;
        }

        public static bool IsGarbageCleanupDue()
        {
            bool ret = true;
            string HKLM_EPGKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\EPG";
            string NEXTDBGC_KEYVALUE = "dbgc:next run time";

            try
            {
                // read registry to see if database garbage cleanup is needed
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(HKLM_EPGKEY, true))
                {
                    string nextRun;
                    if ((nextRun = (string)key.GetValue(NEXTDBGC_KEYVALUE)) != null)
                    {
                        TimeSpan deltaTime = DateTime.Parse(nextRun) - DateTime.Now;
                        if (deltaTime > TimeSpan.FromHours(12) && deltaTime < TimeSpan.FromDays(5))
                        {
                            ret = false;
                        }
                    }
                }
            }
            catch
            {
                Logger.WriteInformation("Could not verify when garbage cleanup was last run.");
            }
            return ret;
        }
    }
}
