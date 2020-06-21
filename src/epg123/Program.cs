using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace epg123
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern uint SetThreadExecutionState(uint esFlags);
    }

    class Program
    {
        public enum ExecutionFlags : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_USER_PRESENT = 0x00000004,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000
        }

        private static string appGuid = "{BAEC0A11-437B-4D39-A2FA-DB56F8C977E3}";

        static void EstablishFileFolderPaths()
        {
            // set the base path and the working directory
            Helper.ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(Helper.ExecutablePath);

            // establish folders with permissions
            string[] folders = { Helper.Epg123BackupFolder, Helper.Epg123CacheFolder, Helper.Epg123LogosFolder, Helper.Epg123OutputFolder, Helper.Epg123SdLogosFolder };
            if (Environment.UserInteractive && !Helper.CreateAndSetFolderAcl(Helper.Epg123ProgramDataFolder))
            {
                Logger.WriteError(string.Format("Failed to set full control permissions for Everyone on folder \"{0}\".", Helper.Epg123ProgramDataFolder));
            }
            else
            {
                foreach (string folder in folders)
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    if (Environment.UserInteractive && !Helper.CreateAndSetFolderAcl(folder))
                    {
                        Logger.WriteError(string.Format("Failed to set full control permissions for Everyone on folder \"{0}\".", folder));
                    }
                }
            }

            // copy custom lineup file to proper location in needed
            string oldCustomFile = Helper.ExecutablePath + "\\customLineup.xml.example";
            if (!File.Exists(Helper.Epg123CustomLineupsXmlPath) && File.Exists(oldCustomFile))
            {
                File.Copy(oldCustomFile, Helper.Epg123CustomLineupsXmlPath);
            }
        }

        [STAThread]
        static int Main(string[] args)
        {
            // setup catch to fatal program crash
            AppDomain.CurrentDomain.UnhandledException += MyUnhandledException;
            Application.ThreadException += MyThreadException;

            // establish file/folder locations
            Logger.Initialize("Media Center", "EPG123");
            EstablishFileFolderPaths();

            // create a mutex and keep alive until program exits
            using (Mutex mutex = new Mutex(true, "Global\\" + appGuid))
            {
                bool showGui = true;
                bool import = false;
                bool match = false;
                bool showProgress = false;
                epgConfig config = null;

                // only evaluate arguments if a configuration file exists, otherwise open the gui
                if (File.Exists(Helper.Epg123CfgPath) && (args != null))
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        switch (args[i].ToLower())
                        {
                            case "-update":
                                showGui = false;
                                break;
                            case "-p":
                                showProgress = true;
                                break;
                            default:
                                return -1;
                        }
                    }
                }

                // check for an instance already running
                if (!mutex.WaitOne(3000, false))
                {
                    if (!showGui)
                    {
                        Logger.WriteMessage("===============================================================================");
                        Logger.WriteError("An instance of EPG123 is already running. Aborting update.");
                        Logger.WriteMessage("===============================================================================");
                        Logger.Close();
                        return -1;
                    }
                    else
                    {
                        MessageBox.Show("An instance of EPG123 is already running.", "Initialization Aborted");
                        Logger.Close();
                        return 0;
                    }
                }

                // open the configuration GUI if needed
                if (showGui)
                {
                    Logger.WriteMessage("===============================================================================");
                    Logger.WriteMessage(string.Format(" Activating the epg123 configuration GUI. version {0}", Helper.epg123Version));
                    Logger.WriteMessage("===============================================================================");
                    frmMain cfgForm = new frmMain();
                    cfgForm.ShowDialog();
                    Logger.Close();
                    if (!cfgForm.Execute)
                    {
                        mutex.ReleaseMutex();
                        return 0;
                    }
                    Logger.Initialize("Media Center", "EPG123");
                    config = cfgForm.config;
                    import = cfgForm.import;
                    match = cfgForm.match;
                }

                // prevent machine from entering sleep mode
                uint prevThreadState = NativeMethods.SetThreadExecutionState((uint)ExecutionFlags.ES_CONTINUOUS |
                                                                             (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                                                                             (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                Logger.WriteMessage("===============================================================================");
                Logger.WriteMessage(string.Format(" Beginning epg123 update execution. version {0}", Helper.epg123Version));
                Logger.WriteMessage("===============================================================================");

                // bring in the configuration
                if (config == null)
                {
                    try
                    {
                        using (StreamReader stream = new StreamReader(Helper.Epg123CfgPath, Encoding.Default))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(epgConfig));
                            TextReader reader = new StringReader(stream.ReadToEnd());
                            config = (epgConfig)serializer.Deserialize(reader);
                            reader.Close();
                        }
                    }
                    catch (IOException ex)
                    {
                        Logger.WriteError(string.Format("Failed to open configuration file during initialization due to IO exception. message: {0}", ex.Message));
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex.ReleaseMutex();
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError(string.Format("Failed to open configuration file during initialization with unknown exception. message: {0}", ex.Message));
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex.ReleaseMutex();
                        return -1;
                    }
                }

                // let's do this
                DateTime startTime = DateTime.UtcNow - TimeSpan.FromMinutes(1.0);
                NotifyIcon notifyIcon = new NotifyIcon()
                {
                    Text = "EPG123\nDownloading and building guide listings...",
                    Icon = Properties.Resources.EPG123_download
                };
                notifyIcon.Visible = true;

                if (showGui || showProgress)
                {
                    frmProgress build = new frmProgress(config);
                    build.ShowDialog();
                }
                else
                {
                    sdJson2mxf.Build(config);
                }
                notifyIcon.Visible = false;
                notifyIcon.Dispose();

                // close the logger and restore power/sleep settings
                Logger.WriteVerbose(string.Format("epg123 update execution time was {0}.", DateTime.UtcNow - startTime - TimeSpan.FromMinutes(1.0)));
                Logger.Close();
                NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                // did a Save&Execute from GUI ... perform import and automatch as well if requested
                if (sdJson2mxf.success && import)
                {
                    // verify output file exists
                    if (!File.Exists(Helper.Epg123MxfPath) || !File.Exists(Helper.Epg123ClientExePath))
                    {
                        mutex.ReleaseMutex();
                        return -1;
                    }

                    // epg123client
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "epg123Client.exe",
                        Arguments = "-i \"" + Helper.Epg123MxfPath + "\"" + (match ? " -match" : string.Empty) + (showGui ? " -p" : string.Empty) + " -nogc",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process proc = Process.Start(startInfo);
                    proc.WaitForExit();
                }

                mutex.ReleaseMutex();
                return 0;
            }
        }

        static void MyUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteError(string.Format("Unhandled exception caught from {0}. message: {1}", AppDomain.CurrentDomain.FriendlyName, (e.ExceptionObject as Exception).Message));
        }
        static void MyThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteError(string.Format("Unhandled thread exception caught from {0}. message: {1}", AppDomain.CurrentDomain.FriendlyName, e.Exception.Message));
        }
    }
}