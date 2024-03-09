﻿using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmImport : Form
    {
        private readonly bool notify;
        private bool downloading = true;
        public bool Success;

        public frmImport(string filepath, bool notifyComplete = true)
        {
            Application.EnableVisualStyles();
            InitializeComponent();
            notify = notifyComplete;

            WmcStore.BackgroundWorker = backgroundWorker1;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(filepath);
            ShowDownload();
        }

        private async void ShowDownload()
        {
            var start = DateTime.Now;
            while (downloading)
            {
                await Task.Delay(100);
                label1.Text = $"Downloading. Please Wait. [{DateTime.Now - start:mm\\:ss}]";
            }
            label1.Text = "";
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Success = WmcStore.ImportMxfFile((string)e.Argument, true);
            statusLogo.StatusImage((string)e.Argument);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBarTask.Value != e.ProgressPercentage)
            {
                downloading = false;
                progressBarTask.Value = e.ProgressPercentage;
                lblTaskProgress.Text = $"{e.ProgressPercentage}%";
            }
            Refresh();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (notify) Helper.SendPipeMessage("Import Complete");
            Close();
        }
    }
}