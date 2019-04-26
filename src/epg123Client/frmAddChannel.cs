using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.TV.Tuning;
using Microsoft.Win32;

namespace epg123
{
    public partial class frmAddChannel : Form
    {
        private string nonTunerText = "No Device Group";
        public bool channelAdded;

        public frmAddChannel()
        {
            InitializeComponent();
            populateScannedLineups();

            tabTunerSpace.Appearance = TabAppearance.FlatButtons;
            tabTunerSpace.ItemSize = new Size(0, 1);
            tabTunerSpace.SizeMode = TabSizeMode.Fixed;
        }

        private void populateScannedLineups()
        {
            HashSet<Lineup> scannedLineups = new HashSet<Lineup>();
            foreach (Device device in clientForm.mergedLineup.DeviceGroup.Devices)
            {
                if (device.ScannedLineup == null) continue;
                scannedLineups.Add(device.ScannedLineup);
            }

            //cmbScannedLineups.Items.Add(nonTunerText);
            if (scannedLineups.Count > 0)
            {
                cmbScannedLineups.Items.AddRange(scannedLineups.ToArray());
                cmbScannedLineups.SelectedIndex = 0;
            }
        }

        TuningSpace tuningSpace;
        string TuningSpaceName;
        private void cmbScannedLineups_SelectedIndexChanged(object sender, EventArgs e)
        {
            tuningSpace = null;
            lvDevices.Items.Clear();
            if (cmbScannedLineups.SelectedItem.ToString().Equals(nonTunerText))
            {
                tabTunerSpace.SelectTab(tabGhostTuner);
                chnNtiServiceType.Text = "TV";
                cmbScannedLineups.Focus();
                return;
            }

            foreach (Device device in clientForm.mergedLineup.DeviceGroup.Devices)
            {
                if (device.ScannedLineup == null) return;
                if (device.ScannedLineup.IsSameAs((Lineup)cmbScannedLineups.SelectedItem))
                {
                    lvDevices.Items.Add(new ListViewItem(device.Name)
                    {
                        Checked = true,
                        Tag = device
                    });

                    if (tuningSpace == null)
                    {
                        tuningSpace = device.DeviceType.TuningSpace;
                        switch (TuningSpaceName = device.DeviceType.TuningSpaceName)
                        {
                            case "ATSC":                                    // Local ATSC Digital Antenna
                                tabTunerSpace.SelectTab(tabChannelTuningInfo);
                                lblChnTiModulationType.Visible = chnTiModulationType.Visible = false;
                                chnTiNumber.Minimum = getTuningSpacesRegistryValue(TuningSpaceName, "MinChannel");
                                chnTiNumber.Maximum = getTuningSpacesRegistryValue(TuningSpaceName, "MaxChannel");

                                lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = true;
                                chnTiSubnumber.Minimum = getTuningSpacesRegistryValue(TuningSpaceName, "Min Minor Channel");
                                chnTiSubnumber.Maximum = getTuningSpacesRegistryValue(TuningSpaceName, "Max Minor Channel");

                                lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = true;
                                chnTiPhysicalNumber.Minimum = getTuningSpacesRegistryValue(TuningSpaceName, "Min Physical Channel");
                                chnTiPhysicalNumber.Maximum = getTuningSpacesRegistryValue(TuningSpaceName, "Max Physical Channel");

                                cmbScannedLineups.Focus();
                                break;
                            case "ClearQAM":                                // Local Digital Cable
                                tabTunerSpace.SelectTab(tabChannelTuningInfo);
                                lblChnTiModulationType.Visible = chnTiModulationType.Visible = true;
                                chnTiNumber.Minimum = getTuningSpacesRegistryValue(TuningSpaceName, "MinChannel");
                                chnTiNumber.Maximum = getTuningSpacesRegistryValue(TuningSpaceName, "MaxChannel");

                                lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = true;
                                chnTiSubnumber.Minimum = getTuningSpacesRegistryValue(TuningSpaceName, "Min Minor Channel");
                                chnTiSubnumber.Maximum = getTuningSpacesRegistryValue(TuningSpaceName, "Max Minor Channel");

                                lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = false;

                                cmbScannedLineups.Focus();
                                break;
                            case "Digital Cable":                           // Local Digital Cable
                                tabTunerSpace.SelectTab(tabChannelTuningInfo);
                                lblChnTiModulationType.Visible = chnTiModulationType.Visible = false;
                                chnTiNumber.Minimum = getTuningSpacesRegistryValue(TuningSpaceName, "MinChannel");
                                chnTiNumber.Maximum = getTuningSpacesRegistryValue(TuningSpaceName, "MaxChannel");

                                lblChnTiPhysicalNumber.Visible = chnTiPhysicalNumber.Visible = false;

                                lblChnTiSubnumber.Visible = chnTiSubnumber.Visible = false;
                                chnTiSubnumber.Value = 0;

                                cmbScannedLineups.Focus();
                                break;

                            // the following is Colossus Software Tuner
                            case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":    // Colossus Software Tuner
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
                            case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":  // Freestyle generic tuning space for STB
                            case "AuxIn1":                                  // Analog Auxiliary Input #1
                            case "Antenna":                                 // Local Analog Antenna
                            case "Cable":                                   // Local Analog Cable
                            case "ATSCCable":                               // Local ATSC Digital Cable

                            // the following are "DvbTuningInfo"
                            case "DVB-T":                                   // Local DVB-T Digital Antenna
                            case "DVB-S":                                   // Default Digital DVB-S Tuning Space
                            case "DVB-C":
                            case "ISDB-T":                                  // Local ISDB-T Digital Antenna
                            case "ISDB-S":                                  // Default Digital ISDB-S Tuning Space
                            case "ISDB-C":

                            default:
                                tabTunerSpace.SelectTab(tabUnsupported);
                                lblUnsupported.Text = string.Format("Tuner type \"{0}\" is currently unsupported. Please contact the author to aid in adding this tuner type.", device.DeviceType.TuningSpaceName);
                                cmbScannedLineups.Focus();
                                break;
                        }
                    }
                }
            }
        }

        private decimal getTuningSpacesRegistryValue(string tuningSpace, string value)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Multimedia\TV\Tuning Spaces\" + tuningSpace);
                if (key != null)
                {
                    return decimal.Parse(key.GetValue(value).ToString());
                }
            }
            catch { }
            return (decimal)0.0;
        }

        private void btnAddChannel_Click(object sender, EventArgs e)
        {
            if (sender.Equals(btnAddChannelTuningInfo))
            {
                // create a new service based on channel callsign
                Service service = new Service()
                {
                    CallSign = chnTiCallsign.Text,
                    Name = chnTiCallsign.Text
                };
                clientForm.mergedLineup.ObjectStore.Add(service);

                // create a new channel with service and channel number(s)
                Channel channel = new Channel()
                {
                    Service = service,
                    Number = (int)chnTiNumber.Value,
                    SubNumber = (int)chnTiSubnumber.Value,
                    OriginalNumber = (int)chnTiNumber.Value,
                    OriginalSubNumber = (int)chnTiSubnumber.Value,
                    ChannelType = ChannelType.UserAdded
                };
                (cmbScannedLineups.SelectedItem as Lineup).AddChannel(channel);
                channel.UniqueId = channel.Id;
                channel.Update();

                // create device tuner info for new channel
                bool success = false;
                foreach (ListViewItem item in lvDevices.Items)
                {
                    switch (TuningSpaceName)
                    {
                        case "ATSC":
                            success = addChannelTuningInfo(new ChannelTuningInfo(item.Tag as Device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, (int)chnTiPhysicalNumber.Value), channel);
                            break;
                        case "ClearQAM":
                            success = addChannelTuningInfo(new ChannelTuningInfo(item.Tag as Device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, (ModulationType)(chnTiModulationType.SelectedIndex + 1)), channel);
                            break;
                        case "Digital Cable":
                            success = addChannelTuningInfo(new ChannelTuningInfo(item.Tag as Device, (int)chnTiNumber.Value, (int)chnTiSubnumber.Value, ModulationType.BDA_MOD_NOT_DEFINED), channel);
                            break;
                        case "dc65aa02-5cb0-4d6d-a020-68702a5b34b8":
                            string tuningString = string.Format("<tune:ChannelID ChannelID=\"{0}\">" +
                                                                "  <tune:TuningSpace xsi:type=\"tune:ChannelIDTuningSpaceType\" Name=\"DC65AA02-5CB0-4d6d-A020-68702A5B34B8\" NetworkType=\"{{DC65AA02-5CB0-4d6d-A020-68702A5B34B8}}\" />" +
                                                                "  <tune:Locator xsi:type=\"tune:ATSCLocatorType\" Frequency=\"-1\" PhysicalChannel=\"-1\" TransportStreamID=\"-1\" ProgramNumber=\"1\" />" +
                                                                "  <tune:Components xsi:type=\"tune:ComponentsType\" />" +
                                                                "</tune:ChannelID>", channel.OriginalNumber);
                            success = addChannelTuningInfo(new StringTuningInfo(item.Tag as Device, tuningString), channel);
                            break;
                        case "{adb10da8-5286-4318-9ccb-cbedc854f0dc}":
                        case "AuxIn1":
                        case "Antenna":
                        case "Cable":
                        case "ATSCCable":
                        default:
                            break;
                    }
                }

                if (success)
                {
                    // at this point, mcepg.dll says we should call NotifyChannelAdded(channel)
                    // but for some reason this prevents epg123 from releasing all resources and
                    // stays in memory. The following tactic is similar to what glugglug uses in
                    // his channelediting code which I adapted to epg123
                    try
                    {
                        // create a merged channel and add to the merged lineup
                        MergedLineup mergedLineup = (cmbScannedLineups.SelectedItem as Lineup).PrimaryProvider ?? (cmbScannedLineups.SelectedItem as Lineup).SecondaryProvider;
                        if (mergedLineup != null)
                        {
                            MergedChannel mergedChannel = new MergedChannel()
                            {
                                Number = channel.Number,
                                SubNumber = channel.SubNumber,
                                OriginalNumber = channel.OriginalNumber,
                                OriginalSubNumber = channel.OriginalSubNumber,
                                ChannelType = ChannelType.UserAdded,
                                PrimaryChannel = channel,
                                Service = channel.Service,
                                Lineup = mergedLineup,
                            };
                            clientForm.mergedLineup.ObjectStore.Add(mergedChannel);
                            foreach (TuningInfo tuningInfo in channel.TuningInfos)
                            {
                                mergedChannel.TuningInfos.Add(tuningInfo);
                            }
                            mergedChannel.UniqueId = mergedChannel.Id;
                            mergedChannel.Update();
                            mergedLineup.AddChannel(mergedChannel);
                            mergedLineup.Update();
                            MessageBox.Show("Successfully added channel to scanned lineup devices.", "Channel Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to create and add a merged channel to the merged lineup." + ex.Message, "Channel Add", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to add channel to scanned lineup devices.", "Channel Add", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (sender.Equals(btnNtiAddChannel))
            {
                // create a new service based on channel callsign
                Service service = new Service()
                {
                    //Name = chnNtiCallsign.Text,
                    CallSign = chnNtiCallsign.Text,
                    ServiceType = chnNtiServiceType.SelectedIndex
                };
                clientForm.mergedLineup.ObjectStore.Add(service);

                // create a new channel with service and channel number(s)
                Channel channel = new Channel()
                {
                    Service = service,
                    Number = (int)chnNtiNumber.Value,
                    SubNumber = (int)chnNtiSubnumber.Value,
                    OriginalNumber = (int)chnNtiNumber.Value,
                    OriginalSubNumber = (int)chnNtiSubnumber.Value,
                    ChannelType = ChannelType.UserAdded
                };
                //clientForm.mergedLineup.AddChannel(channel);
                clientForm.mergedLineup.ObjectStore.Add(channel);
                channel.UniqueId = channel.Id;
                channel.Update();

                // create a merged channel and add to the merged lineup
                MergedChannel mergedChannel = new MergedChannel()
                {
                    //CallSign = chnNtiCallsign.Text,
                    Number = channel.Number,
                    SubNumber = channel.SubNumber,
                    OriginalNumber = channel.OriginalNumber,
                    OriginalSubNumber = channel.OriginalSubNumber,
                    ChannelType = ChannelType.Wmis,
                    Service = service,
                    PrimaryChannel = channel,
                    Lineup = clientForm.mergedLineup,
                    OneTouchNumber = (service.ServiceType == 0) ? 1 : -1,
                };
                clientForm.mergedLineup.ObjectStore.Add(mergedChannel);
                mergedChannel.UniqueId = mergedChannel.Id;
                mergedChannel.Update();

                clientForm.mergedLineup.AddChannel(mergedChannel);
                clientForm.mergedLineup.Update();

                MessageBox.Show("Successfully added channel to scanned lineup devices.", "Channel Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool addChannelTuningInfo(TuningInfo tuningInfo, Channel channel)
        {
            try
            {
                (cmbScannedLineups.SelectedItem as Lineup).ObjectStore.Add(tuningInfo);
                channel.TuningInfos.Add(tuningInfo);
                channelAdded = true;
                return true;
            }
            catch { return false; }
        }
    }
}