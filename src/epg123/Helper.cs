using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace epg123
{
    static class Helper
    {
        public enum PreferredLogos
        {
            white,
            gray,
            dark,
            light,
            none
        };

        public static string epg123Version
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }
        
        public static string backupZipFile { get; set; }
        public static string outputPathOverride { get; set; }

        public static bool CreateAndSetFolderAcl(string folder)
        {
            try
            {
                // establish security identifier for everyone and desired rights
                SecurityIdentifier Everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                FileSystemRights Rights = FileSystemRights.FullControl;

                // make sure directory exists
                DirectoryInfo Info = new DirectoryInfo(folder);
                bool newInstall = !Directory.Exists(folder);
                if (newInstall)
                {
                    Info = Directory.CreateDirectory(folder);
                }

                // check to make sure everyone does not already have access
                DirectorySecurity Security = Info.GetAccessControl(AccessControlSections.Access);
                AuthorizationRuleCollection AuthorizationRules = Security.GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in AuthorizationRules)
                {
                    if ((rule.IdentityReference.Value == Everyone.Translate(typeof(NTAccount)).ToString()) &&
                        (rule.AccessControlType == AccessControlType.Allow) &&
                        (rule.FileSystemRights == Rights))
                    {
                        return true;
                    }
                }

                if (!newInstall && (DialogResult.OK != MessageBox.Show(string.Format("EPG123 is going to add the user 'Everyone' with Full Control rights to the \"{0}\" folder. This may take a while depending on how many files are in the folder and subfolders.", folder), "Edit Permissions", MessageBoxButtons.OK)))
                {
                    return false;
                }

                // *** Add Access Rule to the actual directory itself
                FileSystemAccessRule AccessRule = new FileSystemAccessRule(Everyone,
                                                                           Rights,
                                                                           InheritanceFlags.None,
                                                                           PropagationFlags.NoPropagateInherit,
                                                                           AccessControlType.Allow);

                Security.ModifyAccessRule(AccessControlModification.Set, AccessRule, out bool Result);
                if (!Result)
                    return false;

                // *** Always allow objects to inherit on a directory
                InheritanceFlags iFlags = InheritanceFlags.ObjectInherit;
                iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

                // *** Add Access rule for the inheritance
                AccessRule = new FileSystemAccessRule(Everyone,
                                                      Rights,
                                                      iFlags,
                                                      PropagationFlags.InheritOnly,
                                                      AccessControlType.Allow);
                Result = false;
                Security.ModifyAccessRule(AccessControlModification.Add, AccessRule, out Result);

                if (!Result)
                    return false;

                Info.SetAccessControl(Security);
            }
            catch
            {
                if (!UserHasElevatedRights)
                {
                    MessageBox.Show(string.Format("EPG123 did not have sufficient priveleges to edit the folder \"{0}\" permissions. Please run this GUI with elevated rights (as Administrator) to make the necessary changes.", folder),
                                    "Folder Permissions Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether program is run with elevated rights
        /// </summary>
        /// <returns></returns>
        public static bool UserHasElevatedRights
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static string tableContains(string[] table, string text, bool exactMatch = false)
        {
            if (table != null)
            {
                foreach (string str in table)
                {
                    if (!exactMatch && str.ToLower().Contains(text.ToLower())) return "true";
                    else if (str.ToLower().Equals(text.ToLower())) return "true";
                }
            }
            return null;
        }

        public static string stringContains(string str, string text)
        {
            if (str != null)
            {
                if (str.ToLower().Contains(text.ToLower())) return "true";
            }
            return null;
        }

        public static void WriteEButtonFile(string action)
        {
            using (StreamWriter sw = new StreamWriter(EButtonPath, false))
            {
                sw.WriteLine(action);
                sw.Close();
            }
        }

        #region ========== Folder and File Paths ==========
        /// <summary>
        /// Folder location where the executables are located
        /// </summary>
        public static string ExecutablePath { get; set; }

        /// <summary>
        /// The file path for the epg123.exe executable
        /// </summary>
        public static string Epg123ExePath
        {
            get
            {
                return (ExecutablePath + "\\epg123.exe");
            }
        }

        /// <summary>
        /// The file path for the hdhr2mxf.exe executable
        /// </summary>
        public static string Hdhr2mxfExePath
        {
            get
            {
                return (ExecutablePath + "\\hdhr2mxf.exe");
            }
        }

        /// <summary>
        /// The file path for the epg123Client.exe executable
        /// </summary>
        public static string Epg123ClientExePath
        {
            get
            {
                return (ExecutablePath + "\\epg123Client.exe");
            }
        }

        /// <summary>
        /// The file path for the epg123Transfer.exe executable
        /// </summary>
        public static string Epg123TransferExePath
        {
            get
            {
                return (ExecutablePath + "\\epg123Transfer.exe");
            }
        }

        /// <summary>
        /// The folder for all user writeable files are based from
        /// </summary>
        public static string Epg123ProgramDataFolder
        {
            get
            {
                if (ExecutablePath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower()) ||
                    ExecutablePath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLower()))
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\GaRyan2\\epg123";
                }
                else
                {
                    return ExecutablePath;
                }
            }
        }

        /// <summary>
        /// The file path for the epg123.cfg configuration file
        /// </summary>
        public static string Epg123CfgPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\epg123.cfg";
            }
        }

        /// <summary>
        /// The file path for the custom lineups file
        /// </summary>
        public static string Epg123CustomLineupsXmlPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\customLineup.xml";
            }
        }

        /// <summary>
        /// The folder used to store program and WMC backups
        /// </summary>
        public static string Epg123BackupFolder
        {
            get
            {
                return Epg123ProgramDataFolder + "\\backup";
            }
        }

        /// <summary>
        /// The folder used to store all cached files
        /// </summary>
        public static string Epg123CacheFolder
        {
            get
            {
                return Epg123ProgramDataFolder + "\\cache";
            }
        }

        /// <summary>
        /// The file path for epg123cache.json file
        /// </summary>
        public static string Epg123CacheJsonPath
        {
            get
            {
                return Epg123CacheFolder + "\\epg123cache.json";
            }
        }

        public static string Epg123CompressCachePath
        {
            get
            {
                return Epg123CacheFolder + "\\epg123cache.zip";
            }
        }

        /// <summary>
        /// The folder used to store all the station logos
        /// </summary>
        public static string Epg123LogosFolder
        {
            get
            {
                return Epg123ProgramDataFolder + "\\logos";
            }
        }

        /// <summary>
        /// The folder used to store all the station logos from Schedules Direct
        /// </summary>
        public static string Epg123SdLogosFolder
        {
            get
            {
                return Epg123ProgramDataFolder + "\\sdlogos";
            }
        }

        /// <summary>
        /// The folder used to deposit generated guide files
        /// </summary>
        public static string Epg123OutputFolder
        {
            get
            {
                return Epg123ProgramDataFolder + "\\output";
            }
        }

        /// <summary>
        /// The file path for the epg123.mxf file
        /// </summary>
        public static string Epg123MxfPath
        {
            get
            {
                return Epg123OutputFolder + "\\epg123.mxf";
            }
        }

        /// <summary>
        /// The file path for the epg123.xml file
        /// </summary>
        public static string Epg123XmltvPath
        {
            get
            {
                return Epg123OutputFolder + "\\epg123.xmltv";
            }
        }

        /// <summary>
        /// The file path for the epg123_Guide.pdf help file
        /// </summary>
        public static string Epg123HelpFilePath
        {
            get
            {
                return ExecutablePath + "\\epg123_Guide.pdf";
            }
        }

        /// <summary>
        /// The file path for the EPG123Status.png status logo
        /// </summary>
        public static string Epg123StatusLogoPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\EPG123Status.png";
            }
        }

        /// <summary>
        /// The file path for the guideImages.xml file
        /// </summary>
        public static string Epg123GuideImagesXmlPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\guideImages.xml";
            }
        }

        /// <summary>
        /// The file path for the active trace.log file
        /// </summary>
        public static string Epg123TraceLogPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\trace.log";
            }
        }

        /// <summary>
        /// The file path for the mmuiplus.json UI+ support file
        /// </summary>
        public static string Epg123MmuiplusJsonPath
        {
            get
            {
                return Epg123OutputFolder + "\\mmuiplus.json";
            }
        }

        /// <summary>
        /// The file path for the epg123Task.xml file
        /// </summary>
        public static string Epg123TaskXmlPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\epg123Task.xml";
            }
        }

        /// <summary>
        /// The file to define what action to perform when starting elevated
        /// </summary>
        public static string EButtonPath
        {
            get
            {
                return Epg123ProgramDataFolder + "\\ebutton.txt";
            }
        }

        /// <summary>
        /// The file path to the WMC ehshell.exe file
        /// </summary>
        public static string EhshellExeFilePath
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\ehshell.exe");
            }
        }
        #endregion
    }
}
