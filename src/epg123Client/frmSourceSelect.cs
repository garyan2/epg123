using System.Diagnostics;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmSourceSelect : Form
    {
        public frmSourceSelect()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://shop.silicondust.com/shop/product-category/software/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://schedulesdirect.org/signup");
        }
    }
}
