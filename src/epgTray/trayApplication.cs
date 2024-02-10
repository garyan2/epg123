using epgTray.Properties;
using GaRyan2.Utilities;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace epgTray
{
    class trayApplication : ApplicationContext
    {
        //Component declarations
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayIconContextMenu;
        private ToolStripMenuItem _closeMenuItem;
        private ToolStripMenuItem _openConfigMenuItem;
        private ToolStripMenuItem _openClientMenuItem;
        private ToolStripMenuItem _openTransferToolMenuItem;
        private ToolStripMenuItem _viewLogMenuItem;
        private ToolStripMenuItem _runUpdateMenuItem;
        private ToolStripMenuItem _notificationMenuItem;
        private ToolStripMenuItem _gotoDownloadMenuItem;
        private Timer _timer;
        public bool Shutdown;
        private readonly Thread _serverThread;
        private int _lastStatus;
        private DateTime _nextUpdate = DateTime.MinValue;
        private readonly string _version = $"EPG123 v{Helper.Epg123Version}";

        public trayApplication()
        {
            Application.ApplicationExit += OnApplicationExit;
            _timer = new Timer(TimerEvent);
            InitializeComponent();

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            try
            {
                _serverThread = new Thread(PipeServer);
                _serverThread.Start();
            }
            catch
            {
                Application.Exit();
            }
            _trayIcon.Visible = true;

            if (_lastStatus != 0xDEAD) return;
            _trayIcon.BalloonTipIcon = ToolTipIcon.Error;
            _trayIcon.BalloonTipTitle = _version;
            _trayIcon.BalloonTipText = "There was a problem updating your WMC program guide. View the log file for details.";
            _trayIcon.ShowBalloonTip(10000);
        }

        private void InitializeComponent()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = CurrentStatusImage();

            //Optional - Add a context menu to the TrayIcon:
            _trayIconContextMenu = new ContextMenuStrip();
            _closeMenuItem = new ToolStripMenuItem();
            _openConfigMenuItem = new ToolStripMenuItem();
            _openClientMenuItem = new ToolStripMenuItem();
            _openTransferToolMenuItem = new ToolStripMenuItem();
            _viewLogMenuItem = new ToolStripMenuItem();
            _runUpdateMenuItem = new ToolStripMenuItem();
            _notificationMenuItem = new ToolStripMenuItem();
            _gotoDownloadMenuItem = new ToolStripMenuItem();
            _trayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            _trayIconContextMenu.Items.AddRange(new ToolStripItem[] {
            _runUpdateMenuItem, _notificationMenuItem, new ToolStripSeparator(),
                _openConfigMenuItem, _openClientMenuItem, _openTransferToolMenuItem, _viewLogMenuItem, new ToolStripSeparator(),
                _gotoDownloadMenuItem, _closeMenuItem});
            _trayIconContextMenu.Name = "_trayIconContextMenu";
            // 
            // CloseMenuItem
            // 
            _closeMenuItem.Name = "_closeMenuItem";
            _closeMenuItem.Text = "Exit";
            _closeMenuItem.Click += CloseMenuItem_Click;
            // 
            // OpenClientMenuItem
            // 
            _openClientMenuItem.Name = "_openClientMenuItem";
            _openClientMenuItem.Text = "Open Client GUI";
            _openClientMenuItem.Enabled = File.Exists(Helper.Epg123ClientExePath);
            _openClientMenuItem.Click += OpenFileMenuItem_Click;
            // 
            // OpenConfigMenuItem
            // 
            _openConfigMenuItem.Name = "_openConfigMenuItem";
            _openConfigMenuItem.Text = "Open Configuration GUI";
            _openConfigMenuItem.Enabled = File.Exists(Helper.Epg123GuiPath);
            _openConfigMenuItem.Click += OpenFileMenuItem_Click;
            //
            // OpenTransferToolMenuItem
            //
            _openTransferToolMenuItem.Name = "_openTransferToolMenuItem";
            _openTransferToolMenuItem.Text = "Open Transfer Tool";
            _openTransferToolMenuItem.Enabled = File.Exists(Helper.Epg123TransferExePath);
            _openTransferToolMenuItem.Click += OpenFileMenuItem_Click;
            // 
            // ViewLogMenuItem
            // 
            _viewLogMenuItem.Name = "_viewLogMenuItem";
            _viewLogMenuItem.Text = "View Log File";
            _viewLogMenuItem.Click += OpenFileMenuItem_Click;
            //
            // RunUpdateMenuItem
            //
            _runUpdateMenuItem.Name = "_runUpdateMenuItem";
            _runUpdateMenuItem.Text = "Update Guide Now";
            _runUpdateMenuItem.Enabled = File.Exists(Helper.Epg123ClientExePath);
            _runUpdateMenuItem.Click += RunUpdateMenuItem_Click;
            //
            // NotificationMenuItem
            //
            _notificationMenuItem.Name = "_notificationMenuItem";
            _notificationMenuItem.Text = "Notify on ERRORs only";
            _notificationMenuItem.CheckState = Settings.Default.errorOnly ? CheckState.Checked : CheckState.Unchecked;
            _notificationMenuItem.Click += NotificationMenuItemOnClick;
            //
            // GotoDownload
            //
            _gotoDownloadMenuItem.Name = "_gotoDownloadMenuItem";
            _gotoDownloadMenuItem.Text = "Download/Donate webpage";
            _gotoDownloadMenuItem.Click += OpenFileMenuItem_Click;

            _trayIconContextMenu.ResumeLayout(false);
            _trayIcon.ContextMenuStrip = _trayIconContextMenu;
        }

        private void NotificationMenuItemOnClick(object sender, EventArgs e)
        {
            Settings.Default.errorOnly = !Settings.Default.errorOnly;
            Settings.Default.Save();
            _notificationMenuItem.CheckState = Settings.Default.errorOnly ? CheckState.Checked : CheckState.Unchecked;
            _trayIconContextMenu.Show();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            _trayIcon.Visible = false;
        }

        private Icon CurrentStatusImage()
        {
            var unknown = DateTime.Parse("1970-01-01T00:00:00");
            var lastTime = unknown;
            _lastStatus = 0;

            // read datetime and status of last update run
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\Epg", false))
                {
                    if (key != null)
                    {
                        lastTime = DateTime.Parse((string)key.GetValue("epg123LastUpdateTime", lastTime.ToString("s")));
                        _lastStatus = (int)key.GetValue("epg123LastUpdateStatus", 0);
                    }
                }
            }
            catch
            {
                // ignored
            }

            // establish current status icon
            if (lastTime == unknown)
            {
                SetNotifyIconText($"{_version}\nLast guide update status is unknown.\n{DateTime.Now}");
                return Resources.statusUnknown;
            }
            if (DateTime.Now - lastTime > TimeSpan.FromHours(24.0))
            {
                SetNotifyIconText($"{_version}\nLast update was more than 24 hrs ago.\n{lastTime}");
                return Resources.statusError;
            }

            var statusIcon = Resources.statusOK;
            switch (_lastStatus)
            {
                case 0xDEAD:
                    statusIcon = Resources.statusError;
                    SetNotifyIconText($"{_version}\nThere was an error during last update.\n{lastTime}");
                    break;
                case 0xBAD1:
                    statusIcon = Resources.statusWarning;
                    SetNotifyIconText($"{_version}\nThere was a warning during last update.\n{lastTime}");
                    break;
                case 0x0001:
                    SetNotifyIconText($"{_version}\nThere is an update available.\n{lastTime}");
                    break;
                default:
                    SetNotifyIconText($"{_version}\nLast update was successful.\n{lastTime}");
                    break;
            }

            _nextUpdate = lastTime + TimeSpan.FromHours(24);
            _ = _timer.Change(30000, 30000);

            // set and display icon
            return statusIcon;
        }

        private void SetNotifyIconText(string text)
        {
            var t = typeof(NotifyIcon);
            const BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(_trayIcon, text);
            if ((bool)t.GetField("added", hidden).GetValue(_trayIcon))
            {
                t.GetMethod("UpdateIcon", hidden).Invoke(_trayIcon, new object[] { true });
            }
        }

        private void TimerEvent(object state)
        {
            if (_lastStatus != 0xDEAD && _nextUpdate < DateTime.Now)
            {
                _trayIcon.Icon = CurrentStatusImage();
            }
        }

        private void PipeServer()
        {
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
            var server = new NamedPipeServerStream("Epg123StatusPipe", PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.None, 0, 0, pipeSecurity);
            var reader = new StreamReader(server);

            while (!Shutdown)
            {
                try
                {
                    server.WaitForConnection();

                    // adjust timer to 2 minutes from now for each pipe message to avoid crashing the notification tray
                    _nextUpdate = DateTime.Now + TimeSpan.FromMinutes(2);

                    var line = reader.ReadLine();
                    if (line.StartsWith("Downloading"))
                    {
                        _nextUpdate = DateTime.Now + TimeSpan.FromMinutes(5);
                        _trayIcon.Icon = Resources.statusUpdating;
                        SetNotifyIconText(line.Replace("|", "\n"));
                    }
                    else if (line.StartsWith("Download Complete"))
                    {
                        _nextUpdate = DateTime.Now + TimeSpan.FromSeconds(10);
                    }
                    else if (line.StartsWith("Importing"))
                    {
                        if (line.Contains("Performing garbage cleanup...")) _nextUpdate = DateTime.Now + TimeSpan.FromMinutes(60);
                        _trayIcon.Icon = Resources.statusUpdating;
                        SetNotifyIconText(line.Replace("|", "\n"));
                    }
                    else if (line.StartsWith("Import Complete"))
                    {
                        _trayIcon.Icon = CurrentStatusImage();
                        _trayIcon.BalloonTipTitle = $"{_version}";
                        switch (_lastStatus)
                        {
                            case 0xDEAD:
                                _trayIcon.BalloonTipIcon = ToolTipIcon.Error;
                                _trayIcon.BalloonTipText = "There was an error in updating your WMC program guide. View the log file for details.";
                                break;
                            case 0xBAD1:
                                _trayIcon.BalloonTipIcon = ToolTipIcon.Warning;
                                _trayIcon.BalloonTipText = "There was a warning flagged while updating your WMC program guide. View the log file for details.";
                                break;
                            case 1:
                                _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                                _trayIcon.BalloonTipText = "Your WMC program guide has successfully been updated. An update is available from http://garyan2.github.io/download.html.";
                                break;
                            default:
                                _trayIcon.BalloonTipIcon = ToolTipIcon.None;
                                _trayIcon.BalloonTipText = "Your WMC program guide has successfully been updated.";
                                break;
                        }
                        if (!Settings.Default.errorOnly || _lastStatus == 0xDEAD) _trayIcon.ShowBalloonTip(10000);
                    }
                    else if (line.StartsWith("Shutdown"))
                    {
                        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                        Shutdown = true;
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    server.WaitForPipeDrain();
                    if (server.IsConnected) { server.Disconnect(); }
                }
            }
        }

        private void OpenFileMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            string filePath;
            switch (menuItem.Name)
            {
                case "_openConfigMenuItem":
                    filePath = Helper.Epg123GuiPath;
                    break;
                case "_openClientMenuItem":
                    filePath = Helper.Epg123ClientExePath;
                    break;
                case "_openTransferToolMenuItem":
                    filePath = Helper.Epg123TransferExePath;
                    break;
                case "_viewLogMenuItem":
                    Helper.ViewLogFile();
                    return;
                case "_gotoDownloadMenuItem":
                    filePath = "https://garyan2.github.io/download.html";
                    break;
                default:
                    return;
            }
            Process.Start(filePath);
        }

        private void RunUpdateMenuItem_Click(object sender, EventArgs e)
        {
            var nogcFile = File.Create($"{Helper.Epg123ProgramDataFolder}nogc.txt");
            nogcFile.Close();

            var startInfo = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/run /tn \"epg123_update\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // begin update
            var proc = Process.Start(startInfo);
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                MessageBox.Show("There was a problem starting the scheduled task to update the guide. Does the task exist?", "Error Starting Task", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (_serverThread.IsAlive)
            {
                using (var endClient = new NamedPipeClientStream(".", "Epg123StatusPipe"))
                {
                    try
                    {
                        var streamWriter = new StreamWriter(endClient);
                        endClient.Connect(100);
                        streamWriter.WriteLine("Shutdown");
                        streamWriter.Flush();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                _serverThread.Join();
            }

            Application.Exit();
        }
    }
}
