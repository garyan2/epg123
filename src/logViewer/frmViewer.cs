using GaRyan2.Utilities;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace logViewer
{
    public partial class frmViewer : Form
    {
        private long streamLocation;
        private readonly string _filename;
        private string _lastPath;
        private int _lines;

        public frmViewer(string filename = null)
        {
            _filename = filename;
            InitializeComponent();

            // copy over window size and location from previous version if needed
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            // restore window position and size
            if ((Properties.Settings.Default.WindowLocation != new Point(-1, -1)))
            {
                Location = Properties.Settings.Default.WindowLocation;
            }

            Size = Properties.Settings.Default.WindowSize;
            if (Properties.Settings.Default.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            richTextBox1.ZoomFactor = Properties.Settings.Default.ZoomFactor;
        }

        private void OpenLogFileAndDisplay(string logFile)
        {
            this.Cursor = Cursors.WaitCursor;
            richTextBox1.Hide();
            var zoom = richTextBox1.ZoomFactor;
            richTextBox1.Clear();
            richTextBox1.ZoomFactor = 1.0f;
            richTextBox1.ZoomFactor = zoom;
            streamLocation = _lines = 0;
            DisplayLogFile(logFile);
            richTextBox1.Show();
            this.Cursor = Cursors.Default;
        }

        private void DisplayLogFile(string logFile)
        {
            this.Text = $"EPG123 Log Viewer - {logFile}";
            if (logFile.StartsWith("http"))
            {
                using (var wc = new WebClient())
                {
                    var log = wc.DownloadString(logFile);
                    using (var sr = new StringReader(log))
                    {
                        string line = null;
                        do
                        {
                            line = sr.ReadLine();
                            if (line == null) break;
                            if (line.Length < 2) continue;
                            AddLineOfText(line);
                            ++_lines;
                        }
                        while (true);

                        streamLocation = log.Length;
                    }
                }
            }
            else
            {
                var fi = new FileInfo(logFile);
                _lastPath = fileSystemWatcher1.Path = fi.DirectoryName;
                fileSystemWatcher1.Filter = fi.Name;

                try
                {
                    using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs))
                    {
                        if (fs.Length < streamLocation) streamLocation = 0;
                        else fs.Position = streamLocation;

                        // read the line
                        string line = null;
                        do
                        {
                            line = sr.ReadLine();
                            if (line == null) break;
                            if (line.Length < 2) continue;
                            AddLineOfText(line);
                            ++_lines;
                        }
                        while (line != null);

                        streamLocation = fs.Position;
                    }
                }
                catch { }
            }
            UpdateStatusBar();
        }

        private void AddLineOfText(string line)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            if (line.Contains("[ERROR]"))
            {
                richTextBox1.SelectionColor = Color.Red;
            }
            else if (line.Contains("[WARNG]") || line.ToLower().Contains("failed") || line.Contains("SD API WebException") || line.Contains("exception thrown") || line.Contains("SD responded") || line.Contains("Did not receive") || line.Contains("Problem occurred") || line.Contains("*****") || line.Contains("no tuners"))
            {
                richTextBox1.SelectionColor = Color.Yellow;
            }
            else if (line.Contains("==========") || line.Contains("Activating the") || line.Contains("Beginning"))
            {
                richTextBox1.SelectionColor = Color.White;
            }
            else if (line.Contains("Entering") || line.Contains("Exiting"))
            {
                richTextBox1.SelectionColor = Color.Cyan;
            }
            else
            {
                richTextBox1.SelectionColor = Color.ForestGreen;
            }
            richTextBox1.AppendText($"{line}\r\n");
        }

        private void UpdateStatusBar()
        {
            toolStripStatusLabel1.Text = $"[ length : {streamLocation:N0}  lines : {_lines} ] [ zoom : {(100 * richTextBox1.ZoomFactor):N0}% ]";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectedText.Length == 0) richTextBox1.SelectAll();
            if (richTextBox1.SelectedText.Length > 0)
            {
                Clipboard.SetText(richTextBox1.SelectedText);
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            DisplayLogFile($"{fileSystemWatcher1.Path}\\{fileSystemWatcher1.Filter}");
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog()
            {
                Filter = "Log File|*.log",
                Title = "Select a log file to view",
                Multiselect = false,
                InitialDirectory = _lastPath
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenLogFileAndDisplay(openFileDialog1.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // save the windows size and location
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowLocation = Location;
                Properties.Settings.Default.WindowSize = Size;
            }
            else
            {
                Properties.Settings.Default.WindowLocation = RestoreBounds.Location;
                Properties.Settings.Default.WindowSize = RestoreBounds.Size;
            }
            Properties.Settings.Default.WindowMaximized = (WindowState == FormWindowState.Maximized);
            Properties.Settings.Default.ZoomFactor = richTextBox1.ZoomFactor;
            Properties.Settings.Default.Save();
        }

        private void frmViewer_Shown(object sender, EventArgs e)
        {
            this.Refresh();
            OpenLogFileAndDisplay(_filename ?? Helper.Epg123TraceLogPath);
        }

        private void richTextBox1_Resize(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && (e.KeyCode == Keys.Add || e.KeyCode == Keys.Subtract || e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.OemMinus))
            {
                var dir = 1;
                if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) dir = -1;
                var zoom = Math.Min(Math.Max(richTextBox1.ZoomFactor + dir * 0.10f, 0.1f), 63.9f);
                if (zoom >= 0.91 && zoom <= 1.09) zoom = 1.0f;
                richTextBox1.ZoomFactor = zoom;
                richTextBox1.ScrollToCaret();
                e.Handled = true;
                UpdateStatusBar();
            }
            else if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.Divide)
            {
                richTextBox1.ZoomFactor = 1.0f;
                richTextBox1.ScrollToCaret();
                e.Handled = true;
                UpdateStatusBar();
            }
        }
    }

    class myRichTextBox : RichTextBox
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        //this message is sent to the control when we scroll using the mouse
        private const int MK_CONTROL = 0x0008;
        private const int WM_MOUSEWHEEL = 0x20A;

        //and this one issues the control to perform scrolling
        private const int WM_VSCROLL = 0x115;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                if (((int)(long)m.WParam & MK_CONTROL) == MK_CONTROL)
                {
                    if ((int)(long)m.WParam > 0) SendKeys.Send("{ADD}");
                    else SendKeys.Send("{SUBTRACT}");
                    return;
                }

                int scrollLines = SystemInformation.MouseWheelScrollLines;
                for (int i = 0; i < scrollLines; i++)
                {
                    if ((int)(long)m.WParam > 0) // when wParam is greater than 0
                        SendMessage(this.Handle, WM_VSCROLL, (IntPtr)0, IntPtr.Zero); // scroll up 
                    else
                        SendMessage(this.Handle, WM_VSCROLL, (IntPtr)1, IntPtr.Zero); // else scroll down
                }
                return;
            }
            base.WndProc(ref m);
        }
    }
}