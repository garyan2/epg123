using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace epg123
{
    public static class Logger
    {
        public static void Initialize(string log, string source)
        {
            try
            {
                // initialize/create the event log source for the service
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, log);
                }

                // reset parameters
                eventLog = new EventLog()
                {
                    Source = source,
                    Log = log
                };
                eventMessage = new StringBuilder();
                SingleEventLogEntries = false;
                eventID = 0;
                eventType = EventLogEntryType.FailureAudit;
            }
            catch
            {
                WriteTraceLog(TraceLevel.Warning, string.Format("\"{0}\" is not registered as a source for the \"{1}\" Event Log. This program needs to be run as administrator to add it as a source.", source, log), DateTime.Now);
                return;
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
        public static void WriteEventOnly(string message)
        {
            WriteEventLog(TraceLevel.Info, message, DateTime.Now);
        }
        public static void Close()
        {
            if (eventLog != null)
            {
                SingleEventLogEntries = true;
                eventLog.Close();
            }
        }

        private static void WriteLogEntries(TraceLevel traceLevel, string message)
        {
            DateTime now = DateTime.Now;
            WriteTraceLog(traceLevel, message, now); WriteEventLog(traceLevel, message, now);
        }


        #region ========== Trace Log File ==========
        private const int MAXLOGFILES = 2;
        private const int MAXLOGSIZE = 1024 * 1024;
        private static void checkFileLength()
        {
            try
            {
                if (File.Exists(Helper.Epg123TraceLogPath))
                {
                    FileInfo fileInfo = new FileInfo(Helper.Epg123TraceLogPath);
                    if (fileInfo.Length > MAXLOGSIZE)
                    {
                        // rename current log file with date/time stamp
                        File.Move(Helper.Epg123TraceLogPath, Helper.Epg123TraceLogPath.Replace(".log", string.Format("{0:yyyyMMdd_HHmmss}.log", DateTime.Now)));

                        // find all the trace log files with proper date/time stamps
                        SortedList<string, string> sortedList = new SortedList<string, string>();
                        foreach (string file in Directory.GetFiles(Helper.Epg123ProgramDataFolder, "trace*.log"))
                        {
                            DateTime fileTime;
                            string filename = new FileInfo(file).Name;
                            if (DateTime.TryParseExact(filename, "'trace'yyyyMMdd_HHmmss'.log'", null, System.Globalization.DateTimeStyles.None, out fileTime))
                            {
                                sortedList.Add(filename, file);
                            }
                        }

                        // delete the oldest log file(s)
                        if (sortedList.Count > MAXLOGFILES)
                        {
                            for (int i = 0; i < MAXLOGFILES; i++)
                            {
                                sortedList.RemoveAt(sortedList.Count - 1);
                            }

                            foreach (KeyValuePair<string, string> file in sortedList)
                            {
                                try
                                {
                                    File.Delete(file.Value);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static void WriteTraceLog(TraceLevel traceLevel, string message, DateTime time)
        {
            string[] levels = { "", "[ERROR] ", "[WARNG] ", "[ INFO] ", "[ INFO] " };
            string type = levels[(int)traceLevel];

            try
            {
                checkFileLength();
                using (StreamWriter stream = new StreamWriter(Helper.Epg123TraceLogPath, true))
                {
                    stream.WriteLine(string.Format("[{0:G}] {1}{2}", time, type, message));
                }
            }
            catch { }
        }
        #endregion

        #region ========== Event Log ==========
        private static bool singleEntries;
        public static bool SingleEventLogEntries
        {
            get
            {
                return singleEntries;
            }
            set
            {
                if (!string.IsNullOrEmpty(eventMessage.ToString()) && !singleEntries && (value == true))
                {
                    int MAXCHARS = 16383;
                    for (int i = 0; i < eventMessage.Length; i += MAXCHARS)
                    {
                        eventLog.WriteEntry(eventMessage.ToString().Substring(i, Math.Min(eventMessage.Length - i, MAXCHARS)), eventType, eventID);
                    }
                    eventMessage = null;
                }
                singleEntries = value;
            }
        }
        public static int eventID;
        private static EventLog eventLog;
        private static StringBuilder eventMessage;
        private static EventLogEntryType eventType;

        private static void WriteEventLog(TraceLevel traceLevel, string message, DateTime time)
        {
            // make sure we have an eventlog to write to
            if ((eventLog == null) || string.IsNullOrEmpty(eventLog.Source)) return;

            EventLogEntryType entryType = EventLogEntryType.Information;
            int entryID = 0;
            switch (traceLevel)
            {
                case TraceLevel.Error:      // error that causes aborts
                    entryType = EventLogEntryType.Error;
                    entryID = 0xDEAD;       // 57005
                    break;
                case TraceLevel.Warning:    // alerts/warnings that don't abort
                    entryType = EventLogEntryType.Warning;
                    entryID = 0xBAD1;       // 47825
                    break;
                case TraceLevel.Info:       // minimal information
                    break;
                default: // do not log verbose or off tracelevel stuff
                    return;
            }

            string msg = string.Format("{0:T} - {1}\n", time, message);
            if (SingleEventLogEntries)
            {
                eventLog.WriteEntry(msg, entryType, entryID);
            }
            else
            {
                eventType = (EventLogEntryType)Math.Min((int)eventType, (int)entryType);
                eventID = Math.Max(eventID, entryID);
                eventMessage.Append(msg);
            }
        }
        #endregion
    }
}