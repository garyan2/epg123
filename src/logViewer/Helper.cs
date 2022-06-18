using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace epg123
{
    internal static class Helper
    {
        public static void EstablishFileFolderPaths()
        {
            // set the base path and the working directory
            ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(ExecutablePath)) Directory.SetCurrentDirectory(ExecutablePath);
        }

        /// <summary>
        /// Folder location where the executables are located
        /// </summary>
        public static string ExecutablePath { get; set; }

        /// <summary>
        /// The folder for all user writable files are based from
        /// </summary>
        public static string Epg123ProgramDataFolder
        {
            get
            {
                if (ExecutablePath != null && 
                    (ExecutablePath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower()) ||
                     ExecutablePath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower())))
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\GaRyan2\\epg123";
                }
                return ExecutablePath;
            }
        }

        /// <summary>
        /// The file path for the active trace.log file
        /// </summary>
        public static string Epg123TraceLogPath => Epg123ProgramDataFolder + "\\trace.log";
    }
}