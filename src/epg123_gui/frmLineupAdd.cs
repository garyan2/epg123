using GaRyan2.SchedulesDirectAPI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SdApi = GaRyan2.SchedulesDirect;

namespace epg123_gui
{
    public partial class frmLineupAdd : Form
    {
        public SubscribedLineup AddLineup;

        private readonly List<Country> _countries = new List<Country>();

        private string _mask;
        private List<SubscribedLineup> _headends = new List<SubscribedLineup>();

        public frmLineupAdd()
        {
            InitializeComponent();
            var isoCountry = System.Globalization.RegionInfo.CurrentRegion.ThreeLetterISORegionName;

            var countryResp = SdApi.GetAvailableCountries();
            if (countryResp != null)
            {
                // add the only freeview listing
                _countries.Add(new Country()
                {
                    FullName = "Great Britain Freeview",
                    OnePostalCode = true,
                    PostalCode = "/",
                    PostalCodeExample = null,
                    ShortName = "GBR"
                });

                // get all the region/countries
                var regions = new List<string>(countryResp.Keys);
                foreach (var country in regions.Where(region => !region.ToLower().Equals("zzz")).Select(region => countryResp[region]).SelectMany(regionCountries => regionCountries))
                {
                    _countries.Add(country);
                }

                // sort the countries
                _countries = _countries.OrderBy(o => o.FullName).ToList();
                foreach (var country in _countries)
                {
                    cmbCountries.Items.Add(country.FullName);
                }

                // add a manual option
                _countries.Add(null);
                _countries.Add(new Country()
                {
                    FullName = "Manual lineup input...",
                    OnePostalCode = false,
                    PostalCode = "/[A-Z]+-[A-Z0-9.]+-[A-Z0-9]+",
                    PostalCodeExample = "USA-CA00053-DEFAULT",
                    ShortName = "EPG123"
                });
                cmbCountries.Items.Add(string.Empty); cmbCountries.Items.Add("Manual lineup input...");
            }

            var index = 0;
            for (var i = 0; i < _countries.Count; ++i)
            {
                if (_countries[i] == null) continue;
                if (string.IsNullOrEmpty(_countries[i].ShortName) || !_countries[i].ShortName.ToUpper().Equals(isoCountry)) continue;
                index = i;
                break;
            }
            cmbCountries.SelectedIndex = index;

            // automatically fetch the zipcode as entered during TV Setup and perform a fetch
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings\ProgramGuide", false))
                {
                    if (key == null) return;
                    txtZipcode.Text = ((string)key.GetValue("strLocation", "")).Split(' ')[0];
                    if (txtZipcode.Text != "") btnFetch_Click(btnFetch, null);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void cmbCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            _headends.Clear();
            btnFetch.Enabled = false;
            txtZipcode.Enabled = false;
            lblExample.Text = string.Empty;
            txtZipcode.Text = string.Empty;

            if (string.IsNullOrEmpty(cmbCountries.Text)) return;
            if (string.IsNullOrEmpty(_countries[cmbCountries.SelectedIndex].PostalCodeExample)) GetTransmitters(_countries[cmbCountries.SelectedIndex].ShortName);

            _mask = "(" + _countries[cmbCountries.SelectedIndex].PostalCode.Split('/')[1] + ")";
            if (!_countries[cmbCountries.SelectedIndex].OnePostalCode)
            {
                lblExample.Text = $"Example: {_countries[cmbCountries.SelectedIndex].PostalCodeExample}";
                txtZipcode.Enabled = true;
                btnFetch.Enabled = true;
            }
            else if (_countries[cmbCountries.SelectedIndex].PostalCodeExample == null)
            {
                txtZipcode.Enabled = btnFetch.Enabled = false;
            }
            else
            {
                txtZipcode.Text = _countries[cmbCountries.SelectedIndex].PostalCode.Replace("/", "").Replace("\\", "");
                btnFetch_Click(null, null);
            }
        }

        private void txtZipcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnFetch_Click(null, null);
            }
        }

        private void btnFetch_Click(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;
            Application.DoEvents();

            // evaluate the zipcode format
            var m = Regex.Match(txtZipcode.Text.ToUpper(), _mask);
            if ((m.Length == 0) && (!string.IsNullOrEmpty(_countries[cmbCountries.SelectedIndex].PostalCodeExample)))
            {
                MessageBox.Show("Postal Code is in the wrong format for selected country.\nPlease correct entry and try again.\n", "Invalid Entry", MessageBoxButtons.OK);
            }
            else if (_countries[cmbCountries.SelectedIndex].ShortName.Equals("EPG123"))
            {
                listBox1.Items.Clear();
                listBox1.Items.Add(txtZipcode.Text);

                _headends = new List<SubscribedLineup>
                {
                    new SubscribedLineup()
                    {
                        Transport = "unknown",
                        Name = txtZipcode.Text,
                        Location = "unknown",
                        Lineup = txtZipcode.Text
                    }
                };
            }
            else
            {
                listBox1.Items.Clear();
                _headends = new List<SubscribedLineup>();

                var heads = SdApi.GetHeadends(_countries[cmbCountries.SelectedIndex].ShortName, m.Value);
                if (heads == null)
                {
                    MessageBox.Show("No headends found for entered postal code and country.", "No Headend Found", MessageBoxButtons.OK);
                    UseWaitCursor = false;
                    Enabled = true;
                    return;
                }

                foreach (var head in heads)
                {
                    foreach (var lineup in head.Lineups)
                    {
                        _headends.Add(new SubscribedLineup()
                        {
                            Transport = head.Transport,
                            Name = lineup.Name,
                            Location = head.Location,
                            Lineup = lineup.Lineup
                        });
                    }
                }

                if (_headends.Count > 0)
                {
                    _headends = _headends.OrderBy(o => o.Name).ToList();
                    foreach (var lineup in _headends)
                    {
                        listBox1.Items.Add($"{lineup.Name} ({lineup.Location})");
                    }
                }
            }

            UseWaitCursor = false;
            Enabled = true;
        }

        private void GetTransmitters(string country)
        {
            var xmitters = SdApi.GetTransmitters(country);
            var sites = new List<string>(xmitters.Keys);
            foreach (var site in sites)
            {
                if (!xmitters.TryGetValue(site, out var lineup)) continue;
                _headends.Add(new SubscribedLineup()
                {
                    Transport = "DVB-T",
                    Name = site,
                    Location = null,
                    Lineup = lineup
                });
                listBox1.Items.Add(site);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            AddLineup = _headends[listBox1.SelectedIndex];
            Close();
        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            var index = listBox1.IndexFromPoint(e.Location);
            if ((index == -1) || (index >= listBox1.Items.Count)) return;
            if (toolTip1.GetToolTip(listBox1) != _headends[index].Lineup)
            {
                toolTip1.SetToolTip(listBox1, _headends[index].Lineup);
            }
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            var index = listBox1.IndexFromPoint(e.Location);
            if ((e.Button != MouseButtons.Right) || (index == -1) || (index >= listBox1.Items.Count)) return;
            listBox1.SelectedIndex = listBox1.IndexFromPoint(e.Location);
            var preview = new frmPreview(_headends[index].Lineup.Trim());
            preview.ShowDialog();
        }
    }
}