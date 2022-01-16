using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace epg123
{
    public partial class frmCustomChannel : Form
    {
        private CustomStation _station;

        public frmCustomChannel(CustomStation station, List<myStation> stations)
        {
            _station = station;

            InitializeComponent();
            this.Text = station.Name;
            tbChannel.Text = $"{station.Number}{(station.Subnumber == 0 ? "" : $".{station.Subnumber}")}";
            tbMatchname.Text = station.MatchName;
            comboBox1.Items.AddRange(stations.ToArray());
            comboBox1.Text = $"{station}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var myStation = (myStation) comboBox1.SelectedItem;
            _station.Callsign = myStation.Callsign;
            _station.Name = myStation.Name;
            _station.StationId = myStation.StationId;
            _station.MatchName = tbMatchname.Text;

            var nums = tbChannel.Text.Split('.');
            if (nums.Length == 0)
            {
                _station.Number = -1;
                _station.Subnumber = 0;
                return;
            }

            _station.Number = int.Parse(nums[0]);
            _station.Subnumber = 0;
            if (nums.Length == 2)
            {
                _station.Subnumber = int.Parse(nums[1]);
            }
            this.Close();
        }

        private void tbChannel_KeyPress(object sender, KeyPressEventArgs e)
        {
            // make sure it is numbers only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '-') e.Handled = true;
            if (e.KeyChar == '.' && ((TextBox) sender).Text.IndexOf('.') > -1) e.Handled = true;
            if (e.KeyChar == '-' && ((TextBox) sender).Text.Length != 0) e.Handled = true;
        }
    }
}
