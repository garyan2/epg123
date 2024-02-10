using GaRyan2.SchedulesDirectAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SdApi = GaRyan2.SchedulesDirect;

namespace epg123_gui
{
    public partial class frmLineups : Form
    {
        private readonly LineupResponse _oldLineups;       // existing lineup

        public HashSet<string> NewLineups = new HashSet<string>();
        public bool Cancel = true;

        public frmLineups()
        {
            InitializeComponent();

            // get current lineups
            _oldLineups = SdApi.GetSubscribedLineups();
            if (_oldLineups?.Lineups == null || (_oldLineups.Lineups.Count == 0)) return;

            // populate listview with current lineups
            foreach (var lineup in _oldLineups.Lineups)
            {
                listView1.Items.Add(new ListViewItem(new[] { lineup.Transport, lineup.Name, lineup.Location, lineup.Lineup })
                {
                    Tag = lineup.Lineup,
                    ToolTipText = lineup.Lineup
                });
            }

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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // check to see if at capacity
            if (listView1.Items.Count >= SdApi.MaxLineups)
            {
                MessageBox.Show("You are at the maximum number of supported lineups and must\ndelete a lineup from the list in order to add another.");
                return;
            }

            // open the client subform to add new lineup
            var subform = new frmLineupAdd();
            subform.ShowDialog();
            if (subform.AddLineup == null) return;

            // check to see if lineup is already subscribed to
            if (listView1.Items.Cast<ListViewItem>().Any(item => (string)item.Tag == subform.AddLineup.Lineup))
            {
                MessageBox.Show("Selected lineup already exists in the list.", "Request Ignored");
                return;
            }

            // add the new lineup
            listView1.Items.Add(new ListViewItem(new[] { subform.AddLineup.Transport, subform.AddLineup.Name, subform.AddLineup.Location })
            {
                Tag = subform.AddLineup.Lineup,
                ToolTipText = subform.AddLineup.Lineup
            });
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            for (var i = listView1.CheckedItems.Count; i > 0;)
            {
                listView1.CheckedItems[--i].Remove();
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // determine deletions first
            if (_oldLineups?.Lineups != null)
            {
                foreach (var lineup in _oldLineups.Lineups)
                {
                    var delete = listView1.Items.Cast<ListViewItem>().All(item => (string)item.Tag != lineup.Lineup);
                    if (delete)
                    {
                        SdApi.RemoveLineup(lineup.Lineup);
                    }
                }
            }

            // add the new lineups
            foreach (ListViewItem item in listView1.Items)
            {
                var add = true;
                if (_oldLineups?.Lineups != null)
                {
                    if (_oldLineups.Lineups.Any(lineup => (string)item.Tag == lineup.Lineup))
                    {
                        add = false;
                    }
                }

                if (!add) continue;
                if (SdApi.AddLineup((string)item.Tag))
                {
                    NewLineups.Add((string)item.Tag);
                }
                else
                {
                    MessageBox.Show($"Failed to add lineup \"{(string)item.Tag}\" to your account. Check the log for more details.");
                }
            }
            Cancel = false;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
