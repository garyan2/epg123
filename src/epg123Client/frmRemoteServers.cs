using GaRyan2.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmRemoteServers : Form
    {
        private readonly ImageList imageList = new ImageList();
        public string mxfPath;

        public frmRemoteServers()
        {
            InitializeComponent();

            listView1.SmallImageList = listView1.LargeImageList = imageList;
            imageList.Images.Add(resImages.EPG123OKDark);
            imageList.ImageSize = new System.Drawing.Size(32, 32);
        }

        private void frmRemoteServers_Shown(object sender, EventArgs e)
        {
            btnRefresh.Enabled = btnSearch.Enabled = false;
            listView1.Items.Clear();
            Refresh();

            Cursor = Cursors.WaitCursor;
            listView1.Items.AddRange(UdpFunctions.DiscoverServers(false).Select(server => new ListViewItem { Text = $"{server}", ImageIndex = 0, Tag = server }).ToArray());
            Cursor = Cursors.Default;
            btnRefresh.Enabled = btnSearch.Enabled = true;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string file = "epg123.mxf";
            var server = (UdpFunctions.ServerDetails)listView1.SelectedItems[0].Tag;
            if (!server.Epg123 && server.Hdhr2Mxf) file = "hdhr2mxf.mxf";
            else if (server.Epg123 && server.Hdhr2Mxf && (DialogResult.Yes == MessageBox.Show(
                "Both EPG123 (Schedules Direct) and HDHR2MXF (SiliconDust) are installed on this server. Would you like to use HDHR2MXF instead of the default EPG123?\n\nNote: You must have an active DVR Subscription with SiliconDust.",
                "Select MXF Source", MessageBoxButtons.YesNo, MessageBoxIcon.Question)))
            {
                file = "hdhr2mxf.mxf";
            }
            mxfPath = $"http://{server.Address}:{Helper.TcpUdpPort}/output/{file}";
            Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // determine path to existing file
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.NetworkShortcuts);
            openFileDialog1.Filter = "MXF File|*.mxf";
            openFileDialog1.Title = "Select a MXF File";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            mxfPath = openFileDialog1.FileName;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}