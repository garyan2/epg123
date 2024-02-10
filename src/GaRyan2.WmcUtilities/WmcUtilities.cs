using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        public static System.ComponentModel.BackgroundWorker BackgroundWorker;

        #region ========== MCUpdate ==========
        public static bool PerformWmcConfigurationsBackup()
        {
            var ret = false;
            try
            {
                // establish program to run and environment for import
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.McUpdateExeFilePath,
                    Arguments = "-b -nogc",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin import
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    // wait for exit and process exit code
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        ret = true;
                        Logger.WriteInformation("Successfully forced a Media Center database configuration backup. Exit code: 0");
                    }
                    else
                    {
                        Logger.WriteError($"Error using mcupdate to force a Media Center database configuration backup. Exit code: {proc.ExitCode}");
                    }
                }
                else
                {
                    Logger.WriteError($"Error using mcupdate to force a Media Center database configuration backup.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during PerformWmcConfigurationsBackup(). Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }

        public static bool PerformGarbageCleanup()
        {
            var ret = false;
            const string hklmEpgKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\EPG";
            const string nextDbgcKey = "dbgc:next run time";
            var nextRunTime = DateTime.Now + TimeSpan.FromDays(5);

            Logger.WriteMessage("Entering PerformGarbageCleanup().");
            Helper.SendPipeMessage("Importing|Performing garbage cleanup...");
            try
            {
                // establish program to run and environment
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.McUpdateExeFilePath,
                    Arguments = "-b -dbgc -updateTrigger",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin import
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    proc.OutputDataReceived += Process_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.ErrorDataReceived += Process_ErrorDataReceived;
                    proc.BeginErrorReadLine();

                    // wait for exit and process exit code
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        ret = true;
                        Logger.WriteInformation("Successfully completed garbage cleanup. Exit code: 0");
                    }
                    else
                    {
                        Logger.WriteError($"Error using mcupdate.exe to perform database garbage cleanup. Exit code: {proc.ExitCode}");
                    }
                }
                else
                {
                    Logger.WriteError($"Error using mcupdate.exe to perform database garbage cleanup.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during PerformGarbageCleanup(). Message:{Helper.ReportExceptionMessages(ex)}");
            }

            if (ret)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(hklmEpgKey, true))
                    {
                        if (key != null)
                        {
                            key.SetValue(nextDbgcKey, nextRunTime.ToString("s"));
                        }
                        else
                        {
                            Logger.WriteError("Could not set next garbage cleanup time in registry.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"Exception thrown during PerformGarbageCleanup(). Message:{Helper.ReportExceptionMessages(ex)}");
                }
            }

            Logger.WriteMessage($"Exiting PerformGarbageCleanup(). {(ret ? "SUCCESS" : "FAILURE")}.");
            return ret;
        }
        #endregion

        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.UserAgent = $"EPG123/{Helper.Epg123Version}";
                request.Timeout = 10 * 60 * 1000; // allow 10 minutes for download to start
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        #region ========== LoadMxf ==========
        public static bool ImportMxfFile(string mxfFile, bool forceImport = false)
        {
            var ret = false;
            Logger.WriteMessage($"Entering ImportMxfFile() for file \"{mxfFile}\".");
            try
            {
                // check for http download
                if (mxfFile.StartsWith("http"))
                {
                    try
                    {
                        var uri = new Uri(mxfFile);
                        var filepath = $"{Helper.Epg123OutputFolder}{uri.Segments[uri.Segments.Length - 1]}";
                        Helper.SendPipeMessage("Importing|Downloading remote MXF file...");
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }
                        using (var wc = new MyWebClient())
                        {
                            wc.DownloadFile(new Uri(mxfFile), filepath);
                            mxfFile = filepath;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"Failed to download MXF file from \"{mxfFile}\". Exception:{Helper.ReportExceptionMessages(ex)}");
                        return false;
                    }
                }

                // check file age, server status, and for channels removed from lineups
                try
                {
                    MXF mxf = Helper.ReadXmlFile(mxfFile, typeof(MXF));
                    if (mxf == null) return false;

                    if (mxf.Providers?.SingleOrDefault(arg => arg.Name.Equals("EPG123") || arg.Name.Equals("HDHR2MXF")) != null)
                    {
                        DateTimeOffset.TryParse(mxf.DeviceGroup?.LastConfigurationChange, out DateTimeOffset lastUpdateTime);
                        Logger.WriteInformation($"MXF file was created on {lastUpdateTime.ToLocalTime()}");
                        if (!forceImport && DateTime.UtcNow - lastUpdateTime > TimeSpan.FromHours(24.0))
                        {
                            Logger.WriteError($"The MXF file is {(DateTime.UtcNow - lastUpdateTime).TotalHours:N2} hours old. Aborting import.");
                            Logger.WriteError("ACTION: Review trace.log file to determine cause of failed MXF file creation within last 24 hours.");
                            Logger.WriteError("ACTION: To force an import of the aged MXF file, use the client GUI [Manual Import] button.");
                            return false;
                        }

                        if (mxf.Providers[0].DisplayName.Contains("Available"))
                        {
                            Logger.WriteInformation("The MXF file reports the EPG123 server installation is not up to date.");
                            Logger.WriteInformation("ACTION: Download the latest version from https://garyan2.github.io/ and update the server and/or client(s).");
                        }
                        switch (mxf.Providers[0].Status)
                        {
                            case 0xBAD1:
                                Logger.WriteWarning("There was a WARNING generated during the MXF file creation.");
                                Logger.WriteWarning("ACTION: Review trace.log file to determine cause of warning during MXF file creation within last 24 hours.");
                                break;
                            case 0xDEAD:
                                Logger.WriteError("There was an ERROR generated during the MXF file creation.");
                                Logger.WriteError("ACTION: Review trace.log file to determine cause of error during MXF file creation within last 24 hours.");
                                break;
                            default:
                                break;
                        }

                        foreach (var wmis in GetWmisLineups())
                        {
                            if (!(WmcObjectStore.Fetch(wmis.LineupId) is Lineup lup)) continue;
                            var xml = mxf?.With?.Lineups?.FirstOrDefault(arg => arg.Uid == lup.GetUIdValue());
                            if (xml == null) continue;

                            foreach (var channel in lup.GetChannels())
                            {
                                var mxfch = xml.channels.FirstOrDefault(arg => arg.Uid.Contains(channel.GetUIdValue()));
                                if (mxfch != null) continue;

                                Logger.WriteInformation($"Channel '{channel.ChannelNumber} {channel.CallSign}' was removed from lineup '{lup.Name}'. Unsubscribing channel before import.");
                                foreach (MergedChannel mch in channel.ReferencingPrimaryChannels.ToList())
                                {
                                    SubscribeLineupChannel(0, mch.Id);
                                }
                            }
                        }
                    }
                    Close();
                }
                catch (Exception ex)
                {
                    Logger.WriteWarning($"Exception thrown while comparing WMC lineups with MXF lineups. Message:{Helper.ReportExceptionMessages(ex)}");
                    Close();
                }

                // establish program to run and environment for import
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.LoadMxfExeFilePath,
                    Arguments = $"-i \"{mxfFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin import
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    proc.OutputDataReceived += Process_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.ErrorDataReceived += Process_ErrorDataReceived;
                    proc.BeginErrorReadLine();

                    // wait for exit and process exit code
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        ret = true;
                        Logger.WriteInformation("Successfully imported .mxf file into Media Center database. Exit code: 0");
                    }
                    else
                    {
                        Logger.WriteError($"Error using loadmxf.exe to import new guide information. Exit code: {proc.ExitCode}");
                        Logger.WriteError("ACTION: Review trace.log file to view errors recorded by loadmxf.exe.");
                        Logger.WriteError("ACTION: If error identifies an xml line/column error, the problem is with the generated mxf file.");
                        Logger.WriteError("ACTION:   1) Open the configuration GUI an click the [Clear Cache] button.");
                        Logger.WriteError("ACTION:   2) Click the [Save & Execute] button to build a new mxf file and import into WMC.");
                        Logger.WriteError("ACTION: If error is not mxf/xml file related, then your database may be corrupt. Repair the database by performing the below actions:");
                        Logger.WriteError("ACTION:   1) Open a command prompt (cmd.exe) and execute the following command, \"START /WAIT c:\\windows\\ehome\\mcupdate.exe -b -dbgc -updateTrigger\" without quotes. Wait for it to complete and try to import again using the [Manual Import] button in the client GUI.");
                        Logger.WriteError("ACTION:   2) If above did not work, use the [Rebuild WMC Database] button from the client GUI and import the mxf file to restore guide listings.");
                    }
                }
                else
                {
                    Logger.WriteError($"Error using loadmxf.exe to import new guide information.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during ImportMxfFile(). Message:{Helper.ReportExceptionMessages(ex)}");
            }

            Logger.WriteMessage($"Exiting ImportMxfFile(). {(ret ? "SUCCESS" : "FAILURE")}.");
            return ret;
        }

        public static bool SetWmcTunerLimits(int tuners)
        {
            var ret = false;
            string[] countries = { /*"default", */"au", "be", "br", "ca", "ch", "cn", "cz", "de", "dk", "es", "fi", "fr", "gb", "hk", "hu", "ie", "in",/* "it",*/ "jp", "kr", "mx", "nl", "no", "nz", "pl",/* "pt",*/ "ru", "se", "sg", "sk",/* "tr", "tw",*/ "us", "za" };

            // create mxf file with increased tuner limits
            var xml = "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n" +
                      "<MXF version=\"1.0\" xmlns=\"\">\r\n" +
                      "  <Assembly name=\"mcstore\">\r\n" +
                      "    <NameSpace name=\"Microsoft.MediaCenter.Store\">\r\n" +
                      "      <Type name=\"StoredType\" />\r\n" +
                      "    </NameSpace>\r\n" +
                      "  </Assembly>\r\n" +
                      "  <Assembly name=\"ehshell\">\r\n" +
                      "    <NameSpace name=\"ServiceBus.UIFramework\">\r\n" +
                      "      <Type name=\"TvSignalSetupParams\" />\r\n" +
                      "    </NameSpace>\r\n" +
                      "  </Assembly>\r\n";
            xml += string.Format("  <With maxRecordersForHomePremium=\"{0}\" maxRecordersForUltimate=\"{0}\" maxRecordersForRacing=\"{0}\" maxRecordersForBusiness=\"{0}\" maxRecordersForEnterprise=\"{0}\" maxRecordersForOthers=\"{0}\">\r\n", tuners);

            foreach (var country in countries)
            {
                if (country.Equals("ca"))
                {
                    // sneak this one in for our Canadian friends just north of the (contiguous) border to be able to tune ATSC stations from the USA
                    xml += $"    <TvSignalSetupParams uid=\"tvss-{country}\" atscSupported=\"true\" autoSetupLikelyAtscChannels=\"34, 35, 36, 43, 31, 39, 38, 32, 41, 27, 19, 51, 44, 42, 30, 28\" tvRatingSystem=\"US\" />\r\n";
                }
                else
                {
                    xml += $"    <TvSignalSetupParams uid=\"tvss-{country}\" />\r\n";
                }
            }

            xml += "  </With>\r\n";
            xml += "</MXF>";

            try
            {
                // create temporary file
                var mxfFilepath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mxf");
                using (var writer = new StreamWriter(mxfFilepath, false))
                {
                    writer.Write(xml);
                }

                // import tweak using loadmxf.exe because for some reason the MxfImporter doesn't work for this
                ret = ImportMxfFile(mxfFilepath);

                // delete temporary file
                Helper.DeleteFile(mxfFilepath);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during SetWmcTunerLimits(). Message:{Helper.ReportExceptionMessages(ex)}");
            }
            return ret;
        }
        #endregion

        #region ========== ReIndex Tasks ==========
        public static bool ReindexDatabase()
        {
            bool ret = false;
            try
            {
                Logger.WriteMessage($"Entering ReindexDatabase()");
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = Helper.Epg123ClientExePath,
                    Arguments = "-storage",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (proc != null)
                {
                    proc.OutputDataReceived += Process_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.ErrorDataReceived += Process_ErrorDataReceived;
                    proc.BeginErrorReadLine();
                    proc.WaitForExit(100);
                    proc.CancelErrorRead();
                    proc.CancelOutputRead();
                    ret = true;
                }
            }
            catch { }
            Logger.WriteMessage($"Exiting ReindexDatabase(). {(ret ? "SUCCESS" : "FAILURE")}.");
            return ret;
        }

        public static bool ReindexPvrSchedule()
        {
            return RunWmcIndexTask("PvrScheduleTask", "mcupdate.exe", "-PvrSchedule -nogc");
        }

        private static bool RunWmcIndexTask(string task, string program, string argument)
        {
            var ret = false;
            Logger.WriteMessage($"Entering RunWmcIndexTask({task})");
            try
            {
                // establish program to run and environment
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/run /tn \"Microsoft\\Windows\\Media Center\\{task}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin reindex
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    proc.OutputDataReceived += Process_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.ErrorDataReceived += Process_ErrorDataReceived;
                    proc.BeginErrorReadLine();

                    // wait for exit and process exit code
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        ret = true;
                        Logger.WriteInformation($"Successfully started the {task} task. Exit code: 0");
                        goto Finish;
                    }
                    Logger.WriteInformation($"Error using schtasks.exe to start {task} task. Exit code: {proc.ExitCode}");
                }
                else
                {
                    Logger.WriteInformation($"Error using schtasks.exe to start {task} task.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown during RunWmcIndexTask() using schtasks.exe. Message:{Helper.ReportExceptionMessages(ex)}");
            }

            try
            {
                // if schtasks did not work, try using the program directly
                Logger.WriteVerbose($"Attempting \"{program} {argument}\" to index data.");
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + $"\\ehome\\{program}",
                    Arguments = argument,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin reindex again
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    proc.OutputDataReceived += Process_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.ErrorDataReceived += Process_ErrorDataReceived;
                    proc.BeginErrorReadLine();

                    // wait for exit and process exit code
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        ret = true;
                        Logger.WriteInformation($"Successfully completed the {task} task. Exit code: 0");
                    }
                    else
                    {
                        Logger.WriteError($"Error using {program} to start {task} task. Exit code: {proc.ExitCode}");
                    }
                }
                else
                {
                    Logger.WriteError($"Error using {program} to start {task} task.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Exception thrown during RunWmcIndexTask() using {program}. Message:{Helper.ReportExceptionMessages(ex)}");
            }

        Finish:
            Logger.WriteMessage($"Exiting RunWmcIndexTask({task}). {(ret ? "SUCCESS" : "FAILURE")}.");
            return ret;
        }
        #endregion

        #region ========== Process Outputs ==========
        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            var data = e.Data;
            if (data.Length > 0)
            {
                Logger.WriteInformation(data);
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            var data = e.Data;
            if (data.Length <= 0) return;
            var m = Regex.Match(data, @"[0-9]{1,3}%");
            if (m.Success)
            {
                Helper.SendPipeMessage($"Importing|{data}");
                BackgroundWorker?.ReportProgress(int.Parse(m.Value.TrimEnd('%')));
            }
            else
            {
                Logger.WriteVerbose(data);
            }
        }
        #endregion
    }
}