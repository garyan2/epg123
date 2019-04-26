using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace epg123Transfer
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        public string username;
        public string passwordHash;

        private void btnLogin_Click(object sender, EventArgs e)
        {
            username = txtLoginName.Text;
            passwordHash = HashPassword(txtPassword.Text);
            this.Close();
        }

        private string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = SHA1.Create().ComputeHash(bytes);
            return HexStringFromBytes(hashBytes);
        }
        private string HexStringFromBytes(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnLogin_Click(null, null);
            }
        }
    }
}
