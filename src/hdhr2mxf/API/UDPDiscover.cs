using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;

namespace GaRyan2.SiliconDustApi
{
    internal static class UDPDiscover
    {
        private const int TIMEOUT_MS = 1000;
        private const int udpPort = 65001;
        private static readonly byte[] bcast = { 0x00, 0x02,                 // HDHOMERUN_TYPE_DISCOVER_REQ
                                                 0x00, 0x06,                 // payload size (6 bytes)
                                                 0x01,                       // HDHOMERUN_TAG_DEVICE_TYPE
                                                 0x04,                       // length (4 bytes)
                                                 0xFF, 0xFF, 0xFF, 0xFF,     // HDHOMERUN_DEVICE_TYPE_WILDCARD
                                                 0x4C, 0x20, 0xCB, 0x4E };   // CRC

        public static List<HdhrDiscover> DiscoverDevicesUdp()
        {
            var devices = new List<HdhrDiscover>();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(arg => arg.SupportsMulticast && arg.OperationalStatus == OperationalStatus.Up))
            {
                foreach (var ip in adapter.GetIPProperties().UnicastAddresses.Where(arg => arg.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    var responses = new List<byte[]>(10); // pre-allocate 10 responses
                    var udpServer = new IPEndPoint(IPAddress.Any, 0);
                    var udpClient = new UdpClient(new IPEndPoint(ip.Address, udpPort))
                    {
                        EnableBroadcast = true
                    };
                    udpClient.Client.ReceiveTimeout = TIMEOUT_MS;
                    udpClient.AllowNatTraversal(true);
                    udpClient.Send(bcast, bcast.Length, new IPEndPoint(IPAddress.Broadcast, udpPort));

                    do
                    {
                        try
                        {
                            var buffer = udpClient.Receive(ref udpServer);
                            if (buffer[0] == 0x00 && buffer[1] == 0x03) // discovery response
                            {
                                responses.Add(buffer);
                            }
                        }
                        catch { break; } // client will timeout and throw exception
                    } while (true);
                    udpClient.Close();

                    foreach (var response in responses)
                    {
                        if (!UdpFunctions.ValidCRC(response)) continue;
                        var dev = new HdhrDiscover();
                        var size = response[2] << 8 | response[3];
                        var pos = 4;
                        while (pos < size + 4)
                        {
                            var tag = response[pos++];
                            var len = ((response[pos] & 0x80) == 0)
                                ? response[pos++]
                                : (response[pos++] & 0x7F) | (response[pos++] << 7);
                            var load = new byte[len];
                            Buffer.BlockCopy(response, pos, load, 0, len);
                            pos += len;
                            switch (tag)
                            {
                                case 0x02: // device id & legacy determination
                                    var id = load[0] << 24 | load[1] << 16 | load[2] << 8 | load[3];
                                    dev.DeviceId = $"{id:X2}";
                                    switch (id >> 20)
                                    {
                                        case 0x100: // TECH-US/TECH3-US
                                            dev.Legacy = (id < 0x10040000) ? 1 : 0;
                                            break;
                                        case 0x120: // TECH3-EU
                                            dev.Legacy = (id < 0x12030000) ? 1 : 0;
                                            break;
                                        case 0x101: // HDHR-US
                                        case 0x102: // HDHR-T1-US
                                        case 0x103: // HDHR3-US
                                        case 0x111: // HDHR3-DT
                                        case 0x121: // HDHR-EU
                                        case 0x122: // HDHR3-EU
                                            dev.Legacy = 1;
                                            break;
                                    }
                                    break;
                                case 0x27: // lineup url
                                    dev.LineupUrl = Encoding.ASCII.GetString(load);
                                    break;
                                case 0x28: // storage url
                                    dev.StorageURL = Encoding.ASCII.GetString(load);
                                    break;
                                case 0x2A: // base url
                                    dev.BaseUrl = Encoding.ASCII.GetString(load);
                                    dev.DiscoverUrl = dev.BaseUrl + "/discover.json";
                                    var ipv4 = Regex.Match(dev.BaseUrl, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(:[0-9]+)?\b");
                                    if (ipv4.Success) { dev.LocalIp = ipv4.Value; }
                                    break;
                                case 0x2C: // storage id
                                    dev.StorageID = Encoding.ASCII.GetString(load);
                                    break;
                            }
                        }
                        if (devices.Any(x => x.BaseUrl == dev.BaseUrl)) continue;
                        devices.Add(dev);
                    }
                }
            }
            return devices;
        }
    }
}