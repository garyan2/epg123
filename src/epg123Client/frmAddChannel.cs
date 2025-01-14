﻿using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.TV.Tuning;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmAddChannel : Form
    {
        private const string NonTunerText = "No Device Group";
        public bool ChannelAdded;

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
            foreach (Device device in new Devices(WmcStore.WmcObjectStore))
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

            foreach (Device device in new Devices(WmcStore.WmcObjectStore))
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

                        case "DVB-T":                                   // Local DVB-T Digital Antenna
                            tabTunerSpace.SelectTab(tabDvbTuningInfo);
                            cmbScannedLineups.Focus();
                            break;

                        // the following are "ChannelTuningInfo"
                        //case "AuxIn1":                                  // Analog Auxiliary Input #1
                        //case "Antenna":                                 // Local Analog Antenna
                        //case "ATSCCable":                               // Local ATSC Digital Cable

                        // the following are "DvbTuningInfo"
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
            // get the scannedlineup from the combo list
            var scannedLineup = (Lineup)cmbScannedLineups.SelectedItem;

            Service service; Channel channel; string msg;
            if (sender.Equals(btnAddChannelTuningInfo))
            {
                service = new Service
                {
                    CallSign = chnTiCallsign.Text,
                    Name = chnTiCallsign.Text,
                    ServiceType = ServiceType.TV
                };

                channel = new Channel
                {
                    Service = service,
                    Number = (int)chnTiNumber.Value,
                    SubNumber = (int)chnTiSubnumber.Value,
                    OriginalNumber = (int)chnTiNumber.Value,
                    OriginalSubNumber = (int)chnTiSubnumber.Value,
                    ChannelType = ChannelType.UserAdded
                };

                msg = $"{channel.ChannelNumber}{(chnTiPhysicalNumber.Value > 0 ? $" ({chnTiPhysicalNumber.Value})" : string.Empty)}{(!string.IsNullOrEmpty(channel.CallSign) ? $" {channel.CallSign}" : string.Empty)}";
            }
            else if (sender.Equals(btnAddDvbChannel))
            {
                if (!int.TryParse(dvbFreq.Text, out _) || !int.TryParse(dvbOnid.Text, out _) ||
                    !int.TryParse(dvbNid.Text, out _) || !int.TryParse(dvbTsid.Text, out _) ||
                    !int.TryParse(dvbSid.Text, out _) || !int.TryParse(dvbLcn.Text, out _))
                {
                    return;
                }

                service = new Service
                {
                    CallSign = dvbTiCallsign.Text,
                    Name = dvbTiCallsign.Text
                };

                channel = new Channel
                {
                    Service = service,
                    Number = int.Parse(dvbLcn.Text),
                    SubNumber = 0,
                    OriginalNumber = int.Parse(dvbLcn.Text),
                    OriginalSubNumber = 0,
                    ChannelType = ChannelType.UserAdded
                };

                msg = $"{channel.ChannelNumber}{(!string.IsNullOrEmpty(channel.CallSign) ? $" {channel.CallSign}" : string.Empty)} ({int.Parse(dvbFreq.Text) / 1000.0:n3} MHz, {int.Parse(dvbOnid.Text)}:{int.Parse(dvbTsid.Text)}:{int.Parse(dvbSid.Text)})";
            }
            else return;

            // create a device specific tuning info
            var tuningInfos = new List<TuningInfo>();
            foreach (ListViewItem item in lvDevices.Items)
            {
                var device = (Device)item.Tag;
                switch (device.DeviceType.TuningSpaceName)
                {
                    case "ATSC":
                        channel.MatchName = $"OC:{channel.Number}:{channel.SubNumber}{(!string.IsNullOrEmpty(service.CallSign) ? $"|{service.CallSign}" : "")}";
                        tuningInfos.Add(new ChannelTuningInfo(device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, (int)chnTiPhysicalNumber.Value));
                        break;
                    case "ClearQAM":
                        channel.MatchName = $"OC:{channel.Number}:{channel.SubNumber}{(!string.IsNullOrEmpty(service.CallSign) ? $"|{service.CallSign}" : "")}";
                        tuningInfos.Add(new ChannelTuningInfo(device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, (ModulationType)(chnTiModulationType.SelectedIndex + 1)));
                        break;
                    case "Cable":
                    case "Digital Cable":
                    case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                        channel.MatchName = !string.IsNullOrEmpty(service.CallSign) ? service.CallSign : "";
                        tuningInfos.Add(new ChannelTuningInfo(device, channel.Number, channel.SubNumber, ModulationType.BDA_MOD_NOT_DEFINED));
                        break;
                    case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":
                        channel.MatchName = !string.IsNullOrEmpty(service.CallSign) ? service.CallSign : "";
                        var tuningString = $"<tune:ChannelID ChannelID=\"{channel.OriginalNumber}\">" +
                                           "  <tune:TuningSpace xsi:type=\"tune:ChannelIDTuningSpaceType\" Name=\"DC65AA02-5CB0-4d6d-A020-68702A5B34B8\" NetworkType=\"{DC65AA02-5CB0-4d6d-A020-68702A5B34B8}\" />" +
                                           "  <tune:Locator xsi:type=\"tune:ATSCLocatorType\" Frequency=\"-1\" PhysicalChannel=\"-1\" TransportStreamID=\"-1\" ProgramNumber=\"1\" />" +
                                           "  <tune:Components xsi:type=\"tune:ComponentsType\" />" +
                                           "</tune:ChannelID>";
                        tuningInfos.Add(new StringTuningInfo(device, tuningString));
                        break;
                    case "DVB-T":
                        channel.MatchName = $"DVBT:{dvbOnid.Text}:{dvbTsid.Text}:{dvbSid}{(string.IsNullOrEmpty(service.CallSign) ? $"|{service.CallSign}" : "")}";
                        tuningInfos.Add(new DvbTuningInfo(device, int.Parse(dvbOnid.Text), int.Parse(dvbTsid.Text), int.Parse(dvbSid.Text), int.Parse(dvbNid.Text), int.Parse(dvbFreq.Text), channel.Number));
                        break;
                }
            }

            // add the channel
            WmcStore.AddUserChannel(scannedLineup, service, channel, tuningInfos);
            msg = $"Adding channel {msg} to {scannedLineup.Name}";
            rtbChannelAddHistory.Text += $"{msg}\n\n";
            Logger.WriteInformation(msg);
            ChannelAdded = true;
        }

        private void dvbInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !e.KeyChar.Equals('-')) e.Handled = true;
            if ((e.KeyChar == '-') && !string.IsNullOrEmpty((sender as TextBox).Text)) e.Handled = true;
        }

        private void dvbOnid_KeyUp(object sender, KeyEventArgs e)
        {
            dvbNid.Text = dvbOnid.Text;
        }
    }
}