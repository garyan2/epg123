using System;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmXmltvConfig : Form
    {
        private epgConfig localConfig;

        public frmXmltvConfig(ref epgConfig config)
        {
            InitializeComponent();
            localConfig = config;

            ckChannelNumbers.Checked = config.XmltvIncludeChannelNumbers;
            ckChannelLogos.Checked = !string.IsNullOrEmpty(config.XmltvIncludeChannelLogos) && (config.XmltvIncludeChannelLogos != "false");
            ckLocalLogos.Checked = (config.XmltvIncludeChannelLogos == "local") || (config.XmltvIncludeChannelLogos == "substitute");
            ckUrlLogos.Checked = (config.XmltvIncludeChannelLogos == "url") || !ckLocalLogos.Checked;
            ckSubstitutePath.Checked = (config.XmltvIncludeChannelLogos == "substitute");
            txtSubstitutePath.Text = config.XmltvLogoSubstitutePath;
            ckXmltvFillerData.Checked = config.XmltvAddFillerData;

            ckLogos_CheckedChanged(ckChannelLogos, null);
        }

        private void frmXmltvConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            localConfig.XmltvIncludeChannelNumbers = ckChannelNumbers.Checked;
            localConfig.XmltvLogoSubstitutePath = txtSubstitutePath.Text;
            localConfig.XmltvAddFillerData = ckXmltvFillerData.Checked;

            if (!ckChannelLogos.Checked)
            {
                localConfig.XmltvIncludeChannelLogos = "false";
            }
            else if (ckUrlLogos.Checked)
            {
                localConfig.XmltvIncludeChannelLogos = "url";
            }
            else if (!ckSubstitutePath.Checked)
            {
                localConfig.XmltvIncludeChannelLogos = "local";
            }
            else
            {
                localConfig.XmltvIncludeChannelLogos = "substitute";
            }
        }

        private void ckLogos_CheckedChanged(object sender, EventArgs e)
        {
            if (sender.Equals(ckChannelLogos))
            {
                ckUrlLogos.Enabled = ckLocalLogos.Enabled = ckChannelLogos.Checked;
                ckSubstitutePath.Enabled = (ckLocalLogos.Checked && ckChannelLogos.Checked);
                txtSubstitutePath.Enabled = (ckSubstitutePath.Checked && ckLocalLogos.Checked && ckChannelLogos.Checked);
            }
            else if (sender.Equals(ckUrlLogos))
            {
                ckLocalLogos.Checked = !ckUrlLogos.Checked;
            }
            else if (sender.Equals(ckLocalLogos))
            {
                ckUrlLogos.Checked = !ckLocalLogos.Checked;
                ckSubstitutePath.Enabled = ckLocalLogos.Checked;
                txtSubstitutePath.Enabled = ckSubstitutePath.Checked && ckLocalLogos.Checked;
            }
            else if (sender.Equals(ckSubstitutePath))
            {
                txtSubstitutePath.Enabled = ckSubstitutePath.Checked;
            }
        }
    }
}