using GaRyan2.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmProgress : Form
    {
        private bool _done;
        public frmProgress()
        {
            Application.EnableVisualStyles();
            InitializeComponent();
            progressBarStage.Maximum = (sdJson2mxf.sdJson2Mxf.Stages.Length + 1) * 100;

            sdJson2mxf.sdJson2Mxf.BackgroundWorker = backgroundWorker1;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            sdJson2mxf.sdJson2Mxf.Build();
            if (!sdJson2mxf.sdJson2Mxf.Success)
            {
                Logger.WriteError("Failed to create MXF file. Exiting.");
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var text = ((string[])e.UserState);
            lblTaskProgress.Text = text[2];

            var stage = e.ProgressPercentage / 10000;
            var progress = e.ProgressPercentage % 1000;

            if (progressBarStage.Value != stage)
            {
                if (stage > 0)
                {
                    progressBarStage.Value = stage * 100 + progress;

                }
                lblTaskTitle.Text = text[0];
                lblStageProgress.Text = text[1];
            }

            if (progressBarTask.Value == progress) return;
            progressBarTask.Value = Math.Min(progress, progressBarTask.Maximum);
            if (progress <= 0) return;
            progressBarTask.Value = Math.Min(progress - 1, progressBarTask.Maximum);
            progressBarTask.Value = Math.Min(progress, progressBarTask.Maximum);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _done = true;
            Close();
        }

        private void frmProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_done) return;
            var result = MessageBox.Show("Closing this form will abort the update. Do you wish to Abort?", "Abort Update", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if (!sdJson2mxf.sdJson2Mxf.StationLogosDownloadComplete)
                {
                    sdJson2mxf.sdJson2Mxf.BackgroundDownloader.CancelAsync();
                    while (!sdJson2mxf.sdJson2Mxf.StationLogosDownloadComplete)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
                Logger.WriteInformation("***** Update was aborted by the user. *****");
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
