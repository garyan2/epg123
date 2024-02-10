using System;
using System.Linq;
using System.Windows.Forms;
using SdApi = GaRyan2.SchedulesDirect;

namespace epg123_gui
{
    public partial class frmPreview : Form
    {
        readonly ListViewColumnSorter _sorter = new ListViewColumnSorter();

        private readonly string _previewLineup;
        public frmPreview(string lineup)
        {
            InitializeComponent();
            _previewLineup = lineup;
            Text = _previewLineup;

            listView1.ListViewItemSorter = _sorter;
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void BuildLineupServices(string lineup)
        {
            var channels = SdApi.GetLineupPreviewChannels(lineup);
            if (channels == null)
            {
                MessageBox.Show("There was an error retrieving channels for requested lineup. See trace.log file for more detail.", "Response Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var items = channels.Select(channel => new ListViewItem(new[] { channel.Channel.TrimStart('0'), channel.Callsign, channel.Name })).ToList();

            if (items.Count > 0)
            {
                listView1.Items.AddRange(items.ToArray());
            }
            else
            {
                MessageBox.Show("There were no channels in the lineup to display.", "Empty Lineup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void frmPreview_Shown(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            BuildLineupServices(_previewLineup);
            Cursor = Cursors.Arrow;

            if (listView1.Items.Count == 0)
            {
                Close();
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _sorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                _sorter.Order = _sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _sorter.SortColumn = e.Column;
                _sorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView1.Sort();
            listView1.Refresh();
        }

        private void frmPreview_Load(object sender, EventArgs e)
        {

            // adjust components for screen dpi
            using (var g = CreateGraphics())
            {
                if ((int)g.DpiX == 96 && (int)g.DpiY == 96) return;
                // adjust column widths for list views
                foreach (ColumnHeader column in listView1.Columns)
                {
                    column.Width = (int)(column.Width * g.DpiX / 96) - 1;
                }
            }
        }
    }
}
