using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmUndelete : Form
    {
        readonly double _dpiScaleFactor = 1.0;
        private readonly ListViewColumnSorter _channelColumnSorter = new ListViewColumnSorter();
        public bool ChannelAdded;
        private readonly List<ListViewItem> _listViewItems = new List<ListViewItem>();

        public frmUndelete()
        {
            InitializeComponent();

            // adjust components for screen dpi
            using (var g = CreateGraphics())
            {
                if ((int)g.DpiX != 96 || (int)g.DpiY != 96)
                {
                    _dpiScaleFactor = g.DpiX / 96;
                }
            }
        }

        private void frmUndelete_Shown(object sender, EventArgs e)
        {
            // reset sorting column and order
            _channelColumnSorter.Order = SortOrder.Ascending;
            _channelColumnSorter.SortColumn = 1;

            // build listview
            listView1.BeginUpdate();
            BuildListView();
            listView1.EndUpdate();

            int[] minWidths = { 100, 60, 100, 100 };
            foreach (ColumnHeader header in listView1.Columns)
            {
                var currentWidth = header.Width;
                header.Width = -1;
                header.Width = Math.Max(Math.Max(header.Width, currentWidth), (int)(minWidths[header.Index] * _dpiScaleFactor));
            }
        }

        private void BuildListView()
        {
            // scan all the channels to find any that are orphaned
            // the referencing merged channels will have a null lineup
            var scannedChannels = new Channels(WmcStore.WmcObjectStore).ToList();
            foreach (var scannedChannel in scannedChannels)
            {
                if ((scannedChannel.ChannelType != ChannelType.CalculatedScanned && scannedChannel.ChannelType != ChannelType.Scanned) ||
                    scannedChannel.Lineup == null) continue;

                var orphaned = true;

                // scan through the referencing primary channels
                foreach (MergedChannel channel in scannedChannel.ReferencingPrimaryChannels)
                {
                    if (channel.Lineup == null || channel.ChannelType == ChannelType.UserHidden) continue;
                    orphaned = false;
                    break;
                }

                // scan through the referencing secondary channels
                if (orphaned)
                {
                    foreach (MergedChannel channel in scannedChannel.ReferencingSecondaryChannels)
                    {
                        if (channel.Lineup == null || channel.ChannelType == ChannelType.UserHidden) continue;
                        orphaned = false;
                        break;
                    }
                }

                // if all referencing channels have a null lineup, do some magic
                if (orphaned)
                {
                    _listViewItems.Add(BuildOrphanedChannelLvi(scannedChannel));
                }
            }
            _listViewItems.Sort(_channelColumnSorter);
            listView1.VirtualListSize = _listViewItems.Count;
        }

        private ListViewItem BuildOrphanedChannelLvi(Channel orphanedChannel)
        {
            // build original channel number string
            var originalChannelNumber = orphanedChannel.OriginalNumber.ToString();
            if (orphanedChannel.OriginalSubNumber > 0) originalChannelNumber += "." + orphanedChannel.OriginalSubNumber;

            // build tuning info
            var tuneInfos = WmcStore.GetAllTuningInfos(orphanedChannel);

            // build ListViewItem
            try
            {
                var listViewItem = new ListViewItem(new[]
                {
                    orphanedChannel.CallSign,
                    originalChannelNumber,
                    TrimScannedLineupName(orphanedChannel.Lineup.Name),
                    tuneInfos
                })
                {
                    Tag = orphanedChannel
                };

                return listViewItem;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _channelColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                _channelColumnSorter.Order = _channelColumnSorter.Order == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _channelColumnSorter.SortColumn = e.Column;
                _channelColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            _listViewItems.Sort(_channelColumnSorter);
            listView1.Refresh();
        }

        private static string TrimScannedLineupName(string name)
        {
            var ret = name.Remove(0, 9);
            return ret.Remove(ret.LastIndexOf(')'), 1);
        }

        private void btnUndelete_Click(object sender, EventArgs e)
        {
            foreach (int index in listView1.SelectedIndices)
            {
                try
                {
                    var channel = (Channel)_listViewItems[index].Tag;
                    channel.Lineup.NotifyChannelAdded((Channel)_listViewItems[index].Tag);
                }
                catch (Exception ex)
                {
                    Logger.WriteInformation($"{Helper.ReportExceptionMessages(ex)}");
                }
            }
            ChannelAdded = true;
            Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = _listViewItems.Count == 0 ? new ListViewItem() : _listViewItems[e.ItemIndex];
        }
    }
}