using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmClientSetup : Form
    {
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        [DllImport("User32.dll")]
        private static extern IntPtr FindWindow(string className, string windowText);

        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_RESTORE = 9;
        const int TUNERLIMIT = 32;

        private IntPtr _wmcPtr = IntPtr.Zero;
        private const string BYPASSED = "BACKUP_BYPASSED";
        private readonly bool _colossusInstalled;
        private readonly bool _epg123Installed = File.Exists(Helper.Epg123ExePath);
        private readonly bool _hdhr2mxfInstalled = File.Exists(Helper.Hdhr2MxfExePath);
        private bool _taskWorking;
        private Process _procWmc;
        private bool TimeoutWmc => DateTime.Now > _procWmc.StartTime + TimeSpan.FromSeconds(60);
        private Process _procMcupdate;
        private bool TimeoutMcupdate => DateTime.Now > _procMcupdate.StartTime + TimeSpan.FromSeconds(90);
        public bool ShouldBackup;
        public bool Hdhr2MxfSrv;
        public string mxfImport;

        public frmClientSetup()
        {
            InitializeComponent();

            _colossusInstalled = IsColossusInstalled();
            if (Helper.InstallMethod == Helper.Installation.CLIENT)
            {
                lblConfig.Text = "Import MXF file from remote server";
            }
            else if (_epg123Installed && _hdhr2mxfInstalled)
            {
                lblConfig.Text = "Import MXF file from either EPG123 or HDHR2MXF";
            }
            else if (_hdhr2mxfInstalled)
            {
                lblConfig.Text = "Import MXF file from HDHR2MXF";
            }

            UpdateStatusText("Click the 'Step 1' button to begin.");
        }

        private void UpdateStatusText(string text)
        {
            statusLabel.Text = text;
            Refresh();
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (_taskWorking) return;
            UseWaitCursor = true;
            _taskWorking = true;

            // STEP 1 : Clean Start
            if (sender.Equals(btnCleanStart) && PerformBackup() && CleanStart())
            {
                // disable clean start button
                btnCleanStart.Enabled = lblCleanStart.Enabled = false;
                btnCleanStart.BackColor = System.Drawing.Color.LightGreen;

                if (SystemInformation.BootMode != BootMode.Normal)
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show("Successfully deleted the eHome folder. Please reboot into Normal Mode to continue by either performing Client Setup again or proceeding to Step 2 manually.", "Safe Mode Detected", MessageBoxButtons.OK);
                    return;
                }

                // enable colossus and/or tv setup buttons
                btnTvSetup.Enabled = lblTvSetup.Enabled = true;
                UpdateStatusText("Click the 'Step 2' button to continue.");
            }

            // STEP 2: WMC TV Setup
            else if (sender.Equals(btnTvSetup) && ConfigureHdPvrTuners() && OpenWmc() && ActivateGuide() && DisableBackgroundScanning())
            {
                // disable tv setup button and enable configuration button
                btnTvSetup.Enabled = lblTvSetup.Enabled = false;
                btnTvSetup.BackColor = System.Drawing.Color.LightGreen;

                // enable config button if epg123.exe is present
                btnConfig.Enabled = lblConfig.Enabled = true;
                UpdateStatusText("Click the 'Step 3' button to continue.");
            }

            // STEP 3: Configure EPG123
            else if (sender.Equals(btnConfig) && OpenEpg123Configuration())
            {
                // disable config button
                btnConfig.Enabled = lblConfig.Enabled = false;
                btnConfig.BackColor = System.Drawing.Color.LightGreen;
                UpdateStatusText(string.Empty);
            }

            // bring form into focus
            Application.OpenForms[Name]?.Activate();
            _taskWorking = false;
            UseWaitCursor = false;

            if (!(btnCleanStart.Enabled || btnTvSetup.Enabled || btnConfig.Enabled))
            {
                // restore recording requests if the tool is available
                if (File.Exists("epg123Transfer.exe"))
                {
                    UpdateStatusText("Transferring recording requests ...");
                    Logger.WriteVerbose("Opening recording request transfer tool and waiting for it to close ...");
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "epg123Transfer.exe",
                        Arguments = Helper.BackupZipFile
                    };
                    Process.Start(startInfo)?.WaitForExit();
                    UpdateStatusText(string.Empty);
                }

                MessageBox.Show("Setup is complete.", "Setup Complete", MessageBoxButtons.OK);
                Logger.WriteVerbose("Setup is complete.  Be sure to create a Scheduled Task to perform daily updates and keep your guide up to date!");
                btnCleanStart.Enabled = true;
                Close();
            }
            else if (cbAutostep.Checked && btnTvSetup.Enabled)
            {
                button_Click(btnTvSetup, null);
            }
            else if (cbAutostep.Checked && btnConfig.Enabled)
            {
                button_Click(btnConfig, null);
            }
        }

        private void frmClientSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnCleanStart.Enabled && DialogResult.No == MessageBox.Show("Are you sure you wish to interrupt a clean start setup?", "Process Interruption", MessageBoxButtons.YesNo))
            {
                e.Cancel = true;
                return;
            }

            if ((_procWmc != null) && (!_procWmc.HasExited))
            {
                _procWmc.Kill();
            }
        }

        #region ========== Step 1 Clean Start ==========
        private readonly string[] _regKeysDelete = { @"Service\Epg" };
        private readonly string[] _regKeysCreate = { @"Service\Epg" };

        private void RefreshRegistryKeys()
        {
            UpdateStatusText("Refreshing registry keys ...");
            Logger.WriteVerbose("Refreshing registry keys ...");
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center", true))
            {
                if (key != null)
                {
                    // delete registry keys
                    foreach (var registry in _regKeysDelete)
                    {
                        try
                        {
                            key.DeleteSubKeyTree(registry);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteError($"{Helper.ReportExceptionMessages(ex)}");
                        }
                    }

                    // create fresh keys
                    foreach (var registry in _regKeysCreate)
                    {
                        try
                        {
                            key.CreateSubKey(registry);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteError($"{Helper.ReportExceptionMessages(ex)}");
                        }
                    }
                }
            }

            // reset date/time for garbage cleanup
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\EPG", true))
            {
                const string epg123NextRunTime = "dbgc:next run time";
                var nextRunTime = DateTime.Now + TimeSpan.FromDays(4.5);
                try
                {
                    key?.SetValue(epg123NextRunTime, nextRunTime.ToString());
                }
                catch
                {
                    Logger.WriteError("Could not set next garbage cleanup time in registry.");
                }

                try
                {
                    key?.SetValue("dl", 0);
                }
                catch
                {
                    Logger.WriteInformation("Could not disable WMC downloading with registry key.");
                }
            }

            // reset the status logo
            using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                if (key != null)
                {
                    try
                    {
                        key.DeleteValue("OEMLogoAccent");
                        key.DeleteValue("OEMLogoOpacity");
                        key.SetValue("OEMLogoUri", string.Empty);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            // add channel 1 to the available scan channels
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Multimedia\TV\Tuning Spaces\Digital Cable", true))
            {
                if (key != null && (int)key.GetValue("MinChannel", 2) != 1) key.SetValue("MinChannel", 1, RegistryValueKind.DWord);
            }

            // disable metadata downloads
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings\MCE.GlobalSettings", true))
            {
                if (key != null && (int)key.GetValue("workoffline", 0) != 1) key.SetValue("workoffline", 1);
            }

            // disable guide downloads
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Epg", true))
            {
                if (key != null && (int)key.GetValue("dl", 1) != 0) key.SetValue("dl", 0);
            }

            // disable usage tracking
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings\ProgramGuide", true))
            {
                if (key != null && (int)key.GetValue("fPrivacyLevel", 2) != 1) key.SetValue("fPrivacyLevel", 1);
                if (key != null && (int)key.GetValue("fUsageTracking", 1) != 0) key.SetValue("fUsageTracking", 0);
            }
        }

        private bool PerformBackup()
        {
            if (!string.IsNullOrEmpty(Helper.BackupZipFile)) return true;
            if (DialogResult.Cancel == MessageBox.Show("This procedure will delete all WMC databases in your eHome folder. Current tuner configurations, recording schedules, favorite lineups, and logos will be backed up prior to deletion.\n\nClick 'OK' to proceed.", "EPG Clean Start", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)) return false;

            if (ShouldBackup)
            {
                UpdateStatusText("Backing up WMC configurations ...");
                Logger.WriteVerbose("Backing up WMC configurations ...");
                clientForm.BackupBackupFiles();

                if (string.IsNullOrEmpty(Helper.BackupZipFile))
                {
                    if (DialogResult.Yes == MessageBox.Show("Failed to create a backup of the current WMC configurations and scheduled recording requests.\n\nDo you wish to continue without performing a backup?", "Backup Failure", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    {
                        Helper.BackupZipFile = BYPASSED;
                    }
                }
            }
            else
            {
                Helper.BackupZipFile = BYPASSED;
            }
            UpdateStatusText(string.Empty);
            return !string.IsNullOrEmpty(Helper.BackupZipFile);
        }

        private bool CleanStart()
        {
            // stop all programs and services that access the database and delete the eHome folder
            if (!CleaneHomeFolder())
            {
                MessageBox.Show("Failed to delete the database contents in the eHome folder. Try again or consider trying in Safe Mode.", "Failed Operation", MessageBoxButtons.OK);
                UpdateStatusText("Click the 'Step 1' button to try again.");
                return false;
            }

            if (_colossusInstalled)
            {
                UpdateStatusText("Deleting HD PVR tuner files ...");
                Logger.WriteVerbose("Deleting HD PVR tuner files ...");
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                             @"\Hauppauge\MediaCenterService";
                foreach (var tuner in _hdpvrTuners)
                {
                    try
                    {
                        var file = $"{folder}\\{tuner}.xml";

                        // remove possible read-only attribute
                        if (!File.Exists(file)) continue;
                        var fa = File.GetAttributes(file);
                        File.SetAttributes(file, fa & ~FileAttributes.ReadOnly);
                        File.Delete(file);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            // delete and rebuild registry keys
            RefreshRegistryKeys();

            var crashDetected = false;
            var instance = 0;
            do
            {
                // call up media center to create database file
                UpdateStatusText("Starting Windows Media Center ...");
                Logger.WriteVerbose("Starting Windows Media Center ...");
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.EhshellExeFilePath,
                    Arguments = "/nostartupanimation /homepage:Options.SetupMediaCenter.xml"
                };
                _procWmc = Process.Start(startInfo);
                _procWmc?.WaitForInputIdle(30000);

                // wait for wmc form to show and then hide it
                // waiting for WMDMNotificationWindowClass guarantees? WMC is ready for import
                // WMDM = Windows Media Device Manager (Windows Media Player)
                UpdateStatusText("Waiting for initial WMC database build ...");
                Logger.WriteVerbose("Waiting for initial WMC database build ...");
                do
                {
                    if ((_wmcPtr = FindWindow("eHome Render Window", "Windows Media Center")) != IntPtr.Zero)
                    {
                        if (!IsIconic(_wmcPtr))
                        {
                            Thread.Sleep(1000);
                            ShowWindow(_wmcPtr, SW_SHOWMINIMIZED);
                        }
                        else
                        {
                            ShowWindow(_wmcPtr, SW_HIDE);
                        }
                    }
                    Thread.Sleep(100);
                    Application.DoEvents();
                } while (FindWindow("WMDMNotificationWindowClass", null) == IntPtr.Zero && !TimeoutWmc);

                // detect whether a "recovery" occurred and try again
                var newInstance = 0;
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\EPG", false))
                {
                    if (key == null) continue;
                    try
                    {
                        newInstance = (int)key.GetValue("EPG.instance");
                    }
                    catch
                    {
                    }

                    if (newInstance != instance)
                    {
                        crashDetected = true;
                        Logger.WriteVerbose(
                            "WMC crash detected. Killing current instance of WMC and starting another.");
                        _procWmc.Kill();
                        while (!_procWmc.HasExited) ;

                        instance = newInstance;
                    }
                    else
                    {
                        // process an update to avoid overwriting the increased limit
                        // it appears the very first mcupdate.exe run will reset the tuner limit
                        // WMC will perform mcupdate.exe -manual -nogc -p 0 starting with "Downloading TV Setup Data"
                        UpdateStatusText("Performing a manual database update ...");
                        Logger.WriteVerbose("Performing a manual database update ...");
                        startInfo = new ProcessStartInfo()
                        {
                            FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + @"\ehome\mcupdate.exe",
                            Arguments = "-manual -nogc -p 0",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        _procMcupdate = Process.Start(startInfo);
                        do
                        {
                            Thread.Sleep(100);
                            Application.DoEvents();
                        } while (!_procMcupdate.HasExited && !TimeoutMcupdate);
                    }
                }
            } while (crashDetected && instance < 5);

            // increase the tuner count to 32
            TweakMediaCenterTunerCount(TUNERLIMIT);
            UpdateSatelliteTransponders();
            UpdateStatusText(string.Empty);

            return true;
        }

        private static bool EmptyFolder(DirectoryInfo directoryInfo)
        {
            var ret = true;

            foreach (var file in directoryInfo.GetFiles())
            {
                try
                {
                    //foreach (Process proc in FileUtil.WhoIsLocking(file.FullName))
                    //{
                    //    Logger.WriteVerbose(string.Format("Stopping process \"{0}\"", proc.ProcessName));
                    //    proc.Kill();
                    //    proc.WaitForExit(1000);
                    //}
                    file.Delete();
                }
                catch
                {
                    Logger.WriteError($"Failed to delete file \"{file.FullName}\"");
                    ret = false;
                }
            }

            foreach (var subfolder in directoryInfo.GetDirectories())
            {
                ret = EmptyFolder(subfolder);
            }

            return ret;
        }

        private bool CleaneHomeFolder()
        {
            var ret = true;

            // delete the ehome folder contents
            UpdateStatusText("Deleting eHome folder contents ...");
            Logger.WriteVerbose("Deleting eHome folder contents ...");
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\eHome";
            var di = new DirectoryInfo(folder);
            try
            {
                foreach (var file in di.GetFiles("mcepg*.*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        foreach (var proc in FileUtil.WhoIsLocking(file.FullName))
                        {
                            Logger.WriteVerbose($"Stopping process \"{proc.ProcessName}\"");
                            proc.Kill();
                            proc.WaitForExit(1000);
                        }
                        file.Delete();
                    }
                    catch
                    {
                        Logger.WriteError($"Failed to delete file \"{file.FullName}\"");
                        ret = false;
                    }
                }
            }
            catch
            {
                ret = false;
            }

            // it's okay if we don't delete the folders, I think
            try
            {
                foreach (var dir in di.GetDirectories())
                {
                    if (!dir.Name.StartsWith("mcepg") && !dir.Name.Contains("Packages")) continue;
                    ret &= EmptyFolder(dir);
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    {
                        Logger.WriteError($"Failed to delete folder \"{dir.FullName}\"");
                        ret = false;
                    }
                }
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        private void TweakMediaCenterTunerCount(int count)
        {
            UpdateStatusText("Increasing tuner limits ...");
            Logger.WriteVerbose("Increasing tuner limits ...");
            WmcStore.SetWmcTunerLimits(count);
        }

        private void UpdateSatelliteTransponders()
        {
            UpdateStatusText("Updating satellite transponders ...");
            Logger.WriteVerbose("Updating satellite transponders ...");
            SatMxf.UpdateDvbsTransponders(false);
        }

        private static bool IsTunerCountTweaked()
        {
            var tuners = 0;
            var recorders = 0;
            using (var tunersKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Video\Tuners", false))
            {
                var subkeys = tunersKey.GetSubKeyNames().Where(arg => !arg.Equals("DVR")).ToArray();
                foreach (var subkey in subkeys)
                {
                    using (var tunersKey2 = tunersKey.OpenSubKey(subkey))
                    {
                        tuners += tunersKey2.SubKeyCount;
                    }
                }
            }

            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings", false))
            {
                var subkeys = key.GetSubKeyNames().Where(arg => arg.StartsWith("RecorderSettings")).ToArray();
                foreach (var subkey in subkeys)
                {
                    using (var key2 = key.OpenSubKey(subkey))
                    {
                        if (!key2.GetValue("id").Equals("<<NULL>>")) ++recorders;
                    }
                }
            }

            if (tuners == recorders) return true;
            return DialogResult.Yes != MessageBox.Show($"It appears there are {tuners} tuners available for use but only {recorders} of them configured in WMC.\n\nDo you wish to perform TV Setup again?", "Verify Tuner Count", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        #endregion

        #region ========== Hauppauge HD PVR ==========
        private readonly List<string> _hdpvrTuners = new List<string>();

        private bool IsColossusInstalled()
        {
            // check registry for 32-bit and 64-bit
            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Hauppauge\\HcwDevCentral\\Devices", false) ??
                      Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Hauppauge\\HcwDevCentral\\Devices", false);
            if (key == null) return false;

            // find all the software tuners and store in array
            foreach (var subkey in key.GetSubKeyNames())
            {
                var device = key.OpenSubKey(subkey);
                if ((string)device?.GetValue("FriendlyName") == "Hauppauge HD PVR Software Tuner")
                {
                    _hdpvrTuners.Add(subkey);
                }
                device?.Close();
            }
            key.Close();

            return true;
        }

        private bool ConfigureHdPvrTuners()
        {
            var success = true;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Hauppauge\MediaCenterService";
            foreach (var tuner in _hdpvrTuners.ToArray())
            {
                UpdateStatusText("Creating HD PVR tuner files ...");
                Logger.WriteVerbose("Creating HD PVR tuner files ...");
                try
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    var file = $"{folder}\\{tuner}.xml";
                    var fa = new FileAttributes();

                    // remove possible read-only attribute
                    if (File.Exists(file))
                    {
                        fa = File.GetAttributes(file);
                        File.SetAttributes(file, fa & ~FileAttributes.ReadOnly);
                    }

                    using (var stream = new StreamWriter(file))
                    {
                        // write header
                        stream.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        stream.WriteLine("<Channels>");
                        stream.WriteLine("  <Lineup epg123=\"true\">EPG123 Channels for HD PVR Software Tuner</Lineup>");
                        stream.WriteLine("  <tune:Tuning xmlns:tune=\"http://schemas.microsoft.com/2006/eHome/MediaCenter/Tuning.xsd\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://schemas.microsoft.com/2006/eHome/MediaCenter/Tuning.xsd file://tuning.xsd\" >");

                        // write body
                        for (var i = 1; i < 10000; ++i)
                        {
                            stream.WriteLine($"    <Idx value=\"{i}\" />");
                            stream.WriteLine($"    <tune:ChannelID ChannelID=\"{i}\">");
                            stream.WriteLine("      <tune:TuningSpace xsi:type=\"tune:ChannelIDTuningSpaceType\" Name=\"DC65AA02-5CB0-4d6d-A020-68702A5B34B8\" NetworkType=\"{DC65AA02-5CB0-4d6d-A020-68702A5B34B8}\" />");
                            stream.WriteLine("      <tune:Locator xsi:type=\"tune:ATSCLocatorType\" Frequency=\"-1\" PhysicalChannel=\"-1\" TransportStreamID=\"-1\" ProgramNumber=\"1\" />");
                            stream.WriteLine("      <tune:Components xsi:type=\"tune:ComponentsType\" />");
                            stream.WriteLine("    </tune:ChannelID>");
                            stream.WriteLine($"    <ps:Service xmlns:ps=\"http://schemas.microsoft.com/2006/eHome/MediaCenter/PBDAService.xsd\" name=\"\" callSign=\"Z{i}\" ppv=\"false\" subscribed=\"true\" codec=\"H.264\" channel=\"{i}\" matchname=\"\"></ps:Service>");
                        }

                        // write footer
                        stream.WriteLine("  </tune:Tuning>");
                        stream.WriteLine("</Channels>");
                    }

                    // set read-only attribute
                    File.SetAttributes(file, fa | FileAttributes.ReadOnly);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(Helper.ReportExceptionMessages(ex), "IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Helper.ReportExceptionMessages(ex), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                }
                UpdateStatusText(string.Empty);
            }

            if (!success)
            {
                MessageBox.Show("Failed to configure the HD PVR/Colossus software tuner(s).", "Failed Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return success;
        }
        #endregion

        #region ========== Step 2 WMC TV Setup ==========
        private bool OpenWmc()
        {
            UpdateStatusText("Opening WMC for TV Setup ...");
            Logger.WriteVerbose("Opening WMC for TV Setup ...");
            if (!_procWmc.HasExited)
            {
                // show wmc window
                ShowWindow(_wmcPtr, SW_SHOWNORMAL);
            }
            else
            {
                // open wmc to setup tuners
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.EhshellExeFilePath,
                    Arguments = "/nostartupanimation"
                };
                _procWmc = Process.Start(startInfo);
            }

            UpdateStatusText("Waiting for user to complete Step 2 ...");
            Logger.WriteVerbose("Waiting for user to complete Step 2 ...");

            do
            {
                Thread.Sleep(100);
                Application.DoEvents();
            } while (!_procWmc.HasExited);
            UpdateStatusText(string.Empty);

            return true;
        }

        private bool ActivateGuide()
        {
            UpdateStatusText("Activating guide in registry ...");
            Logger.WriteVerbose("Activating guide in registry ...");

            var ret = WmcStore.ActivateGuide();
            UpdateStatusText(string.Empty);
            return ret;
        }

        private bool DisableBackgroundScanning()
        {
            UpdateStatusText("Disabling background scanner ...");
            Logger.WriteVerbose("Disabling background scanner ...");

            var ret = WmcStore.SetBackgroundScanning(false);
            UpdateStatusText(string.Empty);
            return ret;
        }
        #endregion

        #region ========== EPG123 Configurator ==========
        private bool OpenEpg123Configuration()
        {
            var exeName = "EPG123";
            var exePath = Helper.Epg123GuiPath;
            mxfImport = string.Empty;

            // simple client install
            if (Helper.InstallMethod == Helper.Installation.CLIENT)
            {
                var frmRemote = new frmRemoteServers();
                frmRemote.ShowDialog();
                mxfImport = frmRemote.mxfPath;
                if (string.IsNullOrEmpty(mxfImport))
                {
                    MessageBox.Show($"No MXF files from a remote server was selected for import.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatusText("Click the 'Step 3' button to try again.");
                    return cbAutostep.Checked = false;
                }
            }
            else
            {
                // hdhr2mxf installed and used
                if (_hdhr2mxfInstalled)
                {
                    const string text = "You have both the EPG123 executable for guide listings from Schedules Direct and the HDHR2MXF executable for guide listings from SiliconDust.\n\nDo you wish to proceed with HDHR2MXF?";
                    const string caption = "Multiple Guide Sources";
                    if (!_epg123Installed || DialogResult.Yes == MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        exeName = "HDHR2MXF";
                        exePath = Helper.Hdhr2MxfExePath;
                        mxfImport = Helper.Hdhr2MxfMxfPath;
                        Hdhr2MxfSrv = true;
                    }
                }
                var txt = $"Running {exeName} and importing MXF ...";
                UpdateStatusText(txt);
                Logger.WriteVerbose(txt);

                var proc = Process.GetProcesses().SingleOrDefault(arg => arg.ProcessName.Equals(Path.GetFileNameWithoutExtension(exePath))) ?? Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc.EnableRaisingEvents = true;
                ShowWindow(proc.MainWindowHandle, SW_SHOWNORMAL);
                SetForegroundWindow(proc.MainWindowHandle);
                proc.WaitForExit();

                if (proc.ExitCode == 0xDEAD)
                {
                    MessageBox.Show($"There was an error using {exeName} to create the MXF file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatusText("Click the 'Step 3' button to try again.");
                    return cbAutostep.Checked = false;
                }
            }

            if (!string.IsNullOrEmpty(mxfImport))
            {
                var importForm = new frmImport(mxfImport);
                importForm.ShowDialog();
                if (!importForm.Success)
                {
                    MessageBox.Show("There was an error importing the MXF file.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatusText("Click the 'Step 3' button to try again.");
                    return cbAutostep.Checked = false;
                }
                WmcStore.ActivateEpg123LineupsInStore();
                WmcStore.ActivateGuide();
                WmcStore.AutoMapChannels();
                WmcStore.ReindexDatabase();
                statusLogo.StatusImage(mxfImport);
                Helper.SendPipeMessage("Import Complete");
            }
            return true;
        }
        #endregion
    }
}