using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tokenServer
{
    public partial class Server
    {
        private UdpClient _udpServer;

        public void StartUdpListener()
        {
            _udpServer = new UdpClient(Helper.UdpPort);
            var responseData = Encoding.ASCII.GetBytes(Dns.GetHostName());
            var clientEp = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    var clientRequestData = _udpServer.Receive(ref clientEp);
                    var clientRequest = Encoding.ASCII.GetString(clientRequestData);
                    switch (clientRequest)
                    {
                        case "EPG123ServerDiscovery": // provide host domain name and ip address
                            _udpServer.Send(responseData, responseData.Length, clientEp);
                            break;
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.Interrupted)
                {
                    Helper.WriteLogEntry(e.Message);
                    _udpServer.Close();
                }
            }
        }
    }
}
