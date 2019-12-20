using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.Store.MXF;
using Microsoft.MediaCenter.TV.Tuning;
using epg123Client.Properties;

namespace epg123
{
    public partial class clientForm : Form
    {
        private const string lu_name = "EPG123 Lineups with Schedules Direct";
        private ListViewColumnSorter mergedChannelColumnSorter = new ListViewColumnSorter();
        private LineupListViewSorter lineupChannelColumnSorter = new LineupListViewSorter();
        private epgTaskScheduler task = new epgTaskScheduler();
        private bool forceExit = false;

        bool initLvBuild = true;
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

            // status the registry configuration
            createEventLogSource();

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
        }
        private void clientForm_Shown(object sender, EventArgs e)
        {
            // flag initial form load building
            Application.UseWaitCursor = true;
            initLvBuild = true;

            // update task panel
            updateTaskPanel();

            // if client was started as elevated to perform an action
            if (Helper.UserHasElevatedRights && File.Exists(Helper.EButtonPath))
            {
                Application.UseWaitCursor = false;
                initLvBuild = false;

                using (StreamReader sr = new StreamReader(Helper.EButtonPath))
                {
                    string line = sr.ReadLine();
                    if (line.Contains("setup")) btnSetup_Click(null, null);
                    else if (line.Contains("restore")) btnRestore_Click(null, null);
                    else if (line.Contains("rebuild")) btnRebuild_Click(null, null);
                    else if (line.Contains("createTask") || line.Contains("deleteTask"))
                    {
                        btnTask_Click(null, null);
                        Application.UseWaitCursor = initLvBuild = true;
                    }
                    sr.Close();
                }
                File.Delete(Helper.EButtonPath);

                if (!initLvBuild)
                {
                    return;
                }
            }

            // populate listviews
            if (Store.objectStore != null)
            {
                this.Refresh();
                buildScannedLineupComboBox();
                this.Refresh();
                buildMergedChannelListView();
                this.Refresh();
                buildLineupChannelListView();

                btnImport.Enabled = mergedChannelListViewItems.Count > 0;
            }
            updateStatusBar();
            splitContainer1.Enabled = splitContainer2.Enabled = true;

            Application.UseWaitCursor = false;
            initLvBuild = false;
        }
        private void clientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.UseWaitCursor = true;

            // check to see if any EPG123 lineups are in the database
            if (!forceExit)
            {
                if (cmbObjectStoreLineups.Items.Count > 0 && cmbObjectStoreLineups.FindString("EPG123") > -1 && mergedChannelListView.Items.Count > 0)
                {
                    bool g2g = false;
                    foreach (ListViewItem listViewItem in mergedChannelListView.Items)
                    {
                        if (listViewItem.SubItems[3].Text.StartsWith("EPG123"))
                        {
                            g2g = true;
                            break;
                        }
                    }

                    if (!g2g)
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

        #region ========== Elevated Rights ==========
        private void restartClient(bool forceElevated = false)
        {
            try
            {
                // save the windows size and locations
                saveFormWindowParameters();

                // start a new process
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.Epg123ClientExePath,
                    WorkingDirectory = Helper.ExecutablePath,
                    UseShellExecute = true,
                    Verb = Helper.UserHasElevatedRights || forceElevated ? "runas" : null
                };
                Process proc = Process.Start(startInfo);

                // close this process
                forceExit = true;
                Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
        }
        #endregion

        #region ========== ListView Utilities ==========
        private void lvLineupSort(object sender, ColumnClickEventArgs e)
        {
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
                mergedChannelListView.Sort();
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

                // record if a channel is selected prior to sort
                Channel selectedChannel = null;
                if (lineupChannelListView.SelectedIndices.Count > 0)
                {
                    selectedChannel = lineupChannels[lineupChannelListView.SelectedIndices[0]];
                }

                // Perform the sort with these new sort options.
                lineupChannels.Sort(lineupChannelColumnSorter);
                lineupChannelListView.Refresh();

                // restore channel selection
                if (selectedChannel != null)
                {
                    lineupChannelListView.SelectedIndices.Add(lineupChannels.IndexOf(selectedChannel));
                }
            }
        }
        private void adjustColumnWidths(ListView listView)
        {
            if (!initLvBuild) return;

            int[] minWidths = { 100, 60, 100, 100, 100, 100 };
            foreach (ColumnHeader header in listView.Columns)
            {
                int currentWidth = header.Width;
                header.Width = -1;
                header.Width = Math.Max(Math.Max(header.Width, currentWidth), (int)(minWidths[header.Index] * dpiScaleFactor));
            }
        }
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // only enable the subscribe menu if a lineup service and merged channel has been selected
            subscribeMenuItem.Enabled = (lineupChannelListView.SelectedIndices.Count > 0) && (mergedChannelListView.SelectedItems.Count > 0);

            // determine which menu items are visible based on select listview channel
            bool mergedChannelMenuStrip = (((ContextMenuStrip)sender).SourceControl.Name == mergedChannelListView.Name);
            unsubscribeMenuItem.Visible = mergedChannelMenuStrip;
            toolStripSeparator2.Visible = mergedChannelMenuStrip;
            renameMenuItem.Visible = mergedChannelMenuStrip;

            // only enable rename menu item if a single channel has been selected
            renameMenuItem.Enabled = (mergedChannelListView.SelectedItems.Count == 1) && customLabelsOnly;
            renumberMenuItem.Enabled = (mergedChannelListView.SelectedItems.Count == 1) && customLabelsOnly;
        }
        #endregion

        #region ========== Channel/Station Subscribing Methods ==========
        private void subscribeMenuItem_Click(object sender, EventArgs e)
        {
            bool unsubscribe = unsubscribeMenuItem.Equals((ToolStripMenuItem)sender);

            // suspend listview drawing
            mergedChannelListView.BeginUpdate();
            mergedChannelColumnSorter.Suspend = true;
            this.Cursor = Cursors.WaitCursor;

            // subscribe selected channels to station
            foreach (ListViewItem mergedItem in mergedChannelListView.SelectedItems)
            {
                subscribeChannel(mergedItem.Index, (MergedChannel)mergedItem.Tag,
                                 unsubscribe ? null : (Channel)lineupChannelListView.Items[lineupChannelListView.SelectedIndices[0]].Tag);
            }
            
            // adjust column widths
            adjustColumnWidths(mergedChannelListView);

            // clear all selections
            mergedChannelListView.SelectedItems.Clear();
            lineupChannelListView.SelectedIndices.Clear();

            // resume listview drawing
            mergedChannelListView.EndUpdate();
            mergedChannelColumnSorter.Suspend = false;
            mergedChannelListView.Sort();
            this.Cursor = Cursors.Arrow;
        }
        private void btnAutoMatch_Click(object sender, EventArgs e)
        {
            int btnCase;

            // determine if number or callsign matching
            ToolStripButton[] btns = { btnAutoCallsign, btnAutoNumber };
            for (btnCase = 0; btnCase < btns.Length; ++btnCase)
            {
                if (btns[btnCase].Equals(sender)) break;
            }

            // suspend listview drawing
            mergedChannelListView.BeginUpdate();
            mergedChannelColumnSorter.Suspend = true;
            this.Cursor = Cursors.WaitCursor;

            // subscribe matching channels to stations
            foreach (ListViewItem mergedItem in mergedChannelListView.Items)
            {
                int index = mergedItem.Index;
                MergedChannel mergedChannel = (MergedChannel)mergedItem.Tag;
                foreach (Channel lineupChannel in lineupChannels)
                {
                    string textValue = (btnCase == 0) ? lineupChannel.CallSign : lineupChannel.ChannelNumber.ToString();
                    if (mergedItem.SubItems[btnCase].Text == textValue)
                    {
                        subscribeChannel(index, mergedChannel, lineupChannel);
                        break;
                    }
                }

                // unmatched channels are disabled/blocked
                if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                {
                    mergedChannelListView.Items[index].Checked = false;
                }
            }

            // adjust column widths
            adjustColumnWidths(mergedChannelListView);

            // resume listview drawing
            mergedChannelListView.EndUpdate();
            mergedChannelColumnSorter.Suspend = false;
            mergedChannelListView.Sort();
            this.Cursor = Cursors.Arrow;
        }
        private void subscribeChannel(int listViewIndex, MergedChannel mergedChannel, Channel lineupChannel)
        {
            if (Store.singletonStore != null)
            {
                Channel listings = null;
                MergedChannel channel = Store.singletonStore.Fetch(mergedChannel.Id) as MergedChannel;
                if (lineupChannel != null)
                {
                    // grab the listings
                    listings = Store.singletonStore.Fetch(lineupChannel.Id) as Channel;

                    // add this channel lineup to the device group if necessary
                    foreach (Device device in mergedChannel.Lineup.DeviceGroup.Devices)
                    {
                        try
                        {
                            if (!device.Name.ToLower().Contains("delete") &&
                                (device.ScannedLineup != null) && device.ScannedLineup.IsSameAs(mergedChannel.PrimaryChannel.Lineup) &&
                                ((device.WmisLineups == null) || !device.WmisLineups.Contains(lineupChannel.Lineup)))
                            {
                                device.SubscribeToWmisLineup(lineupChannel.Lineup);
                                device.Update();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteVerbose(string.Format("Failed to associate lineup {0} with device {1} ({2}). {3}", lineupChannel.Lineup,
                                                                device.Name ?? "NULL", (device.ScannedLineup == null) ? "NULL" : device.ScannedLineup.Name, ex.Message));
                        }
                    }
                }

                try
                {
                    channel.AddChannelListings(listings);
                }
                catch { }
            }
            mergedChannel.Refresh();

            // update listviewitem with new information and enable channel
            if (listViewIndex >= 0)
            {
                mergedChannelListView.Items[listViewIndex] = buildMergedChannelLvi(mergedChannel);
                mergedChannelListView.Items[listViewIndex].Checked = (lineupChannel != null) ? true : false;
            }
        }
        private void unsubscribeChannel(MergedChannel mergedChannel)
        {
            subscribeChannel(-1, mergedChannel, null);
        }
        #endregion

        #region ========== Lineup ListView Management ==========
        List<Channel> lineupChannels = new List<Channel>();
        private void buildLineupChannelListView()
        {
            // clear the pulldown list
            cmbObjectStoreLineups.Items.Clear();
            lineupChannelListView.Refresh();

            // populate with lineups in object_store
            foreach (Lineup lineup in new Lineups(Store.objectStore))
            {
                if (!lineup.LineupTypes.Equals("BB") &&
                    !string.IsNullOrEmpty(lineup.Name) &&
                    !lineup.Name.StartsWith("Broadband") &&
                    !lineup.Name.StartsWith("FINAL") &&
                    !lineup.Name.StartsWith("Scanned") &&
                    !lineup.Name.StartsWith("DefaultLineup") &&
                    !lineup.Name.StartsWith("Deleted") &&
                    !lineup.Name.Equals(lu_name) &&
                    !lineup.UIds.Empty)
                {
                    cmbObjectStoreLineups.Items.Add(lineup);
                    checkLineupTypesAndDevices(lineup);
                }
                else if (lineup.Name.Equals(lu_name))
                {
                    foreach (Device device in new Devices(Store.objectStore))
                    {
                        if (device.WmisLineups.Contains(lineup))
                        {
                            device.WmisLineups.RemoveAllMatching(lineup);
                            device.Update();
                        }
                    }
                }
            }

            // preset value to epg123 lineup if exists
            cmbObjectStoreLineups.SelectedIndex = cmbObjectStoreLineups.FindString("EPG123");
            if ((cmbObjectStoreLineups.Items.Count > 0) && (cmbObjectStoreLineups.SelectedIndex < 0))
            {
                cmbObjectStoreLineups.SelectedIndex = 0;
            }
            btnDeleteLineup.Enabled = (cmbObjectStoreLineups.Items.Count > 0);
        }
        private void lineupChannelListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = new ListViewItem(new string[]
            {
                lineupChannels[e.ItemIndex].CallSign,
                lineupChannels[e.ItemIndex].ChannelNumber.ToString(),
                lineupChannels[e.ItemIndex].Service.Name
            })
            {
                Tag = lineupChannels[e.ItemIndex]
            };
        }
        private void lineupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // pause listview drawing
            lineupChannelListView.BeginUpdate();
            this.Cursor = Cursors.WaitCursor;

            // clear the lineup channel listview
            lineupChannelListView.Items.Clear();

            // populate with new lineup channels
            lineupChannels.Clear();
            lineupChannels.AddRange(((Lineup)cmbObjectStoreLineups.SelectedItem).GetChannels());
            if ((lineupChannelListView.VirtualListSize = lineupChannels.Count) > 0)
            {
                lineupChannelListView.TopItem = lineupChannelListView.Items[0];
            }
            lineupChannelListView.SelectedIndices.Clear();

            // adjust column widths
            adjustColumnWidths(lineupChannelListView);

            // reset sorting column and order
            lineupChannelColumnSorter.Order = SortOrder.Ascending;
            lineupChannelColumnSorter.SortColumn = 1;
            lineupChannels.Sort(lineupChannelColumnSorter);

            // resume listview drawing
            lineupChannelListView.EndUpdate();
            this.Cursor = Cursors.Arrow;

            // update the status bar
            updateStatusBar();
        }
        private void checkLineupTypesAndDevices(Lineup lineup)
        {
            // only want to do this with EPG123 lineups
            if (!lineup.Provider.Name.Equals("EPG123") || lineup.Name.Equals(lu_name)) return;

            if (string.IsNullOrEmpty(lineup.LineupTypes) || string.IsNullOrEmpty(lineup.Language))
            {
                lineup.LineupTypes = "WMIS";
                lineup.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                lineup.Update();
            }

            // ensure guide is available in WMC
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
                    Logger.WriteError("Failed to open/edit registry to show the Guide in WMC.");
                    key.Close();
                }
            }
        }
        private void btnRefreshLineups_Click(object sender, EventArgs e)
        {
            mergedChannelListViewItems.Clear();

            // open object store and repopulate the GUI
            initLvBuild = true;
            Application.UseWaitCursor = true;

            buildScannedLineupComboBox();
            buildMergedChannelListView();
            buildLineupChannelListView();
            btnImport.Enabled = mergedChannelListViewItems.Count > 0;

            Application.UseWaitCursor = false;
            initLvBuild = false;
            updateStatusBar();
        }
        private void btnDeleteLineupClick(object sender, EventArgs e)
        {
            Lineup lineupToDelete = (Lineup)cmbObjectStoreLineups.SelectedItem;
            string prompt = string.Format("The lineup \"{0}\" will be removed from the Media Center database. Do you wish to continue?",
                                          lineupToDelete.Name);
            if (MessageBox.Show(prompt, "Delete Channel(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            // unsubscribe any mergedChannels that are subscribed to any lineupToDelete channel
            foreach (MergedChannel mergedChannel in Store.mergedLineup.GetChannels())
            {
                if (mergedChannel.PrimaryChannel.Lineup.IsSameAs(lineupToDelete))
                {
                    unsubscribeChannel(mergedChannel);
                }
            }

            // remove channels from lineupToDelete
            Channel[] channels = lineupToDelete.GetChannels();
            foreach (Channel channel in channels)
            {
                lineupToDelete.RemoveChannel(channel);
            }
            lineupToDelete.Update();

            // remove the lineup from the devices
            foreach (Device device in new Devices(Store.objectStore))
            {
                if (device.WmisLineups.Contains(lineupToDelete))
                {
                    device.WmisLineups.RemoveAllMatching(lineupToDelete);
                    device.Update();
                }
            }

            // null the lineup name
            lineupToDelete.Name = null;
            lineupToDelete.Update();

            // refresh the gui
            btnRefreshLineups_Click(null, null);
        }
        #endregion

        #region ========== Merged Channel ListView Management ==========
        List<ListViewItem> mergedChannelListViewItems = new List<ListViewItem>();
        private bool enabledChannelsOnly = false;
        private bool customLabelsOnly = true;
        private int totalMergedChannels;
        private void buildScannedLineupComboBox()
        {
            cmbSources.Items.Clear();
            cmbSources.Items.Add("All Scanned Sources");
            foreach (Device device in new Devices(Store.objectStore))
            {
                // user created channels will not have a scanned lineup
                if (device.ScannedLineup == null) continue;

                if (!cmbSources.Items.Contains(device.ScannedLineup))
                {
                    cmbSources.Items.Add(device.ScannedLineup);
                }
            }
            cmbSources.SelectedIndex = 0;
        }
        private void IncrementProgressBar(ToolStripProgressBar bar, bool reset = false)
        {
            if (reset)
            {
                lblToolStripStatus.Text = string.Empty;
                getChannelsProgressBar.Value = lvItemsProgressBar.Value = mergedLineupProgressBar.Value = 0;
                statusStrip1.Refresh();
            }
            else
            {
                bar.Value = Math.Min(bar.Value + 2, bar.Maximum);
                --bar.Value;
                Application.DoEvents();
            }
            getChannelsProgressBar.Width = lvItemsProgressBar.Width = mergedLineupProgressBar.Width = mergedChannelListView.Parent.Width / 3 - 1;
        }
        private List<MergedChannel> GetMergedChannels()
        {
            // gather valid merged channels from the merged lineup
            List<MergedChannel> mergedChannels = new List<MergedChannel>();

            // use the progress bar
            Channel[] lineupChannels = Store.mergedLineup.GetChannels();
            getChannelsProgressBar.Maximum = lineupChannels.Length;

            foreach (Channel channel in lineupChannels)
            {
                // increment progress bar
                IncrementProgressBar(getChannelsProgressBar);

                MergedChannel mergedChannel;
                try
                {
                    mergedChannel = (MergedChannel)channel;
                }
                catch
                {
                    Logger.WriteInformation(string.Format("Channel \"{0}\" could not be cast to a MergedChannel.", channel.ToString()));
                    continue;
                }

                try
                {
                    // ignore the channels we don't care about
                    if ((!string.IsNullOrEmpty(mergedChannel.CallSign) && mergedChannel.CallSign.StartsWith("Deleted")) ||  // deleted channel
                         mergedChannel.ChannelType == ChannelType.UserHidden ||                                             // probably tuner override for combined channels
                         mergedChannel.PrimaryChannel.Lineup.LineupTypes.Equals("BB"))                                      // broadband channel
                    {
                        continue;
                    }
                }
                catch
                {
                    if (mergedChannel.PrimaryChannel != null && mergedChannel.PrimaryChannel.Lineup == null)
                    {
                        Logger.WriteInformation(string.Format("Attempting to repair MergedChannel \"{0}\" by unsubscribing all non-scan lineup channels.", mergedChannel.ToString()));
                        unsubscribeChannel(mergedChannel);
                        if (mergedChannel.PrimaryChannel.Lineup != null)
                        {
                            Logger.WriteInformation(string.Format("   Final MergedChannel lineup channels are \"{0}\".", mergedChannel.ToString()));
                        }
                        else
                        {
                            Logger.WriteInformation(string.Format("   Failed to repair MergedChannel \"{0}\". Deleting channel.", mergedChannel.ToString()));
                            if (!deleteChannel(mergedChannel))
                            {
                                Logger.WriteError(string.Format("   Failed to delete MergedChannel \"{0}\".", mergedChannel.ToString()));
                            }
                            continue;
                        }
                    }
                    else
                    {
                        // certainly ignore the channels that don't have a primary channel
                        Logger.WriteInformation(string.Format("{0} has no primary channel or the primary channel has no lineup.", mergedChannel.ToString()));
                        continue;
                    }
                }
                mergedChannels.Add(mergedChannel);
            }
            totalMergedChannels = mergedChannels.Count;
            return mergedChannels;
        }
        private void buildMergedChannelListView()
        {
            if (Store.mergedLineup == null) return;
            else mergedChannelListView.Items.Clear();
            mergedChannelListView.Refresh();

            // attach lineup sorter to listview
            mergedChannelListView.ListViewItemSorter = mergedChannelColumnSorter;

            // pause listview drawing
            mergedChannelListView.BeginUpdate();

            // prep progress bars
            IncrementProgressBar(null, true);

            // populate with new lineup channels
            List<ListViewItem> listViewItems = new List<ListViewItem>();

            // add all merged channels to list
            if (mergedChannelListViewItems.Count == 0)
            {
                List<MergedChannel> mergedChannels = GetMergedChannels();
                lvItemsProgressBar.Maximum = mergedChannels.Count;

                foreach (MergedChannel channel in mergedChannels)
                {
                    // increment progress bar
                    IncrementProgressBar(lvItemsProgressBar);

                    // create the listviewitem
                    ListViewItem lvi;
                    if ((lvi = buildMergedChannelLvi(channel)) == null) continue;
                    lvi.Checked = (channel.UserBlockedState <= UserBlockedState.Enabled);
                    mergedChannelListViewItems.Add(lvi);
                }
            }
            else
            {
                getChannelsProgressBar.Value = getChannelsProgressBar.Maximum;
                lvItemsProgressBar.Value = lvItemsProgressBar.Maximum;
                getChannelsProgressBar.Value = getChannelsProgressBar.Maximum -1;
                lvItemsProgressBar.Value = lvItemsProgressBar.Maximum -1;
            }
            mergedLineupProgressBar.Maximum = mergedChannelListViewItems.Count;

            // pick which items to display
            foreach (ListViewItem listViewItem in mergedChannelListViewItems)
            {
                // increment progress bar
                IncrementProgressBar(mergedLineupProgressBar);

                // hide disabled channels based on selection
                if (enabledChannelsOnly && ((listViewItem.Tag as MergedChannel).UserBlockedState > UserBlockedState.Enabled))
                {
                    continue;
                }

                // only show tuners based on scanned lineup selection
                if (cmbSources.SelectedIndex > 0)
                {
                    bool source = false;
                    if ((listViewItem.Tag as MergedChannel).PrimaryChannel.Lineup.Equals((Lineup)cmbSources.SelectedItem)) source = true;
                    else if ((listViewItem.Tag as MergedChannel).SecondaryChannels != null)
                    {
                        foreach (Channel ch2 in (listViewItem.Tag as MergedChannel).SecondaryChannels)
                        {
                            if (ch2.Lineup != null && ch2.Lineup.Equals((Lineup)cmbSources.SelectedItem))
                            {
                                source = true;
                                break;
                            }
                        }
                    }
                    if (!source) continue;
                }

                try
                {
                    listViewItems.Add(listViewItem);
                }
                catch (Exception e)
                {
                    Logger.WriteError("Exception caught when trying to add a ListViewItem to the MergedChannelListView.");
                    Logger.WriteError(e.Message);
                }
            }

            if (listViewItems.Count > 0)
            {
                mergedChannelListView.Items.AddRange(listViewItems.ToArray());
            }
            getChannelsProgressBar.Width = lvItemsProgressBar.Width = mergedLineupProgressBar.Width = 0;
            getChannelsProgressBar.Value = lvItemsProgressBar.Value = mergedLineupProgressBar.Value = 0;

            // adjust column widths
            adjustColumnWidths(mergedChannelListView);

            // reset sorting column and order
            mergedChannelColumnSorter.Order = SortOrder.Ascending;
            mergedChannelColumnSorter.SortColumn = 1;
            mergedChannelListView.Sort();

            // resume listview drawing
            mergedChannelListView.EndUpdate();
        }
        private ListViewItem buildMergedChannelLvi(MergedChannel mergedChannel)
        {
            // determine whether primary channel is the scanned channel
            bool scanned = mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned");

            // collect all unique channel sources
            HashSet<Lineup> sources = new HashSet<Lineup>();
            if (scanned)
            {
                sources.Add(mergedChannel.PrimaryChannel.Lineup);
            }
            if (mergedChannel.SecondaryChannels != null)
            {
                foreach (Channel ch2 in mergedChannel.SecondaryChannels)
                {
                    if ((ch2.Lineup != null) && (ch2.Lineup.Name != null) && ch2.Lineup.Name.StartsWith("Scanned"))
                    {
                        sources.Add(ch2.Lineup);
                    }
                }
            }

            // build scanned source(s) string
            string source = string.Empty;
            foreach (Lineup lineup in sources)
            {
                if (!string.IsNullOrEmpty(source)) source += " + ";
                if (!string.IsNullOrEmpty(lineup.Name) && (lineup.Name.Length > 9))
                {
                    source += lineup.Name.Remove(0, 9);
                }

                if (source.Contains(")"))
                {
                    source = source.Remove(source.LastIndexOf(')'), 1);
                }
            }
            if (string.IsNullOrEmpty(source))
            {
                Logger.WriteInformation(string.Format("There are no scanned lineups associated with MergedChannel \"{0}\".", mergedChannel.ToString()));
            }

            // build original channel number string
            string originalChannelNumber = mergedChannel.OriginalNumber.ToString();
            if (mergedChannel.OriginalSubNumber > 0) originalChannelNumber += ("." + mergedChannel.OriginalSubNumber.ToString());
            string customChannelNumber = mergedChannel.Number.ToString();
            if (mergedChannel.SubNumber > 0) customChannelNumber += ("." + mergedChannel.SubNumber.ToString());

            // build tuning info
            HashSet<string> tuningInfos = new HashSet<string>();
            foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos)
            {
                // handle any overrides to tuninginfo
                // assumes that tuninginfo is valid unless an override explicitly says otherwise
                bool shown = true;
                if (!tuningInfo.Overrides.Empty)
                {
                    foreach (TuningInfoOverride tuningInfoOverride in tuningInfo.Overrides)
                    {
                        if (tuningInfoOverride.Channel.Id != mergedChannel.Id) continue;
                        else if (!tuningInfoOverride.IsLatestVersion) continue;
                        else if ((tuningInfoOverride.Channel.ChannelType == ChannelType.UserHidden) ||
                                 (tuningInfoOverride.IsUserOverride && tuningInfoOverride.UserBlockedState == UserBlockedState.Disabled))
                        {
                            shown = false;
                        }
                        if (!shown) break;
                    }
                }
                if (!shown) continue;

                if (tuningInfo is DvbTuningInfo)
                {
                    DvbTuningInfo ti = tuningInfo as DvbTuningInfo;
                    switch (tuningInfo.TuningSpace)
                    {
                        case "DVB-T":
                            // formula to convert channel (n) to frequency (fc) is fc = 8n + 306 (in MHz)
                            // offset is -167KHz, 0Hz, +167KHz => int offset = ti.Frequency - (channel * 8000) - 306000;
                            int channel = (ti.Frequency - 305833) / 8000;
                            tuningInfos.Add(string.Format("UHF C{0}", channel));
                            break;
                        case "DVB-S":
                            DVBSLocator locator = ti.TuneRequest.Locator as DVBSLocator;
                            string polarization = string.Empty;
                            switch (locator.SignalPolarisation)
                            {
                                case Polarisation.BDA_POLARISATION_LINEAR_H:
                                    polarization = " H";
                                    break;
                                case Polarisation.BDA_POLARISATION_LINEAR_V:
                                    polarization = " V";
                                    break;
                                case Polarisation.BDA_POLARISATION_CIRCULAR_L:
                                    polarization = " LHC";
                                    break;
                                case Polarisation.BDA_POLARISATION_CIRCULAR_R:
                                    polarization = " RHC";
                                    break;
                                default:
                                    break;
                            }
                            tuningInfos.Add(string.Format("{0:F0}{1} ({2})", ti.Frequency / 1000.0, polarization, ti.Sid));
                            break;
                        case "DVB-C":
                        case "ISDB-T":
                        case "ISDB-S":
                        case "ISDB-C":
                            tuningInfos.Add(string.Format("{0} not implemented yet. Contact me!", tuningInfo.TuningSpace));
                            break;
                        default:
                            break;
                    }
                }
                else if (tuningInfo is ChannelTuningInfo)
                {
                    ChannelTuningInfo ti = tuningInfo as ChannelTuningInfo;
                    switch (tuningInfo.TuningSpace)
                    {
                        case "ATSC":
                            tuningInfos.Add(string.Format("{0} {1}{2}",
                                (ti.PhysicalNumber < 14) ? "VHF" : "UHF", ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                            break;
                        case "Cable":
                        case "ClearQAM":
                        case "Digital Cable":
                            tuningInfos.Add(string.Format("C{0}{1}",
                                ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                            break;
                        case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                        case "AuxIn1":
                        case "Antenna":
                        case "ATSCCable":
                            tuningInfos.Add(string.Format("{0} not implemented yet. Contact me!", tuningInfo.TuningSpace));
                            break;
                        default:
                            break;
                    }
                }
                else if (tuningInfo is StringTuningInfo)
                {
                    StringTuningInfo ti = tuningInfo as StringTuningInfo;
                    switch (tuningInfo.TuningSpace)
                    {
                        case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":
                            foreach (Channel channel in ti.Channels)
                            {
                                tuningInfos.Add("C" + channel.OriginalNumber.ToString());
                            }
                            break;
                        default:
                            tuningInfos.Add(string.Format("{0} not implemented yet. Contact me!", tuningInfo.TuningSpace));
                            break;
                    }
                }
            }

            if (tuningInfos.Count == 0)
            {
                Logger.WriteInformation(string.Format("There are no tuners associated with \"{0}\".", mergedChannel.ToString()));
            }

            // sort the hashset into a new array
            string[] sortedTuningInfos = tuningInfos.ToArray();
            Array.Sort(sortedTuningInfos);

            string tuneInfos = string.Empty;
            foreach (string info in sortedTuningInfos)
            {
                if (!string.IsNullOrEmpty(tuneInfos)) tuneInfos += " + ";
                tuneInfos += info;
            }

            // build ListViewItem
            ListViewItem listViewItem = new ListViewItem(new string[]
            {
                (!customLabelsOnly) ? mergedChannel.PrimaryChannel.CallSign : mergedChannel.CallSign,
                (!customLabelsOnly) ? originalChannelNumber : customChannelNumber,
                (!scanned) ? (mergedChannel.Service.Name ?? null) : null,
                (!scanned) ? (mergedChannel.PrimaryChannel.Lineup.Name ?? null) : null,
                source, tuneInfos
            })
            {
                Tag = mergedChannel
            };
            if (!originalChannelNumber.Equals(customChannelNumber) || !mergedChannel.PrimaryChannel.CallSign.Equals(mergedChannel.CallSign))
            {
                listViewItem.SubItems[0].BackColor = (mergedChannel.PrimaryChannel.CallSign.Equals(mergedChannel.CallSign)) ? SystemColors.Window : Color.Pink;
                listViewItem.SubItems[1].BackColor = (originalChannelNumber.Equals(customChannelNumber)) ? SystemColors.Window : Color.Pink;
                listViewItem.UseItemStyleForSubItems = false;
            }
            return listViewItem;
        }
        private void mergedChannelListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem lvi in mergedChannelListView.Items)
                {
                    lvi.Selected = true;
                }
            }
        }
        private bool deleteChannel(MergedChannel mergedChannel)
        {
            if (Store.singletonStore != null)
            {
                try
                {
                    Channel channel = Store.singletonStore.Fetch(mergedChannel.Id) as Channel;
                    Store.singletonLineup.RemoveChannel(channel);
                }
                catch { }
            }
            return true;
        }
        private void btnDeleteChannel_Click(object sender, EventArgs e)
        {
            string prompt = string.Format("The selected {0} channel(s) will be removed from the Media Center database. Do you wish to continue?",
                                          mergedChannelListView.SelectedItems.Count);
            if ((mergedChannelListView.SelectedItems.Count == 0) ||
                MessageBox.Show(prompt, "Delete Channel(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            // pause listview drawing
            mergedChannelListView.BeginUpdate();
            this.Cursor = Cursors.WaitCursor;

            foreach (ListViewItem lvi in mergedChannelListView.SelectedItems)
            {
                // get the channel from the listviewitem
                if (deleteChannel((MergedChannel)lvi.Tag))
                {
                    // update listview
                    mergedChannelListView.Items.Remove(lvi);
                }
                else
                {
                    Logger.WriteError(string.Format("Failed to delete MergedChannel \"{0}\".", ((MergedChannel)lvi.Tag).ToString()));
                }
            }
            Store.mergedLineup.FullMerge(false);

            // resume listview drawing
            mergedChannelListView.EndUpdate();
            this.Cursor = Cursors.Arrow;
        }
        private void btnChannelDisplay_Click(object sender, EventArgs e)
        {
            enabledChannelsOnly = !enabledChannelsOnly;
            mergedChannelListView.Items.Clear();
            buildMergedChannelListView();

            if (!enabledChannelsOnly)
            {
                btnChannelDisplay.BackColor = SystemColors.Control;
            }
            else
            {
                btnChannelDisplay.BackColor = SystemColors.ControlDark;
            }

            // update the status bar
            updateStatusBar();
        }
        private void updateStatusBar()
        {
            int totalServices = 0;
            foreach (Lineup lineup in cmbObjectStoreLineups.Items)
            {
                totalServices += lineup.GetChannels().Length;
            }
            lblToolStripStatus.Text = string.Format("{4} Merged Channel(s) with {0} shown  |  {1} Lineup(s)  |  {2} Service(s) with {3} shown", mergedChannelListView.Items.Count, cmbObjectStoreLineups.Items.Count, totalServices, lineupChannelListView.Items.Count, totalMergedChannels);
            statusStrip1.Refresh();
        }
        private void mergedChannelListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Ignore item checks when selecting multiple items
            if ((ModifierKeys & (Keys.Shift | Keys.Control)) > 0)
            {
                e.NewValue = e.CurrentValue;
            }
        }
        private void mergedChannelListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (initLvBuild) return;

            // toggle userblockedstate
            MergedChannel mergedChannel = (MergedChannel)mergedChannelListView.Items[e.Item.Index].Tag;
            switch (mergedChannel.UserBlockedState)
            {
                case UserBlockedState.Unknown:
                case UserBlockedState.Enabled:
                    if (!e.Item.Checked)
                    {
                        mergedChannel.UserBlockedState = UserBlockedState.Blocked;
                        mergedChannel.Update();
                    }
                    break;
                case UserBlockedState.Blocked:
                case UserBlockedState.Disabled:
                default:
                    if (e.Item.Checked)
                    {
                        mergedChannel.UserBlockedState = UserBlockedState.Enabled;
                        mergedChannel.Update();
                    }
                    break;
            }

            // immediate update
            Store.mergedLineup.FullMerge(false);
        }
        private void cmbSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbSources.SelectedIndex < 0) || initLvBuild) return;

            buildMergedChannelListView();
            updateStatusBar();
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
            }
        }
        #endregion

        #region ========== Manual Import and Reindex ==========
        private void btnImport_Click(object sender, EventArgs e)
        {
            // verify tuners are set up
            if (mergedChannelListView.Items.Count == 0)
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
            foreach (Microsoft.MediaCenter.Pvr.Recording recording in new Microsoft.MediaCenter.Pvr.Recordings(Store.objectStore))
            {
                if ((recording.State == Microsoft.MediaCenter.Pvr.RecordingState.Initializing) || (recording.State == Microsoft.MediaCenter.Pvr.RecordingState.Recording))
                {
                    if (DialogResult.Yes == MessageBox.Show("There is currently at least one program being recorded. Importing a guide update at this time may result with an aborted recording or worse.\n\nDo you wish to proceed?",
                                                            "Recording In Progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                    {
                        break;
                    }
                    else
                    {
                        return;
                    }
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
                mxfImport.reindexDatabase();
                mxfImport.reindexPvrSchedule();
            }
            else
            {
                MessageBox.Show("There was an error importing the MXF file.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            statusLogo.statusImage();

            // make sure guide is activated and background scanning is stopped
            mxfImport.activateLineupAndGuide();
            disableBackgroundScanning();

            // open object store and repopulate the GUI
            btnRefreshLineups_Click(null, null);
        }
        #endregion

        #region ========== Rename/Renumber Merged Channel ==========
        private int selectedItemForEdit;
        private string oldCallSign = string.Empty;
        private string oldNumber = string.Empty;
        private bool cancelEdit = false;
        private void renumberMenuItem_Click(object sender, EventArgs e)
        {
            cancelEdit = false;

            // record current channel number for potential restore
            selectedItemForEdit = mergedChannelListView.SelectedItems[0].Index;
            oldNumber = mergedChannelListView.Items[selectedItemForEdit].SubItems[1].Text;

            // get bounds of number field
            Rectangle box = mergedChannelListView.Items[selectedItemForEdit].SubItems[1].Bounds;
            lvEditTextBox.SetBounds(box.Left + mergedChannelListView.Left + 2,
                                    box.Top + mergedChannelListView.Top,
                                    box.Width, box.Height);
            lvEditTextBox.Text = oldNumber;
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
                    cancelEdit = false;
                    e.Handled = true;
                    lvEditTextBox.Hide();
                    break;
                case (char)Keys.Escape:
                    cancelEdit = true;
                    e.Handled = true;
                    lvEditTextBox.Hide();
                    break;
                default:
                    break;
            }
        }
        private void lvEditTextBox_LostFocus(object sender, EventArgs e)
        {
            lvEditTextBox.Hide();

            if (!cancelEdit)
            {
                // establish what the original numbers are
                MergedChannel mergedChannel = (MergedChannel)mergedChannelListView.Items[selectedItemForEdit].Tag;
                int number = mergedChannel.OriginalNumber;
                int subnumber = mergedChannel.OriginalSubNumber;

                // if text is empty, then will revert to original numbers
                // otherwise, parse the digits
                if (!string.IsNullOrEmpty(lvEditTextBox.Text))
                {
                    string[] numbers = lvEditTextBox.Text.Split('.');
                    if (numbers.Length > 0)
                    {
                        number = int.Parse(numbers[0]);
                    }
                    if (numbers.Length > 1)
                    {
                        subnumber = int.Parse(numbers[1]);
                    }
                    else subnumber = 0;
                }

                // update the merged channel and listview item
                mergedChannel.Number = number;
                mergedChannel.SubNumber = subnumber;
                mergedChannel.Update();

                ListViewItem newLvi = buildMergedChannelLvi(mergedChannel);
                newLvi.Checked = (mergedChannel.UserBlockedState <= UserBlockedState.Enabled);
                mergedChannelListView.Items[selectedItemForEdit] = newLvi;
            }
            mergedChannelListView.Focus();
        }
        private void renameMenuItem_Click(object sender, EventArgs e)
        {
            // record current callsign for potential restore
            selectedItemForEdit = mergedChannelListView.SelectedItems[0].Index;
            oldCallSign = mergedChannelListView.Items[selectedItemForEdit].Text;

            // enable label editing
            mergedChannelListView.LabelEdit = true;

            // begin edit
            mergedChannelListView.Items[selectedItemForEdit].BeginEdit();
        }
        private void mergedChannelListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // disable label editing
            mergedChannelListView.LabelEdit = false;
            e.CancelEdit = true;

            // grab the new call sign
            MergedChannel mergedChannel = (MergedChannel)mergedChannelListView.Items[selectedItemForEdit].Tag;
            mergedChannel.CallSign = (string.IsNullOrEmpty(e.Label)) ? mergedChannel.PrimaryChannel.CallSign : e.Label;
            mergedChannel.Update();

            // update the listviewitem text
            mergedChannelListView.Items[selectedItemForEdit].Text = mergedChannel.CallSign;

            if (mergedChannel.CallSign.Equals(mergedChannel.PrimaryChannel.CallSign))
            {
                mergedChannelListView.Items[selectedItemForEdit].SubItems[0].BackColor = SystemColors.Window;
            }
            else
            {
                mergedChannelListView.Items[selectedItemForEdit].SubItems[0].BackColor = Color.Pink;
            }
        }
        private void btnCustomDisplay_Click(object sender, EventArgs e)
        {
            customLabelsOnly = !customLabelsOnly;
            foreach (ListViewItem item in mergedChannelListView.Items)
            {
                MergedChannel mergedChannel = (MergedChannel)item.Tag;
                string originalChannelNumber = mergedChannel.OriginalNumber.ToString();
                originalChannelNumber += (mergedChannel.OriginalSubNumber > 0) ? "." + mergedChannel.OriginalSubNumber.ToString() : null;

                item.SubItems[0].Text = (!customLabelsOnly) ? mergedChannel.PrimaryChannel.CallSign : mergedChannel.CallSign;
                item.SubItems[1].Text = (!customLabelsOnly) ? originalChannelNumber : mergedChannel.ChannelNumber.ToString();
            }

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
        #endregion

        #region ========== Registry Entries ==========
        private bool disableBackgroundScanning()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\BackgroundScanner", true);
            if (key != null)
            {
                try
                {
                    if ((key.GetValue("PeriodicScanEnabled") == null) || ((int)key.GetValue("PeriodicScanEnabled") != 0))
                    {
                        key.SetValue("PeriodicScanEnabled", 0);
                    }
                    if ((key.GetValue("PeriodicScanIntervalSeconds") == null) || ((int)key.GetValue("PeriodicScanIntervalSeconds") != 0x7FFFFFFF))
                    {
                        key.SetValue("PeriodicScanIntervalSeconds", 0x7FFFFFFF);
                    }
                    key.Close();
                    return true;
                }
                catch
                {
                    MessageBox.Show("Failed to open/edit registry to disable periodic tuner background scanning.", "Registry Access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    key.Close();
                }
            }
            return false;
        }
        private bool createEventLogSource()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\eventlog\\Media Center\\EPG123Client");
            if (key == null)
            {
                try
                {
                    if (!EventLog.SourceExists("EPG123Client"))
                    {
                        EventSourceCreationData sourceData = new EventSourceCreationData("EPG123Client", "Media Center");
                        EventLog.CreateEventSource(sourceData);
                    }
                }
                catch
                {
                    MessageBox.Show("EPG123Client has not been registered as a source for Media Center event logs. This GUI must be executed with elevated rights to add EPG123Client as a valid source.",
                                    "Event Log Permissions Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region ========== Database Utilities ==========
        private string[] regKeysDelete = new string[] { @"Service\Epg" };
        private string[] regKeysCreate = new string[] { @"Service\Epg" };
        private void isolateEpgDatabase(bool disposeStore = false)
        {
            // close and dispose the object store if necessary
            if (Store.objectStore != null)
            {
                // clear the merged channels and lineup combobox
                cmbSources.Items.Clear();
                mergedChannelListView.Items.Clear();
                mergedChannelListViewItems.Clear();

                // clear the lineups
                cmbObjectStoreLineups.Items.Clear();
                lineupChannels = new List<Channel>();
                lineupChannelListView.Items.Clear();
                lineupChannelListView.VirtualListSize = 0;

                // close store
                if (disposeStore)
                {
                    Store.Close(disposeStore);
                }

                // clear the status text
                lblToolStripStatus.Text = string.Empty;
                statusStrip1.Refresh();
            }
        }
        private static string getDatabaseName()
        {
            string epg_db = string.Empty;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\eHome";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Service\\EPG", false))
            {
                if (key != null)
                {
                    int instance = 0;
                    try
                    {
                        instance = (int)key.GetValue("EPG.instance");
                    }
                    catch { }

                    // Windows 7 is OS version 6.1 and mcepg2-X, all others will use mcepg3-X
                    int version = ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 1)) ? 2 : 3;
                    if (File.Exists(string.Format("{0}\\mcepg{1}-{2}.db", path, version, instance)))
                    {
                        epg_db = string.Format("mcepg{0}-{1}", version, instance);
                    }
                }
            }
            return epg_db;
        }
        private static string getBackupFilename(string dbFolder, string backup)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + string.Format("\\Microsoft\\eHome\\{0}\\backup\\{1}", dbFolder, backup);

            // no folder means no file, return empty string
            if (!Directory.Exists(path))
            {
                Logger.WriteError(string.Format("Backup {0} file does not exist.", backup));
                return string.Empty;
            }

            // get list of files in backup folder
            List<string> backups = new List<string>();
            backups.AddRange(Directory.GetFiles(path));

            // if no files exist, return emtpy string
            if (backups.Count == 0)
            {
                Logger.WriteError(string.Format("Backup {0} file does not exist.", backup));
                return string.Empty;
            }
            backups.Sort();

            // sorted by name would normally make latest the last
            // verify the latest has no file extension
            string latest = backups[backups.Count - 1];
            for (int i = backups.Count; i > 0;)
            {
                FileInfo fi = new FileInfo(backups[--i]);
                if (fi.Extension == string.Empty)
                {
                    latest = backups[i];
                    break;
                }
            }

            // if no file exists, return empty string
            if (latest.Length == 0) return string.Empty;

            return latest;
        }
        public static void backupBackupFiles()
        {
            string[] backupFolders = { "lineup", "recordings", "subscriptions" };
            Dictionary<string, string> backups = new Dictionary<string, string>();
            string dbName = getDatabaseName();

            // going to assume backups are up-to-date if in safe mode
            if (SystemInformation.BootMode == BootMode.Normal)
            {
                initDatabaseUpdate();
            }

            foreach (string backupFolder in backupFolders)
            {
                string filepath = string.Empty;
                if (!string.IsNullOrEmpty(filepath = getBackupFilename(dbName, backupFolder)))
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
        private void restoreBackupFiles()
        {
            string[] backups = { "lineup.mxf", "recordings.mxf", "subscriptions.mxf" };
            foreach (string backup in backups)
            {
                using (Stream stream = CompressXmlFiles.GetBackupFileStream(backup, Helper.backupZipFile))
                {
                    if (stream != null)
                    {
                        if (backup == "lineup.mxf")
                        {
                            if (deleteActiveDatabaseFile() == null) return;
                        }
                        MxfImporter.Import(stream, Store.objectStore);
                    }
                }
            }
        }
        private static bool initDatabaseUpdate()
        {
            // establish program to run and environment for import
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables("%WINDIR%") + @"\ehome\mcupdate.exe",
                Arguments = "-b",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // begin import
            Process proc = Process.Start(startInfo);

            // wait for exit and process exit code
            proc.WaitForExit();
            if (proc.ExitCode == 0)
            {
                Logger.WriteInformation("Successfully forced a Media Center database configuration backup. Exit code: 0");
                return true;
            }
            else
            {
                Logger.WriteError(string.Format("Error using mcupdate to force a Media Center database configuration backup. Exit code: {0}", proc.ExitCode));
            }
            return false;
        }
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
            if (openFileDialog1.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                return;
            }
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
            mxfImport.activateLineupAndGuide();
            disableBackgroundScanning();

            // reenable the containers and restore the cursor
            splitContainer1.Enabled = splitContainer2.Enabled = true;
            this.Cursor = Cursors.Arrow;
        }
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
        private void btnAddChannels_Click(object sender, EventArgs e)
        {
            using (frmAddChannel addChannelForm = new frmAddChannel())
            {
                addChannelForm.ShowDialog();
                if (addChannelForm.channelAdded)
                {
                    Logger.WriteInformation("Restarting EPG123 Client to avoid an external process crashing EPG123 10 seconds after adding channels.");
                    isolateEpgDatabase();
                    restartClient();
                }
            }
        }
        #endregion

        #region ========== Client Setup ==========
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
            frm.shouldBackup = mergedChannelListView.Items.Count > 0;

            // clear everything out
            isolateEpgDatabase(true);

            // open the form
            frm.ShowDialog();
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
        #endregion

        #region ========== Rebuild ===========
        private string deleteActiveDatabaseFile()
        {
            // determine current instance and build database name
            string epg_db = getDatabaseName();

            // ensure there is a database to rebuild
            if (!string.IsNullOrEmpty(epg_db))
            {
                // free the database and delete it
                if (!deleteeHomeFile(epg_db))
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
            if (Store.objectStore == null)
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
            return epg_db;
        }
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
        private bool deleteeHomeFile(string filename)
        {
            // disconnect from store prior to deleting it
            isolateEpgDatabase(true);

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
        public bool importBackupFile(string dbName, string backup)
        {
            // determine filepath of backup file
            string filename = string.Empty;
            if (string.IsNullOrEmpty(filename = getBackupFilename(dbName, backup))) return false;

            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                MxfImporter.Import(stream, Store.objectStore);
            }

            return true;
        }
        #endregion

        #region ========== Tweak WMC ==========
        private void btnTweakWmc_Click(object sender, EventArgs e)
        {
            // open the tweak gui
            frmWmcTweak frm = new frmWmcTweak();
            frm.ShowDialog();
        }
        #endregion

        #region ========== Advanced Functions ==========
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
        private void btnExportMxf_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(Helper.Epg123OutputFolder)) Directory.CreateDirectory(Helper.Epg123OutputFolder);

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(new StreamWriter(Helper.Epg123OutputFolder + "\\mxfExport.mxf"), settings))
            {
                MxfExporter.Export(Store.objectStore, writer, false);
            }

            this.Cursor = Cursors.Arrow;
        }
        #endregion

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(Helper.Epg123TraceLogPath))
            {
                Process.Start(Helper.Epg123TraceLogPath);
            }
        }
    }
}