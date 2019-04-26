using System.ComponentModel;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmImport : Form
    {
        public bool success;
        public frmImport(string filepath)
        {
            Application.EnableVisualStyles();
            InitializeComponent();

            mxfImport.backgroundWorker = backgroundWorker1;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(filepath);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            success = mxfImport.importMxfFile((string)e.Argument);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBarTask.Value != e.ProgressPercentage)
            {
                progressBarTask.Value = e.ProgressPercentage;
                lblTaskProgress.Text = string.Format("{0}%", e.ProgressPercentage);
            }
            this.Refresh();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }
    }
}
