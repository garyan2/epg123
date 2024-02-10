using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace epg123Client
{
    public class myLineupLvi : ListViewItem
    {
        public long ChannelId { get; private set; }

        public string Callsign { get; private set; }

        public string Number { get; private set; }

        public myLineupLvi(Channel channel) : base(new string[3])
        {
            ChannelId = channel.Id;
            SubItems[0].Text = Callsign = channel.CallSign;
            SubItems[1].Text = Number = channel.ChannelNumber.ToString();
            SubItems[2].Text = channel.Service.Name;
        }
    }

    public class myChannelLvi : ListViewItem
    {
        public long ChannelId { get; private set; }
        public HashSet<long> ScannedLineupIds { get; private set; }
        public bool Enabled { get; private set; }
        private bool Custom { get; set; } = true;
        public string Callsign { get; private set; }
        public string CustomCallsign { get; private set; }
        public string Number { get; private set; }
        public string CustomNumber { get; private set; }
        private MergedChannel MergedChannel { get; set; }
        public bool IsEncrypted => MergedChannel.IsEncrypted;
        public bool IsSuggestedBlocked => MergedChannel.IsSuggestedBlocked;
        public bool IsUnknown { get; private set; }
        public bool IsTV { get; private set; }
        public bool IsRadio { get; private set; }
        public bool IsInteractiveTV { get; private set; }

        private void SetServiceTypeFlags()
        {
            var channel = (MergedChannel.PrimaryChannel.ChannelType == ChannelType.Scanned || MergedChannel.PrimaryChannel.ChannelType == ChannelType.CalculatedScanned || MergedChannel.PrimaryChannel.ChannelType == ChannelType.UserAdded) && MergedChannel.PrimaryChannel.Lineup != null
                ? MergedChannel.PrimaryChannel
                : MergedChannel.SecondaryChannels.FirstOrDefault(arg => (arg.ChannelType == ChannelType.Scanned || arg.ChannelType == ChannelType.CalculatedScanned || arg.ChannelType == ChannelType.UserAdded) && arg.Lineup != null);

            IsUnknown = IsTV = IsRadio = IsInteractiveTV = false;
            if (channel == null) return;
            switch (channel.Service.ServiceType)
            {
                case 0:
                    if (channel.ChannelType == ChannelType.Scanned || channel.ChannelType == ChannelType.CalculatedScanned || channel.ChannelType == ChannelType.UserAdded) IsUnknown = true;
                    else IsInteractiveTV = true;
                    break;
                case 1:
                    IsTV = true;
                    break;
                case 2:
                    IsRadio = true;
                    break;
                case 3:
                    IsInteractiveTV = true;
                    break;
            }
        }

        public myChannelLvi(MergedChannel channel) : base(new string[7])
        {
            MergedChannel = channel;
            MergedChannel.Updated += Channel_Updated;

            ChannelId = MergedChannel.Id;
            UseItemStyleForSubItems = false;
            PopulateMergedChannelItems();
        }

        public void RemoveDelegate()
        {
            MergedChannel.Refresh();
            MergedChannel.Updated -= Channel_Updated;
            MergedChannel = null;
        }

        private void Channel_Updated(object sender, StoredObjectEventArgs e)
        {
            if (ListView != null && ListView.InvokeRequired)
            {
                try
                {
                    ListView?.Invoke(new Action(delegate
                    {
                        ((myChannelLvi)ListView?.Items[Index]).PopulateMergedChannelItems();
                        ((myChannelLvi)ListView?.Items[Index]).ShowCustomLabels(Custom);
                        ListView?.Invalidate(Bounds);
                    }));
                }
                catch
                {
                    // do nothing
                }
            }
            else
            {
                PopulateMergedChannelItems();
                ShowCustomLabels(Custom);
            }
        }

        public void ShowCustomLabels(bool set)
        {
            if (Custom == set) return;
            Custom = set;
            SubItems[0].Text = Custom ? CustomCallsign ?? Callsign : Callsign;
            SubItems[1].Text = Custom ? CustomNumber ?? Number : Number;
        }

        public void PopulateMergedChannelItems()
        {
            MergedChannel.Refresh();
            SetServiceTypeFlags();
            var scanned = MergedChannel.PrimaryChannel.Lineup?.Name?.StartsWith("Scanned") ?? false;

            // set callsign and backcolor
            Callsign = MergedChannel.PrimaryChannel.CallSign;
            CustomCallsign = MergedChannel.HasUserSpecifiedCallSign ? MergedChannel.CallSign : null;
            SubItems[0].Text = Custom ? CustomCallsign ?? Callsign : Callsign;
            SubItems[0].BackColor = MergedChannel.HasUserSpecifiedCallSign ? Color.Pink : SystemColors.Window;

            // set number and backcolor
            Number = $"{MergedChannel.OriginalNumber}{(MergedChannel.OriginalSubNumber > 0 ? $".{MergedChannel.OriginalSubNumber}" : "")}";
            CustomNumber = MergedChannel.HasUserSpecifiedNumber || MergedChannel.HasUserSpecifiedSubNumber ? $"{MergedChannel.Number}{(MergedChannel.SubNumber > 0 ? $".{MergedChannel.SubNumber}" : "")}" : null;
            SubItems[1].Text = Custom ? CustomNumber ?? Number : Number;
            SubItems[1].BackColor = MergedChannel.HasUserSpecifiedNumber || MergedChannel.HasUserSpecifiedSubNumber ? Color.Pink : SystemColors.Window;

            // set service name, lineup name, and guide end time
            SubItems[2].Text = !scanned ? MergedChannel.Service?.Name : "";
            SubItems[3].Text = !scanned ? MergedChannel.PrimaryChannel.Lineup?.Name : "";
            SubItems[6].Text = !scanned ? MergedChannel.Service?.ScheduleEndTime.ToLocalTime().ToString() : "";

            // set scanned sources and tuning info
            ScannedLineupIds = WmcStore.GetAllScannedSourcesForChannel(MergedChannel);
            if (ScannedLineupIds.Count > 0)
            {
                var names = new HashSet<string>();
                foreach (var name in ScannedLineupIds.Select(id =>
                    ((Lineup)WmcStore.WmcObjectStore.Fetch(id)).Name.Remove(0, 9)))
                {
                    names.Add(name.Remove(name.Length - 1));
                }

                var text = string.Empty;
                foreach (var name in names)
                {
                    if (!string.IsNullOrEmpty(text)) text += " + ";
                    text += name;
                }
                SubItems[4].Text = text;
            }
            SubItems[5].Text = WmcStore.GetAllTuningInfos((Channel)MergedChannel);

            // set checkbox
            Checked = Enabled = (!MergedChannel.IsSuggestedBlocked || MergedChannel.UserBlockedState != UserBlockedState.Unknown) && MergedChannel.UserBlockedState <= UserBlockedState.Enabled;
        }
    }
}