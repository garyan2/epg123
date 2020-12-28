using System;
using System.Collections.Generic;
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

        public string Username;
        public string PasswordHash;

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Username = txtLoginName.Text;
            PasswordHash = HashPassword(txtPassword.Text);
            Close();
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = SHA1.Create().ComputeHash(bytes);
            return HexStringFromBytes(hashBytes);
        }
        private static string HexStringFromBytes(IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
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