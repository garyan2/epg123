using GaRyan2.Utilities;
using System;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmStorage : Form
    {
        EpgNotifier epgNotifier;

        public frmStorage()
        {
            InitializeComponent();
            epgNotifier = Helper.ReadJsonFile(Helper.EmailNotifier, typeof(EpgNotifier)) ?? new EpgNotifier();
            numWarning.Value = epgNotifier.StorageWarningGB;
            numError.Value = epgNotifier.StorageErrorGB;
            numConflictWarning.Value = epgNotifier.ConflictWarningDays;
            numConflictError.Value = epgNotifier.ConflictErrorDays;
        }

        private void numWarning_ValueChanged(object sender, EventArgs e)
        {
            epgNotifier.StorageWarningGB = (int)numWarning.Value;
        }

        private void numError_ValueChanged(object sender, EventArgs e)
        {
            epgNotifier.StorageErrorGB = (int)numError.Value;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Helper.WriteJsonFile(epgNotifier, Helper.EmailNotifier);
            Close();
        }

        private void numConflictWarning_ValueChanged(object sender, EventArgs e)
        {
            epgNotifier.ConflictWarningDays = (int)numConflictWarning.Value;
        }

        private void numConflictError_ValueChanged(object sender, EventArgs e)
        {
            epgNotifier.ConflictErrorDays = (int)numConflictError.Value;
        }
    }
}
