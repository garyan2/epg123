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

            if (!File.Exists(Helper.Epg123CfgPath))
            {
                Console.WriteLine("Configuration file does not exist. You must create a cfg file prior to attempting an update.");
                Console.WriteLine($"Run \"{Helper.Epg123GuiPath}\" to create the cfg file.");
                return -1;
            }

            bool import, match, showProgress, error, showHelp;
            import = match = showProgress = error = showHelp = false;
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
                           "-P          Shows progress while downloading/importing guide\n" +
                           "            listings (interactive mode only).\n";
                Console.WriteLine(help);
                return error ? -1 : 0;
            }

            // create a mutex and keep alive until program exits
            using (var mutex = Helper.GetProgramMutex($"Global\\{AppGuid}", false))
            {
                // check for an instance already running
                if (mutex == null) goto Exit;

                // prevent machine from entering sleep mode
                var prevThreadState = NativeMethods.SetThreadExecutionState((uint)ExecutionFlags.ES_CONTINUOUS |
                                                                            (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                                                                            (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                // let's do this
                Logger.Initialize(Helper.Epg123TraceLogPath, "Beginning MXF and XMLTV file updates", true);
                Helper.SendPipeMessage("Downloading|Initializing...");
                if (showProgress)
                {
                    var build = new frmProgress();
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
                Logger.CloseAndSendNotification();

                // restore power/sleep settings
                NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                // did a Save&Execute from GUI ... perform import and automatch as well if requested
                if (!sdJson2mxf.sdJson2Mxf.Success) goto Exit;
                if (import)
                {
                    // epg123client
                    var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = Helper.Epg123ClientExePath,
                        Arguments = $"-i \"{Helper.Epg123MxfPath}\"{(showProgress ? " -p -nogc -noverify" : "")}{(match ? " -match" : "")}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    proc.WaitForExit();
                    Logger.Status = proc.ExitCode;
                }
            }

        Exit:
            return Logger.Status;
        }

        private static void MyUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteError($"Unhandled exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {e.ExceptionObject as Exception}");
            Logger.CloseAndSendNotification();
        }

        private static void MyThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteError($"Unhandled thread exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {e.Exception}");
            Logger.CloseAndSendNotification();
        }
    }
}