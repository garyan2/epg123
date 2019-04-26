using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmPreview : Form
    {
        ListViewColumnSorter sorter = new ListViewColumnSorter();

        private string previewLineup;
        public frmPreview(string lineup)
        {
            InitializeComponent();
            previewLineup = lineup;
            this.Text = previewLineup;

            listView1.ListViewItemSorter = sorter;
        }

        private void buildLineupServices(string lineup)
        {
            IList<sdLineupPreviewChannel> channels = sdAPI.sdPreviewLineupChannels(lineup);
            if (channels == null)
            {
                MessageBox.Show("There was an error retrieving channels for requested lineup. See trace.log file for more detail.", "Response Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (sdLineupPreviewChannel channel in channels)
            {
                items.Add(new ListViewItem(new string[]
                {
                        channel.Channel.TrimStart('0'),
                        channel.Callsign,
                        channel.Name
                }));
            }

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
            this.Cursor = Cursors.WaitCursor;
            buildLineupServices(previewLineup);
            this.Cursor = Cursors.Arrow;

            if (listView1.Items.Count == 0)
            {
                this.Close();
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == sorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (sorter.Order == SortOrder.Ascending)
                {
                    sorter.Order = SortOrder.Descending;
                }
                else
                {
                    sorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                sorter.SortColumn = e.Column;
                sorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView1.Sort();
            listView1.Refresh();
        }

        private void frmPreview_Load(object sender, EventArgs e)
        {

            // adjust components for screen dpi
            using (Graphics g = CreateGraphics())
            {
                if ((g.DpiX != 96) || (g.DpiY != 96))
                {
                    // adjust column widths for list views
                    foreach (ColumnHeader column in listView1.Columns)
                    {
                        column.Width = (int)(column.Width * g.DpiX / 96) - 1;
                    }
                }
            }
        }
    }
}
