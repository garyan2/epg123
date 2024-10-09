using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TaskScheduler;

namespace GaRyan2.Utilities
{
    public class EpgTaskScheduler
    {
        private readonly ITaskService taskService = new TaskScheduler.TaskScheduler();

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
            if (!taskService.Connected)
            {
                taskService.Connect();
                if (!taskService.Connected)
                {
                    Logger.WriteVerbose("Failed to connect to Task Scheduler service for status.");
                    return;
                }
            }

            try
            {
                var task = taskService.GetFolder("\\").GetTask(TaskName);
                var taskDefinition = task.Definition;

                Exist = true;
                Wake = taskDefinition.Settings.WakeToRun;

                // get trigger
                _oldTime = SchedTime = DateTime.Parse(((ITrigger)taskDefinition.Triggers[1]).StartBoundary);

                // get executing actions
                var listActions = new List<TaskActions>(taskDefinition.Actions.Count);
                foreach (IExecAction action in taskDefinition.Actions)
                {
                    listActions.Add(new TaskActions
                    {
                        Path = action.Path,
                        Arguments = action.Arguments?.Replace("EPG:", "http:")
                    });
                }
                Actions = listActions.ToArray();

                // build status string
                string[] states = { "Unknown", "Disabled", "Queued", "Ready", "Running" };
                StatusString = $"{states[(int)task.State]}. ";
                if (task.LastRunTime.Year < 2015) StatusString += "The task has not yet run. ";
                else StatusString += $"Last Run {task.LastRunTime}. Exit: 0x{task.LastTaskResult:X8}";
            }
            catch
            {
                Exist = ExistNoAccess = false;
                StatusString = "No task is schedule to run.";
            }
            if (!silent) Logger.WriteVerbose($"Successfully queried the Task Scheduler for status. {StatusString}");
        }

        public bool CreateTask(bool wakeToRun, string startTime, TaskActions[] tActions)
        {
            if (!taskService.Connected)
            {
                taskService.Connect();
                if (!taskService.Connected)
                {
                    Logger.WriteVerbose("Failed to connect to Task Scheduler service to create task.");
                    return false;
                }
            }

            // create definition
            var taskDefinition = taskService.NewTask(0);

            // create registration
            taskDefinition.RegistrationInfo.Author = "GaRyan2";
            taskDefinition.RegistrationInfo.Date = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            taskDefinition.RegistrationInfo.Description = "Utility to update the Windows Media Center Electronic Program Guide by downloading guide information from Schedules Direct and importing the created .mxf file.";
            taskDefinition.RegistrationInfo.SecurityDescriptor = "D:(A;;FRFWSDWDWO;;;BA)(A;;FRFWSDWDWO;;;SY)(A;;FRFWFXDTDCSDWD;;;NS)(A;;FXFR;;;AU)";
            taskDefinition.RegistrationInfo.URI = "\\epg123_update";

            // create settings
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.ExecutionTimeLimit = "PT23H";
            taskDefinition.Settings.Priority = 6;
            //taskDefinition.Settings.RestartCount = 5;
            //taskDefinition.Settings.RestartInterval = "PT30M";
            taskDefinition.Settings.StartWhenAvailable = true;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Settings.WakeToRun = wakeToRun;

            // create principal
            taskDefinition.Principal.Id = "SYSTEM";
            taskDefinition.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
            taskDefinition.Principal.UserId = "S-1-5-18";

            // create trigger
            var taskTrigger = (IDailyTrigger)taskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);
            taskTrigger.Id = "epg123 Daily Trigger";
            taskTrigger.DaysInterval = 1;
            taskTrigger.StartBoundary = $"{DateTime.Now.Year:D4}-{DateTime.Now.Month:D2}-{DateTime.Now.Day:D2}T{startTime.Substring(0, 2)}:{startTime.Substring(3, 2)}:00";

            // create actions
            taskDefinition.Actions.Context = "SYSTEM";
            foreach (var tAction in tActions)
            {
                var action = (IExecAction)taskDefinition.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Arguments = tAction.Arguments?.Replace("http:", "EPG:");
                action.Id = $"epg123 Execution Action {taskDefinition.Actions.Count}";
                action.Path = tAction.Path;
                action.WorkingDirectory = Helper.ExecutablePath;
            }

            // register the task
            if (Helper.UserHasElevatedRights)
            {
                taskService.GetFolder("\\").RegisterTaskDefinition(TaskName, taskDefinition,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT);
                ChangeMcUpdateTaskPath(Helper.Epg123ClientExePath);
                Logger.WriteInformation("Successfully created the daily task in Task Scheduler.");
            }
            else
            {
                using (var stream = new StreamWriter(Helper.Epg123TaskXmlPath, false, Encoding.Unicode))
                {
                    stream.Write(taskDefinition.XmlText);
                }
                Logger.WriteInformation("Successfully wrote task xml file for import when running without elevated rights.");
            }
            return true;
        }

        private void ChangeMcUpdateTaskPath(string path)
        {
            if (!File.Exists(Helper.EhshellExeFilePath)) return;

            _ = taskService.NewTask(0);
            ITaskDefinition newTask = taskService.GetFolder("\\Microsoft\\Windows\\Media Center").GetTask("mcupdate").Definition;
            ((IExecAction)newTask.Actions[1]).Path = path;
            taskService.GetFolder("\\Microsoft\\Windows\\Media Center").RegisterTaskDefinition("mcupdate", newTask,
                (int)_TASK_CREATION.TASK_UPDATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT);
        }

        public bool ImportTask()
        {
            if (!taskService.Connected)
            {
                taskService.Connect();
                if (!taskService.Connected)
                {
                    Logger.WriteVerbose("Failed to connect to Task Scheduler service to import task.");
                    return false;
                }
            }

            if (Helper.UserHasElevatedRights)
            {
                // create definition
                var taskDefinition = taskService.NewTask(0);
                using (var sr = new StreamReader(Helper.Epg123TaskXmlPath, true))
                {
                    try
                    {
                        taskDefinition.XmlText = sr.ReadToEnd();
                        taskService.GetFolder("\\").RegisterTaskDefinition(TaskName, taskDefinition,
                            (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT);
                        Logger.WriteInformation("Successfully imported the daily task into Task Scheduler.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"Failed to import the daily task into Task Scheduler. Exit: {ex.HResult:X8}{Helper.ReportExceptionMessages(ex)}");
                    }
                }
                return false;
            }
            else
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Create /XML \"{Helper.Epg123TaskXmlPath}\" /TN epg123_update /F",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                var proc = Process.Start(startInfo);
                proc.WaitForExit();
                if (proc.ExitCode == 0) Logger.WriteInformation("Successfully imported the daily task into Task Scheduler.");
                else Logger.WriteError("Failed to import the daily task into Task Scheduler without elevated priveleges.");
                _ = Helper.DeleteFile(Helper.Epg123TaskXmlPath);
                return proc.ExitCode == 0;
            }
        }

        public bool DeleteTask()
        {
            if (!taskService.Connected)
            {
                taskService.Connect();
                if (!taskService.Connected)
                {
                    Logger.WriteVerbose("Failed to connect to Task Scheduler service to delete task.");
                    return false;
                }
            }

            if (Helper.UserHasElevatedRights)
            {
                try
                {
                    taskService.GetFolder("\\").DeleteTask(TaskName, 0);
                    ChangeMcUpdateTaskPath("%SystemRoot%\\ehome\\mcupdate.exe");
                    Logger.WriteInformation("Successfully deleted the daily task from Task Scheduler.");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"Failed to delete the daily task from Task Scheduler. Exit: {ex.HResult:X8}{Helper.ReportExceptionMessages(ex)}");
                }
                return false;
            }
            else
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Delete /TN epg123_update /F",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                var proc = Process.Start(startInfo);
                proc.WaitForExit();
                if (proc.ExitCode == 0) Logger.WriteInformation("Successfully deleted the daily task from Task Scheduler.");
                else Logger.WriteError("Failed to delete the daily task fram Task Scheduler without elevated priveleges.");
                return proc.ExitCode == 0;
            }
        }
    }
}