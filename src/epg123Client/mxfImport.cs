using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.MediaCenter.Guide;
using Microsoft.Win32;

namespace epg123
{
    public static class mxfImport
    {
        public static System.ComponentModel.BackgroundWorker backgroundWorker;

        public static bool importMxfFile(string filename)
        {
            Logger.WriteMessage("Entering importMxfFile() for file \"" + filename + "\"");
            try
            {
                // establish program to run and environment for import
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + @"\ehome\loadmxf.exe",
                    Arguments = string.Format("-i \"{0}\"", filename),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin import
                Process proc = Process.Start(startInfo);
                proc.OutputDataReceived += load_OutputDataReceived;
                proc.BeginOutputReadLine();
                proc.ErrorDataReceived += load_ErrorDataReceived;
                proc.BeginErrorReadLine();

                // wait for exit and process exit code
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    Logger.WriteInformation("Successfully imported .mxf file into Media Center database. Exit code: 0");
                    Logger.WriteMessage("Exiting importMxfFile(). SUCCESS.");
                    return true;
                }
                else
                {
                    Logger.WriteError(string.Format("Error using loadmxf.exe to import new guide information. Exit code: {0}", proc.ExitCode));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Exception thrown trying to import .mxf file using loadmxf.exe. Message: {0}", ex.Message));
            }
            Logger.WriteMessage("Exiting importMxfFile(). FAILURE.");
            return false;
        }

        public static void PerformGarbageCleanup()
        {
            string epg123NextRunTime = "dbgc:next run time";
            DateTime nextRunTime = DateTime.Now + TimeSpan.FromDays(4.5);
            bool runDbgc = true;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\EPG", true))
            {
                // verify periodic downloads are not enabled
                if ((int)key.GetValue("dl", 1) != 0) key.SetValue("dl", 0);

                try
                {
                    string nextRun;
                    if ((nextRun = (string)key.GetValue(epg123NextRunTime)) != null)
                    {
                        if (DateTime.Parse(nextRun) > DateTime.Now && (DateTime.Parse(nextRun) - DateTime.Now) < TimeSpan.FromDays(5))
                        {
                            runDbgc = false;
                        }
                        else
                        {
                            // write a last index time in the future to avoid the dbgc kicking off a reindex while importing the mxf file
                            DateTime lastFullReindex = Convert.ToDateTime(key.GetValue("LastFullReindex") as string, CultureInfo.InvariantCulture);
                            key.SetValue("LastFullReindex", Convert.ToString(nextRunTime, CultureInfo.InvariantCulture));
                        }
                    }
                }
                catch
                {
                    Logger.WriteError("Could not verify when garbage cleanup was last run.");
                }
            }

            if (runDbgc)
            {
                Logger.WriteMessage("Entering PerformGarbageCleanup().");
                Helper.SendPipeMessage("Importing|Performing garbage cleanup...");
                try
                {
                    // establish program to run and environment
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + @"\ehome\mcupdate.exe",
                        Arguments = "-dbgc -updateTrigger",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // begin import
                    Process proc = Process.Start(startInfo);
                    proc.OutputDataReceived += task_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.ErrorDataReceived += task_ErrorDataReceived;
                    proc.BeginErrorReadLine();

                    // wait for exit and process exit code
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\EPG", true))
                        {
                            try
                            {
                                key.SetValue(epg123NextRunTime, nextRunTime.ToString("s"));
                            }
                            catch
                            {
                                Logger.WriteError("Could not set next garbage cleanup time in registry.");
                            }
                        }

                        Logger.WriteInformation("Successfully completed garbage cleanup. Exit code: 0");
                        Logger.WriteMessage("Exiting PerformGarbageCleanup(). SUCCESS.");
                    }
                    else
                    {
                        Logger.WriteError(string.Format("Error using mcupdate.exe to perform database garbage cleanup. Exit code: {0}", proc.ExitCode));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(string.Format("Exception thrown trying to perform garbage cleanup using mcupdate.exe. Message: {0}", ex.Message));
                }
            }
        }

        private static void load_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            string data = e.Data.ToString();
            if (data.Length > 0)
            {
                if (data.StartsWith("Loading... "))
                {
                    Helper.SendPipeMessage($"Importing|{data}");
                    if (backgroundWorker != null)
                    {
                        backgroundWorker.ReportProgress(int.Parse(data.Substring(11).TrimEnd('%')));
                    }
                }
            }
        }
        private static void load_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            string data = e.Data.ToString();
            if (data.Length > 0)
            {
                Logger.WriteVerbose(data);
            }
        }

        public static void reindexDatabase()
        {
            runWmcIndexTask("ReindexSearchRoot", "ehPrivJob.exe", "/DoReindexSearchRoot");
        }

        public static void reindexPvrSchedule()
        {
            //runWmcIndexTask("PvrScheduleTask", "mcupdate.exe", "-PvrSchedule -nogc");
        }

        private static bool runWmcIndexTask(string task, string program, string argument)
        {
            Logger.WriteMessage(string.Format("Entering runWmcTask({0})", task));
            try
            {
                // establish program to run and environment
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks.exe",
                    Arguments = string.Format("/run /tn \"Microsoft\\Windows\\Media Center\\{0}\"", task),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin reindex
                Process proc = Process.Start(startInfo);
                proc.OutputDataReceived += task_OutputDataReceived;
                proc.BeginOutputReadLine();
                proc.ErrorDataReceived += task_ErrorDataReceived;
                proc.BeginErrorReadLine();

                // wait for exit and process exit code
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    Logger.WriteInformation(string.Format("Successfully started the {0} task. Exit code: 0", task));
                    Logger.WriteMessage(string.Format("Exiting runWmcTask({0}). SUCCESS.", task));
                    return true;
                }
                else
                {
                    Logger.WriteWarning(string.Format("Error using schtasks.exe to start {0} task. Exit code: {1}", task, proc.ExitCode));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Exception thrown trying to start database reindexing using schtasks.exe. Message: {0}", ex.Message));
            }

            try
            {
                // if schtasks did not work, try using the program directly
                Logger.WriteVerbose(string.Format("Attempting {0} {1} to index data.", program, argument));
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + string.Format("\\ehome\\{0}", program),
                    Arguments = argument,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // begin reindex again
                Process proc = Process.Start(startInfo);
                proc.OutputDataReceived += task_OutputDataReceived;
                proc.BeginOutputReadLine();
                proc.ErrorDataReceived += task_ErrorDataReceived;
                proc.BeginErrorReadLine();

                // wait for exit and process exit code
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    Logger.WriteInformation(string.Format("Successfully completed the {0} task. Exit code: 0", task));
                    Logger.WriteMessage(string.Format("Exiting runWmcTask({0}). SUCCESS", task));
                    return true;
                }
                else
                {
                    Logger.WriteError(string.Format("Error using {0} to start {1} task. Exit code: {2}", program, task, proc.ExitCode));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Exception thrown trying to start database reindexing using {0}. Message: {1}", program, ex.Message));
            }

            Logger.WriteError(string.Format("Exiting runWmcTask({0}). FAILURE", task));
            return false;
        }
        private static void task_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            string data = e.Data.ToString();
            if (data.Length > 0)
            {
                Logger.WriteInformation(data);
            }
        }
        private static void task_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            string data = e.Data.ToString();
            if (data.Length > 0)
            {
                Logger.WriteVerbose(data);
            }
        }

        public static bool activateLineupAndGuide()
        {
            int lineups = 0;
            if (Store.objectStore != null)
            {
                foreach (Lineup lineup in new Lineups(Store.objectStore))
                {
                    // only want to do this with EPG123 lineups
                    if (!lineup.Provider.Name.Equals("EPG123") && !lineup.Provider.Name.Equals("HDHR2MXF")) continue;

                    // make sure the lineup type and language are set
                    if (string.IsNullOrEmpty(lineup.LineupTypes) || string.IsNullOrEmpty(lineup.Language))
                    {
                        lineup.LineupTypes = "WMIS";
                        lineup.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                        lineup.Update();
                    }
                    ++lineups;
                }

                // set registry setting to "activate" the guide if necessary
                // NETWORK SERVICE does not have access to this registry in Win7
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
                    {
                        if ((int)key.GetValue("fAgreeTOS") != 1) key.SetValue("fAgreeTOS", 1);
                        if ((string)key.GetValue("strAgreedTOSVersion") != "1.0") key.SetValue("strAgreedTOSVersion", "1.0");
                    }
                }
                catch
                {
                    Logger.WriteInformation("Could not write/verify the registry settings to activate the guide in WMC."); 
                }
            }
            return (lineups > 0);
        }
    }
}