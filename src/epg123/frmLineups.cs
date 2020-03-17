using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmLineups : Form
    {
        SdLineupResponse oldLineups;       // existing lineup

        public HashSet<string> newLineups = new HashSet<string>();
        public bool cancel = true;

        public frmLineups()
        {
            InitializeComponent();

            // get current lineups
            oldLineups = sdAPI.sdGetLineups();
            if ((oldLineups == null) || (oldLineups.Lineups == null) || (oldLineups.Lineups.Count == 0)) return;

            // populate listview with current lineups
            foreach (SdLineup lineup in oldLineups.Lineups)
            {
                listView1.Items.Add(new ListViewItem(new string[] { lineup.Transport, lineup.Name, lineup.Location, lineup.Lineup })
                {
                    Tag = lineup.Lineup,
                    ToolTipText = lineup.Lineup
                });
            }

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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // check to see if at capacity
            if (listView1.Items.Count >= sdAPI.maxLineups)
            {
                MessageBox.Show("You are at the maximum number of supported lineups and must\ndelete a lineup from the list in order to add another.");
                return;
            }

            // open the client subform to add new lineup
            frmLineupAdd subform = new frmLineupAdd();
            subform.ShowDialog();
            if (subform.addLineup == null) return;

            // check to see if lineup is already subscribed to
            foreach (ListViewItem item in listView1.Items)
            {
                if ((string)item.Tag == subform.addLineup.Lineup)
                {
                    MessageBox.Show("Selected lineup already exists in the list.", "Request Ignored");
                    return;
                }
            }

            // add the new lineup
            listView1.Items.Add(new ListViewItem(new string[] { subform.addLineup.Transport, subform.addLineup.Name, subform.addLineup.Location })
            {
                Tag = subform.addLineup.Lineup,
                ToolTipText = subform.addLineup.Lineup
            });
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            for (int i = listView1.CheckedItems.Count; i > 0;)
            {
                listView1.CheckedItems[--i].Remove();
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // determine deletions first
            if ((oldLineups != null) && (oldLineups.Lineups != null))
            {
                foreach (SdLineup lineup in oldLineups.Lineups)
                {
                    bool delete = true;
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if ((string)item.Tag == lineup.Lineup)
                        {
                            delete = false;
                            break;
                        }
                    }
                    if (delete)
                    {
                        sdAPI.removeLineup(lineup.Lineup);
                    }
                }
            }

            // add the new lineups
            foreach (ListViewItem item in listView1.Items)
            {
                bool add = true;
                if ((oldLineups != null) && (oldLineups.Lineups != null))
                {
                    foreach (SdLineup lineup in oldLineups.Lineups)
                    {
                        if ((string)item.Tag == lineup.Lineup)
                        {
                            add = false;
                            break;
                        }
                    }
                }
                if (add)
                {
                    if (sdAPI.addLineup((string)item.Tag))
                    {
                        newLineups.Add((string)item.Tag);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Failed to add lineup \"{0}\" to your account. Check the log for more details.", (string)item.Tag));
                    }
                }
            }
            cancel = false;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
