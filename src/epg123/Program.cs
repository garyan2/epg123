using System;
using System.Diagnostics;
using System.IO;
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
        internal static extern IntPtr SendMessage(HandleRef hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern uint SetThreadExecutionState(uint esFlags);
    }

    internal class Program
    {
        public enum ExecutionFlags : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            //ES_DISPLAY_REQUIRED = 0x00000002,
            //ES_USER_PRESENT = 0x00000004,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000
        }

        private const string AppGuid = "{BAEC0A11-437B-4D39-A2FA-DB56F8C977E3}";

        [STAThread]
        private static int Main(string[] args)
        {
            // setup catch to fatal program crash
            AppDomain.CurrentDomain.UnhandledException += MyUnhandledException;
            Application.ThreadException += MyThreadException;

            // establish file/folder locations
            Logger.Initialize("Media Center", "EPG123");
            Helper.EstablishFileFolderPaths();

            bool import, match, showProgress;
            import = match = showProgress = false;
            var showGui = true;
            epgConfig config = null;

            // only evaluate arguments if a configuration file exists, otherwise open the gui
            if (File.Exists(Helper.Epg123CfgPath) && args != null)
            {
                foreach (var t in args)
                {
                    switch (t.ToLower())
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

            // create a mutex and keep alive until program exits
            using (var mutex = Helper.GetProgramMutex($"Global\\{AppGuid}", !showGui))
            {
                // check for an instance already running
                if (mutex == null) return -1;

                Logger.WriteMessage("===============================================================================");
                Logger.WriteMessage($" {(showGui ? "Activating the epg123 configuration GUI." : "Beginning epg123 update execution.")} version {Helper.Epg123Version}");
                Logger.WriteMessage("===============================================================================");
                Logger.WriteMessage($"*** {Helper.GetOsDescription()} ***");
                Logger.WriteMessage($"*** {Helper.GetWmcDescription()} ***");

                // open the configuration GUI if needed
                if (showGui)
                {
                    var cfgForm = new frmMain();
                    cfgForm.ShowDialog();
                    Logger.Close();
                    if (!cfgForm.Execute)
                    {
                        mutex.ReleaseMutex(); GC.Collect();
                        if (!cfgForm.RestartAsAdmin) return 0;
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = Helper.Epg123ExePath,
                            WorkingDirectory = Helper.ExecutablePath,
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        Process.Start(startInfo);
                        return 0;
                    }
                    Logger.Initialize("Media Center", "EPG123");
                    config = cfgForm.Config;
                    import = config.AutoImport;
                    match = config.Automatch;
                }

                // prevent machine from entering sleep mode
                var prevThreadState = NativeMethods.SetThreadExecutionState((uint)ExecutionFlags.ES_CONTINUOUS |
                                                                            (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                                                                            (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                // bring in the configuration
                if (config == null)
                {
                    try
                    {
                        using (var stream = new StreamReader(Helper.Epg123CfgPath, Encoding.Default))
                        {
                            var serializer = new XmlSerializer(typeof(epgConfig));
                            TextReader reader = new StringReader(stream.ReadToEnd());
                            config = (epgConfig)serializer.Deserialize(reader);
                            reader.Close();
                        }
                    }
                    catch (IOException ex)
                    {
                        Logger.WriteError($"Failed to open configuration file during initialization due to IO exception. message: {ex.Message}");
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex.ReleaseMutex(); GC.Collect();
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"Failed to open configuration file during initialization with unknown exception. message: {ex.Message}");
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex.ReleaseMutex(); GC.Collect();
                        return -1;
                    }
                }

                // let's do this
                Helper.SendPipeMessage("Downloading|Initializing...");
                var startTime = DateTime.UtcNow;
                if (showGui || showProgress)
                {
                    var build = new frmProgress(config);
                    build.ShowDialog();
                }
                else
                {
                    sdJson2mxf.sdJson2Mxf.Build(config);
                    if (!sdJson2mxf.sdJson2Mxf.Success)
                    {
                        Logger.WriteError("Failed to create MXF file. Exiting.");
                    }
                }
                Helper.SendPipeMessage("Download Complete");

                // close the logger and restore power/sleep settings
                Logger.WriteVerbose($"epg123 update execution time was {DateTime.UtcNow - startTime}.");
                Logger.Close();
                NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                // did a Save&Execute from GUI ... perform import and automatch as well if requested
                if (!sdJson2mxf.sdJson2Mxf.Success) return -1;
                if (import)
                {
                    // verify output file exists
                    if (!File.Exists(Helper.Epg123MxfPath) || !File.Exists(Helper.Epg123ClientExePath))
                    {
                        mutex.ReleaseMutex(); GC.Collect();
                        return -1;
                    }

                    // epg123client
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "epg123Client.exe",
                        Arguments = "-i \"" + Helper.Epg123MxfPath + "\" -nogc -noverify -p" + (match ? " -match" : string.Empty),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var proc = Process.Start(startInfo);
                    proc?.WaitForExit();
                }
                return 0;
            }
        }

        private static void MyUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteError($"Unhandled exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {(e.ExceptionObject as Exception)?.Message}");
        }

        private static void MyThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteError($"Unhandled thread exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {e.Exception.Message}");
        }
    }
}