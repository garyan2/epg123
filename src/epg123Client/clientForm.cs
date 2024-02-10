using epg123Client.Properties;
using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.Store.MXF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace epg123Client
{
    public partial class clientForm : Form
    {
        #region ========== Form Opening ==========

        private readonly ListViewColumnSorter _mergedChannelColumnSorter = new ListViewColumnSorter();
        private readonly ListViewColumnSorter _lineupChannelColumnSorter = new ListViewColumnSorter();
        private readonly EpgTaskScheduler _task = new EpgTaskScheduler();
        private bool _forceExit;
        public bool RestartClientForm;
        public bool RestartAsAdmin;
        private readonly double _dpiScaleFactor = 1.0;

        public clientForm(bool betaTools)
        {
            // required to show UAC shield on buttons
            Application.EnableVisualStyles();

            // create form objects
            InitializeComponent();

            // set flag for advanced tools
            btnStoreExplorer.Visible = btnExportMxf.Visible = betaTools;

            // ensure the toolstrips are displayed in the proper order
            toolStripContainer1.SuspendLayout();
            toolStrip1.Location = new Point(0, 0);
            mergedChannelToolStrip.Location = new Point(0, toolStrip1.Height);
            toolStripContainer1.ResumeLayout();

            toolStripContainer2.SuspendLayout();
            toolStrip2.Location = new Point(0, 0);
            lineupChannelToolStrip.Location = new Point(0, toolStrip2.Height);
            toolStripContainer2.ResumeLayout();

            // adjust components for screen dpi
            using (var g = CreateGraphics())
            {
                if ((int)g.DpiX != 96 || (int)g.DpiY != 96)
                {
                    _dpiScaleFactor = g.DpiX / 96;

                    // adjust combobox widths
                    cmbSources.DropDownHeight = (int)(_dpiScaleFactor * cmbSources.DropDownHeight);
                    cmbSources.DropDownWidth = (int)(_dpiScaleFactor * cmbSources.DropDownWidth);
                    cmbSources.Size = new Size((int)(_dpiScaleFactor * cmbSources.Width), cmbSources.Size.Height);

                    cmbObjectStoreLineups.DropDownHeight = (int)(_dpiScaleFactor * cmbObjectStoreLineups.DropDownHeight);
                    cmbObjectStoreLineups.DropDownWidth = (int)(_dpiScaleFactor * cmbObjectStoreLineups.DropDownWidth);
                    cmbObjectStoreLineups.Size = new Size((int)(_dpiScaleFactor * cmbObjectStoreLineups.Width), cmbObjectStoreLineups.Size.Height);
                }
            }

            mergedChannelToolStrip.ImageScalingSize = new Size((int)(_dpiScaleFactor * 16), (int)(_dpiScaleFactor * 16));
            lineupChannelToolStrip.ImageScalingSize = new Size((int)(_dpiScaleFactor * 16), (int)(_dpiScaleFactor * 16));
        }

        private void clientForm_Load(object sender, EventArgs e)
        {
            // copy over window size and location from previous version if needed
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            // enable/disable the transfer tool button
            btnTransferTool.Enabled = File.Exists(Helper.Epg123TransferExePath);

            // set the split container distances
            splitContainer2.Panel1MinSize = (int)(splitContainer2.Panel1MinSize * _dpiScaleFactor);
            splitContainer1.Panel1MinSize = grpScheduledTask.Width + 6;

            // restore window position and sizes
            if (Settings.Default.WindowLocation != new Point(-1, -1))
            {
                Location = Settings.Default.WindowLocation;
            }

            Size = Settings.Default.WindowSize;
            if (Settings.Default.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            if (Settings.Default.SplitterDistance != 0)
            {
                splitContainer1.SplitterDistance = Settings.Default.SplitterDistance;
            }

            // throw the version number into the title
            Text += $" v{Helper.Epg123Version}{(Logger.Status == 1 ? " (UPDATE AVAILABLE)" : "")}";
        }

        private void clientForm_Shown(object sender, EventArgs e)
        {
            // flag initial form load building
            Application.UseWaitCursor = true;

            // update task panel
            UpdateTaskPanel();

            // if client was started as elevated to perform an action
            if (Helper.UserHasElevatedRights && File.Exists(Helper.EButtonPath))
            {
                Application.UseWaitCursor = false;
                using (var sr = new StreamReader(Helper.EButtonPath))
                {
                    var line = sr.ReadToEnd();
                    if (line.Contains("setup")) btnSetup_Click(null, null);
                    else if (line.Contains("restore")) btnRestore_Click(null, null);
                    else if (line.Contains("rebuild"))
                    {
                        btnRebuild_Click(null, null);
                        Application.UseWaitCursor = true;
                    }
                    else if (line.Contains("createTask") || line.Contains("deleteTask"))
                    {
                        btnTask_Click(null, null);
                        Application.UseWaitCursor = true;
                    }
                }
                Helper.DeleteFile(Helper.EButtonPath);
            }

            // double buffer list views
            mergedChannelListView.DoubleBuffered(true);
            lineupChannelListView.DoubleBuffered(true);

            // populate listviews
            if (WmcStore.WmcObjectStore != null)
            {
                mergedChannelListView.Refresh();
                lineupChannelListView.Refresh();

                BuildLineupChannelListView();
                BuildScannedLineupComboBox();
                BuildMergedChannelListView();
                btnImport.Enabled = true;
            }
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Application.UseWaitCursor = false;
        }
        #endregion

        #region ========== Form Closing and Restarting ==========
        private void RestartClient(bool forceElevated = false)
        {
            try
            {
                // save the windows size and locations
                SaveFormWindowParameters();

                // set flags
                RestartClientForm = true;
                RestartAsAdmin = Helper.UserHasElevatedRights || forceElevated;

                // close this process
                _forceExit = true;
                Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
        }

        private void clientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // check to see if any EPG123 lineups are mapped in the database
            if (!_forceExit)
            {
                if (cmbObjectStoreLineups.Items.Count > 0 && cmbObjectStoreLineups.FindString("EPG123") > -1 && mergedChannelListView.Items.Count > 0)
                {
                    if (!_allMergedChannels.Any(mergedChannel => mergedChannel != null && mergedChannel.SubItems[3].Text.StartsWith("EPG123")))
                    {
                        if (DialogResult.No == MessageBox.Show("It does not appear any EPG123 lineup service guide listings (right side) are associated with any guide channels (left side). You can manually \"Subscribe\" listings to channels or you can use the automated Match by: [# Number] button.\n\nDo you still wish to exit the Client Guide Tool?", "No EPG123 guide listings in WMC", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }

            // save the windows size and locations
            SaveFormWindowParameters();
            IsolateEpgDatabase();
        }

        private void SaveFormWindowParameters()
        {
            try
            {
                if (WindowState == FormWindowState.Normal)
                {
                    Settings.Default.WindowLocation = Location;
                    Settings.Default.WindowSize = Size;
                }
                else
                {
                    Settings.Default.WindowLocation = RestoreBounds.Location;
                    Settings.Default.WindowSize = RestoreBounds.Size;
                }
                Settings.Default.WindowMaximized = WindowState == FormWindowState.Maximized;
                Settings.Default.SplitterDistance = splitContainer1.SplitterDistance;
                Settings.Default.Save();
            }
            catch
            {
                // ignored
            }
        }

        private void IsolateEpgDatabase()
        {
            // clear the merged channels and lineup combobox
            mergedChannelListView.BeginUpdate();
            cmbSources.Items.Clear();
            mergedChannelListView.VirtualListSize = 0;
            _mergedChannelFilter.Clear();
            foreach (var lvi in _allMergedChannels)
            {
                lvi.RemoveDelegate();
            }
            _allMergedChannels?.Clear();
            mergedChannelListView.Items.Clear();
            mergedChannelListView.EndUpdate();

            // clear the lineups
            lineupChannelListView.BeginUpdate();
            cmbObjectStoreLineups.Items.Clear();
            lineupChannelListView.VirtualListSize = 0;
            _lineupListViewItems.Clear();
            lineupChannelListView.Items.Clear();
            lineupChannelListView.EndUpdate();

            // close store
            WmcStore.Close();

            // clear the status text
            lblToolStripStatus.Text = string.Empty;
            statusStrip1.Refresh();
        }
        #endregion

        #region ========== Scheduled Task ==========
        private void UpdateTaskPanel()
        {
            // get status
            _task.QueryTask();

            // set .Enabled flags for components
            tbSchedTime.Enabled = lblUpdateTime.Enabled = cbTaskWake.Enabled = cbAutomatch.Enabled = tbTaskInfo.Enabled = rdoFullMode.Enabled = rdoClientMode.Enabled = !_task.Exist && !_task.ExistNoAccess;

            // set task create/delete button text
            btnTask.Text = _task.Exist || _task.ExistNoAccess ? "Delete" : "Create";

            // update scheduled task run time
            tbSchedTime.Text = _task.SchedTime.ToString("HH:mm");

            // set scheduled task wake checkbox
            cbTaskWake.Checked = _task.Wake;

            // set .Enabled properties
            if (!_task.Exist && !_task.ExistNoAccess)
            {
                rdoFullMode.Enabled = File.Exists(Helper.Epg123ExePath) || File.Exists(Helper.Hdhr2MxfExePath);
            }

            var clientIndex = -1;
            var epg123Index = -1;
            var hdhr2mxfIndex = -1;
            if (_task.Exist)
            {
                for (var i = 0; i < _task.Actions.Length; ++i)
                {
                    if (_task.Actions[i].Path.ToLower().Contains(Helper.Epg123ExePath.ToLower())) epg123Index = i;
                    else if (_task.Actions[i].Path.ToLower().Contains(Helper.Epg123ClientExePath.ToLower())) clientIndex = i;
                    else if (_task.Actions[i].Path.ToLower().Contains(Helper.Hdhr2MxfExePath.ToLower())) hdhr2mxfIndex = i;
                }

                // display task status
                if (clientIndex >= 0)
                {
                    lblSchedStatus.Text = _task.StatusString;
                    lblSchedStatus.ForeColor = Color.Black;
                    if (epg123Index >= 0)
                    {
                        rdoFullMode.Checked = true;
                        tbTaskInfo.Text = _task.Actions[epg123Index].Path;
                    }
                    else if (hdhr2mxfIndex >= 0)
                    {
                        rdoFullMode.Checked = true;
                        tbTaskInfo.Text = _task.Actions[hdhr2mxfIndex].Path;
                    }
                    else
                    {
                        rdoClientMode.Checked = true;
                        var arguments = _task.Actions[clientIndex].Arguments.Split(' ');
                        for (var i = 0; i < arguments.Length; ++i)
                        {
                            if (!arguments[i].Equals("-i") || i >= arguments.Length - 1) continue;
                            tbTaskInfo.Text = arguments[i + 1].Replace("\"", "");
                            break;
                        }
                    }
                    cbAutomatch.Checked = _task.Actions[clientIndex].Arguments.ToLower().Contains("-match");
                    return;
                }

                if (epg123Index >= 0 || hdhr2mxfIndex >= 0)
                {
                    lblSchedStatus.Text = "### Server Mode ONLY - Guide will not be imported. ###";
                    lblSchedStatus.ForeColor = Color.Red;
                    return;
                }

                MessageBox.Show($"The location of this program file is not the same location configured in the Scheduled Task.\n\nThis program:\n{Helper.Epg123ClientExePath}", "Configuration Warning", MessageBoxButtons.OK);
                grpScheduledTask.Enabled = false;
            }

            if (_task.Exist || _task.ExistNoAccess)
            {
                lblSchedStatus.Text = string.Empty;
                tbTaskInfo.Text = "*** UNKNOWN TASK CONFIGURATION ***";
            }
            else
            {
                rdoFullMode.Checked = File.Exists(Helper.Epg123ExePath) || File.Exists(Helper.Hdhr2MxfExePath);
                rdoClientMode.Checked = !rdoFullMode.Checked;
                lblSchedStatus.Text = _task.StatusString;
                lblSchedStatus.ForeColor = Color.Red;
            }
        }

        private void rdoMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFullMode.Checked)
            {
                if (File.Exists(Helper.Epg123ExePath))
                {
                    tbTaskInfo.Text = Helper.Epg123ExePath;
                }
                else if (File.Exists(Helper.Hdhr2MxfExePath))
                {
                    tbTaskInfo.Text = Helper.Hdhr2MxfExePath;
                }
                else
                {
                    tbTaskInfo.Text = "*** Click here to set executable file path. ***";
                }
            }
            else
            {
                tbTaskInfo.Text = "*** Click here to set MXF file path. ***";
            }
        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            if (sender != null) // null sender means we restarted to finish in administrator mode
            {
                // missing information
                if (!_task.Exist && tbTaskInfo.Text.StartsWith("***"))
                {
                    tbTaskInfo_Click(null, null);
                }

                // create new task if file location is valid
                if (!_task.Exist && !tbTaskInfo.Text.StartsWith("***"))
                {
                    // create task using epg123.exe/hdhr2mxf.exe & epg123Client.exe
                    if (rdoFullMode.Checked)
                    {
                        var importFile = Helper.Epg123MxfPath;
                        if (tbTaskInfo.Text.Equals(Helper.Hdhr2MxfExePath, StringComparison.OrdinalIgnoreCase)) importFile = Helper.Hdhr2MxfMxfPath;

                        var actions = new EpgTaskScheduler.TaskActions[2];
                        actions[0].Path = tbTaskInfo.Text;
                        actions[1].Path = Helper.Epg123ClientExePath;
                        actions[1].Arguments = $"-i \"{importFile}\"{(cbAutomatch.Checked ? " -match" : null)}";
                        _task.CreateTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                    }
                    // create task using epg123Client.exe
                    else
                    {
                        var actions = new EpgTaskScheduler.TaskActions[1];
                        actions[0].Path = Helper.Epg123ClientExePath;
                        actions[0].Arguments = $"-i \"{tbTaskInfo.Text}\"{(cbAutomatch.Checked ? " -match" : null)}";
                        _task.CreateTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                    }
                }
            }

            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                if (_task.Exist || _task.ExistNoAccess)
                {
                    Helper.WriteEButtonFile("deleteTask");
                }
                else
                {
                    Helper.WriteEButtonFile("createTask");
                }
                RestartClient(true);
                return;
            }

            if (_task.Exist)
            {
                _task.DeleteTask();
            }
            else if (sender == null)
            {
                _task.ImportTask();
            }

            // update panel with current information
            UpdateTaskPanel();
        }

        private void tbTaskInfo_Click(object sender, EventArgs e)
        {
            // don't modify if text box is displaying current existing task
            if (_task.Exist || _task.ExistNoAccess) return;

            if (rdoFullMode.Checked)
            {
                // determine path to existing file
                openFileDialog1.InitialDirectory = tbTaskInfo.Text.StartsWith("***")
                    ? Helper.ExecutablePath
                    : tbTaskInfo.Text.Substring(0, tbTaskInfo.Text.LastIndexOf('\\'));
                openFileDialog1.Filter = "EPG123 Executable|*.exe";
                openFileDialog1.Title = "Select the EPG123 Executable";
                openFileDialog1.Multiselect = false;
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                tbTaskInfo.Text = openFileDialog1.FileName;
                return;
            }

            var frmRemote = new frmRemoteServers();
            frmRemote.ShowDialog();
            if (!string.IsNullOrEmpty(frmRemote.mxfPath))
            {
                tbTaskInfo.Text = frmRemote.mxfPath;

                // if directed to a MXF file, ask if user wants to import immediately
                if (DialogResult.Yes == MessageBox.Show("Do you wish to import the guide listings now? If not, you can click the [Manual Import] button later or allow the scheduled task to perform the import.", "Import MXF File", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    btnImport_Click(tbTaskInfo, null);
                }
            }
        }
        #endregion

        #region ========== Virtual ListView Events ==========
        private void mergedChannelListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space when mergedChannelListView.SelectedIndices.Count > 0:
                    {
                        e.Handled = true;
                        var state = ((myChannelLvi)mergedChannelListView.Items[mergedChannelListView.SelectedIndices[0]]).Checked;
                        foreach (int index in mergedChannelListView.SelectedIndices)
                        {
                            WmcStore.SetChannelEnableState(_allMergedChannels[_mergedChannelFilter[index]].ChannelId, !state);
                        }
                        break;
                    }
                case Keys.Delete when mergedChannelListView.SelectedIndices.Count > 0:
                    e.Handled = true;
                    btnDeleteChannel_Click(null, null);
                    break;
                case Keys.A when e.Control:
                    e.Handled = true;
                    NativeMethods.SelectAllItems(mergedChannelListView);
                    break;
            }
        }

        private void mergedChannelListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;

            // quirk using virtual listview. to show empty checkbox, must first check it then uncheck it
            if (e.Item.Checked) return;
            e.Item.Checked = true;
            e.Item.Checked = false;
        }

        private void mergedChannelListView_MouseClick(object sender, MouseEventArgs e)
        {
            var lvi = ((ListView)sender).GetItemAt(e.X, e.Y);
            if (lvi == null) return;
            // if it is the checkbox
            if (e.X < (lvi.Bounds.Left + 16))
            {
                WmcStore.SetChannelEnableState(((myChannelLvi)lvi).ChannelId, !lvi.Checked);
            }
        }

        private void mergedChannelListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var lvi = ((ListView)sender).GetItemAt(e.X, e.Y);
            if (lvi != null)
            {
                WmcStore.SetChannelEnableState(((myChannelLvi)lvi).ChannelId, lvi.Checked);
            }
        }

        private void mergedChannelListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void mergedChannelListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex != 5)
            {
                e.DrawDefault = true;
                return;
            }

            var textSize = e.Graphics.MeasureString(e.SubItem.Text, mergedChannelListView.Font);
            if (e.Item.ListView.Columns[e.ColumnIndex].Width < (int)textSize.Width + 10)
            {
                e.Item.ListView.Columns[e.ColumnIndex].Width = (int)textSize.Width + 10;
            }

            Bitmap bmp = null;
            var highlight = false;
            var backColor = e.SubItem.BackColor;
            var foreColor = e.SubItem.ForeColor;
            if (e.Item.ListView.SelectedIndices.Contains(e.Item.Index))
            {
                if ((e.ItemState & ListViewItemStates.Selected) != 0 && e.Item.ListView.ContainsFocus)
                {
                    backColor = SystemColors.Highlight;
                    foreColor = SystemColors.HighlightText;
                    highlight = true;
                }
                else
                {
                    backColor = SystemColors.Control;
                    foreColor = SystemColors.ControlText;
                }
            }

            if (((myChannelLvi)e.Item).IsEncrypted)
            {
                bmp = highlight ? Resources.padlock_highlight : Resources.padlock;
            }
            else if (((myChannelLvi)e.Item).IsSuggestedBlocked)
            {
                bmp = highlight ? Resources.no_entry_sign_highlight : Resources.no_entry_sign;
            }
            else if (((myChannelLvi)e.Item).IsRadio)
            {
                bmp = highlight ? Resources.music_highlight : Resources.music;
            }
            else if (((myChannelLvi)e.Item).IsInteractiveTV)
            {
                bmp = highlight ? Resources.circled_information_source_highlight : Resources.circled_information_source;
            }

            e.DrawBackground();
            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);

            var sf = new StringFormat { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };
            e.Graphics.DrawString(e.SubItem.Text, mergedChannelListView.Font, new SolidBrush(foreColor),
                new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width + 10, e.Bounds.Height), sf);

            if (bmp == null) return;
            var imageRect = new Rectangle(e.Bounds.X + (10 - bmp.Width) / 2, e.Bounds.Y + 4, bmp.Width, 10);
            e.Graphics.DrawImage(bmp, imageRect);
        }
        #endregion

        #region ========== ListView Sorter and Column Widths ==========
        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            // determine which listview
            if (sender.Equals(mergedChannelListView))
            {
                // Determine if clicked column is already the column that is being sorted.
                if (e.Column == _mergedChannelColumnSorter.SortColumn)
                {
                    // Reverse the current sort direction for this column.
                    _mergedChannelColumnSorter.Order = _mergedChannelColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    // Set the column number that is to be sorted; default to ascending.
                    SetSortArrow(((ListView)sender).Columns[_mergedChannelColumnSorter.SortColumn], SortOrder.None);
                    _mergedChannelColumnSorter.SortColumn = e.Column;
                    _mergedChannelColumnSorter.Order = SortOrder.Ascending;
                }
                SetSortArrow(((ListView)sender).Columns[e.Column], _mergedChannelColumnSorter.Order);

                // Perform the sort with these new sort options.
                _allMergedChannels.Sort(_mergedChannelColumnSorter);
                FilterMergedChannels();
                mergedChannelListView.Refresh();
            }
            else if (sender.Equals(lineupChannelListView))
            {
                // Determine if clicked column is already the column that is being sorted.
                if (e.Column == _lineupChannelColumnSorter.SortColumn)
                {
                    // Reverse the current sort direction for this column.
                    _lineupChannelColumnSorter.Order = _lineupChannelColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    // Set the column number that is to be sorted; default to ascending.
                    SetSortArrow(((ListView)sender).Columns[_lineupChannelColumnSorter.SortColumn], SortOrder.None);
                    _lineupChannelColumnSorter.SortColumn = e.Column;
                    _lineupChannelColumnSorter.Order = SortOrder.Ascending;
                }
                SetSortArrow(((ListView)sender).Columns[e.Column], _lineupChannelColumnSorter.Order);

                // Perform the sort with these new sort options.
                _lineupListViewItems.Sort(_lineupChannelColumnSorter);
                lineupChannelListView.Refresh();
            }
            Cursor = Cursors.Arrow;
        }

        private void SetSortArrow(ColumnHeader head, SortOrder order)
        {
            const string ascArrow = "▲";
            const string descArrow = "▼";

            // remove arrow
            if (head.Text.EndsWith(ascArrow) || head.Text.EndsWith(descArrow))
                head.Text = head.Text.Substring(0, head.Text.Length - 1);

            // add arrow
            switch (order)
            {
                case SortOrder.Ascending: head.Text += ascArrow; break;
                case SortOrder.Descending: head.Text += descArrow; break;
            }
        }

        private void AdjustColumnWidths(ListView listView)
        {
            int[] minWidths = { 60, 65, 100, 100, 100, 60, 60 };
            foreach (ColumnHeader header in listView.Columns)
            {
                var currentWidth = header.Width;
                header.Width = -1;
                header.Width = Math.Max(Math.Max(header.Width, currentWidth), (int)(minWidths[header.Index] * _dpiScaleFactor));
            }
        }
        #endregion

        #region ========== ListView Menu Strip ==========
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // only enable the subscribe menu if a lineup service and merged channel has been selected
            subscribeMenuItem.Enabled = (lineupChannelListView.SelectedIndices.Count > 0) && (mergedChannelListView.SelectedIndices.Count > 0);

            // determine which menu items are visible based on select listview channel
            var mergedChannelMenuStrip = (((ContextMenuStrip)sender).SourceControl.Name == mergedChannelListView.Name);
            unsubscribeMenuItem.Visible = mergedChannelMenuStrip;
            toolStripSeparator2.Visible = mergedChannelMenuStrip;
            renameMenuItem.Visible = mergedChannelMenuStrip;
            renumberMenuItem.Visible = mergedChannelMenuStrip;
            toolStripSeparator6.Visible = mergedChannelMenuStrip;
            splitMenuItem.Visible = mergedChannelMenuStrip;
            mergeMenuItem.Visible = mergedChannelMenuStrip;
            toolStripSeparator7.Visible = mergedChannelMenuStrip;
            clipboardMenuItem.Visible = mergedChannelMenuStrip;
            clearListingsMenuItem.Visible = mergedChannelMenuStrip;

            // only enable merge menu item if multiple channels are selected
            mergeMenuItem.Enabled = mergedChannelListView.SelectedIndices.Count > 1;

            // only enable rename menu item if a single channel has been selected
            renameMenuItem.Enabled = mergedChannelListView.SelectedIndices.Count == 1 && _customLabelsOnly;
            renumberMenuItem.Enabled = _customLabelsOnly;
        }

        #region ===== Subscribe/Unsubscribe Channel =====
        private void subscribeMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            var unsubscribe = unsubscribeMenuItem.Equals((ToolStripMenuItem)sender);

            // subscribe selected channels to station
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                WmcStore.SubscribeLineupChannel(unsubscribe ? 0 : ((myLineupLvi)lineupChannelListView.Items[lineupChannelListView.SelectedIndices[0]]).ChannelId, _allMergedChannels[_mergedChannelFilter[index]].ChannelId);
            }

            // clear all selections
            mergedChannelListView.SelectedIndices.Clear();
            lineupChannelListView.SelectedIndices.Clear();

            Cursor = Cursors.Arrow;
        }
        #endregion

        #region ===== Rename Channel =====
        private void renameMenuItem_Click(object sender, EventArgs e)
        {
            // record current callsign for potential restore
            _selectedItemForEdit = mergedChannelListView.SelectedIndices[0];

            // enable label editing
            mergedChannelListView.LabelEdit = true;

            // begin edit
            mergedChannelListView.Items[_selectedItemForEdit].BeginEdit();
        }

        private void mergedChannelListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // disable label editing
            mergedChannelListView.LabelEdit = false;

            // update callsign
            if (e.Label != null)
            {
                WmcStore.SetChannelCustomCallsign(((myChannelLvi)mergedChannelListView.Items[_selectedItemForEdit]).ChannelId, e.Label);
            }
        }
        #endregion

        #region ===== Renumber Channel =====
        private int _selectedItemForEdit;

        private void renumberMenuItem_Click(object sender, EventArgs e)
        {
            // get bounds of number field
            _selectedItemForEdit = mergedChannelListView.SelectedIndices[0];
            mergedChannelListView.Items[_selectedItemForEdit].EnsureVisible();
            var box = mergedChannelListView.Items[_selectedItemForEdit].SubItems[1].Bounds;
            lvEditTextBox.SetBounds(box.Left + mergedChannelListView.Left + 2, box.Top + mergedChannelListView.Top, box.Width, box.Height);
            lvEditTextBox.Show();
            lvEditTextBox.Focus();
        }

        private void lvEditTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // make sure it is numbers only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true;
            if (e.KeyChar == '.' && ((TextBox)sender).Text.IndexOf('.') > -1) e.Handled = true;

            // decide on what to do on a return or escape
            switch (e.KeyChar)
            {
                case (char)Keys.Return:
                    lvEditTextBox.Hide();
                    var digits = lvEditTextBox.Text.Split('.');
                    var number = 0;
                    var subnumber = 0;
                    if (!string.IsNullOrEmpty(digits[0])) number = int.Parse(digits[0]);
                    if (digits.Length > 1) subnumber = int.Parse(digits[1]);

                    Cursor = Cursors.WaitCursor;
                    foreach (int index in mergedChannelListView.SelectedIndices)
                    {
                        WmcStore.SetChannelCustomNumber(((myChannelLvi)mergedChannelListView.Items[index]).ChannelId,
                            $"{(number == 0 ? null : digits.Length == 1 ? $"{number++}" : $"{number}.{subnumber++}")}");
                    }
                    Cursor = Cursors.Default;

                    lvEditTextBox.Text = "";
                    e.Handled = true;
                    break;
                case (char)Keys.Escape:
                    lvEditTextBox.Text = null;
                    lvEditTextBox.Hide();
                    e.Handled = true;
                    break;
            }
        }
        #endregion

        #region ==== Split/Merged Operations =====
        private void splitMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            mergedChannelListView.BeginUpdate();
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                SplitChannels(_allMergedChannels[_mergedChannelFilter[index]].ChannelId);
            }

            btnRefreshLineups_Click(null, null);
            mergedChannelListView.EndUpdate();
            Cursor = Cursors.Default;
        }

        private static void SplitChannels(long id)
        {
            var mergedChannel = WmcStore.WmcObjectStore.Fetch(id) as MergedChannel;
            var subChannels = new List<Channel>();
            if (mergedChannel.PrimaryChannel.ChannelType != ChannelType.Wmis)
            {
                subChannels.Add(mergedChannel.PrimaryChannel);
            }

            foreach (Channel channel in mergedChannel.SecondaryChannels.Where(arg => arg.ChannelType != ChannelType.Wmis))
            {
                subChannels.Add(channel);
            }

            if (subChannels.Count <= 1) return;
            subChannels.RemoveAt(0);

            foreach (var subChannel in subChannels)
            {
                mergedChannel.SplitChannel(subChannel);
            }
        }

        private void mergeMenuItem_Click(object sender, EventArgs e)
        {
            // WMC will create a new mergedchannel with the usermappedlistings and tuners
            // it will then change the channeltype to userhidden of the merging mergedchannels
            Cursor = Cursors.WaitCursor;
            if (mergedChannelListView.SelectedIndices.Count > 5)
            {
                MessageBox.Show("Sorry, EPG123 limits the merging of channels to 5 at a time.", "Sanity Check",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            mergedChannelListView.BeginUpdate();

            var channelIds = new List<long>();
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                channelIds.Add(_allMergedChannels[_mergedChannelFilter[index]].ChannelId);
            }

            var frm = new frmMerge(channelIds);
            frm.ShowDialog();
            if (frm.mergeOrder.Count > 0)
            {
                var primary = frm.mergeOrder.First();
                frm.mergeOrder.Remove(primary);
                MergeChannels(primary, frm.mergeOrder);
                btnRefreshLineups_Click(null, null);
            }

            mergedChannelListView.EndUpdate();
            Cursor = Cursors.Default;
        }

        private void MergeChannels(long primary, List<long> mergingChannels)
        {
            var mergedChannel = WmcStore.WmcObjectStore.Fetch(primary) as MergedChannel;
            foreach (var channel in mergingChannels)
            {
                var merging = WmcStore.WmcObjectStore.Fetch(channel) as MergedChannel;
                merging.CombineIntoChannel(mergedChannel);
            }
        }
        #endregion

        #region ===== Copy to Clipboard =====
        private void clipboardMenuItem_Click(object sender, EventArgs e)
        {
            var textToAdd = "Call Sign\tNumber\tService Name\tSubscribed Lineup\tScanned Source(s)\tTuningInfo\tMatchName\tService Callsign\r\n";
            foreach (var index in _mergedChannelFilter)
            {
                string matchname;
                string callsign;
                if (!(WmcStore.WmcObjectStore.Fetch(_allMergedChannels[index].ChannelId) is MergedChannel mergedChannel)) return;
                if (!mergedChannel.SecondaryChannels.Empty && mergedChannel.SecondaryChannels.First.Lineup.Name.StartsWith("Scanned"))
                {
                    matchname = mergedChannel.SecondaryChannels.First.MatchName;
                    callsign = mergedChannel.SecondaryChannels.First.CallSign;
                }
                else
                {
                    matchname = mergedChannel.MatchName;
                    callsign = mergedChannel.CallSign;
                }
                textToAdd += $"{_allMergedChannels[index].SubItems[0].Text}\t{_allMergedChannels[index].SubItems[1].Text}\t{_allMergedChannels[index].SubItems[2].Text}\t{_allMergedChannels[index].SubItems[3].Text}\t{_allMergedChannels[index].SubItems[4].Text}\t{_allMergedChannels[index].SubItems[5].Text}\t{matchname}\t{callsign}\r\n";
            }
            Clipboard.SetText(textToAdd);
        }
        #endregion

        #region ===== Clear Guide Listings =====
        private void BtnClearScheduleEntries(object sender, EventArgs e)
        {
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                var channelId = _allMergedChannels[_mergedChannelFilter[index]].ChannelId;
                WmcStore.ClearServiceScheduleEntries(channelId);
            }
        }
        #endregion
        #endregion

        #region ========== Channel/Listings AutoMapping ==========
        private void btnAutoMatch_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            foreach (var index in _mergedChannelFilter)
            {
                var mergedChannel = _allMergedChannels[index];
                if (((ToolStripButton)sender).Equals(btnAutoCallsign))
                {
                    var callsign = mergedChannel.SubItems[0].Text;
                    if (string.IsNullOrEmpty(callsign)) continue;

                    var lineupChannels = _lineupListViewItems.Where(arg => arg.Callsign.Equals(callsign)).ToList();
                    if (lineupChannels.Count > 0)
                    {
                        foreach (var channel in lineupChannels)
                        {
                            WmcStore.SubscribeLineupChannel(channel.ChannelId, mergedChannel.ChannelId);
                        }
                    }
                    else goto Disable;
                }
                else if (((ToolStripButton)sender).Equals(btnAutoNumber))
                {
                    var lineupChannels = _lineupListViewItems.Where(arg => arg.Number.Equals(mergedChannel.SubItems[1].Text)).ToList();
                    if (lineupChannels.Count > 0)
                    {
                        foreach (var channel in lineupChannels)
                        {
                            WmcStore.SubscribeLineupChannel(channel.ChannelId, mergedChannel.ChannelId);
                        }
                    }
                    else goto Disable;
                }
                continue;

            Disable:
                if (mergedChannel.Enabled && string.IsNullOrEmpty(mergedChannel.SubItems[3].Text))
                {
                    WmcStore.SetChannelEnableState(mergedChannel.ChannelId, false);
                }
            }
            Cursor = Cursors.Default;
        }
        #endregion

        #region ========== Lineup ListView Management ==========
        private readonly List<myLineupLvi> _lineupListViewItems = new List<myLineupLvi>();

        private void lineupChannelListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = _lineupListViewItems.Count == 0 ? new ListViewItem() : _lineupListViewItems[e.ItemIndex];
        }

        private void BuildLineupChannelListView()
        {
            // clear the pulldown list
            cmbObjectStoreLineups.Items.Clear();

            // populate with lineups in object_store
            foreach (var lineup in WmcStore.GetWmisLineups())
            {
                cmbObjectStoreLineups.Items.Add(lineup);
            }

            // preset value to epg123 lineup if exists
            cmbObjectStoreLineups.SelectedIndex = cmbObjectStoreLineups.FindString("EPG123");
            if (cmbObjectStoreLineups.Items.Count <= 0) return;
            if (cmbObjectStoreLineups.SelectedIndex < 0) cmbObjectStoreLineups.SelectedIndex = 0;
            btnDeleteLineup.Enabled = (cmbObjectStoreLineups.Items.Count > 0);
        }

        private void lineupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // pause listview drawing
            Cursor = Cursors.WaitCursor;
            lineupChannelListView.BeginUpdate();

            // clear the lineup channel listview
            _lineupListViewItems?.Clear();
            lineupChannelListView?.Items.Clear();

            // populate with new lineup channels
            foreach (var channel in WmcStore.GetLineupChannels(((myLineup)cmbObjectStoreLineups.Items[cmbObjectStoreLineups.SelectedIndex]).LineupId))
            {
                _lineupListViewItems.Add(new myLineupLvi(channel));
            }
            if ((lineupChannelListView.VirtualListSize = _lineupListViewItems.Count) > 0)
            {
                lineupChannelListView.TopItem = lineupChannelListView.Items[0];
            }
            lineupChannelListView.SelectedIndices.Clear();

            // adjust column widths
            AdjustColumnWidths(lineupChannelListView);

            // reset sorting column and order
            _lineupChannelColumnSorter.Order = SortOrder.Ascending;
            _lineupChannelColumnSorter.SortColumn = 1;
            SetSortArrow(lineupChannelListView.Columns[1], SortOrder.Ascending);
            _lineupListViewItems.Sort(_lineupChannelColumnSorter);

            // resume listview drawing
            lineupChannelListView.EndUpdate();
            Cursor = Cursors.Arrow;

            // update the status bar
            UpdateStatusBar();
        }

        private void btnRefreshLineups_Click(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;

            foreach (ColumnHeader head in mergedChannelListView.Columns)
            {
                SetSortArrow(head, SortOrder.None);
            }

            foreach (ColumnHeader head in lineupChannelListView.Columns)
            {
                SetSortArrow(head, SortOrder.None);
            }

            mergedChannelListView.VirtualListSize = 0;
            splitContainer1.Enabled = splitContainer2.Enabled = false;
            IsolateEpgDatabase();
            BuildScannedLineupComboBox();
            BuildMergedChannelListView();
            BuildLineupChannelListView();
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            btnImport.Enabled = _allMergedChannels.Count > 0;

            Application.UseWaitCursor = false;
            UpdateStatusBar();
        }

        private void BtnDeleteLineupClick(object sender, EventArgs e)
        {
            var prompt = $"The lineup \"{cmbObjectStoreLineups.SelectedItem}\" will be removed from the Media Center database. Do you wish to continue?";
            if (MessageBox.Show(prompt, "Delete Channel(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            Application.UseWaitCursor = true;
            mergedChannelListView.BeginUpdate();

            WmcStore.UnsubscribeChannelsInLineup(((myLineup)cmbObjectStoreLineups.SelectedItem).LineupId);
            WmcStore.DeleteLineup(((myLineup)cmbObjectStoreLineups.SelectedItem).LineupId);
            BuildLineupChannelListView();

            mergedChannelListView.EndUpdate();
            Application.UseWaitCursor = false;
        }
        #endregion

        #region ========== Merged Channel ListView Management ==========
        private List<myChannelLvi> _allMergedChannels = new List<myChannelLvi>();
        private List<int> _mergedChannelFilter = new List<int>();
        private bool _enabledChannelsOnly;
        private bool _tvChannelsOnly;
        private bool _radioChannelsOnly;
        private bool _encryptedChannelsOnly;
        private bool _unencryptedChannelsOnly;
        private bool _suggestedBlockedOnly;
        private bool _notSuggestBlockedOnly;
        private bool _interactiveTvOnly;
        private bool _customLabelsOnly = true;

        #region ===== Merged Channel ListView Items =====
        private void mergedChannelListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = _allMergedChannels.Count == 0
                ? new ListViewItem()
                : _allMergedChannels[_mergedChannelFilter[e.ItemIndex]];
        }

        private void BuildMergedChannelListView()
        {
            if (WmcStore.WmcMergedLineup == null) return;

            // pause listview drawing
            mergedChannelListView.BeginUpdate();

            // build new everything
            if ((_allMergedChannels?.Count ?? 0) == 0)
            {
                // reset initial label to custom only
                if (!_customLabelsOnly) btnCustomDisplay_Click(null, null);

                // reset sorting column and order
                _mergedChannelColumnSorter.Order = SortOrder.Ascending;
                _mergedChannelColumnSorter.SortColumn = 1;
                SetSortArrow(mergedChannelListView.Columns[1], SortOrder.Ascending);

                // notify something is going on
                lblToolStripStatus.Text = "Collecting merged channels...";
                statusStrip1.Refresh();

                // initialize list to appropriate size
                var channelCount = WmcStore.WmcMergedLineup.UncachedChannels.Count();
                _allMergedChannels = new List<myChannelLvi>(channelCount);
                lvItemsProgressBar.Maximum = channelCount;

                // reset and show progress bar
                IncrementProgressBar(true);

                // populate AllMergedChannels list
                foreach (MergedChannel channel in WmcStore.WmcMergedLineup.UncachedChannels.Cast<MergedChannel>())
                {
                    // increment progress bar
                    IncrementProgressBar();

                    // do not include broadband or user hidden channels
                    if (channel.ChannelType == ChannelType.WmisBroadband || channel.ChannelType == ChannelType.UserHidden) continue;

                    // remove channels that do not have any tuningInfo
                    if (channel.TuningInfos?.Empty ?? true)
                    {
                        WmcStore.DeleteChannel(channel.Id);
                        continue;
                    }

                    // make sure channel has a primary channel with lineup
                    if (channel.PrimaryChannel?.Lineup == null)
                    {
                        if (channel.PrimaryChannel == null)
                        {
                            Logger.WriteInformation($"MergedChannel \"{channel}\" has a <<NULL>> primary channel. Skipping.");
                            continue;
                        }

                        Logger.WriteInformation($"Attempting to repair MergedChannel \"{channel}\" by unsubscribing all non-Scanned Lineup channels.");
                        WmcStore.SubscribeLineupChannel(0, channel.Id);

                        if (channel.PrimaryChannel?.Lineup == null)
                        {
                            Logger.WriteInformation($"    Failed to repair MergedChannel \"{channel}\". Deleting.");
                            WmcStore.DeleteChannel(channel.Id);
                            continue;
                        }
                    }

                    // build default listviewitems
                    _allMergedChannels.Add(new myChannelLvi(channel));
                }
            }
            else
            {
                // reset and show progress bar
                lvItemsProgressBar.Maximum = _allMergedChannels.Count;
                IncrementProgressBar(true);
            }

            lvItemsProgressBar.Width = 0;

            // reset sorting column and order
            _allMergedChannels.Sort(_mergedChannelColumnSorter);

            // filter merged channels based on selections
            FilterMergedChannels();

            // adjust column widths
            AdjustColumnWidths(mergedChannelListView);

            // resume listview drawing
            mergedChannelListView.EndUpdate();

            // refresh status bar
            UpdateStatusBar();
        }

        private void FilterMergedChannels()
        {
            _mergedChannelFilter = new List<int>(_allMergedChannels.Count);
            foreach (var channel in _allMergedChannels.Where(channel => !_enabledChannelsOnly || channel.Enabled))
            {
                if (_tvChannelsOnly || _radioChannelsOnly || _interactiveTvOnly)
                {
                    if (channel.IsRadio && !_radioChannelsOnly) continue;
                    if (channel.IsInteractiveTV && !_interactiveTvOnly) continue;
                    if (channel.IsTV && !_tvChannelsOnly) continue;
                }

                if (_encryptedChannelsOnly || _unencryptedChannelsOnly)
                {
                    if (channel.IsEncrypted && !_encryptedChannelsOnly) continue;
                    if (!channel.IsEncrypted && !_unencryptedChannelsOnly) continue;
                }

                if (_suggestedBlockedOnly || _notSuggestBlockedOnly)
                {
                    if (channel.IsSuggestedBlocked && !_suggestedBlockedOnly) continue;
                    if (!channel.IsSuggestedBlocked && !_notSuggestBlockedOnly) continue;
                }

                if (cmbSources.SelectedIndex > 0 && !channel.ScannedLineupIds.Contains(((myLineup)cmbSources.SelectedItem).LineupId)) continue;
                _mergedChannelFilter.Add(_allMergedChannels.IndexOf(channel));
            }
            _mergedChannelFilter.TrimExcess();
            mergedChannelListView.VirtualListSize = _mergedChannelFilter.Count;
        }
        #endregion

        #region ===== Scanned Lineup Combobox =====
        private void BuildScannedLineupComboBox()
        {
            // clear combobox and add initial entry
            cmbSources.Items.Clear();
            cmbSources.Items.Add("All Scanned Sources");

            // get all sources and set initial selection
            foreach (var source in WmcStore.GetDeviceLineupsAndIds())
            {
                cmbSources.Items.Add(source);
            }
            cmbSources.SelectedIndex = 0;
        }

        private void cmbSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (cmbSources.SelectedIndex < 0) return;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
        }
        #endregion

        #region ===== Progress and Status Bar =====
        private void IncrementProgressBar(bool reset = false)
        {
            if (reset)
            {
                lblToolStripStatus.Text = string.Empty;
                lvItemsProgressBar.Value = 0;
                statusStrip1.Refresh();
            }
            else
            {
                lvItemsProgressBar.Value = Math.Min(lvItemsProgressBar.Value + 2, lvItemsProgressBar.Maximum);
                --lvItemsProgressBar.Value;
                Application.DoEvents();
            }

            // make sure progress bar tracks panel width
            if (lvItemsProgressBar.Width != mergedChannelListView.Parent.Width - 1)
                lvItemsProgressBar.Width = mergedChannelListView.Parent.Width - 1;
        }

        private void UpdateStatusBar()
        {
            var totalServices = cmbObjectStoreLineups.Items.Cast<myLineup>().Sum(lineup => lineup.ChannelCount);
            lblToolStripStatus.Text = $"{_allMergedChannels.Count} Merged Channel(s) with {mergedChannelListView.VirtualListSize} shown  |  {cmbObjectStoreLineups.Items.Count} Lineup(s)  |  {totalServices} Service(s) with {lineupChannelListView.Items.Count} shown";
            statusStrip1.Refresh();
        }
        #endregion

        #region ===== Merged Channel Additional Filtering =====
        private void btnCustomDisplay_Click(object sender, EventArgs e)
        {
            _customLabelsOnly = !_customLabelsOnly;
            foreach (var item in _allMergedChannels)
            {
                item.ShowCustomLabels(_customLabelsOnly);
            }
            mergedChannelListView.Invalidate();

            if (!_customLabelsOnly)
            {
                btnLablesDisplay.Text = "Original Labels";
                btnLablesDisplay.BackColor = Color.PowderBlue;
            }
            else
            {
                btnLablesDisplay.Text = "Custom Labels";
                btnLablesDisplay.BackColor = Color.OrangeRed;
            }
        }

        private void btnChannelDisplay_Click(object sender, EventArgs e)
        {
            _enabledChannelsOnly = !_enabledChannelsOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnChannelDisplay.BackColor = !_enabledChannelsOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnTelevision_Click(object sender, EventArgs e)
        {
            _tvChannelsOnly = !_tvChannelsOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnTelevision.BackColor = !_tvChannelsOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnRadio_Click(object sender, EventArgs e)
        {
            _radioChannelsOnly = !_radioChannelsOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnRadio.BackColor = !_radioChannelsOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnEncrypted_Click(object sender, EventArgs e)
        {
            _encryptedChannelsOnly = !_encryptedChannelsOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnEncrypted.BackColor = !_encryptedChannelsOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnUnencrypted_Click(object sender, EventArgs e)
        {
            _unencryptedChannelsOnly = !_unencryptedChannelsOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnUnencrypted.BackColor = !_unencryptedChannelsOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnBlocked_Click(object sender, EventArgs e)
        {
            _suggestedBlockedOnly = !_suggestedBlockedOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnBlocked.BackColor = !_suggestedBlockedOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnNotSuggestedBlocked_Click(object sender, EventArgs e)
        {
            _notSuggestBlockedOnly = !_notSuggestBlockedOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnNotSuggestedBlocked.BackColor = !_notSuggestBlockedOnly ? SystemColors.Control : SystemColors.ControlDark;
        }

        private void btnInteractive_Click(object sender, EventArgs e)
        {
            _interactiveTvOnly = !_interactiveTvOnly;

            Cursor = Cursors.WaitCursor;
            BuildMergedChannelListView();
            Cursor = Cursors.Default;
            btnInteractive.BackColor = !_interactiveTvOnly ? SystemColors.Control : SystemColors.ControlDark;
        }
        #endregion

        #region ===== Buttons and Dials =====
        private void btnDeleteChannel_Click(object sender, EventArgs e)
        {
            var prompt = $"The selected {mergedChannelListView.SelectedIndices.Count} channel(s) will be removed from the Media Center database. Do you wish to continue?";
            if (mergedChannelListView.SelectedIndices.Count == 0 ||
                MessageBox.Show(prompt, "Delete Channel(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            mergedChannelListView.BeginUpdate();

            // gather the listview items
            var channelsToDelete = (from int index in mergedChannelListView.SelectedIndices
                                    select _allMergedChannels[_mergedChannelFilter[index]]).ToList();

            // delete the channel and remove from listview items
            mergedChannelListView.VirtualListSize -= channelsToDelete.Count;
            foreach (var channel in channelsToDelete)
            {
                channel.RemoveDelegate();
                _allMergedChannels.Remove(channel);
                WmcStore.DeleteChannel(channel.ChannelId);
                Logger.WriteInformation($"Deleted channel {channel.Number}{(!string.IsNullOrEmpty(channel.Callsign) ? $" {channel.Callsign}" : string.Empty)} from {channel.SubItems[4].Text}");
            }
            WmcStore.WmcMergedLineup.FullMerge(false);
            _allMergedChannels.TrimExcess();
            FilterMergedChannels();

            mergedChannelListView.SelectedIndices.Clear();
            mergedChannelListView.EndUpdate();
            Cursor = Cursors.Arrow;

            UpdateStatusBar();
        }
        #endregion
        #endregion

        #region ========== Database Explorer ==========
        private void btnStoreExplorer_Click(object sender, EventArgs e)
        {
            // close the store
            IsolateEpgDatabase();

            // used code by glugalug (glugglug) and modified for epg123
            // https://github.com/glugalug/GuideEditingMisc/tree/master/StoreExplorer

            var sha256Man = new SHA256Managed();
            var clientId = ObjectStore.GetClientId(true);
            const string providerName = @"Anonymous!User";
            var password = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(clientId)));

            var assembly = Assembly.LoadFile(Environment.ExpandEnvironmentVariables(@"%WINDIR%\ehome\mcstore.dll"));
            var module = assembly.GetModules().First();
            var formType = module.GetType("Microsoft.MediaCenter.Store.Explorer.StoreExplorerForm");
            var storeType = module.GetType("Microsoft.MediaCenter.Store.ObjectStore");

            var friendlyNameProperty = storeType.GetProperty("FriendlyName", BindingFlags.Static | BindingFlags.Public);
            friendlyNameProperty.SetValue(null, providerName, null);
            var displayNameProperty = storeType.GetProperty("DisplayName", BindingFlags.Static | BindingFlags.Public);
            displayNameProperty.SetValue(null, password, null);

            var defaultMethod = storeType.GetMethod("get_DefaultSingleton", BindingFlags.Static | BindingFlags.Public);
            var store = defaultMethod.Invoke(null, null);
            var constructor = formType.GetConstructor(new[] { storeType });
            var form = (Form)constructor.Invoke(new[] { store });
            form.ShowDialog();

            // the store explorer form does a DisposeAll on the object store which basically breaks
            // the ObjectStore in the client. Closing form because I haven't found a way to reinit
            // the objectStore_ parameter.
            _forceExit = true;
            Close();
        }
        #endregion

        #region ========== Export Database ==========
        private void btnExportMxf_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(Helper.Epg123OutputFolder)) Directory.CreateDirectory(Helper.Epg123OutputFolder);

            var settings = new XmlWriterSettings() { Indent = true };
            using (var writer = XmlWriter.Create(new StreamWriter(Helper.Epg123OutputFolder + "mxfExport.mxf"), settings))
            {
                MxfExporter.Export(WmcStore.WmcObjectStore, writer, false);
            }
            Cursor = Cursors.Arrow;
        }
        #endregion

        #region ========== Child Forms ==========
        private void btnAddChannels_Click(object sender, EventArgs e)
        {
            using (var addChannelForm = new frmAddChannel())
            {
                addChannelForm.ShowDialog();
                if (!addChannelForm.ChannelAdded) return;
                btnRefreshLineups_Click(null, null);
            }
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                Helper.WriteEButtonFile("setup");
                RestartClient(true);
                return;
            }

            // set cursor and disable the containers so no buttons can be clicked
            Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // prep the client setup form
            var frm = new frmClientSetup { ShouldBackup = WmcStore.WmcObjectStore != null };

            // clear everything out
            IsolateEpgDatabase();

            // make sure mcupdate task is enabled to better complete TV Setup
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/change /tn \"\\Microsoft\\Windows\\Media Center\\mcupdate\" /tr \"'%SystemRoot%\\ehome\\mcupdate.exe' $(Arg0)\" /enable",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            proc?.WaitForExit();

            // open the form
            frm.ShowDialog();
            if (!_task.Exist && !_task.ExistNoAccess)
            {
                if (rdoFullMode.Checked) tbTaskInfo.Text = frm.Hdhr2MxfSrv ? Helper.Hdhr2MxfExePath : Helper.Epg123ExePath;
                else tbTaskInfo.Text = frm.mxfImport ?? "*** Click here to set MXF file path. ***";
            }
            frm.Dispose();

            // restore mcupdate task pointed to client if epg123_update task already exists
            if (_task.Exist)
            {
                proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/change /tn \"\\Microsoft\\Windows\\Media Center\\mcupdate\" /tr \"'{Helper.Epg123ClientExePath}' $(Arg0)\" /enable",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                proc?.WaitForExit();
            }

            // build the listviews and make sure registries are good
            btnRefreshLineups_Click(null, null);

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Cursor = Cursors.Arrow;
        }

        private void btnTransferTool_Click(object sender, EventArgs e)
        {
            // set cursor and disable the containers so no buttons can be clicked
            Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            Process.Start("epg123Transfer.exe")?.WaitForExit();

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Cursor = Cursors.Arrow;
        }

        private void btnTweakWmc_Click(object sender, EventArgs e)
        {
            // open the tweak gui
            var frm = new frmWmcTweak();
            frm.ShowDialog();
        }

        private void btnUndelete_Click(object sender, EventArgs e)
        {
            var frmUndelete = new frmUndelete();
            frmUndelete.ShowDialog();
            if (!frmUndelete.ChannelAdded) return;
            btnRefreshLineups_Click(null, null);
        }

        private void btnSatellites_Click(object sender, EventArgs e)
        {
            var satFrm = new frmSatellites();
            satFrm.ShowDialog();
        }
        #endregion

        #region ========== View Log ==========
        private void btnViewLog_Click(object sender, EventArgs e)
        {
            Helper.ViewLogFile();
        }
        #endregion

        #region ========== Manual Import and Reindex ==========
        private void btnImport_Click(object sender, EventArgs e)
        {
            // verify tuners are set up
            if (_allMergedChannels.Count == 0)
            {
                switch (MessageBox.Show("There doesn't appear to be any tuners setup in WMC. Importing guide information before TV Setup is complete will corrupt the database. Restoring a lineup (tuner configuration) from a backup is safe.\n\nDo you wish to continue?", "Import Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    case DialogResult.Yes:
                        Logger.WriteInformation("User opted to proceed with MXF file import while there are no tuners configured in WMC.");
                        break;
                    default:
                        return;
                }
            }

            // check for recordings in progress
            if (WmcStore.DetermineRecordingsInProgress())
            {
                if (DialogResult.Yes == MessageBox.Show("There is currently at least one program being recorded. Importing a guide update at this time may result with an aborted recording or worse.\n\nDo you wish to proceed?",
                    "Recording In Progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    Logger.WriteInformation("User opted to proceed with manual import while a recording was in progress.");
                }
                else
                {
                    return;
                }
            }

            // configure open file dialog parameters
            openFileDialog1 = new OpenFileDialog()
            {
                FileName = string.Empty,
                Filter = "MXF File|*.mxf",
                Title = "Select a MXF File",
                Multiselect = false
            };

            // determine initial path
            if (Directory.Exists(Helper.Epg123OutputFolder) && (tbTaskInfo.Text.Equals(Helper.Epg123ExePath) || tbTaskInfo.Text.Equals(Helper.Hdhr2MxfExePath)))
            {
                openFileDialog1.InitialDirectory = Helper.Epg123OutputFolder;
            }
            else if (!tbTaskInfo.Text.StartsWith("***") && !tbTaskInfo.Text.StartsWith("http"))
            {
                openFileDialog1.InitialDirectory = tbTaskInfo.Text.Substring(0, tbTaskInfo.Text.LastIndexOf('\\'));
            }

            // open the dialog
            if ((sender?.Equals(tbTaskInfo) ?? false) || tbTaskInfo.Text.StartsWith("http")) openFileDialog1.FileName = tbTaskInfo.Text;
            else if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            // perform the file import with progress form
            Logger.Status = 0;
            var importForm = new frmImport(openFileDialog1.FileName);
            importForm.ShowDialog();

            // kick off the reindex
            if (importForm.Success)
            {
                WmcStore.AutoMapChannels();
                BuildLineupChannelListView();
                WmcStore.ReindexDatabase();
            }
            else
            {
                MessageBox.Show("There was an error importing the MXF file.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            statusLogo.StatusImage(openFileDialog1.FileName);

            // make sure guide is activated and background scanning is stopped
            WmcStore.ActivateEpg123LineupsInStore();
            WmcStore.ActivateGuide();

            // open object store and repopulate the GUI
            if (!sender?.Equals(tbTaskInfo) ?? true) btnRefreshLineups_Click(null, null);
        }
        #endregion

        #region ========== Backup Database ==========
        private static readonly string[] BackupFolders = { "lineup", "recordings", "subscriptions" };

        private void btnBackup_Click(object sender, EventArgs e)
        {
            // set cursor and disable the containers so no buttons can be clicked
            Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // start the thread and wait for it to complete
            var backupThread = new Thread(BackupBackupFiles);
            backupThread.Start();
            while (!backupThread.IsAlive) ;
            while (backupThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            if (!string.IsNullOrEmpty(Helper.BackupZipFile))
            {
                MessageBox.Show("A database backup has been successful. Location of backup file is " + Helper.BackupZipFile, "Database Backup", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("A database backup not successful.", "Database Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Cursor = Cursors.Arrow;
        }

        public static void BackupBackupFiles()
        {
            var backups = new Dictionary<string, string>();

            // going to assume backups are up-to-date if in safe mode
            if (SystemInformation.BootMode == BootMode.Normal)
            {
                _ = WmcStore.PerformWmcConfigurationsBackup();
            }

            foreach (var backupFolder in BackupFolders)
            {
                string filepath;
                if (!string.IsNullOrEmpty(filepath = GetBackupFilename(backupFolder)))
                {
                    backups.Add(filepath, backupFolder + ".mxf");
                }
            }

            Helper.BackupZipFile = backups.Count > 0
                ? CompressXmlFiles.CreatePackage(backups, "backups")
                : string.Empty;
        }

        private static string GetBackupFilename(string backup)
        {
            string ret = null;
            var directory = new DirectoryInfo(WmcStore.GetStoreFilename().Replace(".db", $"\\backup\\{backup}"));
            if (Directory.Exists(directory.FullName))
            {
                ret = directory.GetFiles().OrderByDescending(arg => arg.LastWriteTime).First().FullName;
            }
            else
            {
                Logger.WriteInformation($"Backup {backup} file does not exist.");
            }
            return ret;
        }
        #endregion

        #region ========== Restore Database ==========
        /// <summary>
        /// Method for me to import any users backup to aid in troubleshooting
        /// GUI must already be in administrative mode
        /// </summary>
        private bool restoreOverride;
        private void btnRestore_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                Helper.WriteEButtonFile("restore");
                RestartClient(true);
                return;
            }

            restoreOverride = ModifierKeys == Keys.Control;

            // determine path to existing backup file
            openFileDialog1.InitialDirectory = Helper.Epg123BackupFolder;
            openFileDialog1.Filter = "Compressed File|*.zip";
            openFileDialog1.Title = "Select the Compressed Backup ZIP File";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog1.FileName)) return;
            Helper.BackupZipFile = openFileDialog1.FileName;

            // set cursor and disable the containers so no buttons can be clicked
            Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // clear all listviews and comboboxes
            IsolateEpgDatabase();

            // start the thread and wait for it to complete
            var restoreThread = new Thread(RestoreBackupFiles);
            restoreThread.Start();
            while (!restoreThread.IsAlive) ;
            while (restoreThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            // build the listviews and make sure registries are good
            btnRefreshLineups_Click(null, null);
            WmcStore.ActivateEpg123LineupsInStore();
            WmcStore.ActivateGuide();

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Cursor = Cursors.Arrow;
        }

        private void RestoreBackupFiles()
        {
            foreach (var backup in BackupFolders)
            {
                using (var stream = CompressXmlFiles.GetBackupFileStream(backup + ".mxf", Helper.BackupZipFile))
                {
                    if (stream == null) continue;
                    if (rebuild && backup == "lineup")
                    {
                        if (DeleteActiveDatabaseFile() == null) return;
                    }
                    else if (backup == "lineup")
                    {
                        var lineup = InspectLineupFile(stream);
                        if (lineup == null || DeleteActiveDatabaseFile() == null) return;
                        using (var mem = new MemoryStream())
                        {
                            lineup.Save(mem);
                            mem.Seek(0, SeekOrigin.Begin);
                            MxfImporter.Import(mem, WmcStore.WmcObjectStore);
                            continue;
                        }
                    }
                    MxfImporter.Import(stream, WmcStore.WmcObjectStore);
                }
            }
        }

        class tunerRecorders
        {
            public override string ToString()
            {
                return $"{devName}{(hwOccurence > 0 ? $" #{hwOccurence}" : "")}";
            }

            public bool matched;
            public bool singleTunerGroup;
            public string recorderId;
            public string devName;
            public string rootDevice;
            public string instanceId;
            public int devInstance;
            public int hwOccurence;
        }

        private XDocument InspectLineupFile(Stream stream)
        {
            // read the backup lineup file
            XDocument doc;
            using (var reader = new StreamReader(stream))
            {
                doc = XDocument.Parse(reader.ReadToEnd());
            }

            // collect all the devices that exist in the backup file
            var devices = doc.Descendants()
                .Where(arg => (arg.Name.LocalName.Equals("Device") || arg.Name.LocalName == "device"))
                .Where(arg => arg.Attribute("name") != null).ToList();
            if (devices.Count == 0)
            {
                MessageBox.Show(
                    "There are no tuners present in the backup file to restore.\n\nRestore function is aborted.",
                    "Restore Aborted", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }

            // collect all the enabled tuners that exist in the registry
            var registryTuners = new List<tunerRecorders>();
            using (var tunersKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Service\Video\Tuners", false))
            {
                if (tunersKey != null)
                {
                    foreach (var tunerGroupName in tunersKey.GetSubKeyNames().Where(arg => !arg.Equals("DVR")))
                    {
                        using (var tunerGroupKey = tunersKey.OpenSubKey(tunerGroupName))
                        {
                            foreach (var tunerKey in tunerGroupKey.GetSubKeyNames())
                            {
                                using (var tuner = tunerGroupKey.OpenSubKey(tunerKey))
                                {
                                    using (var usrSetting = tuner.OpenSubKey("UserSettings"))
                                    {
                                        if (usrSetting == null || (int)usrSetting.GetValue("EnabledForMCE", 0) == 0) continue;
                                    }
                                    registryTuners.Add(new tunerRecorders
                                    {
                                        recorderId = tunerKey.Substring(1, 36).ToLower(),
                                        devName = (string)tuner.GetValue("DevName"),
                                        rootDevice = (string)tuner.GetValue("RootDevice"),
                                        instanceId = (string)tuner.GetValue("TunerInstanceId"),
                                        devInstance = (int)tuner.GetValue("DevInstance")
                                    });
                                }
                            }
                        }
                    }
                }
            }

            // check that recorder settings are setup
            var recorders = new List<string>();
            using (var recordingSettings = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings", false))
            {
                recorders = recordingSettings.GetSubKeyNames().Where(arg => arg.StartsWith("RecorderSettings")).ToList();
            }

            // determine whether to abort or not
            if (registryTuners.Count == 0 || recorders.Count == 0)
            {
                MessageBox.Show(
                    "There are no tuners/recorders initialized for WMC on this machine. You must complete WMC TV Setup to at least the 'Scan for channels' stage before restoring this backup.\n" +
                    "\nRestore function is aborted.",
                    "Restore Aborted", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }

            // assign hardware count suffix to devname if needed to match mxf device names
            registryTuners = registryTuners.OrderBy(arg => arg.devInstance).ToList();
            foreach (var tuner in registryTuners)
            {
                if (tuner.hwOccurence != 0) continue;

                var matches = registryTuners.Where(arg => arg.devName == tuner.devName).ToList();
                if (matches.Count == 0) continue;
                if (matches.Count == 1)
                {
                    matches[0].singleTunerGroup = true;
                    continue;
                }

                var i = 1;
                foreach (var match in matches)
                {
                    match.hwOccurence = i++;
                }
            }

            // match backup lineup file devices with registry tuners
            var unmatchedTuners = new List<string>();
            foreach (var device in devices)
            {
                var regTuner = registryTuners.FirstOrDefault(arg => !arg.matched && device.Attribute("name").Value.Equals(arg.ToString())) ?? registryTuners.FirstOrDefault(arg => !arg.matched && arg.singleTunerGroup && device.Attribute("name").Value.Equals($"{arg} #1"));
                if (regTuner != null)
                {
                    regTuner.matched = true;
                    if (regTuner.recorderId.Equals(device.Attribute("recorderId").Value)) continue;

                    device.SetAttributeValue("recorderId", regTuner.recorderId.ToLower());
                    var contentRecorder = device.Element("contentRecorder");
                    if (contentRecorder == null) continue;
                    contentRecorder.SetAttributeValue("uid", $"!Recorders!{regTuner.rootDevice ?? regTuner.recorderId}{regTuner.instanceId ?? ""}");
                    contentRecorder.SetAttributeValue("instanceId", $"{regTuner.rootDevice ?? regTuner.recorderId}{regTuner.instanceId ?? ""}");
                    contentRecorder.SetAttributeValue("hardwareBaseId", $"{regTuner.rootDevice ?? ""}");

                }
                else
                {
                    unmatchedTuners.Add(device.Attribute("name").Value);
                }
            }

            // if any device in backup lineup file does not exist in registry, abort
            if (unmatchedTuners.Count > 0 && !restoreOverride)
            {
                unmatchedTuners.Sort();
                MessageBox.Show(
                    "The following tuners in the backup file do not exist on the host machine.\n" +
                    $"\n{string.Join("\n", unmatchedTuners)}\n\nRestore function is aborted.",
                    "Restore Aborted", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }

            // report any unmatched tuners still in registry
            var unmatchedDevices = registryTuners.Where(arg => !arg.matched).ToList();
            if (unmatchedDevices.Count > 0 && !restoreOverride)
            {
                var deviceNames = unmatchedDevices.Select(device => device.devName).ToList();
                deviceNames.Sort();
                if (DialogResult.No == MessageBox.Show(
                    "The following tuners exist on the host machine, but are not present in the backup file and will not be configured in WMC.\n" +
                    $"\n{string.Join("\n", deviceNames)}\n\nDo you wish to proceed?",
                    "Approval to Proceed", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) return null;
            }

            return doc;
        }

        private string DeleteActiveDatabaseFile()
        {
            // determine current instance and build database name
            var database = WmcStore.GetStoreFilename();

            // ensure there is a database to rebuild
            if (!string.IsNullOrEmpty(database))
            {
                // free the database and delete it
                if (!DeleteeHomeFile(database))
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show("Failed to delete the database file. Try again or consider trying in Safe Mode.", "Failed Operation", MessageBoxButtons.OK);
                    return null;
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
                MessageBox.Show("There is no database to rebuild.", "Failed Rebuild", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return null;
            }

            // open a new object store which creates a fresh database
            // if that fails for some reason, open WMC to create a new database
            if (WmcStore.WmcObjectStore != null) return database;
            var startInfo = new ProcessStartInfo()
            {
                FileName = $"{Environment.ExpandEnvironmentVariables("%WINDIR%")}\\ehome\\ehshell.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForInputIdle();
            proc?.Kill();
            return database;
        }

        private static bool DeleteeHomeFile(string filename)
        {
            // delete the database file
            try
            {
                foreach (var proc in FileUtil.WhoIsLocking(filename))
                {
                    proc.Kill();
                    proc.WaitForExit(1000);
                }
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{Helper.ReportExceptionMessages(ex)}");
                return false;
            }
            return true;
        }
        #endregion

        #region ========== Rebuild Database ===========
        bool rebuild;
        private void btnRebuild_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                Helper.WriteEButtonFile("rebuild");
                RestartClient(true);
                return;
            }

            // give warning
            if (MessageBox.Show("You are about to delete and rebuild the WMC EPG database. All tuners, recording schedules, favorite lineups, and logos will be restored. The Guide Listings will be empty until an MXF file is imported.\n\nClick 'OK' to continue.", "Database Rebuild", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) return;

            // set cursor and disable the containers so no buttons can be clicked
            Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // start the thread and wait for it to complete
            var backupThread = new Thread(BackupBackupFiles);
            backupThread.Start();
            while (!backupThread.IsAlive) ;
            while (backupThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            // clear all listviews and comboboxes
            IsolateEpgDatabase();

            // start the thread and wait for it to complete
            rebuild = true;
            var restoreThread = new Thread(RestoreBackupFiles);
            restoreThread.Start();
            while (!restoreThread.IsAlive) ;
            while (restoreThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            rebuild = false;

            // build the listviews
            btnRefreshLineups_Click(null, null);

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Cursor = Cursors.Arrow;

            // initialize an import
            btnImport_Click(null, null);
        }
        #endregion

        #region ========== Notifier ==========
        private void btnEmail_Click(object sender, EventArgs e)
        {
            var emailForm = new frmEmail();
            emailForm.ShowDialog();
        }

        private void btnStorage_Click(object sender, EventArgs e)
        {
            var storageForm = new frmStorage();
            storageForm.ShowDialog();
        }
        #endregion
    }

    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo?.SetValue(control, enable, null);
        }
    }
}