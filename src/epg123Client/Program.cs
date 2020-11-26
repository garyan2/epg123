using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Pvr;
using epg123Client;
using Microsoft.MediaCenter.Store;

namespace epg123
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
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
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
            NativeMethods.SetItemState(list, -1, 2, 2);
        }

        /// <summary>
        /// Deselect all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be deselected</param>
        public static void DeselectAllItems(ListView list)
        {
            NativeMethods.SetItemState(list, -1, 2, 0);
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
            LVITEM lvItem = new LVITEM();
            lvItem.stateMask = mask;
            lvItem.state = value;
            SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
        }
    }

    static class Program
    {
        #region ========== Binding Redirects ==========
        static Program()
        {
            string[] assemblies = { "mcepg", "mcstore", "BDATunePIA" };

            string version = FindDLLVersion(assemblies[0]);
            foreach (string assembly in assemblies)
            {
                try
                {
                    RedirectAssembly(assembly, version);
                }
                catch { }
            }
        }
        private static string FindDLLVersion(string shortName)
        {
            string[] targetVersions = { "6.1.0.0", "6.2.0.0", "6.3.0.0" };
            foreach (string targetVersion in targetVersions)
            {
                if (IsAssemblyInGAC(string.Format("{0}, Version={1}, Culture=neutral, PublicKeyToken=31bf3856ad364e35", shortName, targetVersion)))
                {
                    return targetVersion;
                }
            }
            return null;
        }
        public static bool IsAssemblyInGAC(string assemblyString)
        {
            bool result = false;
            try
            {
                result = Assembly.ReflectionOnlyLoad(assemblyString).GlobalAssemblyCache;
            }
            catch { }
            return result;
        }
        private static void RedirectAssembly(string shortName, string targetVersionStr)
        {
            ResolveEventHandler handler = null;
            handler = (sender, args) =>
            {
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName) return null;

                requestedAssembly.Version = new Version(targetVersionStr);
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=31bf3856ad364e35").GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;
                return Assembly.Load(requestedAssembly);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }
        #endregion

        public enum ExecutionFlags : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_USER_PRESENT = 0x00000004,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000
        }

        private static string appGuid = "{CD7E6857-7D92-4A2F-B3AB-ED8CB42C6F65}";
        private static string guiGuid = "{0BA29D22-8BB1-4C33-919A-330D5DBA1FF0}";
        private static string impGuid = "{B7CEFF32-CD68-4094-BD1B-A541D246372E}";
        private static string filename = string.Empty;
        private static bool showProgress = false;
        private static int maximumRecordingWaitHours = 23;

        static void EstablishFileFolderPaths()
        {
            // set the base path and the working directory
            Helper.ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(Helper.ExecutablePath);

            // establish folders with permissions
            if (!Helper.CreateAndSetFolderAcl(Helper.Epg123ProgramDataFolder))
            {
                Logger.WriteInformation(string.Format("Failed to set full control permissions for Everyone on folder \"{0}\".", Helper.Epg123ProgramDataFolder));
            }
            string[] folders = { Helper.Epg123BackupFolder };
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            }
        }

        [STAThread]
        static int Main(string[] args)
        {
            // setup catch to fatal program crash
            AppDomain.CurrentDomain.UnhandledException += MyUnhandledException;
            Application.ThreadException += MyThreadException;

            // verify WMC is installed
            if (!File.Exists(Helper.EhshellExeFilePath))
            {
                MessageBox.Show("Could not verify Windows Media Center is installed on this machine. EPG123 Client cannot be started without WMC being present.", "Missing Windows Media Center", MessageBoxButtons.OK);
                return -1;
            }

            // establish file/folder locations
            Logger.Initialize("Media Center", "epg123Client");
            EstablishFileFolderPaths();

            // filter out mcupdate calls that may be redirected
            string arguments = string.Join(" ", args);
            switch (arguments)
            {
                case "-u -nogc": // opening WMC
                case "-uf -nogc":
                    Logger.WriteVerbose($"**** Intercepted \"mcupdate.exe {arguments}\" call. Ignored. ****");
                    Logger.Close();
                    return 0;
                case "-u -manual -nogc -p 0": // guide update
                case "-manual -nogc -p 0":
                    DateTime startTime = DateTime.Now;
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "schtasks.exe",
                        Arguments = "/run /tn \"epg123_update\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    // begin update
                    Process proc = Process.Start(startInfo);
                    proc.WaitForExit();

                    Logger.WriteInformation($"**** Attempted to kick off the epg123_update task on demand. ****");
                    Logger.Close();

                    // monitor the task status until it is complete
                    epgTaskScheduler ts = new epgTaskScheduler();
                    while (true)
                    {
                        // looks like WMC may have a 300000 ms timeout for the update action
                        // no reason to continue with the mcupdate run if it is going to be ignored
                        if (DateTime.Now - startTime > TimeSpan.FromMinutes(5.0)) return 0;

                        ts.queryTask(true);
                        if (ts.statusString.ToLower().Contains("running"))
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
                    Process proc2 = Process.Start(startInfo);
                    proc.WaitForExit();
                    return 0;
                default:
                    break;
            }

            // create a mutex and keep alive until program exits
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                bool match, nologo, import, force, showGui, advanced, nogc, verbose, noverify;
                match = nologo = import = force = showGui = advanced = nogc = verbose = noverify = false;

                if ((args != null) && (args.Length > 0))
                {
                    for (int i = 0; i < args.Length; ++i)
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
                                if ((i + 1) < args.Length)
                                {
                                    if (!File.Exists(filename = args[++i]))
                                    {
                                        string err = string.Format("File \"{0}\" does not exist.", filename);
                                        Logger.WriteError(err);
                                        return -1;
                                    }
                                    filename = new FileInfo(filename).FullName.ToLower();

                                    string testNewFile = filename.Replace("\\epg123.mxf", "\\output\\epg123.mxf");
                                    if (File.Exists(testNewFile))
                                    {
                                        Logger.WriteWarning(string.Format("It appears the MXF file to import is incorrect. Changing the import file from \"{0}\" to \"{1}\".", filename, testNewFile));
                                        filename = testNewFile;
                                    }
                                    statusLogo.mxfFile = filename;
                                }
                                else
                                {
                                    string err = "Missing input filename and path.";
                                    Logger.WriteError(err);
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
                                Logger.Close();
                                return -1;
                        }
                    }
                }
                else
                {
                    showGui = true;
                }

                // use another mutex if the GUI is open
                using (Mutex mutex2 = new Mutex(false, "Global\\" + guiGuid))
                {
                    // check for a gui instance already running
                    if (!mutex2.WaitOne(2000, false) && (showGui || !showProgress))
                    {
                        if (showGui)
                        {
                            MessageBox.Show("An instance of EPG123 Client GUI is already running.", "Initialization Aborted");
                        }
                        else
                        {
                            Logger.WriteError("An instance of EPG123 Client GUI is already running. Initialization Aborted.");
                            Logger.Close();
                        }
                        return -1;
                    }

                    if (showGui)
                    {
                        Logger.WriteMessage("===============================================================================");
                        Logger.WriteMessage(string.Format(" Activating the epg123 client GUI. version {0}", Helper.epg123Version));
                        Logger.WriteMessage("===============================================================================");
                        clientForm client = new clientForm(advanced);
                        client.ShowDialog();

                        mutex2.ReleaseMutex(); GC.Collect();
                        if (client.restartClientForm)
                        {
                            // start a new process
                            ProcessStartInfo startInfo = new ProcessStartInfo()
                            {
                                FileName = Helper.Epg123ClientExePath,
                                WorkingDirectory = Helper.ExecutablePath,
                                UseShellExecute = true,
                                Verb = client.restartAsAdmin ? "runas" : null
                            };
                            Process proc = Process.Start(startInfo);
                        }
                        client.Dispose();
                    }
                    else
                    {
                        // and yet another mutex for the import action
                        using (Mutex mutex3 = new Mutex(false, "Global\\" + impGuid))
                        {
                            // check for an import instance is already running
                            if (!mutex3.WaitOne(0, false))
                            {
                                Logger.WriteError("An instance of EPG123 Client import is already running. Aborting this instance.");
                                Logger.Close();
                                return -1;
                            }

                            // prevent machine from entering sleep mode
                            uint prevThreadState = NativeMethods.SetThreadExecutionState((uint)ExecutionFlags.ES_CONTINUOUS |
                                                                                         (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                                                                                         (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                            Logger.WriteMessage("===============================================================================");
                            Logger.WriteMessage(string.Format(" Beginning epg123 client execution. version {0}", Helper.epg123Version));
                            Logger.WriteMessage("===============================================================================");
                            Logger.WriteInformation(string.Format("Beginning epg123 client execution. {0:u}", DateTime.Now.ToUniversalTime()));
                            Logger.WriteVerbose(string.Format("Import: {0} , Match: {1} , NoLogo: {2} , Force: {3} , ShowProgress: {4}", import, match, nologo, force, showProgress));
                            DateTime startTime = DateTime.UtcNow;

                            if (import)
                            {
                                // check if garbage cleanup is needed
                                if (!nogc && !force && WmcRegistries.IsGarbageCleanupDue() && !programRecording(60))
                                {
                                    WmcUtilities.PerformGarbageCleanup();
                                }

                                // ensure no recordings are active if importing
                                if (!force && programRecording(10))
                                {
                                    Logger.WriteError(string.Format("A program recording is still in progress after {0} hours. Aborting the mxf file import.", maximumRecordingWaitHours));
                                    Logger.Close();
                                    NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                                    statusLogo.statusImage();
                                    return -1;
                                }

                                // import mxf file
                                if (!importMxfFile(filename))
                                {
                                    Logger.WriteError("Failed to import .mxf file. Exiting.");
                                    Logger.Close();
                                    NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                                    statusLogo.statusImage();
                                    return -1;
                                }
                                else if (!noverify)
                                {
                                    VerifyLoad verifyLoad = new VerifyLoad(filename, verbose);
                                }

                                // get lineup and configure lineup type and devices 
                                if (!WmcStore.ActivateEpg123LineupsInStore() || !WmcRegistries.ActivateGuide())
                                {
                                    Logger.WriteError("Failed to locate any lineups from EPG123.");
                                    Logger.Close();
                                    NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);

                                    statusLogo.statusImage();
                                    return -1;
                                }
                            }

                            // remove all channel logos
                            if (nologo)
                            {
                                clearLineupChannelLogos();
                            }

                            // perform automatch
                            if (match)
                            {
                                try
                                {
                                    WmcStore.AutoMapChannels();
                                    Logger.WriteInformation("Completed the automatch of lineup stations to tuner channels.");
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteError($"{ex.Message}");
                                    Logger.WriteError("Failed to perform the automatch of lineup stations to tuner channels task.");
                                }
                            }

                            if (import)
                            {
                                // refresh the lineups after import
                                using (MergedLineups mergedLineups = new MergedLineups(WmcStore.WmcObjectStore))
                                {
                                    foreach (MergedLineup mergedLineup in mergedLineups)
                                    {
                                        mergedLineup.Refresh();
                                    }
                                }
                                Logger.WriteInformation("Completed lineup refresh.");

                                // reindex database
                                WmcUtilities.ReindexDatabase();

                                // set all active recording requests to anyLanguage=true
                                //setSeriesRecordingRequestAnyLanguage();

                                // update status logo
                                statusLogo.statusImage();

                                // signal the notification tray to update the icon
                                Helper.SendPipeMessage("Import Complete");
                            }

                            WmcStore.Close();

                            // all done
                            Logger.WriteInformation("Completed EPG123 client execution.");
                            Logger.WriteVerbose(string.Format("EPG123 client execution time was {0}.", DateTime.UtcNow - startTime));
                            Logger.Close();
                            NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        }
                    }
                }
            }
            Environment.Exit(0);
            return 0;
        }

        #region ========== Import MXF File ==========
        private static bool programRecording(int bufferMinutes)
        {
            DateTime expireTime = DateTime.Now + TimeSpan.FromHours((double)maximumRecordingWaitHours);
            int intervalMinutes = 1;
            int intervalCheck = 0;

            do
            {
                bool active = WmcUtilities.DetermineRecordingsInProgress() || ((WmcRegistries.NextScheduledRecording() - TimeSpan.FromMinutes(bufferMinutes)) < DateTime.Now);
                if (!active && intervalCheck > 0)
                {
                    Thread.Sleep(30000);
                    active = WmcUtilities.DetermineRecordingsInProgress() || ((WmcRegistries.NextScheduledRecording() - TimeSpan.FromMinutes(bufferMinutes)) < DateTime.Now);
                }
                if (!active || (active && expireTime < DateTime.Now))
                {
                    return active;
                }

                Helper.SendPipeMessage($"Importing|Waiting for recordings to end...|Will check again at {DateTime.Now + TimeSpan.FromMinutes(intervalMinutes):HH:mm:ss}");
                if (active && (intervalCheck++ % (60 / intervalMinutes)) == 0)
                {
                    Logger.WriteInformation($"There is a recording in progress or the next scheduled recording is within {bufferMinutes} minutes. Delaying garbage collection and/or import.");
                }
                Thread.Sleep(intervalMinutes * 60000);
            }
            while (true);
        }

        public static bool importMxfFile(string filename)
        {
            //verify tuners are setup in WMC prior to importing
            int deviceCount = 0;
            using (Devices devices = new Devices(WmcStore.WmcObjectStore))
            {
                foreach (Device device in devices)
                {
                    if (!device.Name.ToLower().Contains("delete"))
                    {
                        ++deviceCount;
                        break;
                    }
                }
            }
            if (deviceCount == 0)
            {
                Logger.WriteError("There are no devices/tuners configured in the database store. Perform WMC TV Setup prior to importing guide lisitngs. Aborting Import.");
                return false;
            }

            // do the import with or without progress form
            if (showProgress)
            {
                frmImport frm = new frmImport(filename);
                frm.ShowDialog();
                return frm.success;
            }
            else
            {
                return WmcUtilities.ImportMxfFile(filename);
            }
        }
        #endregion

        #region ========== Remove Channel Logos ==========
        public static void clearLineupChannelLogos()
        {
            Services services = new Services(WmcStore.WmcObjectStore);
            foreach (Service service in services)
            {
                if (service.LogoImage != null)
                {
                    service.LogoImage = null;
                    service.Update();
                }
            }
            ObjectStore.DisposeSingleton();
            Logger.WriteInformation("Completed clearing all station logos.");
        }
        #endregion

        #region ========== Set Recording Request Languages ==========
        private static bool setSeriesRecordingRequestAnyLanguage()
        {
            bool ret = false;
            using (SeriesRequests seriesRequests = new SeriesRequests(WmcStore.WmcObjectStore))
            {
                foreach (Request request in seriesRequests)
                {
                    if (!request.Complete && !request.AnyLanguage)
                    {
                        request.AnyLanguage = true;
                        request.Update();
                        Logger.WriteVerbose(string.Format("Changed \"{0}\" series recording request's 'anyLanguage' setting to TRUE.",
                            request.Title));
                        ret = true;
                    }
                }
            }
            return ret;
        }
        #endregion

        static void MyUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!(e.ExceptionObject as Exception).Message.Equals("access denied"))
            {
                Logger.WriteError(string.Format("Unhandled exception caught from {0}. message: {1}\n{2}", AppDomain.CurrentDomain.FriendlyName, (e.ExceptionObject as Exception).Message, (e.ExceptionObject as Exception).StackTrace));
            }
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }
        static void MyThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteError(string.Format("Unhandled thread exception caught from {0}. message: {1}\n{2}", AppDomain.CurrentDomain.FriendlyName, e.Exception.Message, e.Exception.StackTrace));
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }
    }
}