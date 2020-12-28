using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using epg123;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.TV.Tuning;
using Microsoft.Win32;

namespace epg123Client
{
    public partial class frmAddChannel : Form
    {
        private const string NonTunerText = "No Device Group";
        public bool ChannelAdded;
        private readonly List<Channel> _channelsToAdd = new List<Channel>();

        public frmAddChannel()
        {
            InitializeComponent();
            PopulateScannedLineups();

            tabTunerSpace.Appearance = TabAppearance.FlatButtons;
            tabTunerSpace.ItemSize = new Size(0, 1);
            tabTunerSpace.SizeMode = TabSizeMode.Fixed;
        }

        private void PopulateScannedLineups()
        {
            var scannedLineups = new HashSet<Lineup>();
            foreach (Device device in WmcStore.WmcMergedLineup.DeviceGroup.Devices)
            {
                if (device.ScannedLineup == null) continue;
                scannedLineups.Add(device.ScannedLineup);
            }

            //cmbScannedLineups.Items.Add(nonTunerText);
            if (scannedLineups.Count <= 0) return;
            cmbScannedLineups.Items.AddRange(scannedLineups.ToArray());
            cmbScannedLineups.SelectedIndex = 0;
        }

        private TuningSpace _tuningSpace;
        private string _tuningSpaceName;

        private void cmbScannedLineups_SelectedIndexChanged(object sender, EventArgs e)
        {
            _tuningSpace = null;
            lvDevices.Items.Clear();
            if (cmbScannedLineups.SelectedItem.ToString().Equals(NonTunerText))
            {
                tabTunerSpace.SelectTab(tabGhostTuner);
                chnNtiServiceType.Text = "TV";
                cmbScannedLineups.Focus();
                return;
            }

            foreach (Device device in WmcStore.WmcMergedLineup.DeviceGroup.Devices)
            {
                if (device.ScannedLineup == null) return;
                if (!device.ScannedLineup.IsSameAs((Lineup)cmbScannedLineups.SelectedItem)) continue;
                lvDevices.Items.Add(new ListViewItem(device.Name)
                {
                    Checked = true,
                    Tag = device
                });

                if (_tuningSpace == null)
                {
                    _tuningSpace = device.DeviceType.TuningSpace;
                    switch (_tuningSpaceName = device.DeviceType.TuningSpaceName)
                    {
                        case "ATSC": // Local ATSC Digital Antenna
                            tabTunerSpace.SelectTab(tabChannelTuningInfo);
                            lblChnTiModulationType.Visible = chnTiModulationType.Visible = false;
                            chnTiNumber.Minimum = getTuningSpacesRegistryValue(_tuningSpaceName, "MinChannel");
                            chnTiNumber.Maximum = getTuningSpacesRegistryValue(_tuningSpaceName, "MaxChannel");

                            lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = true;
                            chnTiSubnumber.Minimum = getTuningSpacesRegistryValue(_tuningSpaceName, "Min Minor Channel");
                            chnTiSubnumber.Maximum = getTuningSpacesRegistryValue(_tuningSpaceName, "Max Minor Channel");

                            lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = true;
                            chnTiPhysicalNumber.Minimum = getTuningSpacesRegistryValue(_tuningSpaceName, "Min Physical Channel");
                            chnTiPhysicalNumber.Maximum = getTuningSpacesRegistryValue(_tuningSpaceName, "Max Physical Channel");

                            cmbScannedLineups.Focus();
                            break;
                        case "ClearQAM": // Local Digital Cable
                            tabTunerSpace.SelectTab(tabChannelTuningInfo);
                            lblChnTiModulationType.Visible = chnTiModulationType.Visible = true;
                            chnTiNumber.Minimum = getTuningSpacesRegistryValue(_tuningSpaceName, "MinChannel");
                            chnTiNumber.Maximum = getTuningSpacesRegistryValue(_tuningSpaceName, "MaxChannel");

                            lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = true;
                            chnTiSubnumber.Minimum = getTuningSpacesRegistryValue(_tuningSpaceName, "Min Minor Channel");
                            chnTiSubnumber.Maximum = 99999; // getTuningSpacesRegistryValue(TuningSpaceName, "Max Minor Channel");

                            lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = false;

                            cmbScannedLineups.Focus();
                            break;
                        case "Cable": // Local Analog Cable
                        case "Digital Cable": // Local Digital Cable
                        case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}": // Freestyle generic tuning space for STB
                            tabTunerSpace.SelectTab(tabChannelTuningInfo);
                            lblChnTiModulationType.Visible = chnTiModulationType.Visible = false;
                            chnTiNumber.Minimum = getTuningSpacesRegistryValue(_tuningSpaceName, "MinChannel");
                            chnTiNumber.Maximum = getTuningSpacesRegistryValue(_tuningSpaceName, "MaxChannel");

                            lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = false;

                            lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = false;
                            chnTiSubnumber.Value = 0;

                            cmbScannedLineups.Focus();
                            break;

                        // the following is Colossus Software Tuner
                        case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8": // Colossus Software Tuner
                            tabTunerSpace.SelectTab(tabChannelTuningInfo);
                            lblChnTiModulationType.Visible = chnTiModulationType.Visible = false;
                            chnTiNumber.Minimum = 1;
                            chnTiNumber.Maximum = 9999;

                            lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = false;

                            lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = false;
                            chnTiSubnumber.Value = 0;

                            cmbScannedLineups.Focus();
                            break;

                        // the following are "ChannelTuningInfo"
                        //case "AuxIn1":                                  // Analog Auxiliary Input #1
                        //case "Antenna":                                 // Local Analog Antenna
                        //case "ATSCCable":                               // Local ATSC Digital Cable

                        // the following are "DvbTuningInfo"
                        //case "DVB-T":                                   // Local DVB-T Digital Antenna
                        //case "DVB-S":                                   // Default Digital DVB-S Tuning Space
                        //case "DVB-C":
                        //case "ISDB-T":                                  // Local ISDB-T Digital Antenna
                        //case "ISDB-S":                                  // Default Digital ISDB-S Tuning Space
                        //case "ISDB-C":

                        default:
                            tabTunerSpace.SelectTab(tabUnsupported);
                            lblUnsupported.Text = $"Tuner type \"{device.DeviceType.TuningSpaceName}\" is currently unsupported. Please contact the author to aid in adding this tuner type.";
                            cmbScannedLineups.Focus();
                            break;
                    }
                }
            }
        }

        private decimal getTuningSpacesRegistryValue(string tuningSpace, string value)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Multimedia\TV\Tuning Spaces\" + tuningSpace);
                if (key != null)
                {
                    return decimal.Parse(key.GetValue(value).ToString());
                }
            }
            catch
            {
                // ignored
            }
            return (decimal)0.0;
        }

        private void btnAddChannel_Click(object sender, EventArgs e)
        {
            if (!sender.Equals(btnAddChannelTuningInfo)) return;
            
            // get the scannedlineup from the combo list
            var scannedLineup = (Lineup)cmbScannedLineups.SelectedItem;

            // create a new service based on channel callsign
            var service = new Service
            {
                CallSign = chnTiCallsign.Text,
                Name = chnTiCallsign.Text
            };

            // create a new channel with service and channel number(s)
            var channel = new Channel()
            {
                Service = service,
                Number = (int)chnTiNumber.Value,
                SubNumber = (int)chnTiSubnumber.Value,
                OriginalNumber = (int)chnTiNumber.Value,
                OriginalSubNumber = (int)chnTiSubnumber.Value,
                ChannelType = ChannelType.UserAdded
            };

            // create a device specific tuning info
            var tuningInfos = new List<TuningInfo>();
            foreach (ListViewItem item in lvDevices.Items)
            {
                var device = (Device)item.Tag;
                switch (device.DeviceType.TuningSpaceName)
                {
                    case "ATSC":
                        tuningInfos.Add(new ChannelTuningInfo(device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, (int)chnTiPhysicalNumber.Value));
                        break;
                    case "ClearQAM":
                        tuningInfos.Add(new ChannelTuningInfo(device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, (ModulationType)(chnTiModulationType.SelectedIndex + 1)));
                        break;
                    case "Cable":
                    case "Digital Cable":
                    case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                        tuningInfos.Add(new ChannelTuningInfo(device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, ModulationType.BDA_MOD_NOT_DEFINED));
                        break;
                    case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":
                        var tuningString = $"<tune:ChannelID ChannelID=\"{channel.OriginalNumber}\">" +
                                           "  <tune:TuningSpace xsi:type=\"tune:ChannelIDTuningSpaceType\" Name=\"DC65AA02-5CB0-4d6d-A020-68702A5B34B8\" NetworkType=\"{DC65AA02-5CB0-4d6d-A020-68702A5B34B8}\" />" +
                                           "  <tune:Locator xsi:type=\"tune:ATSCLocatorType\" Frequency=\"-1\" PhysicalChannel=\"-1\" TransportStreamID=\"-1\" ProgramNumber=\"1\" />" +
                                           "  <tune:Components xsi:type=\"tune:ComponentsType\" />" +
                                           "</tune:ChannelID>";
                        tuningInfos.Add(new StringTuningInfo(device, tuningString));
                        break;
                }
            }

            // add the channel
            var msg = $"Adding channel {channel.ChannelNumber}{(chnTiPhysicalNumber.Value > 0 ? $" ({chnTiPhysicalNumber.Value})" : string.Empty)} {channel.CallSign} to {scannedLineup.Name}";
            WmcStore.AddUserChannel(scannedLineup, service, channel, tuningInfos);
            rtbChannelAddHistory.Text += msg + "\n\n";
            Logger.WriteInformation(msg);
            ChannelAdded = true;
        }
    }
}