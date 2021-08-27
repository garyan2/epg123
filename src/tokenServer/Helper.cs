using System;
using System.IO;
using System.Reflection;

namespace tokenServer
{
    internal static class Helper
    {
        public const int TcpPort = 9009;
        public const int UdpPort = 9010;
        public const string SdBaseName = @"https://json.schedulesdirect.org/20141201";
        private static readonly object logLock = new object();

        public static string Epg123Version => Assembly.GetEntryAssembly()?.GetName().Version.ToString();

        public static void WriteLogEntry(string message)
        {
            lock (logLock)
            {
                using (var writer = new StreamWriter(Helper.Epg123ServerLogPath, true))
                {
                    writer.WriteLine($"[{DateTime.Now:G}] {message}");
                }
            }
        }

        #region ========== Folder and File Paths ==========

        /// <summary>
        /// Folder location where the executables are located
        /// </summary>
        public static string ExecutablePath { get; set; }

        /// <summary>
        /// The folder for all user writeable files are based from
        /// </summary>
        public static string Epg123ProgramDataFolder
        {
            get
            {
                if (ExecutablePath.ToLower()
                        .Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower()) ||
                    ExecutablePath.ToLower()
                        .Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower()))
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                           "\\GaRyan2\\epg123";
                }
                return ExecutablePath;
            }
        }

        /// <summary>
        /// The file path for the epg123.cfg configuration file
        /// </summary>
        public static string Epg123CfgPath => Epg123ProgramDataFolder + "\\epg123.cfg";

        /// <summary>
        /// The folder used to deposit generated guide files
        /// </summary>
        public static string Epg123OutputFolder => Epg123ProgramDataFolder + "\\output";

        /// <summary>
        /// The folder used to cache images for use and distribution over the network
        /// </summary>
        public static string Epg123ImageCache => Epg123ProgramDataFolder + "\\images";

        /// <summary>
        /// 
        /// </summary>
        public static string Epg123ImageCachePath => Epg123ImageCache + "\\imageCache.json";

        /// <summary>
        /// The file path for the epg123.mxf file
        /// </summary>
        public static string Epg123MxfPath => Epg123OutputFolder + "\\epg123.mxf";

        /// <summary>
        /// The file path for the epg123.xml file
        /// </summary>
        public static string Epg123XmltvPath => Epg123OutputFolder + "\\epg123.xmltv";

        /// <summary>
        /// The file path for the active trace.log file
        /// </summary>
        public static string Epg123TraceLogPath => Epg123ProgramDataFolder + "\\trace.log";

        /// <summary>
        /// 
        /// </summary>
        public static string Epg123ServerLogPath => Epg123ProgramDataFolder + "\\server.log";
        #endregion
    }
}