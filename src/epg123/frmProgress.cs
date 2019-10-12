using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmProgress : Form
    {
        private bool done = false;
        public frmProgress(epgConfig config)
        {
            Application.EnableVisualStyles();
            InitializeComponent();

            sdJson2mxf.backgroundWorker = backgroundWorker1;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(config);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            sdJson2mxf.Build((epgConfig)e.Argument);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string[] text = ((string[])e.UserState);
            lblTaskProgress.Text = text[2];

            int stage = e.ProgressPercentage / 10000;
            int progress = e.ProgressPercentage % 1000;

            if (progressBarStage.Value != stage)
            {
                if (stage > 0)
                {
                    progressBarStage.Value = stage * 100 + progress;

                }
                lblTaskTitle.Text = text[0];
                lblStageProgress.Text = text[1];
            }

            if (progressBarTask.Value != progress)
            {
                progressBarTask.Value = Math.Min(progress, progressBarTask.Maximum);
                if (progress > 0)
                {
                    progressBarTask.Value = Math.Min(progress - 1, progressBarTask.Maximum);
                    progressBarTask.Value = Math.Min(progress, progressBarTask.Maximum);
                }
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            done = true;
            this.Close();
        }

        private void frmProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!done)
            {
                var result = MessageBox.Show("Closing this form will abort the update. Do you wish to Abort?", "Abort Update", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (!sdJson2mxf.stationLogosDownloadComplete)
                    {
                        sdJson2mxf.backgroundDownloader.CancelAsync();
                        while (!sdJson2mxf.stationLogosDownloadComplete)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    Logger.WriteInformation("Update was aborted by the user.");
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
