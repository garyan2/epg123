using System;
using System.IO;

namespace GaRyan2.Utilities
{
    public static partial class Helper
    {
        private static string _executablePath;

        /// <summary>
        /// Folder location where the executables are located
        /// </summary>
        public static string ExecutablePath
        {
            get
            {
                if (string.IsNullOrEmpty(_executablePath))
                {
                    _executablePath = AppDomain.CurrentDomain.BaseDirectory;
                    if (!string.IsNullOrEmpty(_executablePath)) Directory.SetCurrentDirectory(_executablePath);
                    if (!Directory.Exists(Epg123OutputFolder)) Directory.CreateDirectory(Epg123OutputFolder);
                }
                return _executablePath;
            }
            set { _executablePath = value; }
        }

        #region ========== Folder Locations ==========
        /// <summary>
        /// The folder for all user writable files are based from
        /// </summary>
        public static string Epg123ProgramDataFolder
        {
            get
            {
                if (ExecutablePath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower()) ||
                    ExecutablePath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower()))
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\GaRyan2\\epg123\\";
                }
                return ExecutablePath;
            }
        }

        /// <summary>
        /// The folder used to store program and WMC backups
        /// </summary>
        public static string Epg123BackupFolder => Epg123ProgramDataFolder + "backup\\";

        /// <summary>
        /// The folder used to store all cached files
        /// </summary>
        public static string Epg123CacheFolder => Epg123ProgramDataFolder + "cache\\";

        /// <summary>
        /// The folder used to cache images for use and distribution over the network
        /// </summary>
        public static string Epg123ImageCache => Epg123ProgramDataFolder + "images\\";

        /// <summary>
        /// The folder used to store all the station logos
        /// </summary>
        public static string Epg123LogosFolder => Epg123ProgramDataFolder + "logos\\";

        /// <summary>
        /// The folder used to deposit generated guide files
        /// </summary>
        public static string Epg123OutputFolder => Epg123ProgramDataFolder + "output\\";

        /// <summary>
        /// The folder used to store default satellite overrides
        /// </summary>
        public static string DefaultSatelliteFolder => Epg123ProgramDataFolder + "satellites\\";
        #endregion

        #region ========== Executable Paths ==========
        /// <summary>
        /// The file path for the epg123.exe executable
        /// </summary>
        public static string Epg123ExePath => ExecutablePath + "epg123.exe";

        /// <summary>
        /// The file path for the epg123_gui.exe executable
        /// </summary>
        public static string Epg123GuiPath => ExecutablePath + "epg123_gui.exe";

        /// <summary>
        /// The file path for the hdhr2mxf.exe executable
        /// </summary>
        public static string Hdhr2MxfExePath => ExecutablePath + "hdhr2mxf.exe";

        /// <summary>
        /// The file path for the plutotv.exe executable
        /// </summary>
        public static string PlutoTvExePath => ExecutablePath + "plutotv.exe";

        /// <summary>
        /// The file path for the stirrtv.exe executable
        /// </summary>
        public static string StirrTvExePath => ExecutablePath + "stirrtv.exe";

        /// <summary>
        /// The file path for the epg123Client.exe executable
        /// </summary>
        public static string Epg123ClientExePath => ExecutablePath + "epg123Client.exe";

        /// <summary>
        /// The file path for the epg123Transfer.exe executable
        /// </summary>
        public static string Epg123TransferExePath => ExecutablePath + "epg123Transfer.exe";

        /// <summary>
        /// The file path for the epg123Server.exe executable
        /// </summary>
        public static string TokenServer => ExecutablePath + "epg123Server.exe";

        /// <summary>
        /// The file path for the logViewer.exe executable
        /// </summary>
        public static string LogViewer => ExecutablePath + "logViewer.exe";
        #endregion

        #region ========== Output Files ==========
        /// <summary>
        /// The file path for the epg123.mxf file
        /// </summary>
        public static string Epg123MxfPath => Epg123OutputFolder + "epg123.mxf";

        /// <summary>
        /// The file path for the epg123.xmltv file
        /// </summary>
        public static string Epg123XmltvPath => Epg123OutputFolder + "epg123.xmltv";

        /// <summary>
        /// The file path for the hdhr2mxf.m3u file
        /// </summary>
        public static string Hdhr2MxfM3uPath => Epg123OutputFolder + "hdhr2mxf.m3u";

        /// <summary>
        /// The file path for the hdhr2mxf.mxf file
        /// </summary>
        public static string Hdhr2MxfMxfPath => Epg123OutputFolder + "hdhr2mxf.mxf";

        /// <summary>
        /// The file path for the hdhr2mxf.xmltv file
        /// </summary>
        public static string Hdhr2mxfXmltvPath => Epg123OutputFolder + "hdhr2mxf.xmltv";

        /// <summary>
        /// The file path for the plutotv.m3u file
        /// </summary>
        public static string PlutoTvM3uPath => Epg123OutputFolder + "plutotv.m3u";

        /// <summary>
        /// The file path for the plutotv.xmltv file
        /// </summary>
        public static string PlutoTvXmltvPath => Epg123OutputFolder + "plutotv.xmltv";

        /// <summary>
        /// The file path for the stirrtv.m3u file
        /// </summary>
        public static string StirrTvM3uPath => Epg123OutputFolder + "stirrtv.m3u";

        /// <summary>
        /// The file path for the stirrtv.xmltv file
        /// </summary>
        public static string StirrTvXmltvPath => Epg123OutputFolder + "stirrtv.xmltv";

        /// <summary>
        /// The file path for the mmuiplus.json UI+ support file
        /// </summary>
        public static string Epg123MmuiplusJsonPath => Epg123OutputFolder + "mmuiplus.json";
        #endregion

        #region ========== Cache Files ==========
        /// <summary>
        /// The file path for epg123cache.json file
        /// </summary>
        public static string Epg123CacheJsonPath => Epg123CacheFolder + "epg123cache.json";

        /// <summary>
        /// The file path for the imageCache.json file
        /// </summary>
        public static string Epg123ImageCachePath => Epg123ImageCache + "imageCache.json";
        #endregion

        #region ========== External Executable Files ==========
        /// <summary>
        /// The file path to the WMC ehshell.exe file
        /// </summary>
        public static string EhshellExeFilePath => Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\ehshell.exe");

        /// <summary>
        /// The file path to the WMC loadmxf.exe file
        /// </summary>
        public static string LoadMxfExeFilePath => Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\loadmxf.exe");

        /// <summary>
        /// The file path to the WMC mcupdate.exe file
        /// </summary>
        public static string McUpdateExeFilePath => Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\mcupdate.exe");
        #endregion

        #region ========== Configuration Files ==========
        /// <summary>
        /// The file path for the epg123.cfg configuration file
        /// </summary>
        public static string Epg123CfgPath => Epg123ProgramDataFolder + "epg123.cfg";

        /// <summary>
        /// The file path for the custom lineups file
        /// </summary>
        public static string Epg123CustomLineupsXmlPath => Epg123ProgramDataFolder + "customLineup.xml";


        /// <summary>
        /// The file path for a defaultsatellites.mxf file for custom setups
        /// </summary>
        public static string DefaultSatellitesPath => DefaultSatelliteFolder + "DefaultSatellites.mxf";

        /// <summary>
        /// The file path to update satellite transponders
        /// </summary>
        public static string SatellitesXmlPath => DefaultSatelliteFolder + "satellites.xml";

        /// <summary>
        /// The resulting transponder file based on satellites.xml
        /// </summary>
        public static string TransponderMxfPath => DefaultSatelliteFolder + "transponders.mxf";

        /// <summary>
        /// The file path for the EPG123Status.png status logo
        /// </summary>
        public static string Epg123StatusLogoPath => Epg123ProgramDataFolder + "EPG123Status.png";

        /// <summary>
        /// The file path for the epg123Task.xml file
        /// </summary>
        public static string Epg123TaskXmlPath => Epg123ProgramDataFolder + "epg123Task.xml";

        /// <summary>
        /// The file to define what action to perform when starting elevated
        /// </summary>
        public static string EButtonPath => Epg123ProgramDataFolder + "ebutton.txt";
        #endregion

        #region ========== Log Files ==========
        /// <summary>
        /// The file path for the active trace.log file
        /// </summary>
        public static string Epg123TraceLogPath => Epg123ProgramDataFolder + "trace.log";

        /// <summary>
        /// 
        /// </summary>
        public static string ServerLogPath => Epg123ProgramDataFolder + "server.log";
        #endregion
    }
}
