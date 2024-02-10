using System;
using System.Windows.Forms;

namespace GaRyan2.Utilities
{
    public partial class frmEmail : Form
    {
        EpgNotifier notifier;

        public frmEmail()
        {
            InitializeComponent();

            btnTest.Focus();
        }

        private void frmEmail_Load(object sender, EventArgs e)
        {
            notifier = Helper.ReadJsonFile(Helper.EmailNotifier, typeof(EpgNotifier)) ?? new EpgNotifier();

            txtUsername.Text = notifier.Username;
            txtPassword.Text = notifier.Password;
            txtSmtpServer.Text = notifier.SmtpServer;
            txtPortNumber.Text = notifier.SmtpPort.ToString();
            chkSsl.Checked = notifier.EnableSsl;

            txtSender.Text = notifier.SendFrom;
            txtRecipient.Text = notifier.SendTo;

            chkSuccess.Checked = (notifier.NotifyOn & 0x01) > 0;
            chkUpdate.Checked = (notifier.NotifyOn & 0x02) > 0;
            chkWarning.Checked = (notifier.NotifyOn & 0x04) > 0;
            chkError.Checked = (notifier.NotifyOn & 0x08) > 0;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var config = new EpgNotifier
            {
                Username = txtUsername.Text,
                Password = txtPassword.Text,
                SmtpServer = txtSmtpServer.Text,
                SmtpPort = int.Parse(txtPortNumber.Text),
                EnableSsl = chkSsl.Checked,
                SendFrom = txtSender.Text,
                SendTo = txtRecipient.Text,
                NotifyOn = (chkSuccess.Checked ? 0x01 : 0x00) | (chkUpdate.Checked ? 0x02 : 0x00) | (chkWarning.Checked ? 0x04 : 0x00) | (chkError.Checked ? 0x08 : 0x00),
                StorageWarningGB = notifier.StorageWarningGB,
                StorageErrorGB = notifier.StorageErrorGB,
                ConflictWarningDays = notifier.ConflictWarningDays,
                ConflictErrorDays = notifier.ConflictErrorDays
            };

            Cursor = Cursors.WaitCursor;
            if (Logger.SendTestMessage(config))
            {
                Helper.WriteJsonFile(config, Helper.EmailNotifier);
                _ = MessageBox.Show("Success. Notification configuration has been saved.", "Test E-mail Message");
            }
            else
            {
                _ = MessageBox.Show("Failed. Check your SMTP server configurations and try again.", "Test E-mail Message");
            }
            Cursor = Cursors.Default;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPortNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }
    }
}
