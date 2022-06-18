using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace epg123
{
    public static class Logger
    {
        private static bool firstEntry = true;
        private static bool registered = false;

        private static void CreateEventLogSource(string source)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\services\eventlog\Media Center\{source}");
                if (key == null)
                {
                    if (EventLog.SourceExists(source)) return;
                    var sourceData = new EventSourceCreationData(source, "Media Center");
                    EventLog.CreateEventSource(sourceData);
                }
                registered = true;
            }
            catch (Exception ex)
            {
                WriteInformation($"{source} has not been registered as a source for Media Center event logs. This GUI must be executed with elevated rights to add {source} as a valid source.\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void Initialize(string log, string source)
        {
            try
            {
                // initialize/create the event log source for the service
                CreateEventLogSource(source);

                // reset parameters
                eventLog = new EventLog()
                {
                    Source = source,
                    Log = log
                };
                eventMessage = new StringBuilder();
                SingleEventLogEntries = false;
                EventId = 0;
                eventType = EventLogEntryType.FailureAudit;
            }
            catch
            {
                WriteTraceLog(TraceLevel.Warning, $"\"{source}\" is not registered as a source for the \"{log}\" Event Log. This program needs to be run as administrator to add it as a source.", DateTime.Now);
            }
        }

        public static void WriteError(string message)
        {
            WriteLogEntries(TraceLevel.Error, message);
        }

        public static void WriteWarning(string message)
        {
            WriteLogEntries(TraceLevel.Warning, message);
        }

        public static void WriteInformation(string message)
        {
            WriteLogEntries(TraceLevel.Info, message);
        }

        public static void WriteVerbose(string message)
        {
            WriteTraceLog(TraceLevel.Verbose, message, DateTime.Now);
        }

        public static void WriteMessage(string message)
        {
            WriteTraceLog(TraceLevel.Off, message, DateTime.Now);
        }

        public static void Close()
        {
            if (eventLog == null) return;
            SingleEventLogEntries = true;
            eventLog.Close();
        }

        private static void WriteLogEntries(TraceLevel traceLevel, string message)
        {
            var now = DateTime.Now;
            WriteTraceLog(traceLevel, message, now);
            WriteEventLog(traceLevel, message, now);
        }


        #region ========== Trace Log File ==========

        private const int Maxlogfiles = 2;
        private const int Maxlogsize = 1024 * 1024;

        private static void CheckFileLength()
        {
            try
            {
                if (!File.Exists(Helper.Epg123TraceLogPath)) return;
                var fileInfo = new FileInfo(Helper.Epg123TraceLogPath);
                if (fileInfo.Length <= Maxlogsize) return;

                // rename current log file with date/time stamp
                File.Move(Helper.Epg123TraceLogPath, Helper.Epg123TraceLogPath.Replace(".log", $"{DateTime.Now:yyyyMMdd_HHmmss}.log"));

                // find all the trace log files with proper date/time stamps
                var sortedList = new SortedList<string, string>();
                foreach (var file in Directory.GetFiles(Helper.Epg123ProgramDataFolder, "trace*.log"))
                {
                    var filename = new FileInfo(file).Name;
                    if (DateTime.TryParseExact(filename, "'trace'yyyyMMdd_HHmmss'.log'", null, System.Globalization.DateTimeStyles.None, out _))
                    {
                        sortedList.Add(filename, file);
                    }
                }

                // delete the oldest log file(s)
                if (sortedList.Count <= Maxlogfiles) return;
                for (var i = 0; i < Maxlogfiles; i++)
                {
                    sortedList.RemoveAt(sortedList.Count - 1);
                }

                foreach (var file in sortedList)
                {
                    Helper.DeleteFile(file.Value);
                }
            }
            catch
            {
                // ignored
            }
        }

        private static void WriteTraceLog(TraceLevel traceLevel, string message, DateTime time)
        {
            string[] levels = {"", "[ERROR] ", "[WARNG] ", "[ INFO] ", "[ INFO] "};
            var type = levels[(int) traceLevel];

            try
            {
                if (eventLog == null) Console.WriteLine(message);
                if (firstEntry) CheckFileLength();
                using (var fs = new FileStream(Helper.Epg123TraceLogPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"[{time:G}] {type}{message}");
                }

                firstEntry = false;
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region ========== Event Log ==========

        private static bool singleEntries;

        public static bool SingleEventLogEntries
        {
            get => singleEntries;
            set
            {
                if (!string.IsNullOrEmpty(eventMessage.ToString()) && !singleEntries && value)
                {
                    const int maxchars = 16383;
                    for (var i = 0; i < eventMessage.Length; i += maxchars)
                    {
                        eventLog.WriteEntry(
                            eventMessage.ToString().Substring(i, Math.Min(eventMessage.Length - i, maxchars)),
                            eventType, EventId);
                    }

                    eventMessage = null;
                }

                singleEntries = value;
            }
        }

        public static int EventId;
        private static EventLog eventLog;
        private static StringBuilder eventMessage;
        private static EventLogEntryType eventType;

        private static void WriteEventLog(TraceLevel traceLevel, string message, DateTime time)
        {
            // make sure we have an eventlog to write to
            if ((eventLog == null) || string.IsNullOrEmpty(eventLog.Source)) return;

            var entryType = EventLogEntryType.Information;
            var entryId = 0;
            switch (traceLevel)
            {
                case TraceLevel.Error: // error that causes aborts
                    entryType = EventLogEntryType.Error;
                    entryId = 0xDEAD; // 57005
                    break;
                case TraceLevel.Warning: // alerts/warnings that don't abort
                    entryType = EventLogEntryType.Warning;
                    entryId = 0xBAD1; // 47825
                    break;
                case TraceLevel.Info: // minimal information
                    break;
                default: // do not log verbose or off tracelevel stuff
                    return;
            }

            if (!registered) return;
            var msg = $"{time:T} - {message}\n";
            if (SingleEventLogEntries)
            {
                eventLog.WriteEntry(msg, entryType, entryId);
            }
            else
            {
                eventType = (EventLogEntryType) Math.Min((int) eventType, (int) entryType);
                EventId = Math.Max(EventId, entryId);
                eventMessage.Append(msg);
            }
        }

        #endregion
    }
}