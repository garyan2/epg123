using System;
using System.Drawing;
using System.Windows.Forms;

namespace epg123_gui
{
    internal class MemberListViewItem : ListViewItem
    {
        public MemberStation Station { get; set; }

        public MemberListViewItem(string channelNumber, MemberStation station) : base(new string[4])
        {
            Station = station;

            // build listviewitem text
            SubItems[0].Text = Station.CustomCallsign ?? Station.CallSign;
            SubItems[1].Text = channelNumber;
            SubItems[2].Text = Station.StationId;
            SubItems[3].Text = Station.CustomServiceName ?? Station.Name;

            // build listviewitem appearance
            Checked = Station.Include;
            ForeColor = Station.Include ? SystemColors.WindowText : SystemColors.GrayText;
            BackColor = Station.IsNew ? Color.Pink : default;
            ImageKey = Station.LanguageCode;

            // subscribe to station events
            Station.IncludeChanged += (sender, args) =>
            {
                Checked = Station.Include;
                ForeColor = Station.Include ? SystemColors.WindowText : SystemColors.GrayText;
            };

            Station.LogoChanged += (sender, args) =>
            {
                try
                {
                    if (ListView?.InvokeRequired ?? false) ListView?.Invoke(new Action(delegate { ListView?.Invalidate(Bounds); }));
                    else ListView?.Invalidate(Bounds);
                }
                catch { }
            };
        }
    }
}