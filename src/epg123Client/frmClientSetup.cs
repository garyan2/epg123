using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace epg123
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

        private IntPtr wmcPtr = IntPtr.Zero;
        private string backupZipFile = string.Empty;
        private string BYPASSED = "BACKUP_BYPASSED";
        private bool colossusInstalled = false;
        private bool epg123Installed = false;
        private bool taskWorking = false;
        private Process procWmc;
        private bool timeoutWmc
        {
            get
            {
                return (DateTime.Now > (procWmc.StartTime + TimeSpan.FromSeconds(60)));
            }
        }
        private Process procMcupdate;
        private bool timeoutMcupdate
        {
            get
            {
                return (DateTime.Now > (procMcupdate.StartTime + TimeSpan.FromSeconds(90)));
            }
        }

        public bool shouldBackup;

        public frmClientSetup()
        {
            InitializeComponent();

            colossusInstalled = isColossusInstalled();
            if (!(epg123Installed = File.Exists(Helper.Epg123ExePath))) { lblConfig.Text = "Not Installed"; }

            updateStatusText("Click the 'Step 1' button to begin.");
        }

        private void updateStatusText(string text)
        {
            statusLabel.Text = text;
            this.Refresh();
        }
        private void button_Click(object sender, EventArgs e)
        {
            if (taskWorking) return;
            this.UseWaitCursor = true;
            taskWorking = true;

            // STEP 1 : Clean Start
            if (sender.Equals(btnCleanStart) && PerformBackup() && cleanStart())
            {
                // disable clean start button
                btnCleanStart.Enabled = lblCleanStart.Enabled = false;
                btnCleanStart.BackColor = System.Drawing.Color.LightGreen;

                if (SystemInformation.BootMode != BootMode.Normal)
                {
                    this.Cursor = Cursors.Arrow;
                    MessageBox.Show("Successfully deleted the eHome folder. Please reboot into Normal Mode to continue by either performing Client Setup again or proceeding to Step 2 manually.", "Safe Mode Detected", MessageBoxButtons.OK);
                    return;
                }

                // enable colossus and/or tv setup buttons
                btnTvSetup.Enabled = lblTvSetup.Enabled = true;
                updateStatusText("Click the 'Step 2' button to continue.");
            }

            // STEP 2: WMC TV Setup
            else if (sender.Equals(btnTvSetup) && configureHdPvrTuners() && openWmc() && activateGuide() && disableBackgroundScanning())
            {
                // disable tv setup button and enable configuration button
                btnTvSetup.Enabled = lblTvSetup.Enabled = false;
                btnTvSetup.BackColor = System.Drawing.Color.LightGreen;

                // enable config button if epg123.exe is present
                btnConfig.Enabled = lblConfig.Enabled = epg123Installed;
                if (epg123Installed) updateStatusText("Click the 'Step 3' button to continue.");
                else updateStatusText(string.Empty);
            }

            // STEP 3: Configure EPG123
            else if (sender.Equals(btnConfig) && openEpg123Configuration())
            {
                // disable config button
                btnConfig.Enabled = lblConfig.Enabled = false;
                btnConfig.BackColor = System.Drawing.Color.LightGreen;
                updateStatusText(string.Empty);
            }

            // bring form into focus
            Application.OpenForms[this.Name].Activate();
            taskWorking = false;
            this.UseWaitCursor = false;

            if (!(btnCleanStart.Enabled || btnTvSetup.Enabled || btnConfig.Enabled))
            {
                if (!backupZipFile.Equals(BYPASSED) && File.Exists("epg123Transfer.exe"))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "epg123Transfer.exe",
                        Arguments = backupZipFile
                    };
                    Process.Start(startInfo).WaitForExit();
                }
                MessageBox.Show("Setup is complete. Be sure to create a Scheduled Task to perform daily updates and keep your guide up to date!", "Setup Complete", MessageBoxButtons.OK);
                this.Close();
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
            if ((procWmc != null) && (!procWmc.HasExited))
            {
                procWmc.Kill();
            }
        }

        #region ========== Step 1 Clean Start ==========
        private string[] regKeysDelete = new string[] { @"Service\Epg" };
        private string[] regKeysCreate = new string[] { @"Service\Epg" };
        private void refreshRegistryKeys()
        {
            updateStatusText("Refreshing registry keys ...");
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Media Center", true))
            {
                if (key != null)
                {
                    // delete registry keys
                    foreach (string registry in regKeysDelete)
                    {
                        try
                        {
                            key.DeleteSubKeyTree(registry);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteError(ex.Message);
                        }
                    }

                    // create fresh keys
                    foreach (string registry in regKeysCreate)
                    {
                        try
                        {
                            key.CreateSubKey(registry);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteError(ex.Message);
                        }
                    }
                }
            }

            // reset the status logo
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Start Menu", true))
            {
                if (key != null)
                {
                    try
                    {
                        key.DeleteValue("OEMLogoAccent");
                    }
                    catch { }
                    try
                    {
                        key.DeleteValue("OEMLogoOpacity");
                    }
                    catch { }
                    try
                    {
                        key.SetValue("OEMLogoUri", string.Empty);
                    }
                    catch { }
                }
            }

            // add channel 1 to the available scan channels
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Multimedia\TV\Tuning Spaces\Digital Cable", true))
            {
                if ((key != null) && (key.GetValue("MinChannel").ToString() != "1"))
                {
                    try
                    {
                        key.SetValue("MinChannel", 1, RegistryValueKind.DWord);
                    }
                    catch { }
                }
            }
        }
        private bool PerformBackup()
        {
            if (!string.IsNullOrEmpty(backupZipFile)) return true;
            if (DialogResult.Cancel == MessageBox.Show("This procedure will delete all contents of your eHome folder. Current tuner configurations, recording schedules, favorite lineups, and logos will be removed.\n\nClick 'OK' to proceed.", "EPG Clean Start", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)) return false;

            if (shouldBackup)
            {
                updateStatusText("Backing up WMC configurations ...");
                backupZipFile = clientForm.backupBackupFiles();

                if (string.IsNullOrEmpty(backupZipFile))
                {
                    if (DialogResult.Yes == MessageBox.Show("Failed to create a backup of the current WMC configurations and scheduled recording requests.\n\nDo you wish to continue without performing a backup?", "Backup Failure", MessageBoxButtons.YesNo, MessageBoxIcon.Error))
                    {
                        backupZipFile = BYPASSED;
                    }
                }
            }
            else
            {
                backupZipFile = BYPASSED;
            }
            updateStatusText(string.Empty);
            return !string.IsNullOrEmpty(backupZipFile);
        }
        private bool cleanStart()
        {
            // stop all programs and services that access the database and delete the eHome folder
            if (!cleaneHomeFolder())
            {
                MessageBox.Show("Failed to delete all contents of the eHome folder. Try again or consider trying in Safe Mode.", "Failed Operation", MessageBoxButtons.OK);
                updateStatusText("Click the 'Step 1' button to try again.");
                return false;
            }

            if (colossusInstalled)
            {
                updateStatusText("Deleting HD PVR tuner files ...");
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Hauppauge\MediaCenterService";
                foreach (string tuner in hdpvrTuners)
                {
                    try
                    {
                        string file = string.Format("{0}\\{1}.xml", folder, tuner);
                        FileAttributes fa = new FileAttributes();

                        // remove possible read-only attribute
                        if (File.Exists(file))
                        {
                            fa = File.GetAttributes(file);
                            File.SetAttributes(file, fa & ~FileAttributes.ReadOnly);
                            File.Delete(file);
                        }
                    }
                    catch { }
                }
            }

            // delete and rebuild registry keys
            refreshRegistryKeys();

            // call up media center to create database file
            updateStatusText("Starting Windows Media Center ...");
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Helper.EhshellExeFilePath,
                Arguments = "/nostartupanimation"
            };
            procWmc = Process.Start(startInfo);
            procWmc.WaitForInputIdle(30000);

            // wait for wmc form to show and then hide it
            // waiting for WMDMNotificationWindowClass guarantees? WMC is ready for import
            // WMDM = Windows Media Device Manager (Windows Media Player)
            updateStatusText("Waiting for initial WMC database build ...");
            do
            {
                if ((wmcPtr = FindWindow("eHome Render Window", "Windows Media Center")) != IntPtr.Zero)
                {
                    if (!IsIconic(wmcPtr))
                    {
                        Thread.Sleep(1000);
                        ShowWindow(wmcPtr, SW_SHOWMINIMIZED);
                    }
                    else
                    {
                        ShowWindow(wmcPtr, SW_HIDE);
                    }
                }
                Thread.Sleep(100);
                Application.DoEvents();
            } while ((FindWindow("WMDMNotificationWindowClass", null) == IntPtr.Zero) && !timeoutWmc);

            // process an update to avoid overwriting the increased limit
            // note a mcupdate -uf run will reset tuner limits to 4
            updateStatusText("Performing a manual database update ...");
            startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + @"\ehome\mcupdate.exe",
                Arguments = "-u -manual -nogc"
            };
            procMcupdate = Process.Start(startInfo);
            do
            {
                Thread.Sleep(100);
                Application.DoEvents();
            } while (!procMcupdate.HasExited && !timeoutMcupdate);

            // increase the tuner count to 32
            updateStatusText("Increasing tuner limits ...");
            tweakMediaCenterTunerCount(32);
            updateStatusText(string.Empty);

            return true;
        }
        private bool cleaneHomeFolder()
        {
            bool ret = true;

            // delete the ehome folder contents
            updateStatusText("Deleting eHome folder contents ...");
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\eHome";
            DirectoryInfo di = new DirectoryInfo(folder);
            try
            {
                foreach (FileInfo file in di.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    // keep the playready crypto file in-place if it exists
                    if (file.FullName.ToLower().Contains("mcendindiv")) continue;

                    try
                    {
                        foreach (Process proc in FileUtil.WhoIsLocking(file.FullName))
                        {
                            Logger.WriteVerbose(string.Format("Stopping process \"{0}\"", proc.ProcessName));
                            proc.Kill();
                            proc.WaitForExit(1000);
                        }
                        file.Delete();
                    }
                    catch
                    {
                        Logger.WriteError(string.Format("Failed to delete file \"{0}\"", file.FullName));
                        ret = false;
                    }
                }
            }
            catch { ret = false; }

            // it's okay if we don't delete the folders
            try
            {
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    // keep the playready crypto cache folder in-place if it exists
                    if (dir.FullName.ToLower().Contains("cache")) continue;

                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    {
                        Logger.WriteError(string.Format("Failed to delete folder \"{0}\"", dir.FullName));
                    }
                }
            }
            catch { }

            return ret;
        }
        private bool tweakMediaCenterTunerCount(int count)
        {
            // create mxf file with increased tuner limits
            string[] countries = { /*"default", */"au", "be", "br", "ca", "ch", "cn", "cz", "de", "dk", "es", "fi", "fr", "gb", "hk", "hu", "ie", "in",/* "it",*/ "jp", "kr", "mx", "nl", "no", "nz", "pl",/* "pt",*/ "ru", "se", "sg", "sk",/* "tr", "tw",*/ "us", "za" };
            string xml = "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n" +
                         "<MXF version=\"1.0\" xmlns=\"\">\r\n" +
                         "  <Assembly name=\"mcstore\">\r\n" +
                         "    <NameSpace name=\"Microsoft.MediaCenter.Store\">\r\n" +
                         "      <Type name=\"StoredType\" />\r\n" +
                         "    </NameSpace>\r\n" +
                         "  </Assembly>\r\n" +
                         "  <Assembly name=\"ehshell\">\r\n" +
                         "    <NameSpace name=\"ServiceBus.UIFramework\">\r\n" +
                         "      <Type name=\"TvSignalSetupParams\" />\r\n" +
                         "    </NameSpace>\r\n" +
                         "  </Assembly>\r\n";
            xml += string.Format("  <With maxRecordersForHomePremium=\"{0}\" maxRecordersForUltimate=\"{0}\" maxRecordersForRacing=\"{0}\" maxRecordersForBusiness=\"{0}\" maxRecordersForEnterprise=\"{0}\" maxRecordersForOthers=\"{0}\">\r\n", count);

            foreach (string country in countries)
            {
                xml += string.Format("    <TvSignalSetupParams uid=\"tvss-{0}\" />\r\n", country);
            }

            xml += "  </With>\r\n";
            xml += "</MXF>";

            // create temporary file
            string mxfFilepath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mxf");
            using (StreamWriter writer = new StreamWriter(mxfFilepath, false))
            {
                writer.Write(xml);
            }

            // import tweak using loadmxf.exe because for some reason the MxfImporter doesn't work for this
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\loadmxf.exe"),
                Arguments = string.Format("-i \"{0}\"", mxfFilepath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startInfo))
            {
                process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);
            }

            // delete temporary file
            File.Delete(mxfFilepath);

            return true;
        }
        #endregion

        #region ========== Hauppauge HD PVR ==========
        List<string> hdpvrTuners = new List<string>();
        private bool isColossusInstalled()
        {
            // check registry for 32-bit and 64-bit
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Hauppauge\\HcwDevCentral\\Devices", false);
            if (key == null)
            {
                key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Hauppauge\\HcwDevCentral\\Devices", false);
            }
            if (key == null) return false;

            // find all the software tuners and store in array
            foreach (string subkey in key.GetSubKeyNames())
            {
                RegistryKey device = key.OpenSubKey(subkey);
                if ((string)device.GetValue("FriendlyName") == "Hauppauge HD PVR Software Tuner")
                {
                    hdpvrTuners.Add(subkey);
                }
                device.Close();
            }
            key.Close();

            return true;
        }
        private bool configureHdPvrTuners()
        {
            bool success = true;
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Hauppauge\MediaCenterService";
            foreach (string tuner in hdpvrTuners.ToArray())
            {
                updateStatusText("Creating HD PVR tuner files ...");
                try
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string file = string.Format("{0}\\{1}.xml", folder, tuner);
                    FileAttributes fa = new FileAttributes();

                    // remove possible read-only attribute
                    if (File.Exists(file))
                    {
                        fa = File.GetAttributes(file);
                        File.SetAttributes(file, fa & ~FileAttributes.ReadOnly);
                    }

                    using (StreamWriter stream = new StreamWriter(file))
                    {
                        // write header
                        stream.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        stream.WriteLine("<Channels>");
                        stream.WriteLine("  <Lineup epg123=\"true\">EPG123 Channels for HD PVR Software Tuner</Lineup>");
                        stream.WriteLine("  <tune:Tuning xmlns:tune=\"http://schemas.microsoft.com/2006/eHome/MediaCenter/Tuning.xsd\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://schemas.microsoft.com/2006/eHome/MediaCenter/Tuning.xsd file://tuning.xsd\" >");

                        // write body
                        for (int i = 1; i < 10000; ++i)
                        {
                            stream.WriteLine(string.Format("    <Idx value=\"{0}\" />", i));
                            stream.WriteLine(string.Format("    <tune:ChannelID ChannelID=\"{0}\">", i));
                            stream.WriteLine("      <tune:TuningSpace xsi:type=\"tune:ChannelIDTuningSpaceType\" Name=\"DC65AA02-5CB0-4d6d-A020-68702A5B34B8\" NetworkType=\"{DC65AA02-5CB0-4d6d-A020-68702A5B34B8}\" />");
                            stream.WriteLine("      <tune:Locator xsi:type=\"tune:ATSCLocatorType\" Frequency=\"-1\" PhysicalChannel=\"-1\" TransportStreamID=\"-1\" ProgramNumber=\"1\" />");
                            stream.WriteLine("      <tune:Components xsi:type=\"tune:ComponentsType\" />");
                            stream.WriteLine("    </tune:ChannelID>");
                            stream.WriteLine(string.Format("    <ps:Service xmlns:ps=\"http://schemas.microsoft.com/2006/eHome/MediaCenter/PBDAService.xsd\" name=\"\" callSign=\"Z{0}\" ppv=\"false\" subscribed=\"true\" codec=\"H.264\" channel=\"{0}\" matchname=\"\"></ps:Service>", i));
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
                    MessageBox.Show(ex.Message, "IO Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                }
                updateStatusText(string.Empty);
            }

            if (!success)
            {
                MessageBox.Show("Failed to configure the HD PVR/Colossus software tuner(s).", "Failed Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return success;
        }
        #endregion

        #region ========== Step 2 WMC TV Setup ==========
        private bool openWmc()
        {
            updateStatusText("Opening WMC for TV Setup ...");
            if (!procWmc.HasExited)
            {
                // show wmc window
                ShowWindow(wmcPtr, SW_SHOWNORMAL);
            }
            else
            {
                // open wmc to setup tuners
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.EhshellExeFilePath,
                    Arguments = "/nostartupanimation"
                };
                procWmc = Process.Start(startInfo);
            }
            updateStatusText("Waiting for user to close WMC ...");

            do
            {
                Thread.Sleep(100);
                Application.DoEvents();
            } while (!procWmc.HasExited);
            updateStatusText(string.Empty);

            return true;
        }
        private bool activateGuide()
        {
            bool ret = false;
            updateStatusText("Activating guide in registry ...");
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true);
            if (key != null)
            {
                try
                {
                    if ((int)key.GetValue("fAgreeTOS") != 1) key.SetValue("fAgreeTOS", 1);
                    if ((string)key.GetValue("strAgreeTOSVersion") != "1.0") key.SetValue("strAgreedTOSVersion", "1.0");
                    key.Close();
                }
                catch
                {
                    MessageBox.Show("Failed to open/edit registry to show the Guide in WMC.", "Registry Access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    key.Close();
                }
                ret = true;
            }
            updateStatusText(string.Empty);

            return ret;
        }
        private bool disableBackgroundScanning()
        {
            bool ret = false;
            updateStatusText("Disabling background scanner ...");
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\BackgroundScanner", true);
            if (key != null)
            {
                try
                {
                    if ((key.GetValue("PeriodicScanEnabled") == null) || ((int)key.GetValue("PeriodicScanEnabled") != 0))
                    {
                        key.SetValue("PeriodicScanEnabled", 0, RegistryValueKind.DWord);
                    }
                    if ((key.GetValue("PeriodicScanIntervalSeconds") == null) || ((int)key.GetValue("PeriodicScanIntervalSeconds") != 0x7FFFFFFF))
                    {
                        key.SetValue("PeriodicScanIntervalSeconds", 0x7FFFFFFF, RegistryValueKind.DWord);
                    }
                    key.Close();
                    ret = true;
                }
                catch
                {
                    MessageBox.Show("Failed to open/edit registry to disable periodic tuner background scanning.", "Registry Access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    key.Close();
                }
            }
            updateStatusText(string.Empty);

            return ret;
        }
        #endregion

        #region ========== EPG123 Configurator ==========
        private bool openEpg123Configuration()
        {
            updateStatusText("Opening EPG123 Configuration GUI ...");
            Process procEpg123;
            if (epg123Running())
            {
                Process[] processes = Process.GetProcessesByName("epg123");
                procEpg123 = processes[0];
                if (IsIconic(procEpg123.MainWindowHandle))
                {
                    ShowWindow(procEpg123.MainWindowHandle, SW_RESTORE);
                }
                SetForegroundWindow(procEpg123.MainWindowHandle);
            }
            else
            {
                // start epg123 configuration GUI
                procEpg123 = Process.Start(Helper.Epg123ExePath);
                procEpg123.WaitForInputIdle(10000);
            }
            updateStatusText("Waiting for EPG123 to close ...");

            do
            {
                Thread.Sleep(100);
                Application.DoEvents();
            } while (!procEpg123.HasExited);
            updateStatusText(string.Empty);

            return true;
        }
        private bool epg123Running()
        {
            return (Process.GetProcessesByName("epg123").Length > 0);
        }
        #endregion
    }
}