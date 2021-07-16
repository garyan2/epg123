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

namespace epg123
{
    internal static class Helper
    {
        public enum PreferredLogos
        {
            WHITE,
            GRAY,
            DARK,
            LIGHT,
            NONE
        };

        public static string Epg123Version => Assembly.GetEntryAssembly()?.GetName().Version.ToString();

        public static string BackupZipFile { get; set; }
        public static string OutputPathOverride { get; set; }

        public static void EstablishFileFolderPaths()
        {
            // set the base path and the working directory
            ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(ExecutablePath)) Directory.SetCurrentDirectory(ExecutablePath);

            // establish folders with permissions
            if (Environment.UserInteractive && !CreateAndSetFolderAcl(Epg123ProgramDataFolder))
            {
                Logger.WriteError($"Failed to set full control permissions for Everyone on folder \"{Epg123ProgramDataFolder}\".");
            }
            else
            {
                string[] folders = {Epg123BackupFolder, Epg123CacheFolder, Epg123LogosFolder, Epg123OutputFolder };
                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                }
            }

            // copy custom lineup file to proper location in needed
            var oldCustomFile = ExecutablePath + "\\customLineup.xml.example";
            if (!File.Exists(Epg123CustomLineupsXmlPath) && File.Exists(oldCustomFile))
            {
                File.Copy(oldCustomFile, Epg123CustomLineupsXmlPath);
            }
        }

        public static bool CreateAndSetFolderAcl(string folder)
        {
            try
            {
                // establish security identifier for everyone and desired rights
                var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                const FileSystemRights rights = FileSystemRights.FullControl;

                // make sure directory exists
                var info = new DirectoryInfo(folder);
                var newInstall = !Directory.Exists(folder);
                if (newInstall)
                {
                    info = Directory.CreateDirectory(folder);
                }

                // check to make sure everyone does not already have access
                var security = info.GetAccessControl(AccessControlSections.Access);
                var authorizationRules = security.GetAccessRules(true, true, typeof(NTAccount));
                if (authorizationRules.Cast<FileSystemAccessRule>().Any(rule =>
                    rule.IdentityReference.Value == everyone.Translate(typeof(NTAccount)).ToString() &&
                    rule.AccessControlType == AccessControlType.Allow &&
                    rule.FileSystemRights == rights))
                {
                    return true;
                }

                if (!newInstall && (DialogResult.OK != MessageBox.Show($"EPG123 is going to add the user 'Everyone' with Full Control rights to the \"{folder}\" folder. This may take a while depending on how many files are in the folder and subfolders.",
                    "Edit Permissions", MessageBoxButtons.OK)))
                {
                    return false;
                }

                // *** Add Access Rule to the actual directory itself
                var accessRule = new FileSystemAccessRule(everyone,
                    rights,
                    InheritanceFlags.None,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow);

                security.ModifyAccessRule(AccessControlModification.Set, accessRule, out var result);
                if (!result)
                    return false;

                // *** Always allow objects to inherit on a directory
                const InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

                // *** Add Access rule for the inheritance
                accessRule = new FileSystemAccessRule(everyone,
                    rights,
                    iFlags,
                    PropagationFlags.InheritOnly,
                    AccessControlType.Allow);
                security.ModifyAccessRule(AccessControlModification.Add, accessRule, out result);

                if (!result)
                    return false;

                info.SetAccessControl(security);
            }
            catch
            {
                if (!UserHasElevatedRights)
                {
                    MessageBox.Show($"EPG123 did not have sufficient privileges to edit the folder \"{folder}\" permissions. Please run this GUI with elevated rights (as Administrator) to make the necessary changes.",
                        "Folder Permissions Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                return false;
            }

            return true;
        }

        public static Mutex GetProgramMutex(string uid, bool take)
        {
            var mutex = new Mutex(false, uid);
            try
            {
                var tryAgain = true;
                while (tryAgain)
                {
                    bool result;
                    try
                    {
                        result = mutex.WaitOne(0, false);
                    }
                    catch (AbandonedMutexException e)
                    {
                        result = true;
                    }

                    if (result)
                    {
                        tryAgain = false;
                    }
                    else if (take)
                    {
                        foreach (var proc in Process.GetProcesses())
                        {
                            if (!proc.ProcessName.Equals(Process.GetCurrentProcess().ProcessName) ||
                                proc.Id == Process.GetCurrentProcess().Id) continue;
                            Logger.WriteInformation($"Killing process {proc.ProcessName}[{proc.Id}] to continue execution of new instance.");
                            proc.Kill();
                            proc.WaitForExit(2000);
                            break;
                        }
                    }
                    else
                    {
                        Logger.WriteMessage("===============================================================================");
                        Logger.WriteError($"An instance of {Process.GetCurrentProcess().ProcessName} is already running. Aborting.");
                        Logger.WriteMessage("===============================================================================");
                        Logger.Close();

                        mutex.Close();
                        return null;
                    }
                }
            }
            finally
            {
                // do nothing
            }

            return mutex;
        }

        public static bool DeleteFile(string filepath)
        {
            if (!File.Exists(filepath)) return true;
            try
            {
                File.Delete(filepath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Failed to delete file \"{filepath}\"; {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Determines whether program is run with elevated rights
        /// </summary>
        /// <returns></returns>
        public static bool UserHasElevatedRights
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool TableContains(string[] table, string text, bool exactMatch = false)
        {
            if (table == null) return false;
            foreach (var str in table)
            {
                if (!exactMatch && str.ToLower().Contains(text.ToLower())) return true;
                if (str.ToLower().Equals(text.ToLower())) return true;
            }

            return false;
        }

        public static bool StringContains(string str, string text)
        {
            return str != null && str.ToLower().Contains(text.ToLower());
        }

        public static void WriteEButtonFile(string action)
        {
            using (var sw = new StreamWriter(EButtonPath, false))
            {
                sw.WriteLine(action);
                sw.Flush();
                sw.Close();
            }
        }

        public static void SendPipeMessage(string message)
        {
            var client = new NamedPipeClientStream(".", "Epg123StatusPipe", PipeDirection.Out);
            try
            {
                client.Connect(100);
                var writer = new StreamWriter(client);
                writer.WriteLine(message);
                writer.Flush();
            }
            catch
            {
                // ignored
            }
        }

        public static Bitmap CropAndResizeImage(Bitmap origImg)
        {
            if (origImg == null) return null;

            // set target image size
            const int tgtWidth = 360;
            const int tgtHeight = 270;

            // set target aspect/image size
            const double tgtAspect = 3.0;

            // Find the min/max non-transparent pixels
            var min = new Point(int.MaxValue, int.MaxValue);
            var max = new Point(int.MinValue, int.MinValue);

            for (var x = 0; x < origImg.Width; ++x)
            {
                for (var y = 0; y < origImg.Height; ++y)
                {
                    var pixelColor = origImg.GetPixel(x, y);
                    if (pixelColor.A <= 0) continue;
                    if (x < min.X) min.X = x;
                    if (y < min.Y) min.Y = y;

                    if (x > max.X) max.X = x;
                    if (y > max.Y) max.Y = y;
                }
            }

            // Create a new bitmap from the crop rectangle and increase canvas size if necessary
            var offsetY = 0;
            var cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
            if ((max.X - min.X + 1) / tgtAspect > (max.Y - min.Y + 1))
            {
                offsetY = (int)((max.X - min.X + 1) / tgtAspect - (max.Y - min.Y + 1) + 0.5) / 2;
            }

            var cropImg = new Bitmap(cropRectangle.Width, cropRectangle.Height + offsetY * 2);
            cropImg.SetResolution(origImg.HorizontalResolution, origImg.VerticalResolution);
            using (var g = Graphics.FromImage(cropImg))
            {
                g.DrawImage(origImg, 0, offsetY, cropRectangle, GraphicsUnit.Pixel);
            }

            if (tgtHeight >= cropImg.Height && tgtWidth >= cropImg.Width) return cropImg;

            // resize image if needed
            var scale = Math.Min((double)tgtWidth / cropImg.Width, (double)tgtHeight / cropImg.Height);
            var destWidth = (int)(cropImg.Width * scale);
            var destHeight = (int)(cropImg.Height * scale);
            return new Bitmap(cropImg, new Size(destWidth, destHeight));
        }

        #region ========== Folder and File Paths ==========

        /// <summary>
        /// Folder location where the executables are located
        /// </summary>
        public static string ExecutablePath { get; set; }

        /// <summary>
        /// The file path for the epg123.exe executable
        /// </summary>
        public static string Epg123ExePath => (ExecutablePath + "\\epg123.exe");

        /// <summary>
        /// The file path for the hdhr2mxf.exe executable
        /// </summary>
        public static string Hdhr2MxfExePath => (ExecutablePath + "\\hdhr2mxf.exe");

        /// <summary>
        /// The file path for the epg123Client.exe executable
        /// </summary>
        public static string Epg123ClientExePath => (ExecutablePath + "\\epg123Client.exe");

        /// <summary>
        /// The file path for the epg123Transfer.exe executable
        /// </summary>
        public static string Epg123TransferExePath => (ExecutablePath + "\\epg123Transfer.exe");

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
                else
                {
                    return ExecutablePath;
                }
            }
        }

        /// <summary>
        /// The file path for the epg123.cfg configuration file
        /// </summary>
        public static string Epg123CfgPath => Epg123ProgramDataFolder + "\\epg123.cfg";

        /// <summary>
        /// The file path for the custom lineups file
        /// </summary>
        public static string Epg123CustomLineupsXmlPath => Epg123ProgramDataFolder + "\\customLineup.xml";

        /// <summary>
        /// The folder used to store program and WMC backups
        /// </summary>
        public static string Epg123BackupFolder => Epg123ProgramDataFolder + "\\backup";

        /// <summary>
        /// The folder used to store all cached files
        /// </summary>
        public static string Epg123CacheFolder => Epg123ProgramDataFolder + "\\cache";

        /// <summary>
        /// The file path for epg123cache.json file
        /// </summary>
        public static string Epg123CacheJsonPath => Epg123CacheFolder + "\\epg123cache.json";

        public static string Epg123CompressCachePath => Epg123CacheFolder + "\\epg123cache.zip";

        /// <summary>
        /// The folder used to store all the station logos
        /// </summary>
        public static string Epg123LogosFolder => Epg123ProgramDataFolder + "\\logos";

        /// <summary>
        /// The folder used to deposit generated guide files
        /// </summary>
        public static string Epg123OutputFolder => Epg123ProgramDataFolder + "\\output";

        /// <summary>
        /// The file path for the epg123.mxf file
        /// </summary>
        public static string Epg123MxfPath => Epg123OutputFolder + "\\epg123.mxf";

        /// <summary>
        /// The file path for the epg123.xml file
        /// </summary>
        public static string Epg123XmltvPath => Epg123OutputFolder + "\\epg123.xmltv";

        /// <summary>
        /// The file path for the epg123_Guide.pdf help file
        /// </summary>
        public static string Epg123HelpFilePath => ExecutablePath + "\\epg123_Guide.pdf";

        /// <summary>
        /// The file path for the EPG123Status.png status logo
        /// </summary>
        public static string Epg123StatusLogoPath => Epg123ProgramDataFolder + "\\EPG123Status.png";

        /// <summary>
        /// The file path for the guideImages.xml file
        /// </summary>
        public static string Epg123GuideImagesXmlPath => Epg123ProgramDataFolder + "\\guideImages.xml";

        /// <summary>
        /// The file path for the active trace.log file
        /// </summary>
        public static string Epg123TraceLogPath => Epg123ProgramDataFolder + "\\trace.log";

        /// <summary>
        /// The file path for the mmuiplus.json UI+ support file
        /// </summary>
        public static string Epg123MmuiplusJsonPath => Epg123OutputFolder + "\\mmuiplus.json";

        /// <summary>
        /// The file path for the epg123Task.xml file
        /// </summary>
        public static string Epg123TaskXmlPath => Epg123ProgramDataFolder + "\\epg123Task.xml";

        /// <summary>
        /// The file to define what action to perform when starting elevated
        /// </summary>
        public static string EButtonPath => Epg123ProgramDataFolder + "\\ebutton.txt";

        /// <summary>
        /// The file path to the WMC ehshell.exe file
        /// </summary>
        public static string EhshellExeFilePath =>
            Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\ehshell.exe");

        #endregion
    }
}