using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace logViewer
{
    public partial class frmViewer : Form
    {
        private Color DEFAULT_COLOR;

        private long streamLocation;
        private readonly string _filename;
        private string _lastPath;

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
            DEFAULT_COLOR = richTextBox1.ForeColor;
        }

        private void OpenLogFileAndDisplay(string logFile)
        {
            this.Cursor = Cursors.WaitCursor;
            richTextBox1.Visible = false;
            richTextBox1.Enabled = false;

            var zoom = richTextBox1.ZoomFactor;
            richTextBox1.Clear();
            richTextBox1.ZoomFactor = 1.0f;
            richTextBox1.ZoomFactor = zoom;
            streamLocation = 0;

            if (backgroundWorker1.IsBusy) backgroundWorker1.CancelAsync();
            while (backgroundWorker1.IsBusy) Application.DoEvents();

            DisplayLogFile(logFile);
        }

        private void DisplayLogFile(string logFile)
        {
            this.Text = $"EPG123 Log Viewer - {logFile}";
            if (logFile.StartsWith("http"))
            {
                using (var wc = new WebClient())
                {
                    var log = wc.DownloadString(logFile);
                    if (string.IsNullOrEmpty(log)) return;

                    richTextBox1.Text = log;
                    backgroundWorker1.RunWorkerAsync(richTextBox1.Lines);

                    streamLocation = log.Length;
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

                        if (streamLocation == 0)
                        {
                            richTextBox1.Text = sr.ReadToEnd();
                            backgroundWorker1.RunWorkerAsync(richTextBox1.Lines);
                        }
                        else
                        {
                            string line = null;
                            do
                            {
                                line = sr.ReadLine();
                                if (line == null) break;
                                if (line.Length < 2) continue;
                                AddLineOfText(line);
                            } while (true);
                        }

                        streamLocation = fs.Position;
                    }
                }
                catch { }
            }
            UpdateStatusBar();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var lines = (string[])e.Argument;
            for (int i = 0; i < lines.Length; i++)
            {
                var color = GetLineColor(lines[i]);
                if (color != DEFAULT_COLOR)
                {
                    SetLineTextColor(i, color);
                }
                if (backgroundWorker1.CancellationPending) break;
            }
            e.Cancel = true;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.Enabled = true;
            richTextBox1.Visible = true;
            this.Cursor = Cursors.Default;
        }

        private Color GetLineColor(string line)
        {
            if (line.Contains("ACTION:")) 
                return Color.Orange; // action
            if (line.Contains("[ERROR]")) 
                return Color.Red; // error
            if (line.Contains("[WARNG]") || 
                line.ToLower().Contains("failed") || 
                line.Contains("exception") ||
                line.Contains("Did not receive") ||
                line.Contains("error code") ||
                line.Contains("*****") ||
                line.Contains("no tuners")) 
                return Color.Yellow; // warning
            if (line.Contains("==========") ||
                line.Contains("Beginning") || 
                line.Contains("Activating"))
                return Color.White; // separator
            if (line.Contains("Entering") || 
                line.Contains("Exiting")) 
                return Color.Cyan; // message
            return DEFAULT_COLOR; // default
        }

        private void SetLineTextColor(int line, Color color)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new MethodInvoker(() => { SetLineTextColor(line, color); }));
            }
            else
            {
                int start = richTextBox1.GetFirstCharIndexFromLine(line);
                int end = richTextBox1.GetFirstCharIndexFromLine(line + 1);
                if (end < start) end = richTextBox1.TextLength - 1;

                richTextBox1.Select(start, end - start);
                richTextBox1.SelectionColor = color;
            }
        }

        private void AddLineOfText(string line)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionColor = GetLineColor(line);
            richTextBox1.AppendText($"{line}\r\n");
        }

        private void UpdateStatusBar()
        {
            toolStripStatusLabel1.Text = $"[ length : {streamLocation:N0}  lines : {richTextBox1.Lines.Length} ] [ zoom : {(100 * richTextBox1.ZoomFactor):N0}% ]";
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
            richTextBox1.SelectionStart = richTextBox1.TextLength;
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

        private void supportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_filename?.StartsWith("http") ?? false)
            {
                using (var fs = new FileStream($"{Helper.Epg123ProgramDataFolder}\\remote.log", FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(richTextBox1.Text);
                }
            }
            else if (Helper.InstallMethod != Helper.Installation.SERVER)
            {
                MessageBox.Show("If this machine is using EPG123 in \"Client Mode\", please send the log files from the machine that functions as the server as well.");
            }

            var email = new Support();
            email.AddRecipientTo("support@garyan2.me");
            var logFiles = Directory.GetFiles(Helper.Epg123ProgramDataFolder, "*.log");
            foreach (var log in logFiles)
            {
                var dt = new FileInfo(log).LastWriteTimeUtc;
                if (DateTime.UtcNow - dt < TimeSpan.FromDays(30)) email.AddAttachment(log);
            }
            email.SendMailPopup("EPG123/HDHR2MXF EPG Support",
                "HELP! I am having problems and hope you can help me.\n" +
                "I have reviewed the log files attached and still don't know what to do.\n\n" +
                "***** Please add a description of what is not working. Feel free to add any pictures as well if it helps. *****");
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

    class Support
    {
        // Attribution: This code was copied and adapted from:
        // https://www.codeproject.com/Articles/17561/Programmatically-adding-attachments-to-emails-in-C

        public bool AddRecipientTo(string email)
        {
            return AddRecipient(email, HowTo.MAPI_TO);
        }

        public bool AddRecipientCC(string email)
        {
            return AddRecipient(email, HowTo.MAPI_TO);
        }

        public bool AddRecipientBCC(string email)
        {
            return AddRecipient(email, HowTo.MAPI_TO);
        }

        public void AddAttachment(string strAttachmentFileName)
        {
            m_attachments.Add(strAttachmentFileName);
        }

        public int SendMailPopup(string strSubject, string strBody)
        {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG);
        }

        public int SendMailDirect(string strSubject, string strBody)
        {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI);
        }


        [DllImport("MAPI32.DLL")]
        static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

        int SendMail(string strSubject, string strBody, int how)
        {
            MapiMessage msg = new MapiMessage
            {
                subject = strSubject,
                noteText = strBody
            };

            msg.recips = GetRecipients(out msg.recipCount);
            msg.files = GetAttachments(out msg.fileCount);

            m_lastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how, 0);
            if (m_lastError > 0)
            {
                MessageBox.Show("MAPISendMail failed! " + GetLastError(), "MAPISendMail");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{Helper.Epg123TraceLogPath}\""
                });
            }

            Cleanup(ref msg);
            return m_lastError;
        }

        bool AddRecipient(string email, HowTo howTo)
        {
            MapiRecipDesc recipient = new MapiRecipDesc();

            recipient.recipClass = (int)howTo;
            recipient.name = email;
            m_recipients.Add(recipient);

            return true;
        }

        IntPtr GetRecipients(out int recipCount)
        {
            recipCount = 0;
            if (m_recipients.Count == 0)
                return IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(m_recipients.Count * size);

            long ptr = (long)intPtr;
            foreach (MapiRecipDesc mapiDesc in m_recipients)
            {
                Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            recipCount = m_recipients.Count;
            return intPtr;
        }

        IntPtr GetAttachments(out int fileCount)
        {
            fileCount = 0;
            if (m_attachments == null)
                return IntPtr.Zero;

            if ((m_attachments.Count <= 0) || (m_attachments.Count > maxAttachments))
                return IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(MapiFileDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(m_attachments.Count * size);

            MapiFileDesc mapiFileDesc = new MapiFileDesc();
            mapiFileDesc.position = -1;
            long ptr = (long)intPtr;

            foreach (string strAttachment in m_attachments)
            {
                mapiFileDesc.name = Path.GetFileName(strAttachment);
                mapiFileDesc.path = strAttachment;
                Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            fileCount = m_attachments.Count;
            return intPtr;
        }

        void Cleanup(ref MapiMessage msg)
        {
            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            long ptr = 0;

            if (msg.recips != IntPtr.Zero)
            {
                ptr = (long)msg.recips;
                for (int i = 0; i < msg.recipCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, typeof(MapiRecipDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.recips);
            }

            if (msg.files != IntPtr.Zero)
            {
                size = Marshal.SizeOf(typeof(MapiFileDesc));

                ptr = (long)msg.files;
                for (int i = 0; i < msg.fileCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, typeof(MapiFileDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.files);
            }

            m_recipients.Clear();
            m_attachments.Clear();
            m_lastError = 0;
        }

        public string GetLastError()
        {
            var err = (m_lastError <= 26) ? errors[m_lastError] : m_lastError.ToString();
            return $"MAPI error [{err}]\n\n" +
                    "The log viewer failed to open/send email from an email client on this machine. It will now open file explorer so you can manually select the \"trace.log\" file and the \"server.log\" file if it exists, to attach to an email using your mail client or webmail.\n\n" +
                    "Send the email to support@garyan2.me";
        }

        readonly string[] errors = new string[] {
            "OK [0]", "User abort [1]", "General MAPI failure [2]",
            "MAPI login failure [3]", "Disk full [4]",
            "Insufficient memory [5]", "Access denied [6]",
            "-unknown- [7]", "Too many sessions [8]",
            "Too many files were specified [9]",
            "Too many recipients were specified [10]",
            "A specified attachment was not found [11]",
            "Attachment open failure [12]",
            "Attachment write failure [13]", "Unknown recipient [14]",
            "Bad recipient type [15]", "No messages [16]",
            "Invalid message [17]", "Text too large [18]",
            "Invalid session [19]", "Type not supported [20]",
            "A recipient was specified ambiguously [21]",
            "Message in use [22]", "Network failure [23]",
            "Invalid edit fields [24]", "Invalid recipients [25]",
            "Not supported [26]"
        };

        List<MapiRecipDesc> m_recipients = new List<MapiRecipDesc>();
        List<string> m_attachments = new List<string>();
        int m_lastError = 0;

        const int MAPI_LOGON_UI = 0x00000001;
        const int MAPI_DIALOG = 0x00000008;
        const int maxAttachments = 20;

        enum HowTo { MAPI_ORIG = 0, MAPI_TO, MAPI_CC, MAPI_BCC };
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiMessage
    {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        public IntPtr originator;
        public int recipCount;
        public IntPtr recips;
        public int fileCount;
        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiFileDesc
    {
        public int reserved;
        public int flags;
        public int position;
        public string path;
        public string name;
        public IntPtr type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiRecipDesc
    {
        public int reserved;
        public int recipClass;
        public string name;
        public string address;
        public int eIDSize;
        public IntPtr entryID;
    }
}