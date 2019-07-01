using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
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
        private static MergedLineup mergedLineup_;
        public static MergedLineup mergedLineup
        {
            get
            {
                if (mergedLineup_ == null)
                {
                    using (MergedLineups mergedLineups = new MergedLineups(object_store))
                    {
                        foreach (MergedLineup lineup in mergedLineups)
                        {
                            if (lineup.GetChannels().Length > 0)
                            {
                                mergedLineup_ = lineup;
                            }
                        }
                    }
                }
                return mergedLineup_;
            }
        }

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
            // check to see if program started with elevated rights
            checkForElevatedRights();

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

            // populate listviews
            if (object_store != null)
            {
                this.Refresh();
                buildScannedLineupComboBox();
                this.Refresh();
                buildLineupChannelListView();
                this.Refresh();
                buildMergedChannelListView();
            }
            updateStatusBar();

            // enable manual import button only if object store exists and there are more than 0 merged channels
            btnImport.Enabled = (objectStore_ != null);

            Application.UseWaitCursor = false;
            initLvBuild = false;
        }
        private void clientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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

                // check to make sure a scheduled task has been created
                if (!task.exist && !task.existNoAccess)
                {
                    updateTaskPanel();
                    if (DialogResult.No == MessageBox.Show("There is no scheduled task to continually update the guide data. Are you sure you want to exit?", "Scheduled Task Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // save the windows size and locations
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

            // ensure any changes to the merged channels are pushed to the object store
            if ((objectStore_ != null) && !objectStore_.IsDisposed && (mergedLineup_ != null))
            {
                mergedLineup.FullMerge(false);
                mergedLineup.Update();
            }
            isolateEpgDatabase();
        }

        #region ========== Elevated Rights ==========
        private bool hasElevatedRights;
        private void checkForElevatedRights()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            hasElevatedRights = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void elevateRights()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Helper.Epg123ClientExePath,
                    WorkingDirectory = Helper.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process proc = Process.Start(startInfo);
                forceExit = true;

                // close original process
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
            // suspend listview drawing
            mergedChannelListView.BeginUpdate();
            mergedChannelColumnSorter.Suspend = true;
            this.Cursor = Cursors.WaitCursor;

            // subscribe selected channels to station
            foreach (ListViewItem mergedItem in mergedChannelListView.SelectedItems)
            {
                subscribeChannel(mergedItem.Index, (MergedChannel)mergedItem.Tag, (Channel)lineupChannelListView.Items[lineupChannelListView.SelectedIndices[0]].Tag);
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
            if (!mergedChannel.PrimaryChannel.IsSameAs(lineupChannel))
            {
                // unsubscribe any previous lineup before adding new lineup
                if (!mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                {
                    unsubscribeMenuItem_Click(unsubscribeMenuItem, null);
                }

                if (mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
                {
                    // copy scanned lineup to secondary channels
                    mergedChannel.SecondaryChannels.Add(mergedChannel.PrimaryChannel);

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

                // update primary channel
                mergedChannel.PrimaryChannel = lineupChannel;
                mergedChannel.Service = lineupChannel.Service;
                mergedChannel.Update();

                // update listviewitem with new information and enable channel
                mergedChannelListView.Items[listViewIndex] = buildMergedChannelLvi(mergedChannel);
                mergedChannelListView.Items[listViewIndex].Checked = true;
            }
        }
        private void unsubscribeChannel(MergedChannel mergedChannel)
        {
            // gather all scanned lineup channels from the merged channel
            HashSet<Channel> scannedChannels = new HashSet<Channel>();
            if ((mergedChannel.PrimaryChannel.Lineup != null) && mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
            {
                scannedChannels.Add(mergedChannel.PrimaryChannel);
            }
            foreach (Channel channel in mergedChannel.SecondaryChannels)
            {
                if ((channel.Lineup == null) || string.IsNullOrEmpty(channel.Lineup.Name)) continue;
                if (channel.Lineup.Name.StartsWith("Scanned")) scannedChannels.Add(channel);
            }

            // clear the secondary
            List<Channel> secondaryChannels = mergedChannel.SecondaryChannels.ToList();
            foreach (Channel secondaryChannel in secondaryChannels)
            {
                mergedChannel.SecondaryChannels.RemoveAllMatching(secondaryChannel);
            }

            if (scannedChannels.Count > 0)
            {
                bool first = true;
                foreach (Channel scannedChannel in scannedChannels)
                {
                    if (first)
                    {
                        // set the primary and service
                        mergedChannel.PrimaryChannel = scannedChannel;
                        mergedChannel.Service = mergedChannel.PrimaryChannel.Service;

                        // remaining channels to be populated in secondary
                        first = false;
                    }
                    else
                    {
                        mergedChannel.SecondaryChannels.Add(scannedChannel);
                    }
                }
            }
            //else // shouldn't be here
            //{
            //    // find all the scanned lineups for the devices of this mergedchannel
            //    HashSet<Lineup> scannedLineups = new HashSet<Lineup>();
            //    foreach (Device device in mergedChannel.Lineup.DeviceGroup.Devices)
            //    {
            //        if (device.ScannedLineup == null) continue;
            //        scannedLineups.Add(device.ScannedLineup);
            //    }

            //    // build the primary and secondary channels
            //    bool first = true;
            //    foreach (Lineup scannedLineup in scannedLineups)
            //    {
            //        Channel channel = scannedLineup.GetChannelFromNumber(mergedChannel.ChannelNumber.Number, mergedChannel.ChannelNumber.SubNumber);
            //        if (first && (channel != null))
            //        {
            //            // set the primary and service
            //            mergedChannel.PrimaryChannel = channel;
            //            mergedChannel.Service = mergedChannel.PrimaryChannel.Service;

            //            // all secondary to be populated
            //            first = false;
            //        }
            //        else if (channel != null)
            //        {
            //            mergedChannel.SecondaryChannels.Add(channel);
            //        }
            //    }
            //}
            mergedChannel.Update();
        }
        private void unsubscribeMenuItem_Click(object sender, EventArgs e)
        {
            // suspend listview drawing
            mergedChannelListView.BeginUpdate();
            mergedChannelColumnSorter.Suspend = true;
            this.Cursor = Cursors.WaitCursor;

            foreach (ListViewItem lvi in mergedChannelListView.SelectedItems)
            {
                // grab MergedChannel and record current enable state
                MergedChannel mergedChannel = (MergedChannel)lvi.Tag;
                bool enabled = (mergedChannel.UserBlockedState <= UserBlockedState.Enabled);

                // unsubscribe channel
                unsubscribeChannel(mergedChannel);

                // update the ListViewItem and determine final enable state
                int index = lvi.Index;
                mergedChannelListView.Items[index] = buildMergedChannelLvi(mergedChannel);
                mergedChannelListView.Items[index].Checked = (enabled && !mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"));
            }

            // adjust column widths
            adjustColumnWidths(mergedChannelListView);

            // resume listview drawing and clear selections
            mergedChannelListView.SelectedItems.Clear();
            mergedChannelListView.EndUpdate();
            mergedChannelColumnSorter.Suspend = false;
            mergedChannelListView.Sort();
            this.Cursor = Cursors.Arrow;
        }
        #endregion

        #region ========== Lineup ListView Management ==========
        List<Channel> lineupChannels = new List<Channel>();
        private void buildLineupChannelListView()
        {
            // clear the pulldown list
            cmbObjectStoreLineups.Items.Clear();

            // populate with lineups in object_store
            foreach (Lineup lineup in new Lineups(object_store).ToArray())
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
                    foreach (Device device in new Devices(object_store))
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
        }
        private void btnRefreshLineups_Click(object sender, EventArgs e)
        {
            // close all object store items
            isolateEpgDatabase();

            // null the merged lineup
            mergedLineup_ = null;

            // open object store and repopulate the GUI
            clientForm_Shown(null, null);
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
            foreach (MergedChannel mergedChannel in mergedLineup.GetChannels())
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
            foreach (Device device in new Devices(object_store))
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
        private bool enabledChannelsOnly = false;
        private bool customLabelsOnly = true;
        private int totalMergedChannels;
        private void buildScannedLineupComboBox()
        {
            cmbSources.Items.Clear();
            cmbSources.Items.Add("All Scanned Sources");
            foreach (Device device in new Devices(object_store))
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
        private void buildMergedChannelListView()
        {
            if (mergedLineup == null) return;

            // clear the totalMergedChannels count
            totalMergedChannels = 0;

            // attach lineup sorter to listview
            mergedChannelListView.ListViewItemSorter = mergedChannelColumnSorter;

            // pause listview drawing
            mergedChannelListView.BeginUpdate();

            // populate with new lineup channels
            List<ListViewItem> listViewItems = new List<ListViewItem>();
            foreach (Channel channel in mergedLineup.GetChannels())
            {
                MergedChannel mergedChannel;
                try
                {
                    mergedChannel = (MergedChannel)channel;
                }
                catch
                {
                    continue;
                }

                try
                {
                    // ignore the channels we don't care about
                    if ((!string.IsNullOrEmpty(mergedChannel.CallSign) && mergedChannel.CallSign.StartsWith("Deleted")) ||
                            mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("FINAL") || (mergedChannel.ChannelType == ChannelType.UserHidden) ||
                            mergedChannel.PrimaryChannel.Lineup.LineupTypes.Equals("BB"))
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
                        continue;
                    }
                }
                ++totalMergedChannels;

                if (enabledChannelsOnly && (mergedChannel.UserBlockedState > UserBlockedState.Enabled))
                {
                    continue;
                }

                if (cmbSources.SelectedIndex > 0)
                {
                    bool source = false;
                    if (mergedChannel.PrimaryChannel.Lineup.Equals((Lineup)cmbSources.SelectedItem)) source = true;
                    else
                    {
                        foreach (Channel ch2 in mergedChannel.SecondaryChannels)
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
                    // create the listviewitem
                    ListViewItem lvi;
                    if ((lvi = buildMergedChannelLvi(mergedChannel)) == null) continue;
                    lvi.Checked = (mergedChannel.UserBlockedState <= UserBlockedState.Enabled);
                    listViewItems.Add(lvi);
                }
                catch
                {
                    Logger.WriteError("Exception caught when trying to add a ListViewItem to the MergedChannelListView.");
                }
            }

            if (listViewItems.Count > 0)
            {
                mergedChannelListView.Items.AddRange(listViewItems.ToArray());
            }

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
            List<Lineup> sources = new List<Lineup>();
            if ((mergedChannel.PrimaryChannel.Lineup != null) && mergedChannel.PrimaryChannel.Lineup.Name.StartsWith("Scanned"))
            {
                sources.Add(mergedChannel.PrimaryChannel.Lineup);
            }
            if (mergedChannel.SecondaryChannels != null)
            {
                foreach (Channel ch2 in mergedChannel.SecondaryChannels)
                {
                    if ((ch2.Lineup != null) && (ch2.Lineup.Name != null) && ch2.Lineup.Name.StartsWith("Scanned") && !sources.Contains(ch2.Lineup))
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
                Logger.WriteInformation(string.Format("There are no scanned lineups associated with MergedChannel \"{0}\". Deleting channel.", mergedChannel.ToString()));
                if (!deleteChannel(mergedChannel))
                {
                    Logger.WriteError(string.Format("   Failed to delete MergedChannel \"{0}\".", mergedChannel.ToString()));
                }
                return null;
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
                        case "ClearQAM":
                        case "Digital Cable":
                            tuningInfos.Add(string.Format("C{0}{1}",
                                ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                            break;
                        case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                        case "AuxIn1":
                        case "Antenna":
                        case "Cable":
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
                Logger.WriteInformation(string.Format("There are no tuners associated with \"{0}\". Deleting channel.", mergedChannel.ToString()));
                if (!deleteChannel(mergedChannel))
                {
                    Logger.WriteError(string.Format("   Failed to delete MergedChannel \"{0}\".", mergedChannel.ToString()));
                }
                return null;
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
            // unsubscribe all lineups except for scanned
            unsubscribeChannel(mergedChannel);

            try
            {
                do
                {
                    // clear some channel fields
                    Channel channel = mergedChannel.PrimaryChannel;
                    channel.Service = null;
                    channel.TuningInfos.Clear();
                    foreach (UId id in channel.UIds)
                    {
                        id.Target = null;
                        id.Update();
                    }
                    channel.Update();

                    // remove the Channel from the scanned lineup
                    Lineup scannedLineup = mergedChannel.PrimaryChannel.Lineup;
                    if (scannedLineup != null)
                    {
                        scannedLineup.RemoveChannel(channel);
                        scannedLineup.UncachedChannels.RemoveAllMatching(channel);
                        scannedLineup.ClearChannelCache();
                        scannedLineup.Update();
                    }

                    // clear some MergedChannel fields
                    mergedChannel.PrimaryChannel = mergedChannel.SecondaryChannels.Empty ? null : mergedChannel.SecondaryChannels.First;
                    if (mergedChannel.PrimaryChannel != null)
                    {
                        mergedChannel.SecondaryChannels.RemoveAllMatching(mergedChannel.PrimaryChannel);
                    }
                    mergedChannel.Service = null;
                    mergedChannel.TuningInfos.Clear();
                    mergedChannel.Update();

                    // remove the MergedChannel from the MergedLineup
                    MergedLineup mergedLineup = mergedChannel.Lineup as MergedLineup;
                    if (mergedLineup != null)
                    {
                        mergedLineup.RemoveChannel(mergedChannel);
                        mergedLineup.UncachedChannels.RemoveAllMatching(mergedChannel);
                        mergedLineup.ClearChannelCache();
                        mergedLineup.Update();
                    }
                } while (mergedChannel.PrimaryChannel != null);
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return false;
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

            // resume listview drawing
            mergedChannelListView.EndUpdate();
            btnRefreshLineups_Click(null, null);
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

            MergedChannel mergedChannel = (MergedChannel)mergedChannelListView.Items[e.Item.Index].Tag;

            // if new value is checked, enable merged channel
            if (e.Item.Checked)
            {
                if (mergedChannel.UserBlockedState != UserBlockedState.Enabled)
                {
                    mergedChannel.UserBlockedState = UserBlockedState.Enabled;
                    mergedChannel.Update();
                }
            }
            // if new value is unchecked, block merged channel
            else
            {
                if (mergedChannel.UserBlockedState <= UserBlockedState.Enabled)
                {
                    mergedChannel.UserBlockedState = UserBlockedState.Blocked;
                    mergedChannel.Update();
                }
            }
        }
        private void cmbSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbSources.SelectedIndex < 0) || initLvBuild) return;

            mergedChannelListView.Items.Clear();
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
            rdoFullMode.Enabled = (!task.exist && hasElevatedRights && File.Exists(Helper.Epg123ExePath));
            rdoClientMode.Enabled = (!task.exist && hasElevatedRights);
            tbSchedTime.Enabled = (!task.exist && hasElevatedRights);
            lblUpdateTime.Enabled = (!task.exist && hasElevatedRights);
            cbTaskWake.Enabled = (!task.exist && hasElevatedRights);
            cbAutomatch.Enabled = (!task.exist && hasElevatedRights);
            tbTaskInfo.Enabled = (!task.exist && hasElevatedRights);

            // set radio button controls
            rdoFullMode.Checked = (task.exist && task.actions[0].Path.ToLower().Contains("epg123.exe")) ||
                                  (!task.exist && File.Exists(Helper.Epg123ExePath));
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

            // set automatch checkbox state
            cbAutomatch.Checked = (clientIndex >= 0) && task.actions[clientIndex].Arguments.ToLower().Contains("-match");

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
            btnTask.Text = (task.exist) ? "Delete" : "Create";
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
            // check for elevated rights and open new process if necessary
            if (!hasElevatedRights)
            {
                elevateRights();
                return;
            }

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

                // update panel with current information
                updateTaskPanel();
            }
            // delete current scheduled task
            else if (task.exist)
            {
                task.deleteTask();
                updateTaskPanel();
            }
        }
        private void tbTaskInfo_Click(object sender, EventArgs e)
        {
            // don't modify if text box is displaying current existing task
            if (task.exist || !hasElevatedRights) return;

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
            if ((mergedChannelListView.Items.Count == 0) &&
                (DialogResult.No == MessageBox.Show("There doesn't appear to be any tuners setup in WMC. Importing guide information before TV Setup is complete will corrupt the database. Restoring a lineup (tuner configuration) from a backup is safe.\n\nDo you wish to continue?", "Import Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))) return;

            // configure open file dialog parameters
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
            {
                FileName = string.Empty,
                Filter = "MXF File|*.mxf",
                Title = "Select a MXF File",
                Multiselect = false
            };

            // determine initial path
            if (Directory.Exists(Helper.Epg123OutputFolder) && tbTaskInfo.Text.Equals(Helper.Epg123ExePath))
            {
                openFileDialog1.InitialDirectory = Helper.Epg123OutputFolder;
            }
            else if (!tbTaskInfo.Text.StartsWith("***"))
            {
                openFileDialog1.InitialDirectory = tbTaskInfo.Text.Substring(0, tbTaskInfo.Text.LastIndexOf('\\'));
            }

            // open the dialog
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            // close all object store items
            forceExit = true;
            clientForm_FormClosing(null, null);

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
            activateGuide();
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
        private bool activateGuide()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Media Center\\Settings\\ProgramGuide", true);
            if (key != null)
            {
                try
                {
                    if ((int)key.GetValue("fAgreeTOS") != 1) key.SetValue("fAgreeTOS", 1);
                    if ((string)key.GetValue("strAgreedTOSVersion") != "1.0") key.SetValue("strAgreedTOSVersion", "1.0");
                    key.Close();
                }
                catch
                {
                    MessageBox.Show("Failed to open/edit registry to show the Guide in WMC.", "Registry Access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    key.Close();
                }
                return true;
            }
            return false;
        }
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
        private void isolateEpgDatabase()
        {
            try
            {
                // close and dispose the object store if necessary
                if (objectStore_ != null)
                {
                    // clear the merged channels and lineup combobox
                    mergedChannelListView.Items.Clear();
                    cmbObjectStoreLineups.Items.Clear();
                    cmbSources.Items.Clear();

                    // clear the lineups
                    lineupChannels.Clear();
                    lineupChannelListView.Items.Clear();
                    lineupChannelListView.VirtualListSize = 0;

                    // dispose of the objectstore
                    objectStore_.Dispose();
                    while (!objectStore_.IsDisposed) ;
                    objectStore_ = null;

                    // dispose of the mergedlineup
                    mergedLineup_ = null;

                    // clear the status text
                    lblToolStripStatus.Text = "0 Merged Channels | 0 Lineups | 0 Services";
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                return;
            }
            return;
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
        public static string backupBackupFiles()
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
                return CompressXmlFiles.CreatePackage(backups, "backups");
            }
            return string.Empty;
        }
        private void restoreBackupFiles()
        {
            // determine path to existing  backup file
            openFileDialog1.InitialDirectory = Helper.Epg123BackupFolder;
            openFileDialog1.Filter = "Compressed File|*.zip";
            openFileDialog1.Title = "Select the Compressed Backup ZIP File";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string[] backups = { "lineup.mxf", "recordings.mxf", "subscriptions.mxf" };
            foreach (string backup in backups)
            {
                using (Stream stream = CompressXmlFiles.GetBackupFileStream(backup, openFileDialog1.FileName))
                {
                    if (stream != null)
                    {
                        if (backup == "lineup.mxf")
                        {
                            if (deleteActiveDatabaseFile() == null) return;
                        }
                        MxfImporter.Import(stream, object_store);
                    }
                }
            }
            btnRefreshLineups_Click(null, null);
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
        private void ebtnRestore_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!hasElevatedRights)
            {
                elevateRights();
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            restoreBackupFiles();
            activateGuide();
            disableBackgroundScanning();
            this.Cursor = Cursors.Arrow;
        }
        private void btnBackup_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            backupBackupFiles();
            this.Cursor = Cursors.Arrow;
        }
        private void btnAddChannels_Click(object sender, EventArgs e)
        {
            if (mergedLineup == null) return;

            frmAddChannel addChannelForm = new frmAddChannel();
            addChannelForm.ShowDialog();
            if (addChannelForm.channelAdded)
            {
                this.Cursor = Cursors.WaitCursor;
                btnRefreshLineups_Click(null, null);
                this.Cursor = Cursors.Arrow;
            }
        }
        #endregion

        #region ========== Client Setup ==========
        private void btnSetup_Click(object sender, EventArgs e)
        {
            // check for elevated rights and open new process if necessary
            if (!hasElevatedRights)
            {
                elevateRights();
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            frmClientSetup frm = new frmClientSetup();
            frm.shouldBackup = mergedChannelListView.Items.Count > 0;
            isolateEpgDatabase();
            frm.ShowDialog();

            // refresh listviews
            btnRefreshLineups_Click(null, null);
            this.Cursor = Cursors.Arrow;
        }
        private void btnTransferTool_Click(object sender, EventArgs e)
        {
            Process.Start("epg123Transfer.exe").WaitForExit();
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
            if (object_store == null)
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
            if (!hasElevatedRights)
            {
                elevateRights();
                return;
            }

            // give warning
            if (MessageBox.Show("You are about to delete and rebuild the WMC EPG database. All tuners, recording schedules, favorite lineups, and logos will be restored. The Guide Listings will be empty until an MXF file is imported.\n\nClick 'OK' to continue.", "Database Rebuild", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) return;
            this.Cursor = Cursors.WaitCursor;
            initDatabaseUpdate();

            string epg_db;
            if ((epg_db = deleteActiveDatabaseFile()) == null) return;

            // import lineup, subscription, and recording backups ... try max 3 times each
            bool success = true;
            success = ((importBackupFile(epg_db, "lineup") || importBackupFile(epg_db, "lineup") || importBackupFile(epg_db, "lineup")) &&
                      (importBackupFile(epg_db, "subscriptions") || importBackupFile(epg_db, "subscriptions") || importBackupFile(epg_db, "subscriptions")) &&
                      (importBackupFile(epg_db, "recordings") || importBackupFile(epg_db, "recordings") || importBackupFile(epg_db, "recordings")));

            // do not import the listings ... will confuse the user when there is no search ability (not indexed)
            if (success && (SystemInformation.BootMode != BootMode.Normal))
            {
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("Successfully deleted and rebuilt the database file with tuner configuration and scheduled recordings. Please reboot into Normal Mode and manually import the MXF file to complete the rebuild.", "Safe Mode Detected", MessageBoxButtons.OK);
                return;
            }

            // bring up prompt for loading guide mxf file
            btnRefreshLineups_Click(null, null);
            if (!success)
            {
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("Error occurred during database rebuild. Extended information is recorded in the trace.log file.", "Database Rebuild Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Cursor = Cursors.Arrow;
            btnImport_Click(null, null);
        }
        private bool deleteeHomeFile(string filename)
        {
            // disconnect from store prior to deleting it
            isolateEpgDatabase();

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
                MxfImporter.Import(stream, object_store);
            }

            return true;
        }
        #endregion

        #region ========== Tweak WMC ==========
        private void btnTweakWmc_Click(object sender, EventArgs e)
        {
            // open the tweak gui
            epg123.frmWmcTweak frm = new epg123.frmWmcTweak();
            frm.ShowDialog();
        }
        #endregion

        #region ========== Advanced Functions ==========
        private void btnStoreExplorer_Click(object sender, EventArgs e)
        {
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
            this.Close();
        }
        private void btnExportMxf_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (!Directory.Exists(Helper.Epg123OutputFolder)) Directory.CreateDirectory(Helper.Epg123OutputFolder);

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(new StreamWriter(Helper.Epg123OutputFolder + "\\mxfExport.mxf"), settings))
            {
                MxfExporter.Export(object_store, writer, false);
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