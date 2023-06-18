using GaRyan2;
using GaRyan2.Utilities;
using System;
using System.IO;
using System.ServiceProcess;

namespace tokenServer
{
    public partial class Server : ServiceBase
    {
        HttpImageServer _imageServer = new HttpImageServer();
        HttpFileServer _fileServer = new HttpFileServer();
        UdpServer _udpServer = new UdpServer();
        ConfigServer _configServer = new ConfigServer();

        public Server()
        {
            InitializeComponent();

            this.CanShutdown = true;

            Directory.CreateDirectory(Helper.Epg123ImageCache);
        }

        protected override void OnStart(string[] args)
        {
            Github.Initialize($"EPG123/{Helper.Epg123Version}", "epg123");
            Helper.DeleteFile(Helper.ServerLogPath);
            StartConfigFileWatcher();
            JsonImageCache.GetAllImageSizes();
            JsonImageCache.AddImagesMissingInCacheFile();

            SchedulesDirect.Initialize();
            _imageServer.Start();
            _fileServer.Start();

            _udpServer.Start();

            _configServer.Start();

            WebStats.StartTime = DateTime.Now;
        }

        protected override void OnShutdown()
        {
            Cleanup();
            base.OnShutdown();
        }

        protected override void OnStop()
        {
            Cleanup();
            base.OnStop();
        }

        private void Cleanup()
        {
            StopConfigFileWatcher();
            _imageServer.Stop();
            _fileServer.Stop();
            _udpServer.Stop();
            _configServer.Stop();
            JsonImageCache.Save();
        }
    }
}
