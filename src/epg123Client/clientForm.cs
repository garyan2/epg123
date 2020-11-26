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
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.Store.MXF;
using epg123Client.Properties;
using epg123Client;

namespace epg123
{
    public partial class clientForm : Form
    {
        #region ========== Form Opening ==========
        private ListViewColumnSorter mergedChannelColumnSorter = new ListViewColumnSorter();
        private ListViewColumnSorter lineupChannelColumnSorter = new ListViewColumnSorter();
        private epgTaskScheduler task = new epgTaskScheduler();
        private bool forceExit = false;
        public bool restartClientForm = false;
        public bool restartAsAdmin = false;
        double dpiScaleFactor = 1.0;

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
            using (Graphics g = CreateGraphics())
            {
                if ((g.DpiX != 96) || (g.DpiY != 96))
                {
                    dpiScaleFactor = g.DpiX / 96;

                    // adjust combobox widths
                    cmbSources.DropDownHeight = (int)(dpiScaleFactor * cmbSources.DropDownHeight);
                    cmbSources.DropDownWidth = (int)(dpiScaleFactor * cmbSources.DropDownWidth);
                    cmbSources.Size = new Size((int)(dpiScaleFactor * cmbSources.Width), cmbSources.Size.Height);

                    cmbObjectStoreLineups.DropDownHeight = (int)(dpiScaleFactor * cmbObjectStoreLineups.DropDownHeight);
                    cmbObjectStoreLineups.DropDownWidth = (int)(dpiScaleFactor * cmbObjectStoreLineups.DropDownWidth);
                    cmbObjectStoreLineups.Size = new Size((int)(dpiScaleFactor * cmbObjectStoreLineups.Width), cmbObjectStoreLineups.Size.Height);
                }
            }

            mergedChannelToolStrip.ImageScalingSize = new Size((int)(dpiScaleFactor * 16), (int)(dpiScaleFactor * 16));
            lineupChannelToolStrip.ImageScalingSize = new Size((int)(dpiScaleFactor * 16), (int)(dpiScaleFactor * 16));

            // ensure WMC is installed
            if (!File.Exists(Helper.EhshellExeFilePath))
            {
                MessageBox.Show("WMC is not present on this machine. Closing EPG123 Client Guide Tool.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.Close();
            }
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
            splitContainer2.Panel1MinSize = (int)(splitContainer2.Panel1MinSize * dpiScaleFactor);
            splitContainer1.Panel1MinSize = grpScheduledTask.Width + 6;

            // restore window position and sizes
            if ((Settings.Default.WindowLocation != null) && (Settings.Default.WindowLocation != new Point(-1, -1)))
            {
                this.Location = Settings.Default.WindowLocation;
            }
            if (Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
            }
            if (Settings.Default.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            if (Settings.Default.SplitterDistance != 0)
            {
                splitContainer1.SplitterDistance = Settings.Default.SplitterDistance;
            }

            // throw the version number into the title
            string[] version = Helper.epg123Version.Split('.');
            this.Text += $" v{version[0]}.{version[1]}.{version[2]}";
        }

        private void clientForm_Shown(object sender, EventArgs e)
        {
            // flag initial form load building
            Application.UseWaitCursor = true;

            // update task panel
            updateTaskPanel();

            // if client was started as elevated to perform an action
            if (Helper.UserHasElevatedRights && File.Exists(Helper.EButtonPath))
            {
                Application.UseWaitCursor = false;
                using (StreamReader sr = new StreamReader(Helper.EButtonPath))
                {
                    string line = sr.ReadLine();
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
                    sr.Close();
                }

                try
                {
                    File.Delete(Helper.EButtonPath);
                }
                catch { }
            }

            // double buffer list views
            mergedChannelListView.DoubleBuffered(true);
            lineupChannelListView.DoubleBuffered(true);

            // populate listviews
            if (WmcStore.WmcObjectStore != null)
            {
                mergedChannelListView.Refresh();
                lineupChannelListView.Refresh();

                buildLineupChannelListView();
                buildScannedLineupComboBox();
                buildMergedChannelListView();
                btnImport.Enabled = true;
            }
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            Application.UseWaitCursor = false;
        }
        #endregion

        #region ========== Form Closing and Restarting ==========
        private void restartClient(bool forceElevated = false)
        {
            try
            {
                // save the windows size and locations
                saveFormWindowParameters();

                // set flags
                restartClientForm = true;
                restartAsAdmin = Helper.UserHasElevatedRights || forceElevated;

                // close this process
                forceExit = true;
                Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
        }

        private void clientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            // check to see if any EPG123 lineups are mapped in the database
            if (!forceExit)
            {
                if (cmbObjectStoreLineups.Items.Count > 0 && cmbObjectStoreLineups.FindString("EPG123") > -1 && mergedChannelListView.Items.Count > 0)
                {
                    bool g2g = false;
                    foreach (myChannelLvi mergedChannel in AllMergedChannels)
                    {
                        if (mergedChannel != null && mergedChannel.SubItems[3].Text.StartsWith("EPG123"))
                        {
                            g2g = true;
                            break;
                        }
                    }

                    if (!g2g)
                    {
                        if (DialogResult.No == MessageBox.Show("It does not appear any EPG123 lineup service guide listings (right side) are associated with any guide channels (left side). You can manually \"Subscribe\" listings to channels or you can use the automated Match by: [# Number] button.\n\nDo you still wish to exit the Client Guide Tool?", "No EPG123 guide listings in WMC", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
                        {
                            Application.UseWaitCursor = false;
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }

            // save the windows size and locations
            saveFormWindowParameters();
            isolateEpgDatabase(!forceExit);
        }

        private void saveFormWindowParameters()
        {
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    Settings.Default.WindowLocation = this.Location;
                    Settings.Default.WindowSize = this.Size;
                }
                else
                {
                    Settings.Default.WindowLocation = RestoreBounds.Location;
                    Settings.Default.WindowSize = RestoreBounds.Size;
                }
                Settings.Default.WindowMaximized = (this.WindowState == FormWindowState.Maximized);
                Settings.Default.SplitterDistance = splitContainer1.SplitterDistance;
                Settings.Default.Save();
            }
            catch { }
        }

        private void isolateEpgDatabase(bool disposeStore = false)
        {
            // clear the merged channels and lineup combobox
            mergedChannelListView.BeginUpdate();
            cmbSources.Items.Clear();
            mergedChannelListView.VirtualListSize = 0;
            MergedChannelFilter.Clear();
            foreach (myChannelLvi lvi in AllMergedChannels)
            {
                lvi.RemoveDelegate();
            }
            AllMergedChannels?.Clear();
            mergedChannelListView.Items.Clear();
            mergedChannelListView.EndUpdate();

            // clear the lineups
            lineupChannelListView.BeginUpdate();
            cmbObjectStoreLineups.Items.Clear();
            lineupChannelListView.VirtualListSize = 0;
            lineupListViewItems.Clear();
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
        private void updateTaskPanel()
        {
            // get status
            task.queryTask();

            // set .Enabled properties
            if (!task.exist && !task.existNoAccess)
            {
                rdoFullMode.Enabled = File.Exists(Helper.Epg123ExePath) || File.Exists(Helper.Hdhr2mxfExePath);
                rdoClientMode.Enabled = tbSchedTime.Enabled = lblUpdateTime.Enabled = cbTaskWake.Enabled = cbAutomatch.Enabled = tbTaskInfo.Enabled = true;
            }
            else
            {
                rdoFullMode.Enabled = rdoClientMode.Enabled = tbSchedTime.Enabled = lblUpdateTime.Enabled = cbTaskWake.Enabled = cbAutomatch.Enabled = tbTaskInfo.Enabled = false;
            }

            // set radio button controls
            rdoFullMode.Checked = (task.exist && (task.actions[0].Path.ToLower().Contains("epg123.exe") || task.actions[0].Path.ToLower().Contains("hdhr2mxf.exe"))) ||
                                  (!task.exist && (File.Exists(Helper.Epg123ExePath) || File.Exists(Helper.Hdhr2mxfExePath)));
            rdoClientMode.Checked = !rdoFullMode.Checked;

            // update scheduled task run time
            tbSchedTime.Text = task.schedTime.ToString("HH:mm");

            // set sheduled task wake checkbox
            cbTaskWake.Checked = task.wake;

            // determine which action is the client action
            int clientIndex = -1;
            if (task.exist)
            {
                for (int i = 0; i < task.actions.Length; ++i)
                {
                    if (task.actions[i].Path.ToLower().Contains("epg123client.exe")) clientIndex = i;
                }
            }

            // verify task configuration with respect to this executable
            if (clientIndex >= 0 && !task.actions[clientIndex].Path.ToLower().Replace("\"", "").Equals(Helper.Epg123ClientExePath.ToLower()))
            {
                MessageBox.Show(string.Format("The location of this program file is not the same location configured in the Scheduled Task.\n\nThis program:\n{0}\n\nTask program:\n{1}",
                                              Helper.Epg123ExePath, task.actions[clientIndex].Path), "Configuration Warning", MessageBoxButtons.OK);
            }

            // set automatch checkbox state
            cbAutomatch.Checked = !task.exist || ((clientIndex >= 0) && task.actions[clientIndex].Arguments.ToLower().Contains("-match"));

            // set task info text and label
            if (task.exist && rdoFullMode.Checked)
            {
                tbTaskInfo.Text = task.actions[0].Path;
            }
            else if (task.exist && (clientIndex >= 0))
            {
                string arg = task.actions[clientIndex].Arguments;
                tbTaskInfo.Text = arg.Substring(arg.ToLower().IndexOf("-i") + 3,
                                                arg.ToLower().IndexOf(".mxf") - arg.ToLower().IndexOf("-i") + 1).TrimStart('\"');
            }
            else if (task.exist)
            {
                tbTaskInfo.Text = "*** UNKNOWN TASK CONFIGURATION ***";
            }

            // set task create/delete button text and update status string
            btnTask.Text = (task.exist || task.existNoAccess) ? "Delete" : "Create";
            if (task.exist && (clientIndex >= 0))
            {
                lblSchedStatus.Text = task.statusString;
                lblSchedStatus.ForeColor = System.Drawing.Color.Black;
            }
            else if (task.exist && rdoFullMode.Enabled)
            {
                lblSchedStatus.Text = "### Server Mode ONLY - Guide will not be imported. ###";
                lblSchedStatus.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                lblSchedStatus.Text = task.statusString;
                lblSchedStatus.ForeColor = System.Drawing.Color.Red;
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
                else if (File.Exists(Helper.Hdhr2mxfExePath))
                {
                    tbTaskInfo.Text = Helper.Hdhr2mxfExePath;
                }
                else
                {
                    tbTaskInfo.Text = "*** Click here to set executable file path. ***";
                }
            }
            else
            {
                if (File.Exists(Helper.Epg123MxfPath))
                {
                    tbTaskInfo.Text = Helper.Epg123MxfPath;
                }
                else
                {
                    tbTaskInfo.Text = "*** Click here to set MXF file path. ***";
                }
            }
        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            if (sender != null) // null sender means we restarted to finish in administrator mode
            {
                // missing information
                if (!task.exist && tbTaskInfo.Text.StartsWith("***"))
                {
                    tbTaskInfo_Click(null, null);
                }

                // create new task if file location is valid
                if ((!task.exist) && !tbTaskInfo.Text.StartsWith("***"))
                {
                    // create task using epg123.exe & epg123Client.exe
                    if (rdoFullMode.Checked)
                    {
                        epgTaskScheduler.TaskActions[] actions = new epgTaskScheduler.TaskActions[2];
                        actions[0].Path = tbTaskInfo.Text;
                        actions[0].Arguments = "-update";
                        actions[1].Path = Helper.Epg123ClientExePath;
                        actions[1].Arguments = string.Format("-i \"{0}\"", Helper.Epg123MxfPath) + ((cbAutomatch.Checked) ? " -match" : null);
                        task.createTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                    }
                    // create task using epg123Client.exe
                    else
                    {
                        epgTaskScheduler.TaskActions[] actions = new epgTaskScheduler.TaskActions[1];
                        actions[0].Path = Helper.Epg123ClientExePath;
                        actions[0].Arguments = string.Format("-i \"{0}\"", tbTaskInfo.Text) + ((cbAutomatch.Checked) ? " -match" : null);
                        task.createTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                    }
                }
            }

            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                if (task.exist || task.existNoAccess)
                {
                    Helper.WriteEButtonFile("deleteTask");
                }
                else
                {
                    Helper.WriteEButtonFile("createTask");
                }
                restartClient(true);
                return;
            }
            else if (task.exist)
            {
                task.deleteTask();
            }
            else
            {
                task.importTask();
            }

            // update panel with current information
            updateTaskPanel();
        }

        private void tbTaskInfo_Click(object sender, EventArgs e)
        {
            // don't modify if text box is displaying current existing task
            if (task.exist || task.existNoAccess) return;

            // determine path to existing file
            if (tbTaskInfo.Text.StartsWith("***"))
            {
                openFileDialog1.InitialDirectory = Helper.Epg123OutputFolder;
            }
            else
            {
                openFileDialog1.InitialDirectory = tbTaskInfo.Text.Substring(0, tbTaskInfo.Text.LastIndexOf('\\'));
            }
            openFileDialog1.Filter = (rdoFullMode.Checked) ? "EPG123 Executable|*.exe" : "MXF File|*.mxf";
            openFileDialog1.Title = (rdoFullMode.Checked) ? "Select the EPG123 Executable" : "Select a MXF File";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbTaskInfo.Text = openFileDialog1.FileName;

                // if directed to a MXF file, ask if user wants to import immediately
                if (!rdoFullMode.Checked)
                {
                    if (DialogResult.Yes == MessageBox.Show("Do you wish to import the guide listings now? If not, you can click the [Manual Import] button later or allow the scheduled task to perform the import.", "Import MXF File", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        btnImport_Click(null, null);
                    }
                }
            }
        }
        #endregion

        #region ========== Virtual ListView Events ==========
        private void mergedChannelListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && mergedChannelListView.SelectedIndices.Count > 0)
            {
                e.Handled = true;
                bool state = ((myChannelLvi)mergedChannelListView.Items[mergedChannelListView.SelectedIndices[0]]).Checked;
                foreach (int index in mergedChannelListView.SelectedIndices)
                {
                    WmcStore.SetChannelEnableState(AllMergedChannels[MergedChannelFilter[index]].ChannelId, !state);
                }
            }
            if (e.KeyCode == Keys.Delete && mergedChannelListView.SelectedIndices.Count > 0)
            {
                e.Handled = true;
                btnDeleteChannel_Click(null, null);
            }
            if (e.KeyCode == Keys.A && e.Control)
            {
                e.Handled = true;
                NativeMethods.SelectAllItems(mergedChannelListView);
            }
        }

        private void mergedChannelListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;

            // quirk using virtual listview. to show empty checkbox, must first check it then uncheck it
            if (!e.Item.Checked)
            {
                e.Item.Checked = true;
                e.Item.Checked = false;
            }
        }

        private void mergedChannelListView_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = ((ListView)sender).GetItemAt(e.X, e.Y);
            if (lvi != null)
            {
                // if it is the checkbox
                if (e.X < (lvi.Bounds.Left + 16))
                {
                    WmcStore.SetChannelEnableState(((myChannelLvi)lvi).ChannelId, !lvi.Checked);
                }
            }
        }

        private void mergedChannelListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = ((ListView)sender).GetItemAt(e.X, e.Y);
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
            e.DrawDefault = true;
        }
        #endregion

        #region ========== ListView Sorter and Column Widths ==========
        private void lvLineupSort(object sender, ColumnClickEventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            // determine which listview
            if (sender.Equals(mergedChannelListView))
            {
                // Determine if clicked column is already the column that is being sorted.
                if (e.Column == mergedChannelColumnSorter.SortColumn)
                {
                    // Reverse the current sort direction for this column.
                    if (mergedChannelColumnSorter.Order == SortOrder.Ascending)
                    {
                        mergedChannelColumnSorter.Order = SortOrder.Descending;
                    }
                    else
                    {
                        mergedChannelColumnSorter.Order = SortOrder.Ascending;
                    }
                }
                else
                {
                    // Set the column number that is to be sorted; default to ascending.
                    mergedChannelColumnSorter.SortColumn = e.Column;
                    mergedChannelColumnSorter.Order = SortOrder.Ascending;
                }

                // Perform the sort with these new sort options.
                AllMergedChannels.Sort(mergedChannelColumnSorter);
                FilterMergedChannels();
                mergedChannelListView.Refresh();
            }
            else if (sender.Equals(lineupChannelListView))
            {
                // Determine if clicked column is already the column that is being sorted.
                if (e.Column == lineupChannelColumnSorter.SortColumn)
                {
                    // Reverse the current sort direction for this column.
                    if (lineupChannelColumnSorter.Order == SortOrder.Ascending)
                    {
                        lineupChannelColumnSorter.Order = SortOrder.Descending;
                    }
                    else
                    {
                        lineupChannelColumnSorter.Order = SortOrder.Ascending;
                    }
                }
                else
                {
                    // Set the column number that is to be sorted; default to ascending.
                    lineupChannelColumnSorter.SortColumn = e.Column;
                    lineupChannelColumnSorter.Order = SortOrder.Ascending;
                }

                // Perform the sort with these new sort options.
                lineupListViewItems.Sort(lineupChannelColumnSorter);
                lineupChannelListView.Refresh();
            }
            Cursor = Cursors.Arrow;
        }

        private void adjustColumnWidths(ListView listView)
        {
            int[] minWidths = { 60, 60, 100, 100, 100, 60 , 60 };
            foreach (ColumnHeader header in listView.Columns)
            {
                int currentWidth = header.Width;
                header.Width = -1;
                header.Width = Math.Max(Math.Max(header.Width, currentWidth), (int)(minWidths[header.Index] * dpiScaleFactor));
            }
        }
        #endregion

        #region ========== ListView Menu Strip ==========
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // only enable the subscribe menu if a lineup service and merged channel has been selected
            subscribeMenuItem.Enabled = (lineupChannelListView.SelectedIndices.Count > 0) && (mergedChannelListView.SelectedIndices.Count > 0);

            // determine which menu items are visible based on select listview channel
            bool mergedChannelMenuStrip = (((ContextMenuStrip)sender).SourceControl.Name == mergedChannelListView.Name);
            unsubscribeMenuItem.Visible = mergedChannelMenuStrip;
            toolStripSeparator2.Visible = mergedChannelMenuStrip;
            renameMenuItem.Visible = mergedChannelMenuStrip;
            renumberMenuItem.Visible = mergedChannelMenuStrip;
            toolStripSeparator6.Visible = mergedChannelMenuStrip;
            clipboardMenuItem.Visible = mergedChannelMenuStrip;
            clearListingsMenuItem.Visible = mergedChannelMenuStrip;

            // only enable rename menu item if a single channel has been selected
            renameMenuItem.Enabled = (mergedChannelListView.SelectedIndices.Count == 1) && customLabelsOnly;
            renumberMenuItem.Enabled = (mergedChannelListView.SelectedIndices.Count == 1) && customLabelsOnly;
        }

        #region ===== Subscribe/Unsubscribe Channel =====
        private void subscribeMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            bool unsubscribe = unsubscribeMenuItem.Equals((ToolStripMenuItem)sender);

            // subscribe selected channels to station
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                WmcStore.SubscribeLineupChannel(unsubscribe ? 0 : ((myLineupLvi)lineupChannelListView.Items[lineupChannelListView.SelectedIndices[0]]).ChannelId, AllMergedChannels[MergedChannelFilter[index]].ChannelId);
            }

            // clear all selections
            mergedChannelListView.SelectedIndices.Clear();
            lineupChannelListView.SelectedIndices.Clear();

            this.Cursor = Cursors.Arrow;
        }
        #endregion

        #region ===== Rename Channel =====
        private void renameMenuItem_Click(object sender, EventArgs e)
        {
            // record current callsign for potential restore
            selectedItemForEdit = mergedChannelListView.SelectedIndices[0];

            // enable label editing
            mergedChannelListView.LabelEdit = true;

            // begin edit
            mergedChannelListView.Items[selectedItemForEdit].BeginEdit();
        }

        private void mergedChannelListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // disable label editing
            mergedChannelListView.LabelEdit = false;

            // update callsign
            if (e.Label != null)
            {
                WmcStore.SetChannelCustomCallsign(((myChannelLvi)mergedChannelListView.Items[selectedItemForEdit]).ChannelId, e.Label);
            }
        }
        #endregion

        #region ===== Renumber Channel =====
        private int selectedItemForEdit;

        private void renumberMenuItem_Click(object sender, EventArgs e)
        {
            // get bounds of number field
            selectedItemForEdit = mergedChannelListView.SelectedIndices[0];
            Rectangle box = mergedChannelListView.Items[selectedItemForEdit].SubItems[1].Bounds;
            lvEditTextBox.SetBounds(box.Left + mergedChannelListView.Left + 2,
                                    box.Top + mergedChannelListView.Top,
                                    box.Width, box.Height);
            lvEditTextBox.Show();
            lvEditTextBox.Focus();
        }

        private void lvEditTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // make sure it is numbers only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.')) e.Handled = true;
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1)) e.Handled = true;

            // decide on what to do on a return or escape
            switch (e.KeyChar)
            {
                case (char)Keys.Return:
                    lvEditTextBox.Hide();
                    WmcStore.SetChannelCustomNumber(((myChannelLvi)mergedChannelListView.Items[selectedItemForEdit]).ChannelId, lvEditTextBox.Text);
                    e.Handled = true;
                    break;
                case (char)Keys.Escape:
                    lvEditTextBox.Text = null;
                    lvEditTextBox.Hide();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ===== Copy to Clipboard =====
        private void clipboardMenuItem_Click(object sender, EventArgs e)
        {
            string TextToAdd = "Call Sign\tNumber\tService Name\tSubscribed Lineup\tScanned Source(s)\tTuningInfo\tMatchName\tService Callsign\r\n";
            foreach (int index in MergedChannelFilter)
            {
                string matchname = string.Empty;
                string callsign = string.Empty;
                MergedChannel mergedChannel = WmcStore.WmcObjectStore.Fetch(AllMergedChannels[index].ChannelId) as MergedChannel;
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
                TextToAdd += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\r\n",
                    AllMergedChannels[index].SubItems[0].Text,
                    AllMergedChannels[index].SubItems[1].Text,
                    AllMergedChannels[index].SubItems[2].Text,
                    AllMergedChannels[index].SubItems[3].Text,
                    AllMergedChannels[index].SubItems[4].Text,
                    AllMergedChannels[index].SubItems[5].Text,
                    matchname, callsign);
            }
            Clipboard.SetText(TextToAdd);
        }
        #endregion

        #region ===== Clear Guide Listings =====
        private void btnClearScheduleEntries(object sender, EventArgs e)
        {
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                long channelId = AllMergedChannels[MergedChannelFilter[index]].ChannelId;
                WmcStore.ClearServiceScheduleEntries(channelId);
            }
        }
        #endregion
        #endregion

        #region ========== Channel/Listings AutoMapping ==========
        private void btnAutoMatch_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            foreach (myChannelLvi mergedChannel in AllMergedChannels)
            {
                if (((ToolStripButton)sender).Equals(btnAutoCallsign))
                {
                    string callsign = mergedChannel.SubItems[0].Text;
                    if (string.IsNullOrEmpty(callsign)) continue;

                    List<myLineupLvi> lineupChannels = lineupListViewItems.Where(arg => arg.callsign.Equals(callsign)).ToList();
                    if (lineupChannels != null && lineupChannels.Count > 0)
                    {
                        foreach (myLineupLvi channel in lineupChannels)
                        {
                            WmcStore.SubscribeLineupChannel(channel.ChannelId, mergedChannel.ChannelId);
                        }
                    }
                }
                else if (((ToolStripButton)sender).Equals(btnAutoNumber))
                {
                    List<myLineupLvi> lineupChannels = lineupListViewItems.Where(arg => arg.number.Equals(mergedChannel.SubItems[1].Text)).ToList();
                    if (lineupChannels != null && lineupChannels.Count > 0)
                    {
                        foreach (myLineupLvi channel in lineupChannels)
                        {
                            WmcStore.SubscribeLineupChannel(channel.ChannelId, mergedChannel.ChannelId);
                        }
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        #endregion

        #region ========== Lineup ListView Management ==========
        List<myLineupLvi> lineupListViewItems = new List<myLineupLvi>();

        private void lineupChannelListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (lineupListViewItems.Count == 0) e.Item = new ListViewItem();
            else e.Item = lineupListViewItems[e.ItemIndex];
        }

        private void buildLineupChannelListView()
        {
            // clear the pulldown list
            cmbObjectStoreLineups.Items.Clear();

            // populate with lineups in object_store
            foreach (myLineup lineup in WmcStore.GetWmisLineups())
            {
                cmbObjectStoreLineups.Items.Add(lineup);                
            }

            // preset value to epg123 lineup if exists
            cmbObjectStoreLineups.SelectedIndex = cmbObjectStoreLineups.FindString("EPG123");
            if (cmbObjectStoreLineups.Items.Count > 0)
            {
                if (cmbObjectStoreLineups.SelectedIndex < 0) cmbObjectStoreLineups.SelectedIndex = 0;
                btnDeleteLineup.Enabled = (cmbObjectStoreLineups.Items.Count > 0);
            }
            else if (cmbObjectStoreLineups.Items.Count == 0)
            {
                cmbObjectStoreLineups.SelectedIndex = -1;
                lineupChannelListView.Clear();
            }
        }

        private void lineupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // pause listview drawing
            this.Cursor = Cursors.WaitCursor;
            lineupChannelListView.BeginUpdate();

            // clear the lineup channel listview
            lineupListViewItems?.Clear();
            lineupChannelListView?.Items.Clear();

            // populate with new lineup channels
            lineupListViewItems.AddRange(WmcStore.GetLineupChannels(((myLineup)cmbObjectStoreLineups.Items[cmbObjectStoreLineups.SelectedIndex]).LineupId));
            if ((lineupChannelListView.VirtualListSize = lineupListViewItems.Count) > 0)
            {
                lineupChannelListView.TopItem = lineupChannelListView.Items[0];
            }
            lineupChannelListView.SelectedIndices.Clear();

            // adjust column widths
            adjustColumnWidths(lineupChannelListView);

            // reset sorting column and order
            lineupChannelColumnSorter.Order = SortOrder.Ascending;
            lineupChannelColumnSorter.SortColumn = 1;
            lineupListViewItems.Sort(lineupChannelColumnSorter);

            // resume listview drawing
            lineupChannelListView.EndUpdate();
            this.Cursor = Cursors.Arrow;

            // update the status bar
            updateStatusBar();
        }

        private void btnRefreshLineups_Click(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;

            splitContainer1.Enabled = splitContainer2.Enabled = false;
            isolateEpgDatabase();
            buildScannedLineupComboBox();
            buildMergedChannelListView();
            buildLineupChannelListView();
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            btnImport.Enabled = AllMergedChannels.Count > 0;

            Application.UseWaitCursor = false;
            updateStatusBar();
        }

        private void btnDeleteLineupClick(object sender, EventArgs e)
        {
            string prompt = $"The lineup \"{cmbObjectStoreLineups.SelectedItem}\" will be removed from the Media Center database. Do you wish to continue?";
            if (MessageBox.Show(prompt, "Delete Channel(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            Application.UseWaitCursor = true;
            mergedChannelListView.BeginUpdate();

            WmcStore.UnsubscribeChannelsInLineup(((myLineup)cmbObjectStoreLineups.SelectedItem).LineupId);
            WmcStore.DeleteLineup(((myLineup)cmbObjectStoreLineups.SelectedItem).LineupId);
            buildLineupChannelListView();

            mergedChannelListView.EndUpdate();
            Application.UseWaitCursor = false;
        }
        #endregion

        #region ========== Merged Channel ListView Management ==========
        List<myChannelLvi> AllMergedChannels = new List<myChannelLvi>();
        List<int> MergedChannelFilter = new List<int>();
        private bool enabledChannelsOnly = false;
        private bool customLabelsOnly = true;

        #region ===== Merged Channel ListView Items =====
        private void mergedChannelListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (AllMergedChannels.Count == 0) e.Item = new ListViewItem();
            else e.Item = AllMergedChannels[MergedChannelFilter[e.ItemIndex]];
        }

        private void buildMergedChannelListView()
        {
            if (WmcStore.WmcMergedLineup == null) return;

            // pause listview drawing
            mergedChannelListView.BeginUpdate();

            // build new everything
            if ((AllMergedChannels?.Count ?? 0) == 0)
            {
                // reset initial label to custom only
                if (!customLabelsOnly) btnCustomDisplay_Click(null, null);

                // reset sorting column and order
                mergedChannelColumnSorter.Order = SortOrder.Ascending;
                mergedChannelColumnSorter.SortColumn = 1;

                // notify something is going on
                lblToolStripStatus.Text = "Collecting merged channels...";
                statusStrip1.Refresh();

                // initialize list to appropriate size
                int channelCount = WmcStore.WmcMergedLineup.UncachedChannels.Count();
                AllMergedChannels = new List<myChannelLvi>(channelCount);
                lvItemsProgressBar.Maximum = channelCount;

                // reset and show progress bar
                IncrementProgressBar(true);

                // populate AllMergedChannels list
                foreach (MergedChannel channel in WmcStore.WmcMergedLineup.UncachedChannels)
                {
                    // increment progress bar
                    IncrementProgressBar();

                    // build default listviewitems
                    AllMergedChannels.Add(new myChannelLvi(channel));
                }
            }
            else
            {
                // reset and show progress bar
                lvItemsProgressBar.Maximum = AllMergedChannels.Count;
                IncrementProgressBar(true);
            }
            lvItemsProgressBar.Width = 0;

            // reset sorting column and order
            AllMergedChannels.Sort(mergedChannelColumnSorter);

            // filter merged channels based on selections
            FilterMergedChannels();

            // adjust column widths
            adjustColumnWidths(mergedChannelListView);

            // resume listview drawing
            mergedChannelListView.EndUpdate();

            // refresh status bar
            updateStatusBar();
        }

        private void FilterMergedChannels()
        {
            MergedChannelFilter = new List<int>(AllMergedChannels.Count);
            foreach (myChannelLvi channel in AllMergedChannels)
            {
                // no filtering
                if (!enabledChannelsOnly && cmbSources.SelectedIndex == 0)
                {
                    MergedChannelFilter.Add(AllMergedChannels.IndexOf(channel));
                }
                // enabled only
                else if (enabledChannelsOnly && cmbSources.SelectedIndex == 0 && channel.Enabled)
                {
                    MergedChannelFilter.Add(AllMergedChannels.IndexOf(channel));
                }
                // enabled and/or scanned source
                else if (cmbSources.SelectedIndex > 0 && channel.ScannedLineupIds.Contains(((myLineup)cmbSources.SelectedItem).LineupId) &&
                         (!enabledChannelsOnly || channel.Enabled))
                {
                    MergedChannelFilter.Add(AllMergedChannels.IndexOf(channel));
                }
            }
            MergedChannelFilter.TrimExcess();
            mergedChannelListView.VirtualListSize = MergedChannelFilter.Count;
        }
        #endregion

        #region ===== Scanned Lineup Combobox =====
        private void buildScannedLineupComboBox()
        {
            // clear combobox and add initial entry
            cmbSources.Items.Clear();
            cmbSources.Items.Add("All Scanned Sources");

            // get all sources and set initial selection
            foreach (myLineup source in WmcStore.GetDeviceLineupsAndIds())
            {
                cmbSources.Items.Add(source);
            }
            cmbSources.SelectedIndex = 0;
        }

        private void cmbSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (cmbSources.SelectedIndex < 0) return;
            buildMergedChannelListView();
            this.Cursor = Cursors.Default;
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
            if (lvItemsProgressBar.Width != mergedChannelListView.Parent.Width -1)
                lvItemsProgressBar.Width = mergedChannelListView.Parent.Width - 1;
        }

        private void updateStatusBar()
        {
            int totalServices = 0;
            foreach (myLineup lineup in cmbObjectStoreLineups.Items)
            {
                totalServices += lineup.ChannelCount;
            }
            lblToolStripStatus.Text = $"{AllMergedChannels.Count} Merged Channel(s) with {mergedChannelListView.VirtualListSize} shown  |  {cmbObjectStoreLineups.Items.Count} Lineup(s)  |  {totalServices} Service(s) with {lineupChannelListView.Items.Count} shown";
            statusStrip1.Refresh();
        }
        #endregion

        #region ===== Merged Channel Additional Filtering =====
        private void btnCustomDisplay_Click(object sender, EventArgs e)
        {
            customLabelsOnly = !customLabelsOnly;
            foreach (myChannelLvi item in AllMergedChannels)
            {
                item.ShowCustomLabels(customLabelsOnly);
            }
            mergedChannelListView.Invalidate();

            if (!customLabelsOnly)
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
            enabledChannelsOnly = !enabledChannelsOnly;

            this.Cursor = Cursors.WaitCursor;
            buildMergedChannelListView();
            this.Cursor = Cursors.Default;

            if (!enabledChannelsOnly)
            {
                btnChannelDisplay.BackColor = SystemColors.Control;
                btnChannelDisplay.ToolTipText = "Display Enabled Channels only";
            }
            else
            {
                btnChannelDisplay.BackColor = SystemColors.ControlDark;
                btnChannelDisplay.ToolTipText = "Display All Channels";
            }

            // update the status bar
            updateStatusBar();
        }
        #endregion

        #region ===== Buttons and Dials =====
        private void btnDeleteChannel_Click(object sender, EventArgs e)
        {
            string prompt = $"The selected {mergedChannelListView.SelectedIndices.Count} channel(s) will be removed from the Media Center database. Do you wish to continue?";
            if ((mergedChannelListView.SelectedIndices.Count == 0) ||
                MessageBox.Show(prompt, "Delete Channel(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            mergedChannelListView.BeginUpdate();

            // gather the listview items
            List<myChannelLvi> channelsToDelete = new List<myChannelLvi>();
            foreach (int index in mergedChannelListView.SelectedIndices)
            {
                channelsToDelete.Add(AllMergedChannels[MergedChannelFilter[index]]);
            }

            // delete the channel and remove from listview items
            mergedChannelListView.VirtualListSize -= channelsToDelete.Count;
            foreach (myChannelLvi channel in channelsToDelete)
            {
                channel.RemoveDelegate();
                AllMergedChannels.Remove(channel);
                WmcStore.DeleteChannel(channel.ChannelId);
            }
            WmcStore.WmcMergedLineup.FullMerge(false);
            AllMergedChannels.TrimExcess();
            FilterMergedChannels();

            mergedChannelListView.SelectedIndices.Clear();
            mergedChannelListView.EndUpdate();
            this.Cursor = Cursors.Arrow;

            updateStatusBar();
        }
        #endregion
        #endregion

        #region ========== Database Explorer ==========
        private void btnStoreExplorer_Click(object sender, EventArgs e)
        {
            // close the store
            isolateEpgDatabase(true);

            // used code by glugalug (glugglug) and modified for epg123
            // https://github.com/glugalug/GuideEditingMisc/tree/master/StoreExplorer

            SHA256Managed sha256Man = new SHA256Managed();
            string clientId = ObjectStore.GetClientId(true);
            string providerName = @"Anonymous!User";
            string password = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(clientId)));

            Assembly assembly = Assembly.LoadFile(Environment.ExpandEnvironmentVariables(@"%WINDIR%\ehome\mcstore.dll"));
            Module module = assembly.GetModules().First();
            Type formType = module.GetType("Microsoft.MediaCenter.Store.Explorer.StoreExplorerForm");
            Type storeType = module.GetType("Microsoft.MediaCenter.Store.ObjectStore");

            PropertyInfo friendlyNameProperty = storeType.GetProperty("FriendlyName", BindingFlags.Static | BindingFlags.Public);
            friendlyNameProperty.SetValue(null, providerName, null);
            PropertyInfo displayNameProperty = storeType.GetProperty("DisplayName", BindingFlags.Static | BindingFlags.Public);
            displayNameProperty.SetValue(null, password, null);

            MethodInfo defaultMethod = storeType.GetMethod("get_DefaultSingleton", BindingFlags.Static | BindingFlags.Public);
            object store = defaultMethod.Invoke(null, null);
            ConstructorInfo constructor = formType.GetConstructor(new Type[] { storeType });
            Form form = (Form)constructor.Invoke(new object[] { store });
            form.ShowDialog();

            // the store explorer form does a DisposeAll on the object store which basically breaks
            // the ObjectStore in the client. Closing form because I haven't found a way to reinit
            // the objectStore_ parameter.
            forceExit = true;
            this.Close();
        }
        #endregion

        #region ========== Export Database ==========
        private void btnExportMxf_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(Helper.Epg123OutputFolder)) Directory.CreateDirectory(Helper.Epg123OutputFolder);

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(new StreamWriter(Helper.Epg123OutputFolder + "\\mxfExport.mxf"), settings))
            {
                MxfExporter.Export(WmcStore.WmcObjectStore, writer, false);
            }
            this.Cursor = Cursors.Arrow;
        }
        #endregion

        #region ========== Child Forms ==========
        private void btnAddChannels_Click(object sender, EventArgs e)
        {
            using (frmAddChannel addChannelForm = new frmAddChannel())
            {
                addChannelForm.ShowDialog();
                if (addChannelForm.channelAdded)
                {
                    Logger.WriteInformation("Restarting EPG123 Client to avoid an external process crashing EPG123 10 seconds after adding channels.");
                    btnRefreshLineups_Click(null, null);
                }
            }
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                Helper.WriteEButtonFile("setup");
                restartClient(true);
                return;
            }

            // set cursor and disable the containers so no buttons can be clicked
            this.Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // prep the client setup form
            frmClientSetup frm = new frmClientSetup();
            frm.shouldBackup = (WmcStore.WmcObjectStore != null);

            // clear everything out
            isolateEpgDatabase(true);

            // open the form
            frm.ShowDialog();
            tbTaskInfo.Text = frm.hdhr2mxfSrv ? Helper.Hdhr2mxfExePath : Helper.Epg123ExePath;
            frm.Dispose();

            // build the listviews and make sure registries are good
            btnRefreshLineups_Click(null, null);

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            this.Cursor = Cursors.Arrow;
        }

        private void btnTransferTool_Click(object sender, EventArgs e)
        {
            // set cursor and disable the containers so no buttons can be clicked
            this.Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            Process.Start("epg123Transfer.exe").WaitForExit();

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            this.Cursor = Cursors.Arrow;
        }

        private void btnTweakWmc_Click(object sender, EventArgs e)
        {
            // open the tweak gui
            frmWmcTweak frm = new frmWmcTweak();
            frm.ShowDialog();
        }

        private void btnUndelete_Click(object sender, EventArgs e)
        {
            frmUndelete frmUndelete = new frmUndelete();
            frmUndelete.ShowDialog();
            if (frmUndelete.channelAdded)
            {
                Logger.WriteInformation("Restarting EPG123 Client to avoid an external process crashing EPG123 10 seconds after adding channels.");
                btnRefreshLineups_Click(null, null);
            }
        }
        #endregion

        #region ========== View Log ==========
        private void btnViewLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(Helper.Epg123TraceLogPath))
            {
                Process.Start("notepad.exe", Helper.Epg123TraceLogPath);
            }
        }
        #endregion

        #region ========== Manual Import and Reindex ==========
        private void btnImport_Click(object sender, EventArgs e)
        {
            // verify tuners are set up
            if (AllMergedChannels.Count == 0)
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
            if (WmcUtilities.DetermineRecordingsInProgress())
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
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
            {
                FileName = string.Empty,
                Filter = "MXF File|*.mxf",
                Title = "Select a MXF File",
                Multiselect = false
            };

            // determine initial path
            if (Directory.Exists(Helper.Epg123OutputFolder) && (tbTaskInfo.Text.Equals(Helper.Epg123ExePath) || tbTaskInfo.Text.Equals(Helper.Hdhr2mxfExePath)))
            {
                openFileDialog1.InitialDirectory = Helper.Epg123OutputFolder;
            }
            else if (!tbTaskInfo.Text.StartsWith("***"))
            {
                openFileDialog1.InitialDirectory = tbTaskInfo.Text.Substring(0, tbTaskInfo.Text.LastIndexOf('\\'));
            }

            // open the dialog
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            // perform the file import with progress form
            Logger.eventID = 0;
            statusLogo.mxfFile = openFileDialog1.FileName;
            frmImport importForm = new frmImport(openFileDialog1.FileName);
            importForm.ShowDialog();

            // kick off the reindex
            if (importForm.success)
            {
                WmcUtilities.ReindexDatabase();
            }
            else
            {
                MessageBox.Show("There was an error importing the MXF file.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            statusLogo.statusImage();

            // make sure guide is activated and background scanning is stopped
            WmcStore.ActivateEpg123LineupsInStore();
            WmcRegistries.ActivateGuide();

            // open object store and repopulate the GUI
            btnRefreshLineups_Click(null, null);
        }
        #endregion

        #region ========== Backup Database ==========
        private static string[] backupFolders = { "lineup", "recordings", "subscriptions" };

        private void btnBackup_Click(object sender, EventArgs e)
        {
            // set cursor and disable the containers so no buttons can be clicked
            this.Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // start the thread and wait for it to complete
            Thread backupThread = new Thread(backupBackupFiles);
            backupThread.Start();
            while (!backupThread.IsAlive) ;
            while (backupThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            if (!string.IsNullOrEmpty(Helper.backupZipFile))
            {
                MessageBox.Show("A database backup has been successful. Location of backup file is " + Helper.backupZipFile, "Database Backup", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("A database backup not successful.", "Database Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            this.Cursor = Cursors.Arrow;
        }

        public static void backupBackupFiles()
        {
            Dictionary<string, string> backups = new Dictionary<string, string>();

            // going to assume backups are up-to-date if in safe mode
            if (SystemInformation.BootMode == BootMode.Normal)
            {
                WmcUtilities.PerformWmcConfigurationsBackup();
            }

            foreach (string backupFolder in backupFolders)
            {
                string filepath;
                if (!string.IsNullOrEmpty(filepath = getBackupFilename(backupFolder)))
                {
                    backups.Add(filepath, backupFolder + ".mxf");
                }
            }

            if (backups.Count > 0)
            {
                Helper.backupZipFile = CompressXmlFiles.CreatePackage(backups, "backups");
            }
            else
            {
                Helper.backupZipFile = string.Empty;
            }
        }

        private static string getBackupFilename(string backup)
        {
            string ret = null;
            DirectoryInfo directory = new DirectoryInfo(WmcRegistries.GetStoreFilename().Replace(".db", $"\\backup\\{backup}"));
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
        private void btnRestore_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                Helper.WriteEButtonFile("restore");
                restartClient(true);
                return;
            }

            // determine path to existing backup file
            openFileDialog1.InitialDirectory = Helper.Epg123BackupFolder;
            openFileDialog1.Filter = "Compressed File|*.zip";
            openFileDialog1.Title = "Select the Compressed Backup ZIP File";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                Helper.backupZipFile = openFileDialog1.FileName;

                // set cursor and disable the containers so no buttons can be clicked
                this.Cursor = Cursors.WaitCursor;
                splitContainer1.Enabled = splitContainer2.Enabled = false;

                // clear all listviews and comboboxes
                isolateEpgDatabase();

                // start the thread and wait for it to complete
                Thread restoreThread = new Thread(restoreBackupFiles);
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
                WmcRegistries.ActivateGuide();

                // reenable the containers and restore the cursor
                splitContainer1.Enabled = splitContainer2.Enabled = true;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void restoreBackupFiles()
        {
            foreach (string backup in backupFolders)
            {
                using (Stream stream = CompressXmlFiles.GetBackupFileStream(backup, Helper.backupZipFile))
                {
                    if (stream != null)
                    {
                        if (backup == "lineup.mxf")
                        {
                            if (deleteActiveDatabaseFile() == null) return;
                        }
                        MxfImporter.Import(stream, WmcStore.WmcObjectStore);
                    }
                }
            }
        }

        private string deleteActiveDatabaseFile()
        {
            // determine current instance and build database name
            string database = WmcRegistries.GetStoreFilename();

            // ensure there is a database to rebuild
            if (!string.IsNullOrEmpty(database))
            {
                // free the database and delete it
                if (!deleteeHomeFile(database))
                {
                    this.Cursor = Cursors.Arrow;
                    MessageBox.Show("Failed to delete the database file. Try again or consider trying in Safe Mode.", "Failed Operation", MessageBoxButtons.OK);
                    return null;
                }
            }
            else
            {
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("There is no database to rebuild.", "Failed Rebuild", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return null;
            }

            // open a new object store which creates a fresh database
            // if that fails for some reason, open WMC to create a new database
            if (WmcStore.WmcObjectStore == null)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = string.Format("{0}\\ehome\\ehshell.exe", Environment.ExpandEnvironmentVariables("%WINDIR%")),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process proc = Process.Start(startInfo);
                proc.WaitForInputIdle();
                proc.Kill();
            }
            return database;
        }

        private bool deleteeHomeFile(string filename)
        {
            // delete the database file
            string path = string.Format("{0}\\Microsoft\\eHome\\{1}.db", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), filename);
            try
            {
                foreach (Process proc in FileUtil.WhoIsLocking(path))
                {
                    proc.Kill();
                    proc.WaitForExit(1000);
                }
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region ========== Rebuild Database ===========
        private void btnRebuild_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                Helper.WriteEButtonFile("rebuild");
                restartClient(true);
                return;
            }

            // give warning
            if (MessageBox.Show("You are about to delete and rebuild the WMC EPG database. All tuners, recording schedules, favorite lineups, and logos will be restored. The Guide Listings will be empty until an MXF file is imported.\n\nClick 'OK' to continue.", "Database Rebuild", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) return;

            // set cursor and disable the containers so no buttons can be clicked
            this.Cursor = Cursors.WaitCursor;
            splitContainer1.Enabled = splitContainer2.Enabled = false;

            // start the thread and wait for it to complete
            Thread backupThread = new Thread(backupBackupFiles);
            backupThread.Start();
            while (!backupThread.IsAlive) ;
            while (backupThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            // clear all listviews and comboboxes
            isolateEpgDatabase();

            // start the thread and wait for it to complete
            Thread restoreThread = new Thread(restoreBackupFiles);
            restoreThread.Start();
            while (!restoreThread.IsAlive) ;
            while (restoreThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            // build the listviews
            btnRefreshLineups_Click(null, null);

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            this.Cursor = Cursors.Arrow;

            // initialize an import
            btnImport_Click(null, null);
        }
        #endregion
    }

    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
    }
}