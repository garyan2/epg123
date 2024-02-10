using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmMerge : Form
    {
        private ListViewItem _itemDnD = null;
        public List<long> mergeOrder = new List<long>();

        public frmMerge(List<long> idList)
        {
            InitializeComponent();

            foreach (var id in idList)
            {
                listView1.Items.Add(new myChannelLvi(WmcStore.WmcObjectStore.Fetch(id) as MergedChannel) { Tag = id });
            }

            listView1.Columns[5].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[4].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[3].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            _itemDnD = listView1.GetItemAt(e.X, e.Y);
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_itemDnD == null) return;
            Cursor = Cursors.Hand;
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_itemDnD == null) return;

            var itemOver = listView1.GetItemAt(0, e.Y);
            if (itemOver == null) return;

            var rc = itemOver.GetBounds(ItemBoundsPortion.Entire);

            var insertBefore = false || e.Y < rc.Top + (rc.Height / 2);

            if (_itemDnD != itemOver)
            {
                listView1.Items.Remove(_itemDnD);
                if (insertBefore)
                {
                    listView1.Items.Insert(itemOver.Index, _itemDnD);
                }
                else
                {
                    listView1.Items.Insert(itemOver.Index + 1, _itemDnD);
                }
            }

            Cursor = Cursors.Default;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                mergeOrder.Add((long)item.Tag);
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}