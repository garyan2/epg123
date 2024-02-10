using GaRyan2.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace epg123Server
{
    public class UdpServer : IDisposable
    {
        private readonly UdpClient _listener;
        private readonly Thread _listenerThread;
        private readonly ManualResetEvent _stop;
        private readonly byte[] DiscoveryResponse;

        public UdpServer()
        {
            // build discovery response
            var hostName = Dns.GetHostName();
            var payloadSz = hostName.Length + 5;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write((byte)0x10); // server to client
                writer.Write((byte)0xFF); // discovery response
                writer.Write((ushort)payloadSz); // message size
                writer.Write((byte)0x01); // supported features
                writer.Write((byte)0x01); // features size
                writer.Write((byte)((File.Exists(Helper.Epg123ExePath) ? 1 : 0) | (File.Exists(Helper.Hdhr2MxfExePath) ? 2 : 0))); // features
                writer.Write((byte)0x02); // server dns name
                writer.Write(hostName); // dns host name
                writer.Write(uint.MinValue); // empty crc
                DiscoveryResponse = ms.ToArray();
            }
            Buffer.BlockCopy(BitConverter.GetBytes(UdpFunctions.CalculateCRC(DiscoveryResponse)), 0, DiscoveryResponse, DiscoveryResponse.Length - 4, 4);

            // initialize
            _stop = new ManualResetEvent(false);
            _listener = new UdpClient(Helper.TcpUdpPort);
            _listenerThread = new Thread(HandleRequests);
        }

        public void Start()
        {
            _listenerThread.Start();
            Logger.WriteInformation($"UDP Server initialized.");
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            _listener.Close();
        }

        private void HandleRequests()
        {
            while (true)
            {
                var client = _listener.BeginReceive(ProcessDatagram, null);
                if (0 == WaitHandle.WaitAny(new[] { _stop, client.AsyncWaitHandle })) return;
            }
        }

        private void ProcessDatagram(IAsyncResult ar)
        {
            var clientEp = new IPEndPoint(IPAddress.Any, 0);
            var buffer = _listener.EndReceive(ar, ref clientEp);
            if (buffer.SequenceEqual(UdpFunctions.DiscoveryRequest))
            {
                _listener.Send(DiscoveryResponse, DiscoveryResponse.Length, clientEp);
            }
        }
    }
}