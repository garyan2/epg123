using GaRyan2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace GaRyan2.SiliconDustApi
{
    internal static class UDPDiscover
    {
        private const int TIMEOUT_MS = 500;
        private const int udpPort = 65001;
        private static readonly byte[] bcast = { 0x00, 0x02,                 // HDHOMERUN_TYPE_DISCOVER_REQ
                                                 0x00, 0x06,                 // payload size (6 bytes)
                                                 0x01,                       // HDHOMERUN_TAG_DEVICE_TYPE
                                                 0x04,                       // length (4 bytes)
                                                 0xFF, 0xFF, 0xFF, 0xFF,     // HDHOMERUN_DEVICE_TYPE_WILDCARD
                                                 0x4C, 0x20, 0xCB, 0x4E };   // CRC
        private enum HDHOMERUN_TYPE
        {
            DISCOVER_REQ = 0x0002,
            DISCOVER_RPY = 0x0003,
            GETSET_REQ = 0x0004,
            GETSET_RPY = 0x0005,
            UPGRADE_REQ = 0x0006,
            UPGRADE_RPY = 0x0007
        }
        private enum HDHOMERUN_TAG
        {
            DEVICE_TYPE = 0x01,
            DEVICE_ID = 0x02,
            GETSET_NAME = 0x03,
            GETSET_VALUE = 0x04,
            ERROR_MESSAGE = 0x05,
            TUNER_COUNT = 0x10,
            GETSET_LOCKKEY = 0x15,
            LINEUP_URL = 0x27,
            STORAGE_URL = 0x28,
            DEVICE_AUTH_BIN_DEPRECATED = 0x29,
            BASE_URL = 0x2A,
            DEVICE_AUTH_STR = 0x2B,
            STORAGE_ID = 0x2C,
            MULTI_TYPE = 0x2D
        }
        public static List<HdhrDiscover> DiscoverDevicesUdp()
        {
            var devices = new List<HdhrDiscover>();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(arg => arg.SupportsMulticast && arg.OperationalStatus == OperationalStatus.Up))
            {
                foreach (var ip in adapter.GetIPProperties().UnicastAddresses.Where(arg => arg.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    var responses = new List<byte[]>(10); // pre-allocate 10 responses
                    var udpServer = new IPEndPoint(IPAddress.Any, 0);
                    var udpClient = new UdpClient(new IPEndPoint(ip.Address, 0))
                    {
                        EnableBroadcast = true
                    };
                    udpClient.Client.ReceiveTimeout = TIMEOUT_MS;
                    udpClient.AllowNatTraversal(true);

                    try { udpClient.Send(bcast, bcast.Length, new IPEndPoint(IPAddress.Broadcast, udpPort)); } catch { continue; }

                    do
                    {
                        try
                        {
                            var buffer = udpClient.Receive(ref udpServer);
                            if (buffer[0] == 0x00 && buffer[1] == (byte)HDHOMERUN_TYPE.DISCOVER_RPY) // discovery response
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
                            switch ((HDHOMERUN_TAG)tag)
                            {
                                case HDHOMERUN_TAG.DEVICE_TYPE:
                                case HDHOMERUN_TAG.MULTI_TYPE:
                                    break;
                                case HDHOMERUN_TAG.DEVICE_ID:
                                    if (BitConverter.IsLittleEndian) Array.Reverse(load);
                                    var id = BitConverter.ToInt32(load, 0);
                                    dev.DeviceId = $"{id:X2}";
                                    switch (id >> 20)
                                    {
                                        case 0x100: // TECH-US/TECH3-US
                                            dev.Legacy = (id < 0x10040000);
                                            break;
                                        case 0x120: // TECH3-EU
                                            dev.Legacy = (id < 0x12030000);
                                            break;
                                        case 0x101: // HDHR-US
                                        case 0x102: // HDHR-T1-US
                                        case 0x103: // HDHR3-US
                                        case 0x111: // HDHR3-DT
                                        case 0x121: // HDHR-EU
                                        case 0x122: // HDHR3-EU
                                            dev.Legacy = true;
                                            break;
                                    }
                                    break;
                                case HDHOMERUN_TAG.TUNER_COUNT:
                                    dev.TunerCount = load[0];
                                    break;
                                case HDHOMERUN_TAG.LINEUP_URL:
                                    dev.LineupUrl = Encoding.ASCII.GetString(load);
                                    break;
                                case HDHOMERUN_TAG.STORAGE_URL:
                                    dev.StorageURL = Encoding.ASCII.GetString(load);
                                    break;
                                case HDHOMERUN_TAG.BASE_URL:
                                    dev.BaseUrl = Encoding.ASCII.GetString(load);
                                    dev.DiscoverUrl = dev.BaseUrl + "/discover.json";
                                    var ipv4 = Regex.Match(dev.BaseUrl, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(:[0-9]+)?\b");
                                    if (ipv4.Success) { dev.LocalIp = ipv4.Value; }
                                    break;
                                case HDHOMERUN_TAG.DEVICE_AUTH_STR:
                                    dev.DeviceAuth = Encoding.ASCII.GetString(load);
                                    break;
                                case HDHOMERUN_TAG.STORAGE_ID:
                                    dev.StorageID = Encoding.ASCII.GetString(load);
                                    break;
                            }
                        }
                        if (devices.Any(x => x.BaseUrl == dev.BaseUrl) || (dev.StorageID != null && devices.Any(x => x.StorageID == dev.StorageID))) continue;
                        devices.Add(dev);
                    }
                }
            }
            return devices;
        }
    }
}