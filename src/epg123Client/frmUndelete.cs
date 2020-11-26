using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using epg123;

namespace epg123Client
{
    public partial class frmUndelete : Form
    {
        double dpiScaleFactor = 1.0;
        private ListViewColumnSorter channelColumnSorter = new ListViewColumnSorter();
        public bool channelAdded = false;
        private List<ListViewItem> listViewItems = new List<ListViewItem>();

        public frmUndelete()
        {
            InitializeComponent();

            // adjust components for screen dpi
            using (Graphics g = CreateGraphics())
            {
                if ((g.DpiX != 96) || (g.DpiY != 96))
                {
                    dpiScaleFactor = g.DpiX / 96;
                }
            }
        }

        private void frmUndelete_Shown(object sender, EventArgs e)
        {
            // reset sorting column and order
            channelColumnSorter.Order = SortOrder.Ascending;
            channelColumnSorter.SortColumn = 1;

            // build listview
            listView1.BeginUpdate();
            buildListView();
            listView1.EndUpdate();

            int[] minWidths = { 100, 60, 100, 100 };
            foreach (ColumnHeader header in listView1.Columns)
            {
                int currentWidth = header.Width;
                header.Width = -1;
                header.Width = Math.Max(Math.Max(header.Width, currentWidth), (int)(minWidths[header.Index] * dpiScaleFactor));
            }
        }

        private void buildListView()
        {
            // scan all the channels to find any that are orphaned
            // the referencing merged channels will have a null lineup
            List<Channel> scannedChannels = new Channels(WmcStore.WmcObjectStore).ToList();
            foreach (Channel scannedChannel in scannedChannels)
            {
                if ((scannedChannel.ChannelType != ChannelType.CalculatedScanned && scannedChannel.ChannelType != ChannelType.Scanned) ||
                    scannedChannel.Lineup == null) continue;

                bool orphaned = true;

                // scan through the referencing primary channels
                if (orphaned)
                {
                    foreach (MergedChannel channel in scannedChannel.ReferencingPrimaryChannels)
                    {
                        if (channel.Lineup != null) { orphaned = false; break; }
                    }
                }

                // scan through the referencing secondary channels
                if (orphaned)
                {
                    foreach (MergedChannel channel in scannedChannel.ReferencingSecondaryChannels)
                    {
                        if (channel.Lineup != null) { orphaned = false; break; }
                    }
                }

                // if all referencing channels have a null lineup, do some magic
                if (orphaned)
                {
                    listViewItems.Add(buildOrphanedChannelLvi(scannedChannel));
                }
            }
            listViewItems.Sort(channelColumnSorter);
            listView1.VirtualListSize = listViewItems.Count;
        }

        private ListViewItem buildOrphanedChannelLvi(Channel orphanedChannel)
        {
            // build original channel number string
            string originalChannelNumber = orphanedChannel.OriginalNumber.ToString();
            if (orphanedChannel.OriginalSubNumber > 0) originalChannelNumber += ("." + orphanedChannel.OriginalSubNumber.ToString());

            // build tuning info
            string tuneInfos = WmcStore.GetAllTuningInfos(orphanedChannel);

            // build ListViewItem
            try
            {
                ListViewItem listViewItem = new ListViewItem(new string[]
                {
                    orphanedChannel.CallSign,
                    originalChannelNumber,
                    trimScannedLineupName(orphanedChannel.Lineup.Name),
                    tuneInfos
                })
                {
                    Tag = orphanedChannel
                };

                return listViewItem;
            }
            catch { }

            return null;
        }

        private void lvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == channelColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (channelColumnSorter.Order == SortOrder.Ascending)
                {
                    channelColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    channelColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                channelColumnSorter.SortColumn = e.Column;
                channelColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listViewItems.Sort(channelColumnSorter);
            listView1.Refresh();
        }

        private string trimScannedLineupName(string name)
        {
            string ret = name.Remove(0, 9);
            return ret.Remove(ret.LastIndexOf(')'), 1);
        }

        private void btnUndelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                try
                {
                    Channel channel = (Channel)item.Tag;
                    channel.Lineup.NotifyChannelAdded(channel);
                }
                catch (Exception ex)
                {
                    Logger.WriteInformation(ex.Message);
                }
            }
            channelAdded = true;
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (listViewItems.Count == 0) e.Item = new ListViewItem();
            else e.Item = listViewItems[e.ItemIndex];
        }
    }
}