using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using ScheduledTask;
using Microsoft.Win32;

namespace epg123
{
    class epgTaskScheduler
    {
        private Random random = new Random();
        const string taskName = "epg123_update";
        public DateTime schedTime;
        private DateTime oldTime;
        public string statusString;
        public bool exist = false;
        public bool existNoAccess = false;
        public bool wake = false;
        public TaskActions[] actions;
        public struct TaskActions
        {
            public string Path;
            public string Arguments;
        }

        public void queryTask(bool silent = false)
        {
            if (oldTime != DateTime.MinValue) schedTime = oldTime;
            else schedTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, random.Next(0, 23), random.Next(0, 59), 0);
            responseString = string.Empty;
            errorString = string.Empty;
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "schtasks.exe",
                Arguments = string.Format("/query /xml /tn \"{0}\"", taskName),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process proc = Process.Start(startInfo);
            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.BeginOutputReadLine();
            proc.ErrorDataReceived += Proc_ErrorDataReceived;
            proc.BeginErrorReadLine();

            proc.WaitForExit();
            if (exist = (proc.ExitCode == 0))
            {
                existNoAccess = false;
                XmlSerializer serializer = new XmlSerializer(typeof(taskType));
                using (StringReader reader = new StringReader(responseString))
                {
                    taskType task = (taskType)(serializer.Deserialize(reader));
                    actions = new TaskActions[task.Actions.Exec.Length];
                    for (int i = 0; i < task.Actions.Exec.Length; ++i)
                    {
                        actions[i].Path = task.Actions.Exec[i].Command;
                        actions[i].Arguments = task.Actions.Exec[i].Arguments;
                    }
                    wake = task.Settings.WakeToRun;
                    oldTime = schedTime = task.Triggers.Items[0].StartBoundary.Date + task.Triggers.Items[0].StartBoundary.TimeOfDay;
                }

                responseString = string.Empty;
                startInfo.Arguments = string.Format("/query /fo csv /v /tn \"{0}\"", taskName);
                proc = Process.Start(startInfo);
                proc.OutputDataReceived += Proc_OutputDataReceived;
                proc.BeginOutputReadLine();

                proc.WaitForExit();
                if (!string.IsNullOrEmpty(responseString))
                {
                    statusString = string.Empty;
                    string[] lines = responseString.Split('\n');
                    string[] columns = lines[0].Replace("\",\"", "|").TrimStart('\"').TrimEnd('\"').Split('|');
                    string[] values = lines[1].Replace("\",\"", "|").TrimStart('\"').TrimEnd('\"').Split('|');
                    for (int i = 0; i < columns.Length; ++i)
                    {
                        switch (columns[i])
                        {
                            case "Status":
                                statusString += string.Format("{0}.", values[i]);
                                break;
                            case "Last Run Time":
                                DateTime dt;
                                if (DateTime.TryParse(values[i], out dt) && (dt.Year > 2015))
                                {
                                    statusString += string.Format(" Last Run {0};", values[i]);
                                }
                                else
                                {
                                    statusString += "The task has not yet run.";
                                }
                                break;
                            case "Last Result":
                                statusString += string.Format(" Exit: 0x{0}", int.Parse(values[i]).ToString("X8"));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else if (errorString.ToLower().Contains("access")) // permission problem
            {
                existNoAccess = true;
                statusString = errorString;
            }
            else
            {
                statusString = "No task is scheduled to run.";
            }
            if (!silent) Logger.WriteVerbose(string.Format("Successfully queried the Task Scheduler for status. {0}", statusString));
            return;
        }

        public string errorString;
        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            errorString += e.Data.ToString() + " ";
        }

        private string responseString = string.Empty;
        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            responseString += e.Data.ToString() + "\n";
        }

        public bool createTask(bool wakeToRun, string startTime, TaskActions[] tActions)
        {
            //string xmlFilename = "epg123Task.xml";
            DateTime date = DateTime.Parse(string.Format("{0}/{1}/{2}T{3}:{4}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                startTime.Substring(0, 2), startTime.Substring(3, 2)));

            // create registration
            registrationInfoType registration = new registrationInfoType()
            {
                Author = "GaRyan2's epg123",
                Description = "Utility to update the Windows Media Center Electronic Program Guide by downloading guide information from Schedules Direct and importing the created .mxf file.",
                URI = "\\epg123_update",
                SecurityDescriptor = "D:(A;;FRFWSDWDWO;;;BA)(A;;FRFWSDWDWO;;;SY)(A;;FRFWFXDTDCSDWD;;;NS)(A;;FXFR;;;AU)"
            };

            // create trigger
            triggersType triggers = new triggersType()
            {
                Items = new triggerBaseType[]
                {
                    new calendarTriggerType()
                    {
                        id = "epg123 Daily Trigger",
                        Item = new dailyScheduleType()
                        {
                            DaysInterval = 1,
                            DaysIntervalSpecified = true
                        },
                        StartBoundary = date,
                        StartBoundarySpecified = true
                    }
                }
            };

            // create settings
            settingsType settings = new settingsType()
            {
                DisallowStartIfOnBatteries = false,
                StopIfGoingOnBatteries = false,
                ExecutionTimeLimit = "PT23H",
                Priority = 6,
                RestartOnFailure = new restartType()
                {
                    Count = 5,
                    Interval = "PT30M"
                },
                StartWhenAvailable = true,
                //RunOnlyIfNetworkAvailable = true,
                WakeToRun = wakeToRun
            };

            // create principal
            // Windows10 1607 Anniversary Update introduced a bug which only executes the first
            // task action unless running a service or possibly administrator?
            // Also, network service does not have access to some registry keys in Win7
            principalsType principals;
            if (false)//isWindows10())
            {
                principals = new principalsType()
                {
                    Principal = new principalType()
                    {
                        id = @"NT AUTHORITY\NETWORKSERVICE",
                        UserId = "S-1-5-20",
                        RunLevel = runLevelType.HighestAvailable,
                        RunLevelSpecified = true
                    }
                };
            }
            else
            {
                principals = new principalsType()
                {
                    Principal = new principalType()
                    {
                        id = @"SYSTEM",
                        UserId = "S-1-5-18",
                        RunLevel = runLevelType.HighestAvailable,
                        RunLevelSpecified = true
                    }
                };
            }

            // create action(s)
            execType[] executions = new execType[tActions.Length];
            for (int i = 0; i < tActions.Length; ++i)
            {
                executions[i] = new execType()
                {
                    id = string.Format("epg123 Execution Action {0}", i + 1),
                    Command = tActions[i].Path,
                    Arguments = tActions[i].Arguments,
                    WorkingDirectory = Helper.ExecutablePath
                };
            }
            actionsType actions = new actionsType()
            {
                Exec = executions
            };

            // build complete task
            taskType newTask = new taskType()
            {
                RegistrationInfo = registration,
                Triggers = triggers,
                Settings = settings,
                Principals = principals,
                Actions = actions,
            };

            // serialize to xml and save to the drive
            try
            {
                using (StreamWriter stream = new StreamWriter(Helper.Epg123TaskXmlPath, false, Encoding.Unicode))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(taskType));
                    TextWriter writer = stream;
                    serializer.Serialize(writer, newTask);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Failed to create a {0} daily update task XML file. message: {1}", startTime, ex.Message));
            }

            return false;
        }

        public bool importTask()
        {
            // create the scheduled task from the created xml file
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks.exe",
                    Arguments = string.Format("/create /xml \"{0}\" /tn {1}", Helper.Epg123TaskXmlPath, taskName),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process proc = Process.Start(startInfo);

                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    Logger.WriteInformation("Successfully created the daily update task in Task Scheduler.");
                }
                else
                {
                    Logger.WriteError(string.Format("Failed to create a daily update task in Task Scheduler. Exit: {0}", proc.ExitCode.ToString("X8")));
                    return false;
                }

                if (File.Exists(Helper.EhshellExeFilePath) && File.Exists(Helper.Epg123ClientExePath))
                {
                    startInfo.Arguments = $"/change /tn \"\\Microsoft\\Windows\\Media Center\\mcupdate\" /tr \"'{Helper.Epg123ClientExePath}' $(Arg0)\" /enable";
                    Process proc2 = Process.Start(startInfo);
                    proc2.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Failed to create a daily update task in Task Scheduler. message: {0}", ex.Message));
                return false;
            }
            return true;
        }

        private bool isWindows10()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string)reg.GetValue("ProductName");
            return productName.StartsWith("Windows 10");
        }

        public bool deleteTask()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks.exe",
                    Arguments = string.Format("/delete /f /tn {0}", taskName),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process proc = Process.Start(startInfo);

                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    Logger.WriteInformation("Successfully deleted the daily update task from Task Scheduler.");
                }
                else
                {
                    Logger.WriteError(string.Format("Failed to delete the daily task in Task Scheduler. Exit: {0}", proc.ExitCode.ToString("X8")));
                    return false;
                }

                if (File.Exists(Helper.EhshellExeFilePath))
                {
                    startInfo.Arguments = "/change /tn \"\\Microsoft\\Windows\\Media Center\\mcupdate\" /tr \"'%SystemRoot%\\ehome\\mcupdate.exe' $(Arg0)\" /disable";
                    Process proc2 = Process.Start(startInfo);
                    proc2.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Failed to delete the daily task in Task Scheduler. message: {0}", ex.Message));
                return false;
            }
            return true;
        }
    }
}