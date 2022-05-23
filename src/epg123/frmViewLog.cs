using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmViewLog : Form
    {
        public frmViewLog()
        {
            InitializeComponent();

            using (StreamReader sr = new StreamReader(Helper.Epg123TraceLogPath))
            {
                richTextBox1.SuspendLayout();

                // read the line
                string line = null;
                do
                {
                    line = sr.ReadLine();
                    if (line == null) break;

                    // determine if within last 24 hours
                    DateTime dt = DateTime.MinValue;
                    if (!DateTime.TryParse(line.Substring(1, Math.Max(line.IndexOf(']') - 1, 0)), out dt) && richTextBox1.Text.Length == 0) continue;

                    // add line with color
                    if (line.Contains("[ERROR]") || dt == DateTime.MinValue)
                    {
                        richTextBox1.SelectionColor = Color.Red;
                    }
                    else if (line.Contains("[WARNG]") || line.ToLower().Contains("failed") || line.Contains("SD API WebException") || line.Contains("SD responded") || line.Contains("Did not receive") || line.Contains("Problem occurred"))
                    {
                        richTextBox1.SelectionColor = Color.Yellow;
                    }
                    else if (line.Contains("==========") || line.Contains("Activating") || line.Contains("Beginning"))
                    {
                        richTextBox1.SelectionColor = Color.White;
                    }
                    else if (line.Contains("Entering") || line.Contains("Exiting"))
                    {
                        richTextBox1.SelectionColor = Color.Cyan;
                    }
                    else
                    {
                        richTextBox1.SelectionColor = Color.ForestGreen;
                    }
                    richTextBox1.AppendText($"{line}\n");
                }
                while (line != null);

                richTextBox1.ResumeLayout();
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
