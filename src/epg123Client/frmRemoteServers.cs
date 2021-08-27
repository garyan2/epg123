using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using epg123;

namespace epg123Client
{
    public partial class frmRemoteServers : Form
    {
        private readonly ImageList imageList = new ImageList();
        private bool serviceRunning;
        public string mxfPath;

        public frmRemoteServers()
        {
            InitializeComponent();

            listView1.SmallImageList = listView1.LargeImageList = imageList;
            imageList.Images.Add(resImages.EPG123OKDark);
            imageList.ImageSize = new System.Drawing.Size(32, 32);
        }

        private List<string> GetEpg123Servers()
        {
            var responses = new List<string>();
            var request = "EPG123ServerDiscovery";
            var RequestData = Encoding.ASCII.GetBytes(request);

            // need to verify server is not running
            if (ServiceController.GetServices().Any(arg => arg.ServiceName.Equals("epg123Server")))
            {
                var sc = new ServiceController
                {
                    ServiceName = "epg123Server"
                };
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    serviceRunning = true;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "net.exe",
                        Arguments = "stop epg123Server",
                        Verb = "runas"
                    })?.WaitForExit();
                }
            }

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(arg => arg.SupportsMulticast && arg.OperationalStatus == OperationalStatus.Up))
            {
                foreach (var ip in adapter.GetIPProperties().UnicastAddresses.Where(arg => arg.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    var serverEP = new IPEndPoint(IPAddress.Broadcast, Helper.UdpPort);
                    var clientEP = new IPEndPoint(ip.Address, Helper.UdpPort);

                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    socket.ReceiveTimeout = 200;
                    socket.Bind(clientEP);
                    socket.SendTo(RequestData, serverEP);

                    do
                    {
                        try
                        {
                            var buffer = new byte[256];
                            socket.Receive(buffer);
                            var response = Encoding.ASCII.GetString(buffer).Trim('\0');
                            if (!response.Equals(request))
                            {
                                responses.Add(response);
                            }
                        }
                        catch { break; }
                    } while (socket.ReceiveTimeout != 0);
                    socket.Close();
                }
            }

            responses.Sort();
            return responses;
        }

        private void frmRemoteServers_Shown(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            listView1.Items.AddRange(GetEpg123Servers().Select(server => new ListViewItem {Text = server, ImageIndex = 0}).ToArray());
        }

        private void frmRemoteServers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serviceRunning)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "net.exe",
                    Arguments = "start epg123Server",
                    Verb = "runas"
                })?.WaitForExit();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            mxfPath = $"http://{listView1.SelectedItems[0].Text}:{Helper.TcpPort}/output/epg123.mxf";
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
