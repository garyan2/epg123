using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using epg123;
using epg123Client.SatMxf;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Satellites;

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
            var mxf = new Mxf();
            foreach (MergedChannel mergedChannel in WmcStore.WmcMergedLineup.UncachedChannels)
            {
                if (mergedChannel.UserBlockedState > UserBlockedState.Enabled && cbEnabled.Checked) continue;

                var svcType = GetMergedChannelServiceType(mergedChannel);
                if (svcType == 2 && !cbRadio.Checked) continue;
                if (svcType == 3 && !cbData.Checked) continue;
                mxf.AddChannel(mergedChannel, cbEncrypted.Checked);
            }

            // create the temporary mxf file
            using (var stream = new StreamWriter(Helper.DefaultSatellitesPath, false, Encoding.UTF8))
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
            {
                var serializer = new XmlSerializer(typeof(Mxf));
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                serializer.Serialize(writer, mxf, ns);
            }
            Cursor = Cursors.Arrow;
        }

        private void btnTransponders_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            WmcUtilities.UpdateDvbsTransponders(true);
            Cursor = Cursors.Arrow;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            WmcUtilities.UpdateDvbsTransponders(false);
            Cursor = Cursors.Arrow;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start("http://satellites-xml.org");
        }
    }
}
