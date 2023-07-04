using System;
using System.Diagnostics;
using System.IO;

namespace GaRyan2.Utilities
{
    public static partial class Logger
    {
        private static readonly object _logLock = new object();
        private const int Maxlogsize = 1024 * 1024;
        private static readonly TraceLevel _level = TraceLevel.Verbose;
        private static bool firstEntry = true;
        private static string _sessionString;

        private static string _logFile;
        public static int Status;

        public static void Initialize(string logFile)
        {
            Status = 0x0;
            _logFile = logFile;
            firstEntry = true;
            _sessionString = string.Empty;
        }

        public static void CloseAndSendNotification()
        {
            SendNotification();
        }

        public static void WriteError(string message)
        {
            Status = Math.Max(Status, 0xDEAD);
            if (_level == TraceLevel.Off) return;
            if (_level >= TraceLevel.Error)
                WriteLogEntries(TraceLevel.Error, message);
        }

        public static void WriteWarning(string message)
        {
            Status = Math.Max(Status, 0xBAD1);
            if (_level == TraceLevel.Off) return;
            if (_level >= TraceLevel.Warning)
                WriteLogEntries(TraceLevel.Warning, message);
        }

        public static void WriteInformation(string message)
        {
            if (_level == TraceLevel.Off) return;
            if (_level >= TraceLevel.Info)
                WriteLogEntries(TraceLevel.Info, message);
        }

        public static void WriteVerbose(string message)
        {
            if (_level == TraceLevel.Off) return;
            if (_level >= TraceLevel.Verbose)
                WriteLogEntries(TraceLevel.Verbose, message);
        }

        public static void WriteMessage(string message)
        {
            if (_level == TraceLevel.Off) return;
            if (_level >= TraceLevel.Verbose)
                WriteLogEntries(TraceLevel.Off, message);
        }

        private static void WriteLogEntries(TraceLevel traceLevel, string message)
        {
            string[] levels = { "", "[ERROR] ", "[WARNG] ", "[ INFO] ", "[ INFO] " };
            string msg = $"[{DateTime.Now:G}] {levels[(int)traceLevel]}{message}";

            try
            {
                _sessionString += $"{msg}\n";
                Console.WriteLine(msg);
                if (firstEntry) CheckFileLength();
                lock (_logLock)
                {
                    using (var fs = new FileStream(_logFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(msg);
                    }
                }
            }
            catch { }
        }

        private static void CheckFileLength()
        {
            try
            {
                firstEntry = false;
                if (!File.Exists(_logFile)) return;
                var fileInfo = new FileInfo(_logFile);
                if (fileInfo.Length <= Maxlogsize) return;

                // rename current log file with date/time stamp
                File.Move(_logFile, _logFile.Replace(fileInfo.Extension, $"{DateTime.Now:yyyyMMdd_HHmmss}{fileInfo.Extension}"));
            }
            catch { }
        }
    }
}