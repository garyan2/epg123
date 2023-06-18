using epg123.Properties;
using GaRyan2.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmRemoteServers : Form
    {
        private readonly ImageList imageList = new ImageList();
        private readonly bool serviceRunning;
        public string cfgPath;

        public frmRemoteServers()
        {
            InitializeComponent();

            listView1.SmallImageList = listView1.LargeImageList = imageList;
            imageList.Images.Add(Resources.EPG123);
            imageList.ImageSize = new System.Drawing.Size(32, 32);
            serviceRunning = UdpFunctions.StopService();
        }

        private void frmRemoteServers_Shown(object sender, EventArgs e)
        {
            btnRefresh.Enabled = false;
            listView1.Items.Clear();
            Refresh();

            Cursor = Cursors.WaitCursor;
            listView1.Items.AddRange(UdpFunctions.DiscoverServers(true).Select(server => new ListViewItem { Text = $"{server}", ImageIndex = 0, Tag = server }).ToArray());
            Cursor = Cursors.Default;
            btnRefresh.Enabled = true;
        }

        private void frmRemoteServers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serviceRunning) UdpFunctions.StartService();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var server = (UdpFunctions.ServerDetails)listView1.SelectedItems[0].Tag;
            cfgPath = $"http://{server.Address}:{Helper.TcpUdpPort}/epg123/epg123.cfg";
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}