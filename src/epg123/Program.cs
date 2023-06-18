using GaRyan2.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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

            // make sure configuration file exists
            if (!File.Exists(Helper.Epg123CfgPath))
            {
                Console.WriteLine("There is no configuration file to use for updating guide listings.\nOpen epg123_gui.exe to create a configuration file.");
                return -1;
            }

            bool import = false, match = false, showProgress = false;
            bool error = false, showHelp = false;
            epgConfig config = null;

            if (args != null)
            {
                foreach (var t in args)
                {
                    switch (t.ToLower())
                    {
                        case "-import":
                            if (File.Exists(Helper.EhshellExeFilePath)) import = true;
                            else Console.WriteLine("WMC is not installed on this machine; -import is invalid.");
                            break;
                        case "-match":
                            match = true;
                            break;
                        case "-p":
                            showProgress = true;
                            break;
                        case "-update": // deprecated
                            break;
                        case "-h":
                        case "/h":
                        case "/?":
                            showHelp = true;
                            break;
                        default:
                            Console.WriteLine($"Invalid switch - \"{t}\"\n");
                            error = true;
                            break;
                    }
                }
            }
            if (error || showHelp)
            {
                var help = "EPG123 [-IMPORT [-MATCH]] [-P]\n\n" +
                           "-IMPORT     Automatically imports guide listings into WMC.\n" +
                           "-MATCH      Automatically match guide listing in WMC.\n" +
                           "-P          Shows progress GUI while downloading/importing guide\n" +
                           "            listings.\n";
                Console.WriteLine(help);
                return error ? -1 : 0;
            }

            // initialize logger
            Logger.Initialize(Helper.Epg123TraceLogPath);

            // create a mutex and keep alive until program exits
            using (var mutex = Helper.GetProgramMutex($"Global\\{AppGuid}", true))
            {
                // check for an instance already running
                if (mutex == null) return -1;

                // prevent machine from entering sleep mode
                var prevThreadState = NativeMethods.SetThreadExecutionState((uint)ExecutionFlags.ES_CONTINUOUS |
                                                                            (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                                                                            (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                // bring in the configuration
                config = Helper.ReadXmlFile(Helper.Epg123CfgPath, typeof(epgConfig));
                if (config == null)
                {
                    NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                    mutex.ReleaseMutex();
                    return -1;
                }

                // let's do this
                Helper.SendPipeMessage("Downloading|Initializing...");
                var startTime = DateTime.UtcNow;
                if (showProgress)
                {
                    var build = new frmProgress(config);
                    build.ShowDialog();
                }
                else
                {
                    sdJson2mxf.sdJson2Mxf.Build();
                    if (!sdJson2mxf.sdJson2Mxf.Success)
                    {
                        Logger.WriteError("Failed to create MXF file. Exiting.");
                    }
                }
                Helper.SendPipeMessage("Download Complete");

                // restore power/sleep settings
                NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                // did a Save&Execute from GUI ... perform import and automatch as well if requested
                if (!sdJson2mxf.sdJson2Mxf.Success) return -1;
                if (import)
                {
                    // verify output file exists
                    if (!File.Exists(Helper.Epg123MxfPath) || !File.Exists(Helper.Epg123ClientExePath))
                    {
                        mutex.ReleaseMutex();
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
            Logger.WriteError($"Unhandled exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {e.ExceptionObject as Exception}");
        }

        private static void MyThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteError($"Unhandled thread exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {e.Exception}");
        }
    }
}