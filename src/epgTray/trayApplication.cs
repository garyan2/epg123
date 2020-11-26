using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using epg123;
using Microsoft.Win32;

namespace epgTray
{
    class trayApplication : ApplicationContext
    {
        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private ToolStripMenuItem OpenConfigMenuItem;
        private ToolStripMenuItem OpenClientMenuItem;
        private ToolStripMenuItem ViewLogMenuItem;
        private ToolStripMenuItem RunUpdateMenuItem;
        private System.Threading.Timer timer;
        public bool Shutdown = false;
        private Thread serverThread;
        private int lastStatus;
        private DateTime nextUpdate = DateTime.MinValue;

        public trayApplication()
        {
            // set the base path and the working directory
            Helper.ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(Helper.ExecutablePath);

            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();

            try
            {
                serverThread = new Thread(new ThreadStart(PipeServer));
                serverThread.Start();
            }
            catch
            {
                Application.Exit();
            }
            TrayIcon.Visible = true;

            if (lastStatus == 0xDEAD)
            {
                TrayIcon.BalloonTipIcon = ToolTipIcon.Error;
                TrayIcon.BalloonTipTitle = "EPG123";
                TrayIcon.BalloonTipText = "There was a problem updating your WMC program guide. View the log file for details.";
                TrayIcon.ShowBalloonTip(10000);
            }
        }

        private void InitializeComponent()
        {
            TrayIcon = new NotifyIcon();
            TrayIcon.Icon = currentStatusImage();

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            OpenConfigMenuItem = new ToolStripMenuItem();
            OpenClientMenuItem = new ToolStripMenuItem();
            ViewLogMenuItem = new ToolStripMenuItem();
            RunUpdateMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
            this.RunUpdateMenuItem, new ToolStripSeparator(),
                this.OpenConfigMenuItem, this.OpenClientMenuItem, this.ViewLogMenuItem, new ToolStripSeparator(),
                this.CloseMenuItem});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Text = "Exit";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);
            // 
            // OpenClientMenuItem
            // 
            this.OpenClientMenuItem.Name = "OpenClientMenuItem";
            this.OpenClientMenuItem.Text = "Open Client GUI";
            this.OpenClientMenuItem.Enabled = File.Exists(Helper.Epg123ClientExePath);
            this.OpenClientMenuItem.Click += new EventHandler(this.OpenFileMenuItem_Click);
            // 
            // OpenConfigMenuItem
            // 
            this.OpenConfigMenuItem.Name = "OpenConfigMenuItem";
            this.OpenConfigMenuItem.Text = "Open Configuration GUI";
            this.OpenConfigMenuItem.Enabled = File.Exists(Helper.Epg123ExePath);
            this.OpenConfigMenuItem.Click += new EventHandler(this.OpenFileMenuItem_Click);
            // 
            // ViewLogMenuItem
            // 
            this.ViewLogMenuItem.Name = "ViewLogMenuItem";
            this.ViewLogMenuItem.Text = "View Log File";
            this.ViewLogMenuItem.Click += new EventHandler(this.OpenFileMenuItem_Click);
            //
            // RunUpdateMenuItem
            //
            this.RunUpdateMenuItem.Name = "RunUpdateMenuItem";
            this.RunUpdateMenuItem.Text = "Update Guide Now";
            this.RunUpdateMenuItem.Enabled = File.Exists(Helper.Epg123ClientExePath);
            this.RunUpdateMenuItem.Click += new EventHandler(this.RunUpdateMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private Icon currentStatusImage()
        {
            DateTime unknown = DateTime.Parse("1970-01-01T00:00:00");
            DateTime lastTime = unknown;
            lastStatus = 0;

            // read datetime and status of last update run
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\Epg", false))
                {
                    if (key != null)
                    {
                        lastTime = DateTime.Parse((string)key.GetValue("epg123LastUpdateTime", lastTime.ToString("s")));
                        lastStatus = (int)key.GetValue("epg123LastUpdateStatus", 0);
                    }
                }
            }
            catch { }

            // establish current status icon
            if (lastTime == unknown)
            {
                SetNotifyIconText($"EPG123\nLast guide update status is unknown.\n{DateTime.Now}");
                return epgTray.Properties.Resources.statusUnknown;
            }
            if (DateTime.Now - lastTime > TimeSpan.FromHours(24.0))
            {
                SetNotifyIconText($"EPG123\nLast update was more than 24 hrs ago.\n{lastTime}");
                return epgTray.Properties.Resources.statusError;
            }

            Icon statusIcon = epgTray.Properties.Resources.statusOK;
            if (lastStatus == 0xDEAD)
            {
                statusIcon = epgTray.Properties.Resources.statusError;
                SetNotifyIconText($"EPG123\nThere was an error during last update.\n{lastTime}");
            }
            else if (lastStatus == 0xBAD1)
            {
                statusIcon = epgTray.Properties.Resources.statusWarning;
                SetNotifyIconText($"EPG123\nThere was a warning during last update.\n{lastTime}");
            }
            else if (lastStatus == 0x0001)
            {
                SetNotifyIconText($"EPG123\nThere is an update available.\n{lastTime}");
            }
            else
            {
                SetNotifyIconText($"EPG123\nLast update was successful.\n{lastTime}");
            }

            nextUpdate = lastTime + TimeSpan.FromHours(24);
            timer = new System.Threading.Timer(TimerEvent);
            timer.Change(60000, 60000);

            // set and display icon
            return statusIcon;
        }

        private void SetNotifyIconText(string text)
        {
            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(TrayIcon, text);
            if ((bool)t.GetField("added", hidden).GetValue(TrayIcon))
            {
                t.GetMethod("UpdateIcon", hidden).Invoke(TrayIcon, new object[] { true });
            }
        }

        private void TimerEvent(object state)
        {
            if (lastStatus != 0xDEAD && nextUpdate < DateTime.Now)
            {
                TrayIcon.Icon = currentStatusImage();
            }
        }

        private void PipeServer()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));
            NamedPipeServerStream server = new NamedPipeServerStream("Epg123StatusPipe", PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.None, 0, 0, pipeSecurity);
            StreamReader reader = new StreamReader(server);

            while (!Shutdown)
            {
                try
                {
                    server.WaitForConnection();
                    
                    // adjust timer to 1.1 hours after each pipe message to avoid crashing the notification tray
                    // the checking for recordings in progress interval is 1 hour so need to be greater than that
                    nextUpdate = DateTime.Now + TimeSpan.FromMinutes(60.0);

                    string line = reader.ReadLine();
                    if (line.StartsWith("Downloading"))
                    {
                        TrayIcon.Icon = epgTray.Properties.Resources.statusUpdating;
                        SetNotifyIconText(line.Replace("|", "\n"));
                    }
                    else if (line.StartsWith("Download Complete"))
                    {
                        //TrayIcon.Icon = epgTray.Properties.Resources.statusUnknown;
                    }
                    else if (line.StartsWith("Importing"))
                    {
                        TrayIcon.Icon = epgTray.Properties.Resources.statusUpdating;
                        SetNotifyIconText(line.Replace("|", "\n"));
                    }
                    else if (line.StartsWith("Import Complete"))
                    {
                        TrayIcon.Icon = currentStatusImage();
                        TrayIcon.BalloonTipTitle = "EPG123";
                        if (lastStatus == 0xDEAD)
                        {
                            TrayIcon.BalloonTipIcon = ToolTipIcon.Error;
                            TrayIcon.BalloonTipText = "There was an error in updating your WMC program guide. View the log file for details.";
                        }
                        else if (lastStatus == 0xBAD1)
                        {
                            TrayIcon.BalloonTipIcon = ToolTipIcon.Warning;
                            TrayIcon.BalloonTipText = "There was a warning flagged while updating your WMC program guide. View the log file for details.";
                        }
                        else if (lastStatus == 1)
                        {
                            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
                            TrayIcon.BalloonTipText = "Your WMC program guide has successfully been updated. An update is available from http://epg123.garyan2.net/download.";
                        }
                        else
                        {
                            TrayIcon.BalloonTipIcon = ToolTipIcon.None;
                            TrayIcon.BalloonTipText = "Your WMC program guide has successfully been updated.";
                        }
                        TrayIcon.ShowBalloonTip(10000);
                    }
                    else if (line.StartsWith("Shutdown"))
                    {
                        if (timer != null)
                        {
                            timer.Change(Timeout.Infinite, Timeout.Infinite);
                        }
                        Shutdown = true;
                    }
                }
                catch { }
                finally
                {
                    server.WaitForPipeDrain();
                    if (server.IsConnected) { server.Disconnect(); }
                }
            }
        }

        private void OpenFileMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            string filePath;
            string arguments = string.Empty;
            switch (menuItem.Name)
            {
                case "OpenConfigMenuItem":
                    filePath = Helper.Epg123ExePath;
                    break;
                case "OpenClientMenuItem":
                    filePath = Helper.Epg123ClientExePath;
                    break;
                case "ViewLogMenuItem":
                    filePath = "notepad.exe";
                    arguments = Helper.Epg123TraceLogPath;
                    break;
                default:
                    return;
            }
            Process.Start(filePath, arguments);
        }

        private void RunUpdateMenuItem_Click(object sender, EventArgs e)
        {
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

            if (proc.ExitCode != 0)
            {
                MessageBox.Show("There was a problem starting the scheduled task to update the guide. Does the task exist?", "Error Starting Task", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (serverThread.IsAlive)
            {
                using (NamedPipeClientStream endClient = new NamedPipeClientStream(".", "Epg123StatusPipe"))
                {
                    try
                    {
                        StreamWriter streamWriter = new StreamWriter(endClient);
                        endClient.Connect(100);
                        streamWriter.WriteLine("Shutdown");
                        streamWriter.Flush();
                    }
                    catch { }
                }
                serverThread.Join();
            }

            Application.Exit();
        }
    }
}
