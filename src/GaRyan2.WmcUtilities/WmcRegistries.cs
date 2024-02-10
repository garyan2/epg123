using GaRyan2.Utilities;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        private const string HklmProgramGuide = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings\ProgramGuide";

        public static bool ActivateGuide()
        {
            var ret = false;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(HklmProgramGuide, true))
                {
                    if (key != null)
                    {
                        if ((int)key.GetValue("fAgreeTOS", 0) != 1) key.SetValue("fAgreeTOS", 1);
                        if ((string)key.GetValue("strAgreedTOSVersion", "") != "1.0") key.SetValue("strAgreedTOSVersion", "1.0");
                    }
                    else
                    {
                        Logger.WriteInformation($"Could not write/verify the registry settings to activate the guide in WMC.");
                    }
                }

                ret = true;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Could not write/verify the registry settings to activate the guide in WMC. Exception:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }

        public static bool SetBackgroundScanning(bool enable)
        {
            var ret = false;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\BackgroundScanner", true))
                {
                    if (key != null)
                    {
                        if ((int)key.GetValue("PeriodicScanEnabled", -1) != (enable ? 1 : 0)) key.SetValue("PeriodicScanEnabled", enable ? 1 : 0);
                        ret = true;
                    }
                    else
                    {
                        Logger.WriteInformation($"Could not write/verify the registry settings to {(enable ? "en" : "dis")}able background scanning.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Could not write/verify the registry settings to {(enable ? "en" : "dis")}able background scanning. Exception:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }

        public static string GetStoreFilename()
        {
            string ret = null;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Epg", false))
                {
                    if (key != null)
                    {
                        var version = 2; // version 2 is Win7, version 3 is Win8/8.1/10
                        var instance = (int)key.GetValue("EPG.instance", 0);
                        var pattern = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Microsoft\\eHome\\mcepg{0}-{1}.db";
                        if (File.Exists(string.Format(pattern, version, instance)) || File.Exists(string.Format(pattern, ++version, instance)))
                            ret = string.Format(pattern, version, instance);
                    }
                    else
                    {
                        Logger.WriteInformation("Could not construct the store filename from the registry. Using <<null>> to open store.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Could not construct the store filename from the registry. Using <<null>> to open store. Exception:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }

        public static DateTime NextScheduledRecording()
        {
            var ret = DateTime.MaxValue;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Recording", false))
                {
                    if (key != null)
                    {
                        ret = DateTime.Parse((string)key.GetValue("NextRecordingAt"));
                        if (ret < DateTime.Now) ret = DateTime.MaxValue;
                    }
                    else
                    {
                        Logger.WriteInformation("Could not determine when next recording is to start.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Could not determine when next recording is to start. Exception:{Helper.ReportExceptionMessages(ex)}");
            }

            return ret;
        }

        public static bool IsGarbageCleanupDue()
        {
            var ret = true;
            var HKLM_EPGKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\EPG";
            var NEXTDBGC_KEYVALUE = "dbgc:next run time";
            var nextRunTime = DateTime.Now + TimeSpan.FromDays(5);

            try
            {
                // read registry to see if database garbage cleanup is needed
                using (var key = Registry.LocalMachine.OpenSubKey(HKLM_EPGKEY, true))
                {
                    if (key != null)
                    {
                        string nextRun;
                        if (!string.IsNullOrEmpty(nextRun = (string)key.GetValue(NEXTDBGC_KEYVALUE)))
                        {
                            var deltaTime = DateTime.Parse(nextRun) - DateTime.Now;
                            if (deltaTime > TimeSpan.FromHours(12) && deltaTime < TimeSpan.FromDays(5))
                            {
                                ret = false;
                            }
                        }
                        else
                        {
                            key.SetValue(NEXTDBGC_KEYVALUE, $"{nextRunTime:s}");
                        }

                        // verify periodic downloads are not enabled
                        if ((int)key.GetValue("dl", 1) != 0) key.SetValue("dl", 0);

                        // write a last index time in the future to avoid the dbgc kicking off a reindex while importing the mxf file
                        key.SetValue("LastFullReindex", Convert.ToString(nextRunTime, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Logger.WriteInformation("Could not verify when garbage cleanup was last run.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Could not verify when garbage cleanup was last run. Exception:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }
    }
}