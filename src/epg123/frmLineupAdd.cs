using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace epg123
{
    public partial class frmLineupAdd : Form
    {
        public SdLineup addLineup;

        Dictionary<string, IList<sdCountry>> countryResp;
        List<sdCountry> countries = new List<sdCountry>();

        string mask;
        List<SdLineup> headends = new List<SdLineup>();

        public frmLineupAdd()
        {
            InitializeComponent();
            string isoCountry = System.Globalization.RegionInfo.CurrentRegion.ThreeLetterISORegionName;

            countryResp = sdAPI.getCountryAvailables();
            if (countryResp != null)
            {
                // add the only freeview listing
                countries.Add(new sdCountry()
                {
                    FullName = "Great Britain Freeview",
                    PostalCode = "/",
                    PostalCodeExample = null,
                    ShortName = "GBR"
                });

                // get all the region/countries
                List<string> regions = new List<string>(countryResp.Keys);
                foreach (string region in regions)
                {
                    if (region.ToLower().Equals("zzz")) continue;
                    IList<sdCountry> regionCountries = countryResp[region];
                    foreach (sdCountry country in regionCountries)
                    {
                        countries.Add(country);
                    }
                }

                // sort the countries
                countries = countries.OrderBy(o => o.FullName).ToList();
                foreach (sdCountry country in countries)
                {
                    cmbCountries.Items.Add(country.FullName);
                }

                // add the DVB satellites
                foreach (string region in regions)
                {
                    if (!region.ToLower().Equals("zzz")) continue;
                    IList<sdCountry> regionCountries = countryResp[region];

                    countries.Add(null); cmbCountries.Items.Add(string.Empty);
                    foreach (sdCountry country in regionCountries)
                    {
                        countries.Add(country);
                        cmbCountries.Items.Add(country.FullName);
                    }
                }

                // add a manual option
                countries.Add(null);
                countries.Add(new sdCountry()
                {
                    FullName = "Manual lineup input...",
                    OnePostalCode = false,
                    PostalCode = "/[A-Z]+-[A-Z0-9]+-[A-Z0-9]+",
                    PostalCodeExample = "USA-CA00053-DEFAULT",
                    ShortName = "EPG123"
                });
                cmbCountries.Items.Add(string.Empty); cmbCountries.Items.Add("Manual lineup input...");
            }

            int index = 0;
            for (int i = 0; i < countries.Count; ++i)
            {
                if (countries[i] == null) continue;
                if (!string.IsNullOrEmpty(countries[i].ShortName) && countries[i].ShortName.ToUpper().Equals(isoCountry))
                {
                    index = i;
                    break;
                }
            }
            cmbCountries.SelectedIndex = index;

            // automatically fetch the zipcode as entered during TV Setup and perform a fetch
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Media Center\Settings\ProgramGuide", false))
                {
                    if ((key != null) && (!string.IsNullOrEmpty(key.GetValue("strLocation").ToString())))
                    {
                        txtZipcode.Text = key.GetValue("strLocation").ToString().Split(' ')[0];
                        btnFetch_Click(btnFetch, null);
                    }
                }
            }
            catch { }
        }

        private void cmbCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            headends.Clear();
            btnFetch.Enabled = false;
            txtZipcode.Enabled = false;
            lblExample.Text = string.Empty;
            txtZipcode.Text = string.Empty;

            if (string.IsNullOrEmpty(cmbCountries.Text))
            {
                return;
            }
            else if (countries[cmbCountries.SelectedIndex].ShortName.Equals("ZZZ"))
            {
                getSatellites();
            }
            else if (countries[cmbCountries.SelectedIndex].PostalCodeExample == null)
            {
                getTransmitters(countries[cmbCountries.SelectedIndex].ShortName);
            }

            if (!string.IsNullOrEmpty(countries[cmbCountries.SelectedIndex].PostalCodeExample))
            {
                mask = "(" + countries[cmbCountries.SelectedIndex].PostalCode.Split('/')[1] + ")";
                if (!countries[cmbCountries.SelectedIndex].OnePostalCode)
                {
                    lblExample.Text = "Example: " + countries[cmbCountries.SelectedIndex].PostalCodeExample;
                    txtZipcode.Enabled = true;
                    btnFetch.Enabled = true;
                }
                else
                {
                    txtZipcode.Text = countries[cmbCountries.SelectedIndex].PostalCode.Replace("/", "").Replace("\\", "");
                    btnFetch_Click(null, null);
                }
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
            this.Enabled = false;
            this.UseWaitCursor = true;
            Application.DoEvents();

            // evaluate the zipcode format
            Match m = Regex.Match(txtZipcode.Text.ToUpper(), mask);
            if ((m.Length == 0) && (!string.IsNullOrEmpty(countries[cmbCountries.SelectedIndex].PostalCodeExample)))
            {
                MessageBox.Show("Postal Code is in the wrong format for selected country.\nPlease correct entry and try again.\n", "Invalid Entry", MessageBoxButtons.OK);
            }
            else if (countries[cmbCountries.SelectedIndex].ShortName.Equals("EPG123"))
            {
                listBox1.Items.Clear();
                listBox1.Items.Add(txtZipcode.Text);

                headends = new List<SdLineup>();
                headends.Add(new SdLineup()
                {
                    Transport = "unknown",
                    Name = txtZipcode.Text,
                    Location = "unknown",
                    Lineup = txtZipcode.Text
                });
            }
            else
            {
                listBox1.Items.Clear();
                headends = new List<SdLineup>();

                IList<sdHeadendResponse> heads = sdAPI.getHeadends(countries[cmbCountries.SelectedIndex].ShortName, m.Value);
                if (heads == null)
                {
                    MessageBox.Show("No headends found for entered postal code and country.", "No Headend Found", MessageBoxButtons.OK);
                    return;
                }

                foreach (sdHeadendResponse head in heads)
                {
                    foreach (sdHeadendLineup lineup in head.Lineups)
                    {
                        headends.Add(new SdLineup()
                        {
                            Transport = head.Transport,
                            Name = lineup.Name,
                            Location = head.Location,
                            Lineup = lineup.Lineup
                        });
                        //listBox1.Items.Add(string.Format("{0} ({1})", lineup.Name, head.Location));
                    }

                }

                if (headends.Count > 0)
                {
                    headends = headends.OrderBy(o => o.Lineup).OrderBy(o => o.Name).ToList();
                    foreach (SdLineup lineup in headends)
                    {
                        listBox1.Items.Add(string.Format("{0} ({1})", lineup.Name, lineup.Location));
                    }
                }
            }

            this.UseWaitCursor = false;
            this.Enabled = true;
        }
        private void getSatellites()
        {
            dynamic satcom = sdAPI.getSatelliteAvailables();
            for (int i = 0; i < satcom.Count; ++i)
            {
                headends.Add(new SdLineup()
                {
                    Transport = "DVB-S",
                    Name = satcom[i].lineup,
                    Location = null,
                    Lineup = satcom[i].lineup
                });
                listBox1.Items.Add(satcom[i].lineup);
            }
        }
        private void getTransmitters(string country)
        {
            Dictionary<string, string> xmitters = sdAPI.getTransmitters(country);
            List<string> sites = new List<string>(xmitters.Keys);
            foreach (string site in sites)
            {
                string lineup = string.Empty;
                if (xmitters.TryGetValue(site, out lineup))
                {
                    headends.Add(new SdLineup()
                    {
                        Transport = "DVB-T",
                        Name = site,
                        Location = null,
                        Lineup = lineup
                    });
                    listBox1.Items.Add(site);
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            addLineup = headends[listBox1.SelectedIndex];
            this.Close();
        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);
            if ((index != -1) && (index < listBox1.Items.Count))
            {
                if (toolTip1.GetToolTip(listBox1) != headends[index].Lineup)
                {
                    toolTip1.SetToolTip(listBox1, headends[index].Lineup);
                }
            }
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);
            if ((e.Button == MouseButtons.Right) && (index != -1) && (index < listBox1.Items.Count))
            {
                listBox1.SelectedIndex = listBox1.IndexFromPoint(e.Location);
                frmPreview preview = new frmPreview(headends[index].Lineup.Trim());
                preview.ShowDialog();
            }
        }
    }
}