using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Pvr;
using Microsoft.MediaCenter.Store;
using Microsoft.Win32;

namespace epg123
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern uint SetThreadExecutionState(uint esFlags);
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

        private static string epg123ClientVersion
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

        private static string appGuid = "{CD7E6857-7D92-4A2F-B3AB-ED8CB42C6F65}";
        private static string guiGuid = "{0BA29D22-8BB1-4C33-919A-330D5DBA1FF0}";
        private static string impGuid = "{B7CEFF32-CD68-4094-BD1B-A541D246372E}";
        private const string lu_name = "EPG123 Lineups with Schedules Direct";
        private static string filename = string.Empty;
        private static bool showProgress = false;

        private static ObjectStore objectStore_;
        public static ObjectStore object_store
        {
            get
            {
                if (objectStore_ == null)
                {
                    SHA256Managed sha256Man = new SHA256Managed();
                    string clientId = ObjectStore.GetClientId(true);
                    string providerName = @"Anonymous!User";
                    string password = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(clientId)));
                    objectStore_ = ObjectStore.Open(null, providerName, password, true);
                }
                return objectStore_;
            }
        }

        private static NotifyIcon notifyIcon;

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

            // create a mutex and keep alive until program exits
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                bool match, nologo, import, force, showGui, advanced;
                match = nologo = import = force = showGui = advanced = false;

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
                            default:
                                Console.WriteLine("ERROR: \"{0}\" is not a valid argument.", args[i]);
                                return -1;
                        }
                    }
                }
                else
                {
                    showGui = true;
                }

                if (showGui)
                {
                    // use another mutex if the GUI is open
                    using (Mutex mutex2 = new Mutex(false, "Global\\" + guiGuid))
                    {
                        // check for a gui instance already running
                        if (!mutex2.WaitOne(2000, false))
                        {
                            MessageBox.Show("An instance of EPG123 Client is already running.", "Initialization Aborted");
                            return 0;
                        }

                        Logger.WriteMessage("===============================================================================");
                        Logger.WriteMessage(string.Format(" Activating the epg123 client GUI. version {0}", Helper.epg123Version));
                        Logger.WriteMessage("===============================================================================");
                        clientForm client = new clientForm(advanced);
                        client.ShowDialog();
                        Logger.Close();
                        mutex2.ReleaseMutex();
                        return 0;
                    }
                }

                // prevent machine from entering sleep mode
                uint prevThreadState = NativeMethods.SetThreadExecutionState((uint)ExecutionFlags.ES_CONTINUOUS |
                                                                             (uint)ExecutionFlags.ES_SYSTEM_REQUIRED |
                                                                             (uint)ExecutionFlags.ES_AWAYMODE_REQUIRED);

                // and yet another mutex for the import action
                using (Mutex mutex3 = new Mutex(false, "Global\\" + impGuid))
                {
                    // check for an import instance is already running
                    if (!mutex3.WaitOne(0, false))
                    {
                        Logger.WriteError("An instance of EPG123 Client import is already running. Aborting this instance.");
                        return -1;
                    }

                    Logger.WriteMessage("===============================================================================");
                    Logger.WriteMessage(string.Format(" Beginning epg123 client execution. version {0}", Helper.epg123Version));
                    Logger.WriteMessage("===============================================================================");
                    Logger.WriteInformation(string.Format("Beginning epg123 client execution. {0:u}", DateTime.Now.ToUniversalTime()));
                    Logger.WriteVerbose(string.Format("Import: {0} , Match: {1} , NoLogo: {2} , Force: {3} , ShowProgress: {4}", import, match, nologo, force, showProgress));
                    DateTime startTime = DateTime.UtcNow;

                    // remove all channel logos
                    if (nologo)
                    {
                        clearLineupChannelLogos();
                    }

                    notifyIcon = new NotifyIcon()
                    {
                        Text = "EPG123\nImporting guide listings...",
                        Icon = epg123.Properties.Resources.EPG123_import
                    };
                    notifyIcon.Visible = true;

                    // ensure no recordings are active if importing
                    if (import && !force && programRecording())
                    {
                        Logger.WriteError("A program recording is still in progress after 5 hours. Aborting the mxf file import.");
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex3.ReleaseMutex();

                        notifyIcon.Visible = false;
                        notifyIcon.Dispose();

                        statusLogo.statusImage();
                        return -1;
                    }

                    // import mxf file
                    if (import && !importMxfFile(filename))
                    {
                        Logger.WriteError("Failed to import .mxf file. Exiting.");
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex3.ReleaseMutex();

                        notifyIcon.Visible = false;
                        notifyIcon.Dispose();

                        statusLogo.statusImage();
                        return -1;
                    }
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();

                    // get lineup and configure lineup type and devices 
                    if (import && !activateLineupAndGuide())
                    {
                        Logger.WriteError("Failed to locate any lineups from EPG123.");
                        Logger.Close();
                        NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                        mutex3.ReleaseMutex();

                        statusLogo.statusImage();
                        return -1;
                    }

                    // perform automatch
                    if (match)
                    {
                        try
                        {
                            matchLineups();
                            Logger.WriteInformation("Completed the automatch of lineup stations to tuner channels.");
                        }
                        catch
                        {
                            Logger.WriteError("Failed to perform the automatch of lineup stations to tuner channels task.");
                        }
                    }

                    // refresh the lineups after import
                    if (import)
                    {
                        refreshLineups();
                        Logger.WriteInformation("Completed lineup refresh.");
                    }

                    // reindex database
                    if (import)
                    {
                        mxfImport.reindexDatabase();
                    }

                    // set all active recording requests to anyLanguage=true
                    if (setSeriesRecordingRequestAnyLanguage() || import)
                    {
                        mxfImport.reindexPvrSchedule();
                    }

                    // update status logo
                    if (import)
                    {
                        statusLogo.statusImage();
                    }

                    // all done
                    Logger.WriteInformation("Completed EPG123 client execution.");
                    Logger.WriteVerbose(string.Format("EPG123 client execution time was {0}.", DateTime.UtcNow - startTime));
                    Logger.Close();
                    mutex3.ReleaseMutex();
                }

                NativeMethods.SetThreadExecutionState(prevThreadState | (uint)ExecutionFlags.ES_CONTINUOUS);
                return 0;
            }
        }

        #region ========== Import MXF File ==========
        private static bool programRecording()
        {
            bool active = false;
            DateTime expireTime = DateTime.Now + TimeSpan.FromHours(5);
            int intervalMinutes = 60;

            do
            {
                active = false;
                DateTime timeReady = DateTime.Now;
                foreach (Recording recording in new Recordings(object_store))
                {
                    if ((recording.State == RecordingState.Initializing) || (recording.State == RecordingState.Recording))
                    {
                        active = true;
                        Logger.WriteInformation(string.Format("Recording in progress: {0:hh:mm tt} - {1:hh:mm tt} on channel {2}{3} -> {4} - {5}",
                                                          recording.ScheduleEntry.StartTime.ToLocalTime(),
                                                          recording.ScheduleEntry.EndTime.ToLocalTime(),
                                                          recording.Channel.ChannelNumber,
                                                          (recording.ScheduleEntry.Service != null) ? " " + recording.ScheduleEntry.Service.CallSign : string.Empty,
                                                          (recording.ScheduleEntry.Program != null) ? recording.ScheduleEntry.Program.Title : "unknown program title",
                                                          (recording.ScheduleEntry.Program != null) ? recording.ScheduleEntry.Program.EpisodeTitle : string.Empty));
                        if (recording.RequestedEndTime.ToLocalTime() > timeReady) timeReady = recording.RequestedEndTime.ToLocalTime();
                    }
                }

                if ((timeReady > DateTime.Now) && (DateTime.Now < expireTime))
                {
                    TimeSpan delay = TimeSpan.FromTicks(Math.Min((timeReady - DateTime.Now).Ticks + TimeSpan.FromMinutes(1).Ticks,
                                                                 TimeSpan.FromMinutes(intervalMinutes).Ticks));

                    notifyIcon.Text = "EPG123\nDelaying import due to recording in progress...";
                    notifyIcon.Icon = epg123.Properties.Resources.EPG123_pause;

                    Thread.Sleep((int)delay.TotalMilliseconds);
                }
                else
                {
                    notifyIcon.Icon = epg123.Properties.Resources.EPG123_import;
                    return active;
                }
            } while (active);

            return active;
        }
        public static bool importMxfFile(string filename)
        {
            // verify tuners are setup in WMC prior to importing
            int devCount = 0;
            foreach (Device dev in new Devices(object_store).ToArray())
            {
                if (!dev.Name.ToLower().Contains("delete"))
                {
                    ++devCount;
                    break;
                }
            }
            if (devCount == 0)
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
                return mxfImport.importMxfFile(filename);
            }
        }
        #endregion

        #region ========== Lineup ==========
        public static bool activateLineupAndGuide()
        {
            int lineups = 0;
            if (object_store != null)
            {
                foreach (Lineup lineup in new Lineups(object_store).ToArray())
                {
                    // only want to do this with EPG123 lineups
                    if (!lineup.Provider.Name.Equals("EPG123") || lineup.Name.Equals(lu_name)) continue;

                    // make sure the lineup type and language are set
                    if (string.IsNullOrEmpty(lineup.LineupTypes) || string.IsNullOrEmpty(lineup.Language))
                    {
                        lineup.LineupTypes = "WMIS";
                        lineup.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                        lineup.Update();
                    }

                    // set registry setting to "activate" the guide if necessary
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true))
                    {
                        try
                        {
                            if ((int)key.GetValue("fAgreeTOS") != 1) key.SetValue("fAgreeTOS", 1);
                            if ((string)key.GetValue("strAgreedTOSVersion") != "1.0") key.SetValue("strAgreedTOSVersion", "1.0");
                        }
                        catch
                        {
                            Logger.WriteError("Could not write/verify the registry settings to activate the guide in WMC.");
                        }
                    }
                    ++lineups;
                }
            }
            return (lineups > 0);
        }
        #endregion

        #region ========== Remove Channel Logos ==========
        public static void clearLineupChannelLogos()
        {
            foreach (Service service in new Services(object_store).ToArray())
            {
                if (service.LogoImage != null)
                {
                    service.LogoImage = null;
                    service.Update();
                }
            }
            Logger.WriteInformation("Completed clearing all station logos.");
        }
        #endregion

        #region ========== Match and Refresh Lineup ==========
        public static void matchLineups()
        {
            // get all active channels in lineup(s) from EPG123
            List<Channel> epg123Channels = new List<Channel>();
            foreach (Lineup lineup in new Lineups(object_store).ToArray())
            {
                if (lineup.Name.StartsWith("EPG123") && !lineup.Name.Equals(lu_name))
                {
                    epg123Channels.AddRange(lineup.GetChannels());
                }
            }

            // scan down to the merged channels
            using (MergedLineups mergedLineups = new MergedLineups(object_store))
            {
                foreach (MergedLineup mergedLineup in mergedLineups)
                {
                    foreach (Channel channel in mergedLineup.GetChannels())
                    {
                        // cast as mergedchannel for evaluation
                        MergedChannel mergedChannel;
                        try
                        {
                            mergedChannel = (MergedChannel)channel;
                        }
                        catch
                        {
                            continue;
                        }

                        // using the mergedchannel channel number, determine whether to match&enable, unmatch&disable, disable, or nothing
                        Channel epg123Channel = null;
                        foreach (Lineup lineup in new Lineups(object_store))
                        {
                            if (!lineup.Name.StartsWith("EPG123") || lineup.Name.Equals(lu_name)) continue;

                            while ((epg123Channel = lineup.GetChannelFromNumber(mergedChannel.OriginalNumber, mergedChannel.OriginalSubNumber)) != null)
                            {
                                if (channelsContain(ref epg123Channels, ref epg123Channel)) break;
                                else
                                {
                                    Logger.WriteVerbose(string.Format("Removing {0} from channel {1} in lineup {2}.", epg123Channel.CallSign, mergedChannel.ChannelNumber, lineup.Name));
                                    lineup.RemoveChannel(epg123Channel);
                                    lineup.Update();
                                }
                            }
                            if (epg123Channel != null) break;
                        }

                        // perform match if not already primary channel
                        if ((epg123Channel != null) && !mergedChannel.PrimaryChannel.IsSameAs(epg123Channel))
                        {
                            try
                            {
                                if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                                {
                                    Logger.WriteVerbose(string.Format("Matching {0} to channel {1}", epg123Channel.CallSign, mergedChannel.ChannelNumber));

                                    // copy current primary to secondary channels
                                    if (!mergedChannel.SecondaryChannels.Contains(mergedChannel.PrimaryChannel))
                                    {
                                        mergedChannel.SecondaryChannels.Add(mergedChannel.PrimaryChannel);
                                    }

                                    // add this channel lineup to the device group if necessary
                                    foreach (Device device in mergedChannel.Lineup.DeviceGroup.Devices)
                                    {
                                        try
                                        {
                                            if (!device.Name.ToLower().Contains("delete") &&
                                                (device.ScannedLineup != null) && device.ScannedLineup.IsSameAs(mergedChannel.PrimaryChannel.Lineup) &&
                                                ((device.WmisLineups == null) || !device.WmisLineups.Contains(epg123Channel.Lineup)))
                                            {
                                                device.SubscribeToWmisLineup(epg123Channel.Lineup);
                                                device.Update();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.WriteVerbose(string.Format("Failed to associate lineup {0} with device {1} ({2}). {3}", epg123Channel.Lineup,
                                                                              device.Name ?? "NULL", (device.ScannedLineup == null) ? "NULL" : device.ScannedLineup.Name, ex.Message));
                                        }
                                    }

                                    // update primary channel and service
                                    mergedChannel.PrimaryChannel = epg123Channel;
                                    mergedChannel.Service = epg123Channel.Service;
                                    mergedChannel.UserBlockedState = UserBlockedState.Enabled;
                                    mergedChannel.Update();
                                }
                                else
                                {
                                    Logger.WriteVerbose(string.Format("Skipped matching {0} to channel {1} due to channel already having an assigned listing.",
                                                                      epg123Channel.CallSign, mergedChannel.ChannelNumber));
                                }
                            }
                            catch (Exception ex)
                            {
                                string prefix = string.Format("Error trying to match {0} to channel {1}.", epg123Channel.CallSign, mergedChannel.ChannelNumber);

                                // report the channels that contain errors
                                if (mergedChannel.PrimaryChannel == null) Logger.WriteError(string.Format("{0} PrimaryChannel is null. {1}", prefix, ex.Message));
                                else if (mergedChannel.PrimaryChannel.Lineup == null) Logger.WriteError(string.Format("{0} PrimaryChannel.Lineup is null. {1}", prefix, ex.Message));
                                else if (mergedChannel.PrimaryChannel.Lineup.Name == null) Logger.WriteError(string.Format("{0} PrimaryChannel.Lineup.Name is null. {1}", prefix, ex.Message));
                                else Logger.WriteError(prefix + " " + ex.Message);
                            }
                        }
                        else if ((mergedChannel.UserBlockedState < UserBlockedState.Blocked) && mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                        {
                            // disable the channel in the guide
                            mergedChannel.UserBlockedState = UserBlockedState.Blocked;
                            mergedChannel.Update();
                        }
                    }

                    // finish it
                    mergedLineup.FullMerge(false);
                    mergedLineup.Update();
                }
            }
        }
        private static bool channelsContain(ref List<Channel> channels, ref Channel channel)
        {
            foreach (Channel c in channels)
            {
                if (c.IsSameAs(channel)) return true;
            }
            return false;
        }
        private static void refreshLineups()
        {
            using (MergedLineups mergedLineups = new MergedLineups(object_store))
            {
                foreach (MergedLineup mergedLineup in mergedLineups)
                {
                    mergedLineup.Refresh();
                }
            }
        }
        #endregion

        #region ========== Set Recording Request Languages ==========
        private static bool setSeriesRecordingRequestAnyLanguage()
        {
            bool ret = false;
            foreach (Request request in new SeriesRequests(object_store))
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
            return ret;
        }
        #endregion

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