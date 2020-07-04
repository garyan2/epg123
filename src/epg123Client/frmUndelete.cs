using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.TV.Tuning;

namespace epg123
{
    public partial class frmUndelete : Form
    {
        double dpiScaleFactor = 1.0;
        private ListViewColumnSorter channelColumnSorter = new ListViewColumnSorter();
        public bool channelAdded = false;

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
            // attach lineup sorter to listview
            listView1.ListViewItemSorter = channelColumnSorter;

            // scan all the channels to find any that are orphaned
            // the referencing merged channels will have a null lineup
            List<Channel> scannedChannels = new Channels(Store.objectStore).ToList();
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
                    //scannedChannel.Lineup.NotifyChannelAdded(scannedChannel);
                    listView1.Items.Add(buildOrphanedChannelLvi(scannedChannel));
                }
            }
        }

        private ListViewItem buildOrphanedChannelLvi(Channel orphanedChannel)
        {
            // build original channel number string
            string originalChannelNumber = orphanedChannel.OriginalNumber.ToString();
            if (orphanedChannel.OriginalSubNumber > 0) originalChannelNumber += ("." + orphanedChannel.OriginalSubNumber.ToString());
            string customChannelNumber = orphanedChannel.Number.ToString();
            if (orphanedChannel.SubNumber > 0) customChannelNumber += ("." + orphanedChannel.SubNumber.ToString());

            // build tuning info
            HashSet<string> tuningInfos = new HashSet<string>();
            foreach (TuningInfo tuningInfo in orphanedChannel.TuningInfos)
            {
                // handle any overrides to tuninginfo
                // assumes that tuninginfo is valid unless an override explicitly says otherwise
                bool shown = true;
                if (!tuningInfo.Overrides.Empty)
                {
                    foreach (TuningInfoOverride tuningInfoOverride in tuningInfo.Overrides)
                    {
                        if (tuningInfoOverride.Channel.Id != orphanedChannel.Id) continue;
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

                // unfortunately the lock emoji didn't come into play until Unicode 6.0.0
                // Windows 7 uses Unicode 5.x, it will just show an open block
                string lockSymbol = "\uD83D\uDD12";
                if (tuningInfo is DvbTuningInfo)
                {
                    DvbTuningInfo ti = tuningInfo as DvbTuningInfo;
                    switch (tuningInfo.TuningSpace)
                    {
                        case "DVB-T":
                            // formula to convert channel (n) to frequency (fc) is fc = 8n + 306 (in MHz)
                            // offset is -167KHz, 0Hz, +167KHz => int offset = ti.Frequency - (channel * 8000) - 306000;
                            int channel = (ti.Frequency - 305833) / 8000;
                            tuningInfos.Add(string.Format("{0}UHF C{1}", ti.IsEncrypted ? lockSymbol : string.Empty, channel));
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
                            tuningInfos.Add(string.Format("{0}{1:F0}{2} ({3})", ti.IsEncrypted ? lockSymbol : string.Empty, ti.Frequency / 1000.0, polarization, ti.Sid));
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
                            tuningInfos.Add(string.Format("{0}C{1}{2}", ti.IsEncrypted ? lockSymbol : string.Empty,
                                ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                            break;
                        case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                            tuningInfos.Add(string.Format("IR {0}{1}",
                                ti.PhysicalNumber, (ti.SubNumber > 0) ? "." + ti.SubNumber.ToString() : null));
                            break;
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
            listView1.Sort();
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
    }
}