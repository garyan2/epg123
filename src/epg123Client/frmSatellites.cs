using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Satellites;
using Microsoft.MediaCenter.Store;
using Microsoft.MediaCenter.TV.Tuning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace epg123Client
{
    public partial class frmSatellites : Form
    {
        public frmSatellites()
        {
            InitializeComponent();

            // if LNBs are empty, that means there are no satellites currently configured
            // disable the default satellite mxf create button if no LNBs exist
            var lnbs = new LowNoiseBlocks(WmcStore.WmcObjectStore);
            btnCreateDefault.Enabled = cbRadio.Enabled = cbEncrypted.Enabled = cbEnabled.Enabled = cbData.Enabled = !lnbs.Empty;
        }

        private int GetMergedChannelServiceType(MergedChannel mergedChannel)
        {
            var channel = mergedChannel.ChannelType == ChannelType.Scanned || mergedChannel.ChannelType == ChannelType.CalculatedScanned
                ? mergedChannel as Channel
                : mergedChannel.SecondaryChannels.FirstOrDefault(arg => arg.ChannelType == ChannelType.Scanned || arg.ChannelType == ChannelType.CalculatedScanned);

            if (channel == null) return 0;
            switch (channel.Service.ServiceType)
            {
                case 0:
                    return channel.ChannelType == ChannelType.Scanned ? 0 : 3;
                default:
                    return channel.Service.ServiceType;
            }
        }

        private void btnCreateDefault_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            var mxf = new MXF(null, null, null, null, MXF.TYPEMXF.SATELLITES);
            foreach (MergedChannel mergedChannel in WmcStore.WmcMergedLineup.UncachedChannels.Cast<MergedChannel>())
            {
                if (mergedChannel.UserBlockedState > UserBlockedState.Enabled && cbEnabled.Checked) continue;

                var svcType = GetMergedChannelServiceType(mergedChannel);
                if (svcType == 2 && !cbRadio.Checked) continue;
                if (svcType == 3 && !cbData.Checked) continue;

                foreach (TuningInfo tuningInfo in mergedChannel.TuningInfos.Cast<TuningInfo>())
                {
                    // make sure it is DVBS
                    if (!(tuningInfo is DvbTuningInfo dvbTuningInfo) || !dvbTuningInfo.TuningSpace.Equals("DVB-S")) continue;
                    var locator = dvbTuningInfo.TuneRequest.Locator as DVBSLocator;

                    // filter on options
                    if ((dvbTuningInfo.IsEncrypted || dvbTuningInfo.IsSuggestedBlocked) && !cbEncrypted.Checked) continue;

                    // determine satellite, transponder, and service for channel
                    var satellite = GetOrCreateSatellite(locator.OrbitalPosition, mxf);
                    var transponder = satellite.GetOrCreateTransponder(dvbTuningInfo.Frequency / 1000,
                        (int)locator.SignalPolarisation - 1, locator.SymbolRate / 1000, dvbTuningInfo.Onid,
                        dvbTuningInfo.Tsid);
                    var service = transponder.GetOrCreateService(mergedChannel.CallSign, dvbTuningInfo.Sid,
                        mergedChannel.Service.ServiceType == 2 ? 1 : mergedChannel.Service.ServiceType == 3 ? 2 : 0,
                        dvbTuningInfo.IsEncrypted || dvbTuningInfo.IsSuggestedBlocked);

                    // add channel with callsign and channel number
                    var keyValues = new KeyValues(WmcStore.WmcObjectStore);
                    var region = mxf.GetOrCreateRegion(keyValues.Single(arg => arg.Key == "ClientCountryCode").Value);
                    var footprint = region.GetOrCreateFootprint(satellite);
                    var headend = footprint.GetOrCreateHeadend(satellite.PositionEast);
                    headend.AddChannel(service, int.Parse(mergedChannel.ChannelNumber.ToString()));
                    mxf.AddReferenceHeadend(headend);
                }
            }

            // create the temporary mxf file
            Helper.WriteXmlFile(mxf, Helper.DefaultSatellitesPath);
            Cursor = Cursors.Arrow;
        }

        private MxfDvbsSatellite GetOrCreateSatellite(int position, MXF mxf)
        {
            var satellite = mxf.DvbsDataSet._allSatellites.SingleOrDefault(arg => arg.PositionEast == position);
            if (satellite != null) return satellite;

            var dvbSatellites = new DvbsSatellites(WmcStore.WmcObjectStore);
            var dvbSatellite = dvbSatellites.Single(arg => arg.PositionEast == position);
            satellite = new MxfDvbsSatellite
            {
                PositionEast = position,
                Name = dvbSatellite.Name,
                _transponders = new List<MxfDvbsTransponder>()
            };
            mxf.DvbsDataSet._allSatellites.Add(satellite);
            return satellite;
        }

        private void btnTransponders_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            SatMxf.UpdateDvbsTransponders(true);
            Cursor = Cursors.Arrow;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            SatMxf.UpdateDvbsTransponders(false);
            Cursor = Cursors.Arrow;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start("http://satellites-xml.org");
        }
    }
}
