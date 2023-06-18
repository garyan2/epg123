using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace epg123Client
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern uint SetThreadExecutionState(uint esFlags);

        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETITEMSTATE = LVM_FIRST + 43;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
        };

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

        /// <summary>
        /// Select all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be selected</param>
        public static void SelectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 2);
        }

        /// <summary>
        /// Deselect all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be deselected</param>
        public static void DeselectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 0);
        }

        /// <summary>
        /// Set the item state on the given item
        /// </summary>
        /// <param name="list">The listview whose item's state is to be changed</param>
        /// <param name="itemIndex">The index of the item to be changed</param>
        /// <param name="mask">Which bits of the value are to be set?</param>
        /// <param name="value">The value to be set</param>
        public static void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            var lvItem = new LVITEM {stateMask = mask, state = value};
            SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
        }
    }

    internal static class Program
    {
        #region ========== Binding Redirects ==========
        static Program()
        {
            // initialize logger
            Logger.Initialize(Helper.Epg123TraceLogPath);

            // ensure WMC is installed
            if (!File.Exists(Helper.EhshellExeFilePath))
            {
                MessageBox.Show("WMC is not present on this machine. Closing EPG123 Client Guide Tool.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Logger.WriteError("*** WMC is not present on this machine. ***");
                //Logger.Close();
                Application.Exit();
            }

            string[] assemblies = { "mcepg", "mcstore", "BDATunePIA" };
            string version = null;
            foreach (var assembly in assemblies)
            {
                if ((version = FindDllVersion(assembly)) != null) break;
                Logger.WriteMessage($"*** {assembly}.dll was not found in the Global Assembly Cache (GAC). WMC has not completed its install, or is not installed correctly. ***");
            }

            if (string.IsNullOrEmpty(version))
            {
                // verify WMC is installed
                MessageBox.Show("Could not verify Windows Media Center is installed on this machine. EPG123 Client cannot be started without WMC being present.", "Missing Windows Media Center", MessageBoxButtons.OK);
                //Logger.Close();
                Application.Exit();
            }

            foreach (var assembly in assemblies)
            {
                RedirectAssembly(assembly, version);
            }
        }

        private static string FindDllVersion(string shortName)
        {
            string[] targetVersions = {"6.1.0.0", "6.2.0.0", "6.3.0.0"};
            return targetVersions.FirstOrDefault(targetVersion => IsAssemblyInGac($"{shortName}, Version={targetVersion}, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
        }

        public static bool IsAssemblyInGac(string assemblyString)
        {
            try
            {
                return Assembly.ReflectionOnlyLoad(assemblyString).GlobalAssemblyCache;
            }
            catch
            {
                return false;
            }
        }

        private static void RedirectAssembly(string shortName, string targetVersionStr)
        {
            Assembly Handler(object sender, ResolveEventArgs args)
            {
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName) return null;

                requestedAssembly.Version = new Version(targetVersionStr);
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=31bf3856ad364e35").GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= Handler;
                return Assembly.Load(requestedAssembly);
            }
            AppDomain.CurrentDomain.AssemblyResolve += Handler;
        }
        #endregion

        public enum ExecutionFlags : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,

            //ES_DISPLAY_REQUIRED = 0x00000002,
            //ES_USER_PRESENT = 0x00000004,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000
        }

        private const string AppGuid = "{CD7E6857-7D92-4A2F-B3AB-ED8CB42C6F65}";
        private static string filename = string.Empty;
        private static bool showProgress;
        private const int MaximumRecordingWaitHours = 23;

        [STAThread]
        private static int Main(string[] args)
        {
            // setup catch to fatal program crash
            AppDomain.CurrentDomain.UnhandledException += MyUnhandledException;
            Application.ThreadException += MyThreadException;

            // filter out mcupdate calls that may be redirected
            var arguments = string.Join(" ", args);
            switch (arguments)
            {
                case "-u -nogc": // opening WMC
                case "-uf -nogc":
                    Logger.WriteVerbose($"**** Intercepted \"mcupdate.exe {arguments}\" call. Ignored. ****");
                    //Logger.Close();
                    return 0;
                case "-u -manual -nogc -p 0": // guide update
                case "-manual -nogc -p 0":
                    var startTime = DateTime.Now;
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "schtasks.exe",
                        Arguments = "/run /tn \"epg123_update\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    // begin update
                    var proc = Process.Start(startInfo);
                    proc?.WaitForExit();

                    Logger.WriteInformation("**** Attempted to kick off the epg123_update task on demand. ****");
                    //Logger.Close();

                    // monitor the task status until it is complete
                    var ts = new EpgTaskScheduler();
                    while (true)
                    {
                        // looks like WMC may have a 300000 ms timeout for the update action
                        // no reason to continue with the mcupdate run if it is going to be ignored
                        if (DateTime.Now - startTime > TimeSpan.FromMinutes(5.0)) return 0;

                        ts.QueryTask(true);
                        if (ts.StatusString.ToLower().Contains("running"))
                        {
                            Thread.Sleep(100);
                        }
                        else break;
                    }

                    // kick off mcupdate so it can fail but signal WMC that update is complete
                    startInfo = new ProcessStartInfo()
                    {
                        FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\mcupdate.exe"),
                        Arguments = arguments
                    };
                    proc = Process.Start(startInfo);
                    proc?.WaitForExit();
                    return 0;
            }

            // evaluate arguments
            bool nologo, import, force, showGui, advanced, nogc, verbose, noverify;
            var match = nologo = import = force = showGui = advanced = nogc = verbose = noverify = false;

            if (File.Exists($"{Helper.Epg123ProgramDataFolder}nogc.txt"))
            {
                File.Delete($"{Helper.Epg123ProgramDataFolder}nogc.txt");
                nogc = true;
            }
            if (args.Length > 0)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    switch (args[i].ToLower())
                    {
                        case "-match":
                            match = true;
                            break;
                        case "-nologo":
                            nologo = true;
                            break;
                        case "-i":
                            if (i + 1 < args.Length)
                            {
                                filename = args[++i].Replace("EPG:", "http:");
                                if (!filename.StartsWith("http") && !File.Exists(filename))
                                {
                                    Logger.WriteError($"File \"{filename}\" does not exist.");
                                    return -1;
                                }
                            }
                            else
                            {
                                Logger.WriteError("Missing input filename and path.");
                                return -1;
                            }
                            import = true;
                            break;
                        case "-f":
                            force = true;
                            break;
                        case "-p":
                            showProgress = true;
                            break;
                        case "-x":
                            advanced = true;
                            showGui = true;
                            break;
                        case "-nogc":
                            nogc = true;
                            break;
                        case "-verbose":
                            verbose = true;
                            break;
                        case "-noverify":
                            noverify = true;
                            break;
                        default:
                            Logger.WriteVerbose($"**** Invalid arguments for epg123Client.exe; \"{arguments}\" ****");
                            //Logger.Close();
                            return -1;
                    }
                }
            }
            else
            {
                showGui = true;
            }

            // create a mutex and keep alive until program exits
            using (var mutex = !showGui && showProgress ? new Mutex(false, $"Global\\{AppGuid}") : Helper.GetProgramMutex($"Global\\{AppGuid}", !showGui))
            {
                // check for an instance already running
                if (mutex == null) return -1;

                Logger.WriteMessage("===============================================================================");
                Logger.WriteMessage($" {(showGui ? "Activating the epg123 client GUI." : "Beginning epg123 client execution.")} version {Helper.Epg123Version}");
                Logger.WriteMessage("===============================================================================");
                Logger.LogOsDescription();
                Logger.LogDotNetDescription();
                Logger.LogWmcDescription();

                // show gui if needed
                if (showGui)
                {
                    var client = new clientForm(advanced);
                    client.ShowDialog();

                    if (client.RestartClientForm)
                    {
                        // start a new process
                        _ = Process.Start(new ProcessStartInfo
                        {
                            FileName = Helper.Epg123ClientExePath,
                            WorkingDirectory = Helper.ExecutablePath,
                            UseShellExecute = true,
                            Verb = client.RestartAsAdmin ? "runas" : null
                        });
                    }
                    client.Dispose();
                }
                else
                {
                    // prevent machine from entering sleep mode
                    var prevThreadState = NativeMethods.SetThreadExecutionState(
                        (uint)ExecutionFlags.ES_CONTINUOUS |
                        (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                        (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                    Logger.WriteVerbose($"Import: {import} , Match: {match} , NoLogo: {nologo} , Force: {force} , ShowProgress: {showProgress} , NoGC: {force || nogc} , NoVerify: {noverify} , Verbose: {verbose}");
                    var startTime = DateTime.UtcNow;

                    if (import)
                    {
                        // check if garbage cleanup is needed
                        if (!nogc && !force && WmcStore.IsGarbageCleanupDue() && !ProgramRecording(60))
                        {
                            _ = WmcStore.PerformGarbageCleanup();
                        }

                        // ensure no recordings are active if importing
                        if (!force && ProgramRecording(10))
                        {
                            Logger.WriteError($"A program recording is still in progress after {MaximumRecordingWaitHours} hours. Aborting the mxf file import.");
                            goto CompleteImport;
                        }
                        WmcStore.Close();

                        // import mxf file
                        if (!ImportMxfFile(filename))
                        {
                            Logger.WriteError("Failed to import .mxf file. Exiting.");
                            goto CompleteImport;
                        }

                        // perform verification
                        if (!noverify)
                        {
                            _ = new VerifyLoad(filename, verbose);
                        }

                        // get lineup and configure lineup type and devices 
                        if (!WmcStore.ActivateEpg123LineupsInStore() || !WmcStore.ActivateGuide())
                        {
                            Logger.WriteError("Failed to locate any lineups from EPG123.");
                            goto CompleteImport;
                        }
                    }

                    // remove all channel logos
                    if (nologo)
                    {
                        WmcStore.ClearLineupChannelLogos();
                    }

                    // perform post mxf import cleanup and automatch
                    if (match || import)
                    {
                        try
                        {
                            WmcStore.AutoMapChannels(match);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteError("Failed to complete the automatic mapping of lineup stations to tuner channels.");
                            Logger.WriteError($"{ex}");
                        }
                    }

                    // import success
                    if (import)
                    {
                        // refresh the lineups after import
                        using (var mergedLineups = new MergedLineups(WmcStore.WmcObjectStore))
                        {
                            foreach (MergedLineup mergedLineup in mergedLineups)
                            {
                                _ = mergedLineup.Refresh();
                            }
                        }
                        Logger.WriteInformation("Completed lineup refresh.");

                        // reindex database
                        _ = WmcStore.ReindexPvrSchedule();
                        _ = WmcStore.ReindexDatabase();
                    }

                    // import complete
                    CompleteImport:
                    if (import)
                    {
                        // update status logo
                        statusLogo.StatusImage(filename);

                        // signal the notification tray to update the icon
                        Helper.SendPipeMessage("Import Complete");
                    }

                    WmcStore.Close();

                    // all done
                    Logger.WriteInformation("Completed EPG123 client execution.");
                    Logger.WriteVerbose($"EPG123 client execution time was {DateTime.UtcNow - startTime}.");
                    //Logger.Close();
                    NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                }
            }
            Environment.Exit(0);
            return 0;
        }

        #region ========== Import MXF File ==========
        private static bool ProgramRecording(int bufferMinutes)
        {
            var expireTime = DateTime.Now + TimeSpan.FromHours(MaximumRecordingWaitHours);
            const int intervalMinutes = 1;
            var intervalCheck = 0;

            do
            {
                var active = WmcStore.DetermineRecordingsInProgress() || WmcStore.NextScheduledRecording() - TimeSpan.FromMinutes(bufferMinutes) < DateTime.Now;
                if (!active && intervalCheck > 0)
                {
                    Thread.Sleep(30000);
                    active = WmcStore.DetermineRecordingsInProgress() || WmcStore.NextScheduledRecording() - TimeSpan.FromMinutes(bufferMinutes) < DateTime.Now;
                }

                if (!active || expireTime < DateTime.Now)
                {
                    WmcStore.Close();
                    return active;
                }

                Helper.SendPipeMessage($"Importing|Waiting for recordings to end...|Will check again at {DateTime.Now + TimeSpan.FromMinutes(intervalMinutes):HH:mm:ss}");
                if (intervalCheck++ % (60 / intervalMinutes) == 0)
                {
                    Logger.WriteInformation($"There is a recording in progress or the next scheduled recording is within {bufferMinutes} minutes. Delaying garbage collection and/or import.");
                }
                Thread.Sleep(intervalMinutes * 60000);
            } while (true);
        }

        public static bool ImportMxfFile(string file)
        {
            //verify tuners are setup in WMC prior to importing
            var deviceCount = 0;
            using (var devices = new Devices(WmcStore.WmcObjectStore))
            {
                foreach (Device device in devices)
                {
                    if (device.Name.ToLower().Contains("delete")) continue;
                    ++deviceCount;
                    break;
                }
            }

            if (deviceCount == 0)
            {
                Logger.WriteError("There are no devices/tuners configured in the database store. Perform WMC TV Setup prior to importing guide listings. Aborting Import.");
                return false;
            }

            // do the import with or without progress form
            if (!showProgress) return WmcStore.ImportMxfFile(file);
            var frm = new frmImport(file, false);
            frm.ShowDialog();
            return frm.Success;
        }
        #endregion

        private static void MyUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!((Exception) e.ExceptionObject).Message.Equals("access denied"))
            {
                Logger.WriteError($"Unhandled exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {((Exception) e.ExceptionObject).Message}\n{((Exception) e.ExceptionObject).StackTrace}");
            }
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }

        private static void MyThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteError($"Unhandled thread exception caught from {AppDomain.CurrentDomain.FriendlyName}. message: {e.Exception.Message}\n{e.Exception.StackTrace}");
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }
    }
}