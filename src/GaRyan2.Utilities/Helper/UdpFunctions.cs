using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace GaRyan2.Utilities
{
    public static class UdpFunctions
    {
        public class ServerDetails
        {
            public override string ToString()
            {
                return $"{Name} ({Address})";
            }
            public string Name { get; set; }
            public string Address { get; set; }
            public bool Epg123 { get; set; }
            public bool Hdhr2Mxf { get; set; }
        }

        private static readonly int TIMEOUT_MS = 1000;
        public static readonly byte[] DiscoveryRequest = new byte[] { 0x01, 0x7F, 0x00, 0x04,   // client to server, discovery request, 4-byte payload
                                                                      0x01, 0x01, 0xFF,         // all supported features
                                                                      0x02, 0x00,               // dns name
                                                                      0x7D, 0xD9, 0xE1, 0x16 }; // crc
        public static uint CalculateCRC(byte[] data)
        {
            var index = 0;
            uint crc = 0xFFFFFFFF;
            while (index < data.Length - 4)
            {
                uint x = crc ^ data[index++];
                crc >>= 8;
                if ((x & 0x01) > 0) crc ^= 0x77073096;
                if ((x & 0x02) > 0) crc ^= 0xEE0E612C;
                if ((x & 0x04) > 0) crc ^= 0x076DC419;
                if ((x & 0x08) > 0) crc ^= 0x0EDB8832;
                if ((x & 0x10) > 0) crc ^= 0x1DB71064;
                if ((x & 0x20) > 0) crc ^= 0x3B6E20C8;
                if ((x & 0x40) > 0) crc ^= 0x76DC4190;
                if ((x & 0x80) > 0) crc ^= 0xEDB88320;
            }
            return crc ^= 0xFFFFFFFF;
        }

        public static bool ValidCRC(byte[] data)
        {
            return CalculateCRC(data) == BitConverter.ToUInt32(data, data.Length - 4);
        }

        public static List<ServerDetails> DiscoverServers(bool epg123Only)
        {
            var servers = new List<ServerDetails>();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(arg => arg.SupportsMulticast && arg.OperationalStatus == OperationalStatus.Up))
            {
                foreach (var ip in adapter.GetIPProperties().UnicastAddresses.Where(arg => arg.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    var responses = new Dictionary<IPAddress, byte[]>();
                    var udpServer = new IPEndPoint(IPAddress.Any, 0);
                    var udpClient = new UdpClient(new IPEndPoint(ip.Address, 0))
                    {
                        EnableBroadcast = true
                    };
                    udpClient.Client.ReceiveTimeout = TIMEOUT_MS;
                    udpClient.AllowNatTraversal(true);

                    try
                    {
                        udpClient.Send(DiscoveryRequest, DiscoveryRequest.Length, new IPEndPoint(IPAddress.Broadcast, Helper.TcpUdpPort));
                    }
                    catch { continue; }

                    do
                    {
                        try
                        {
                            var buffer = udpClient.Receive(ref udpServer);
                            if (buffer[0] == 0x10 && buffer[1] == 0xFF)
                            {
                                if (udpServer.Address.Equals(ip.Address)) continue;
                                responses.Add(udpServer.Address, buffer);
                            }
                        }
                        catch { break; }
                    } while (true);
                    udpClient.Close();

                    foreach (var response in responses)
                    {
                        if (!ValidCRC(response.Value)) continue;
                        var server = new ServerDetails() { Address = $"{response.Key}" };
                        var size = response.Value[3] << 8 | response.Value[2];
                        var pos = 4;
                        while (pos < size - 4)
                        {
                            var tag = response.Value[pos++];
                            var len = response.Value[pos++];
                            var load = new byte[len];
                            Buffer.BlockCopy(response.Value, pos, load, 0, len);
                            pos += len;
                            switch (tag)
                            {
                                case 0x01:
                                    if ((load[0] & 0x01) > 0) server.Epg123 = true;
                                    if ((load[0] & 0x02) > 0) server.Hdhr2Mxf = true;
                                    break;
                                case 0x02:
                                    server.Name = Encoding.ASCII.GetString(load);
                                    break;
                            }
                        }
                        if (server.Epg123 || (server.Hdhr2Mxf && !epg123Only)) servers.Add(server);
                    }
                }
            }
            return servers.OrderBy(arg => arg.Name).ToList();
        }

        public static bool StopService()
        {
            bool ret;
            if (ret = ServiceRunning()) StartStopService("stop");
            return ret;
        }

        public static void StartService()
        {
            if (!ServiceRunning())
            {
                StartStopService("start");
                Thread.Sleep(100);
            }
        }

        private static void StartStopService(string arg)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "net.exe",
                Arguments = $"{arg} epg123Server",
                Verb = "runas"
            })?.WaitForExit();
        }

        public static bool ServiceRunning()
        {
            if (ServiceController.GetServices().Any(arg => arg.ServiceName.Equals("epg123Server")))
            {
                var sc = new ServiceController
                {
                    ServiceName = "epg123Server"
                };
                return sc.Status == ServiceControllerStatus.Running;
            }
            return false;
        }
    }
}