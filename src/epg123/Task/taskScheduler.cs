using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using ScheduledTask;

namespace epg123.Task
{
    internal class epgTaskScheduler
    {
        private readonly Random _random = new Random();
        private const string TaskName = "epg123_update";
        public DateTime SchedTime;
        private DateTime _oldTime;
        public string StatusString;
        public bool Exist;
        public bool ExistNoAccess;
        public bool Wake;
        public TaskActions[] Actions;

        public struct TaskActions
        {
            public string Path;
            public string Arguments;
        }

        public void QueryTask(bool silent = false)
        {
            SchedTime = _oldTime != DateTime.MinValue ? _oldTime : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _random.Next(0, 23), _random.Next(0, 59), 0);
            _responseString = string.Empty;
            ErrorString = string.Empty;
            var startInfo = new ProcessStartInfo()
            {
                FileName = "schtasks.exe",
                Arguments = $"/query /xml /tn \"{TaskName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var proc = Process.Start(startInfo);
            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.BeginOutputReadLine();
            proc.ErrorDataReceived += Proc_ErrorDataReceived;
            proc.BeginErrorReadLine();

            proc.WaitForExit();
            if (proc.ExitCode == 0)
            {
                Exist = true;
                ExistNoAccess = false;
                var serializer = new XmlSerializer(typeof(taskType));
                using (var reader = new StringReader(_responseString))
                {
                    var task = (taskType) (serializer.Deserialize(reader));
                    Actions = new TaskActions[task.Actions.Exec.Length];
                    for (var i = 0; i < task.Actions.Exec.Length; ++i)
                    {
                        Actions[i].Path = task.Actions.Exec[i].Command;
                        Actions[i].Arguments = task.Actions.Exec[i].Arguments;
                    }

                    Wake = task.Settings.WakeToRun;
                    _oldTime = SchedTime = task.Triggers.Items[0].StartBoundary.Date +
                                           task.Triggers.Items[0].StartBoundary.TimeOfDay;
                }

                _responseString = string.Empty;
                startInfo.Arguments = $"/query /fo csv /v /tn \"{TaskName}\"";
                proc = Process.Start(startInfo);
                proc.OutputDataReceived += Proc_OutputDataReceived;
                proc.BeginOutputReadLine();

                proc.WaitForExit();
                if (!string.IsNullOrEmpty(_responseString))
                {
                    StatusString = string.Empty;
                    var lines = _responseString.Split('\n');
                    var columns = lines[0].Replace("\",\"", "|").TrimStart('\"').TrimEnd('\"').Split('|');
                    var values = lines[1].Replace("\",\"", "|").TrimStart('\"').TrimEnd('\"').Split('|');
                    for (var i = 0; i < columns.Length; ++i)
                    {
                        switch (columns[i])
                        {
                            case "Status":
                                StatusString += $"{values[i]}.";
                                break;
                            case "Last Run Time":
                                if (DateTime.TryParse(values[i], out var dt) && (dt.Year > 2015))
                                {
                                    StatusString += $" Last Run {values[i]};";
                                }
                                else
                                {
                                    StatusString += "The task has not yet run.";
                                }

                                break;
                            case "Last Result":
                                StatusString += $" Exit: 0x{int.Parse(values[i]):X8}";
                                break;
                        }
                    }
                }
            }
            else if (ErrorString.ToLower().Contains("access")) // permission problem
            {
                ExistNoAccess = true;
                StatusString = ErrorString;
            }
            else
            {
                StatusString = "No task is scheduled to run.";
            }

            if (!silent) Logger.WriteVerbose($"Successfully queried the Task Scheduler for status. {StatusString}");
        }

        public string ErrorString;

        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            ErrorString += e.Data + " ";
        }

        private string _responseString = string.Empty;

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            _responseString += e.Data + "\n";
        }

        public bool CreateTask(bool wakeToRun, string startTime, TaskActions[] tActions)
        {
            //string xmlFilename = "epg123Task.xml";
            var date = DateTime.Parse($"{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}T{startTime.Substring(0, 2)}:{startTime.Substring(3, 2)}");

            // create registration
            var registration = new registrationInfoType()
            {
                Author = "GaRyan2's epg123",
                Description = "Utility to update the Windows Media Center Electronic Program Guide by downloading guide information from Schedules Direct and importing the created .mxf file.",
                URI = "\\epg123_update",
                SecurityDescriptor = "D:(A;;FRFWSDWDWO;;;BA)(A;;FRFWSDWDWO;;;SY)(A;;FRFWFXDTDCSDWD;;;NS)(A;;FXFR;;;AU)"
            };

            // create trigger
            var triggers = new triggersType()
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
            var settings = new settingsType()
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
            var principals = new principalsType()
            {
                Principal = new principalType()
                {
                    id = @"SYSTEM",
                    UserId = "S-1-5-18",
                    RunLevel = runLevelType.HighestAvailable,
                    RunLevelSpecified = true
                }
            };

            // create action(s)
            var executions = new execType[tActions.Length];
            for (var i = 0; i < tActions.Length; ++i)
            {
                executions[i] = new execType()
                {
                    id = $"epg123 Execution Action {i + 1}",
                    Command = tActions[i].Path,
                    Arguments = tActions[i].Arguments,
                    WorkingDirectory = Helper.ExecutablePath
                };
            }

            var actions = new actionsType()
            {
                Exec = executions
            };

            // build complete task
            var newTask = new taskType()
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
                using (var stream = new StreamWriter(Helper.Epg123TaskXmlPath, false, Encoding.Unicode))
                {
                    var serializer = new XmlSerializer(typeof(taskType));
                    TextWriter writer = stream;
                    serializer.Serialize(writer, newTask);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to create a {startTime} daily update task XML file. message: {ex.Message}");
            }
            return false;
        }

        public bool ImportTask()
        {
            // create the scheduled task from the created xml file
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/create /xml \"{Helper.Epg123TaskXmlPath}\" /tn {TaskName}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        Logger.WriteInformation("Successfully created the daily update task in Task Scheduler.");
                    }
                    else
                    {
                        Logger.WriteError($"Failed to create a daily update task in Task Scheduler. Exit: {proc.ExitCode:X8}");
                        return false;
                    }
                }
                else
                {
                    Logger.WriteError($"Failed to create a daily update task in Task Scheduler.");
                    return false;
                }

                if (File.Exists(Helper.EhshellExeFilePath) && File.Exists(Helper.Epg123ClientExePath))
                {
                    startInfo.Arguments = $"/change /tn \"\\Microsoft\\Windows\\Media Center\\mcupdate\" /tr \"'{Helper.Epg123ClientExePath}' $(Arg0)\" /enable";
                    var proc2 = Process.Start(startInfo);
                    proc2?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to create a daily update task in Task Scheduler. message: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool DeleteTask()
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/delete /f /tn {TaskName}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                var proc = Process.Start(startInfo);
                if (proc != null)
                {
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        Logger.WriteInformation("Successfully deleted the daily update task from Task Scheduler.");
                    }
                    else
                    {
                        Logger.WriteError($"Failed to delete the daily task in Task Scheduler. Exit: {proc.ExitCode:X8}");
                        return false;
                    }
                }
                else
                {
                    Logger.WriteError($"Failed to delete the daily task in Task Scheduler.");
                }

                if (File.Exists(Helper.EhshellExeFilePath))
                {
                    startInfo.Arguments = "/change /tn \"\\Microsoft\\Windows\\Media Center\\mcupdate\" /tr \"'%SystemRoot%\\ehome\\mcupdate.exe' $(Arg0)\" /enable";
                    var proc2 = Process.Start(startInfo);
                    proc2?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to delete the daily task in Task Scheduler. message: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}