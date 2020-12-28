using System.ComponentModel;
using System.Windows.Forms;
using epg123;

namespace epg123Client
{
    public partial class frmImport : Form
    {
        public bool Success;

        public frmImport(string filepath)
        {
            Application.EnableVisualStyles();
            InitializeComponent();

            WmcUtilities.BackgroundWorker = backgroundWorker1;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(filepath);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Success = WmcUtilities.ImportMxfFile((string)e.Argument);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBarTask.Value != e.ProgressPercentage)
            {
                progressBarTask.Value = e.ProgressPercentage;
                lblTaskProgress.Text = $"{e.ProgressPercentage}%";
            }
            Refresh();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Helper.SendPipeMessage("Import Complete");
            Close();
        }
    }
}