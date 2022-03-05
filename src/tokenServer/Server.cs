﻿using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace tokenServer
{
    public partial class Server : ServiceBase
    {
        public Server()
        {
            InitializeComponent();

            this.CanShutdown = true;

            Helper.ExecutablePath = AppDomain.CurrentDomain.BaseDirectory;
            Directory.CreateDirectory(Helper.Epg123ImageCache);
        }

        protected override void OnStart(string[] args)
        {
            Helper.DeleteLogFile();
            StartRegistryWatcher();
            WebStats.StartTime = DateTime.Now;
            JsonImageCache.GetAllImageSizes();
            JsonImageCache.AddImagesMissingInCacheFile();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                StartTcpListener();
            }).Start();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                StartUdpListener();
            }).Start();
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
            _regWatcher?.Stop();
            _tcpListener?.Stop();
            _udpServer?.Close();
            JsonImageCache.Save();
        }
    }
}
