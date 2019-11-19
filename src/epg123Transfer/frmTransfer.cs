using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.MediaCenter.Pvr;
using Microsoft.MediaCenter.Store;
using epg123Transfer.MxfXml;

namespace epg123Transfer
{
    public partial class frmTransfer : Form
    {
        private string oldWmcFile;
        MXF oldRecordings = new MXF()
        {
            SeriesRequest = new List<MxfRequest>(),
            ManualRequest = new List<MxfRequest>(),
            OneTimeRequest = new List<MxfRequest>(),
            WishListRequest = new List<MxfRequest>()
        };

        public frmTransfer(string file)
        {
            oldWmcFile = file;
            InitializeComponent();

            importBinFile();
            assignColumnSorters();
            buildListViews();

            if (!string.IsNullOrEmpty(file))
            {
                if (lvMxfRecordings.Items.Count > 0)
                {
                    btnTransfer_Click(btnAddRecordings, null);
                }
            }
        }

        private void buildListViews()
        {
            wmcRecording = new HashSet<string>();
            lvMxfRecordings.Items.Clear();
            lvWmcRecordings.Items.Clear();

            if (!string.IsNullOrEmpty(oldWmcFile))
            {
                if (oldWmcFile.ToLower().EndsWith(".zip"))
                {
                    using (Stream stream = epg123.CompressXmlFiles.GetBackupFileStream("recordings.mxf", oldWmcFile))
                    {
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(MXF));
                            oldRecordings = (MXF)serializer.Deserialize(stream);
                        }
                        catch
                        {
                            MessageBox.Show("This zip file does not contain a recordings.mxf file with recording requests.", "Invalid File", MessageBoxButtons.OK);
                            oldWmcFile = string.Empty;
                        }
                    }
                }
                else
                {
                    using (StreamReader stream = new StreamReader(oldWmcFile, Encoding.Default))
                    {
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(MXF));
                            TextReader reader = new StringReader(stream.ReadToEnd());
                            oldRecordings = (MXF)serializer.Deserialize(reader);
                            reader.Close();
                        }
                        catch
                        {
                            MessageBox.Show("Not a valid mxf file containing recording requests.", "Invalid File", MessageBoxButtons.OK);
                            oldWmcFile = string.Empty;
                        }
                    }
                }
                if (oldRecordings.ManualRequest.Count + oldRecordings.OneTimeRequest.Count + oldRecordings.SeriesRequest.Count + oldRecordings.WishListRequest.Count == 0)
                {
                    //MessageBox.Show("There are no scheduled recording requests in the backup file to restore.", "Empty Requests", MessageBoxButtons.OK);
                }
            }

            populateWmcRecordings();
            populateOldWmcRecordings();
        }

        #region ========= Binary Table File ==========
        Dictionary<string, string> idTable = new Dictionary<string, string>();
        private void importBinFile()
        {
            string url = @"http://epg123.garyan2.net/downloads/seriesXfer.bin";

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CopyStream(WebRequest.Create(url).GetResponse().GetResponseStream(), memoryStream);
                    memoryStream.Position = 0;

                    using (BinaryReader reader = new BinaryReader(memoryStream))
                    {
                        DateTime timestamp = DateTime.FromBinary(reader.ReadInt64());
                        lblDateTime.Text += timestamp.ToLocalTime().ToString();

                        while (memoryStream.Position < memoryStream.Length)
                        {
                            idTable.Add(string.Format("!MCSeries!{0}", reader.ReadInt32()), string.Format("!Series!{0:D8}", reader.ReadInt32()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error trying to download the series ID cross-reference tables from the EPG123 website.\n\n" + ex.Message,
                                "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblDateTime.Text += "ERROR";
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
        #endregion

        #region ========== Previous WMC Recordings ==========
        private void populateOldWmcRecordings()
        {
            List<ListViewItem> listViewItems = new List<ListViewItem>();
            foreach (MxfRequest request in oldRecordings.SeriesRequest)
            {
                // determine title & series and whether to display or not
                string title = string.IsNullOrEmpty(request.PrototypicalTitle) ? string.IsNullOrEmpty(request.Title) ? (request.SeriesElement == null) ? string.Empty : request.SeriesElement.Title : request.Title : request.PrototypicalTitle;
                string series = string.IsNullOrEmpty(request.SeriesAttribute) ? string.IsNullOrEmpty(request.SeriesElement.Uid) ? string.Empty : request.SeriesElement.Uid : request.SeriesAttribute;

                // if title is empty, then probably recreated a previously cancelled series... need to get title from that one
                if (string.IsNullOrEmpty(title) && !request.Complete)
                {
                    foreach (MxfRequest iRequest in oldRecordings.SeriesRequest)
                    {
                        string iSeries = string.IsNullOrEmpty(iRequest.SeriesAttribute) ? string.IsNullOrEmpty(iRequest.SeriesElement.Uid) ? string.Empty : iRequest.SeriesElement.Uid : iRequest.SeriesAttribute;
                        if (iSeries == series)
                        {
                            request.PrototypicalTitle = title = string.IsNullOrEmpty(iRequest.PrototypicalTitle) ? string.IsNullOrEmpty(iRequest.Title) ? (iRequest.SeriesElement == null) ? string.Empty : iRequest.SeriesElement.Title : iRequest.Title : iRequest.PrototypicalTitle;
                            if (!string.IsNullOrEmpty(title)) break;
                        }
                    }
                }

                if (request.Complete || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(series))
                {
                    continue;
                }

                string epgSeries = series;
                Color background = Color.LightPink;
                if (series.StartsWith("!Series!") || idTable.TryGetValue(series, out epgSeries))
                {
                    // if series already exists in wmc, don't display
                    if (wmcRecording.Contains(epgSeries)) continue;

                    // write over existing Uid with gracenote Uid
                    if (!string.IsNullOrEmpty(request.SeriesAttribute)) request.SeriesAttribute = epgSeries;
                    else request.SeriesElement.Uid = epgSeries;
                    background = Color.LightGreen;
                }

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "Series",
                        title
                    })
                {
                    BackColor = background,
                    Checked = (background == Color.LightGreen),
                    Tag = request
                });
            }

            foreach (MxfRequest request in oldRecordings.WishListRequest)
            {
                if (request.Complete || wmcRecording.Contains(request.Keywords)) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "WishList",
                        request.Keywords
                    })
                {
                    BackColor = Color.LightGreen,
                    Checked = true,
                    Tag = request
                });
            }

            foreach (MxfRequest request in oldRecordings.OneTimeRequest)
            {
                if (request.Complete || (request.PrototypicalStartTime < DateTime.UtcNow) ||
                    wmcRecording.Contains(request.PrototypicalTitle + " " + request.PrototypicalStartTime)) continue;

                // detect if onetime request is from EPG123
                bool epg123 = request.PrototypicalService.StartsWith("!Service!EPG123") &&
                             (request.PrototypicalProgram.StartsWith("!Program!SH") || request.PrototypicalProgram.StartsWith("!Program!EP") ||
                              request.PrototypicalProgram.StartsWith("!Program!MV") || request.PrototypicalProgram.StartsWith("!Program!SP"));

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "OneTime",
                        request.PrototypicalTitle ?? request.Title
                    })
                {
                    BackColor = epg123 ? Color.LightGreen : Color.LightSalmon,
                    Checked = epg123,
                    Tag = request
                });
            }

            foreach (MxfRequest request in oldRecordings.ManualRequest)
            {
                if (request.Complete || (request.PrototypicalStartTime < DateTime.Now) || 
                    wmcRecording.Contains(request.Title + " " + request.PrototypicalStartTime + " " + request.PrototypicalChannelNumber)) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "Manual",
                        request.Title ?? request.PrototypicalTitle
                    })
                {
                    BackColor = Color.LightGreen,
                    Checked = true,
                    Tag = request
                });
            }

            lvMxfRecordings.Items.AddRange(listViewItems.ToArray());
        }

        private void oldRecordingListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Ignore item checks when selecting multiple items
            if ((ModifierKeys & (Keys.Shift | Keys.Control)) > 0)
            {
                e.NewValue = e.CurrentValue;
            }
            else if (((ListView)sender).Items[e.Index].BackColor != Color.LightGreen)
            {
                e.NewValue = CheckState.Unchecked;
            }
        }

        private void btnOpenBackup_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = epg123.Helper.Epg123BackupFolder;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                oldWmcFile = openFileDialog1.FileName;
                buildListViews();
            }
        }
        #endregion

        #region ========== Current WMC Recordings ==========
        HashSet<string> wmcRecording = new HashSet<string>();
        private void populateWmcRecordings()
        {
            List<ListViewItem> listViewItems = new List<ListViewItem>();
            foreach (SeriesRequest request in new SeriesRequests(epg123.Store.objectStore))
            {
                // do not display archived/completed entries
                if (request.Complete) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "Series",
                        request.ToString().Substring(7).Replace(" - Complete: False", "")
                    })
                {
                    BackColor = !request.Series.GetUIdValue().Contains("!Series!") ? Color.Pink : Color.LightGreen,
                    Tag = request
                });

                // add the series Uid to the hashset
                wmcRecording.Add(request.Series.GetUIdValue());
            }

            foreach (ManualRequest request in new ManualRequests(epg123.Store.objectStore))
            {
                // do not display archived/completed entries
                if (request.Complete || (request.StartTime < DateTime.UtcNow)) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "Manual",
                        request.ToString().Substring(7)
                    })
                {
                    BackColor = Color.LightGreen,
                    Tag = request
                });

                // add the manaul recording title, starttime, and channel number
                wmcRecording.Add(request.Title + " " + request.StartTime + " " + request.Channel.ChannelNumber.Number + "." + request.Channel.ChannelNumber.SubNumber);
            }

            foreach (WishListRequest request in new WishListRequests(epg123.Store.objectStore))
            {
                // do not display archived/completed entries
                if (request.Complete) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "WishList",
                        request.ToString().Substring(9)
                    })
                {
                    BackColor = Color.LightGreen,
                    Tag = request
                });

                // add keywords to hashset
                wmcRecording.Add(request.Keywords);
            }

            foreach (OneTimeRequest request in new OneTimeRequests(epg123.Store.objectStore))
            {
                // do not display archived/completed entries
                if (request.Complete || (request.StartTime < DateTime.UtcNow)) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new string[]
                    {
                        "OneTime",
                        request.Title
                    })
                {
                    BackColor = Color.LightGreen,
                    Tag = request
                });

                // add the manaul recording title, starttime, and channel number
                wmcRecording.Add(request.Title + " " + request.StartTime);
            }

            if (listViewItems.Count > 0)
            {
                lvWmcRecordings.Items.AddRange(listViewItems.ToArray());
            }
        }

        private void toolStripMenuItemCancel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvWmcRecordings.SelectedItems)
            {
                Request request = (Request)item.Tag;
                request.Cancel();
                request.Update();
            }
            buildListViews();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (lvWmcRecordings.SelectedItems.Count <= 0) e.Cancel = true;
        }
        #endregion

        #region ========== Column Sorting ==========
        private void assignColumnSorters()
        {
            ListView[] listviews = { lvMxfRecordings, lvWmcRecordings };
            foreach (ListView listview in listviews)
            {
                // create and assign listview item sorter
                listview.ListViewItemSorter = new ListViewColumnSorter()
                {
                    SortColumn = 1,
                    Order = SortOrder.Ascending
                };
                listview.Sort();
            }
        }

        private void lvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine which column sorter this click applies to
            ListViewColumnSorter lvcs = (ListViewColumnSorter)((ListView)sender).ListViewItemSorter;

            // Determine if clicked column is already the column that is being sorted
            if (e.Column == lvcs.SortColumn)
            {
                // Reverse the current sort direction for this column
                lvcs.Order = (lvcs.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvcs.SortColumn = e.Column;
                lvcs.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            ((ListView)sender).Sort();
        }
        #endregion

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            if (lvMxfRecordings.Items.Count <= 0) return;

            // clear the recordings
            oldRecordings.ManualRequest = new List<MxfRequest>();
            oldRecordings.OneTimeRequest = new List<MxfRequest>();
            oldRecordings.SeriesRequest = new List<MxfRequest>();
            oldRecordings.WishListRequest = new List<MxfRequest>();

            // set version of mxf file to Win7 in order to import into any WMC
            oldRecordings.Assembly[0].Version = "6.1.0.0";
            oldRecordings.Assembly[1].Version = "6.1.0.0";

            // populate the good stuff from the listview
            int checkedItems = 0;
            foreach (ListViewItem item in lvMxfRecordings.Items)
            {
                if (!item.Checked) continue;
                else ++checkedItems;

                switch (item.Text)
                {
                    case "Series":
                        oldRecordings.SeriesRequest.Add((MxfRequest)item.Tag);
                        break;
                    case "WishList":
                        oldRecordings.WishListRequest.Add((MxfRequest)item.Tag);
                        break;
                    case "OneTime":
                        oldRecordings.OneTimeRequest.Add((MxfRequest)item.Tag);
                        break;
                    case "Manual":
                        oldRecordings.ManualRequest.Add((MxfRequest)item.Tag);
                        break;
                    default:
                        break;
                }
            }
            if (checkedItems == 0) return;

            try
            {
                string mxfFilepath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mxf");
                using (StreamWriter stream = new StreamWriter(mxfFilepath, false))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MXF));
                    TextWriter writer = stream;
                    serializer.Serialize(writer, oldRecordings);
                }

                // import recordings
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\loadmxf.exe"),
                    Arguments = string.Format("-i \"{0}\"", mxfFilepath),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (Process process = Process.Start(startInfo))
                {
                    process.StandardOutput.ReadToEnd();
                    process.WaitForExit(30000);
                }

                // kick off the pvr schedule task
                startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%WINDIR%\ehome\mcupdate.exe"),
                    Arguments = "-PvrSchedule",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process proc = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import the old recording requests.\n\n" + ex.Message, "Failed to Import", MessageBoxButtons.OK);
            }

            buildListViews();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            if (lvMxfRecordings.SelectedItems.Count != 1) e.Cancel = true;

            ListViewItem lvi = lvMxfRecordings.SelectedItems[0];
            if (lvi.BackColor == Color.LightPink)
            {
                matchVerifyToolStripMenuItem.Text = "Match";
            }
            else if (lvi.Text.Equals("Series"))
            {
                matchVerifyToolStripMenuItem.Text = "Verify";
            }
            else e.Cancel = true;
        }

        private void matchVerifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MxfRequest request = (MxfRequest)lvMxfRecordings.SelectedItems[0].Tag;
            frmManualMatch frm = new frmManualMatch(request);
            if (((ToolStripMenuItem)sender).Text.Equals("Match"))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    int topIndexItem = lvMxfRecordings.TopItem.Index;
                    idTable.Add(frm.idWas, frm.idIs);
                    buildListViews();
                    lvMxfRecordings.TopItem = lvMxfRecordings.Items[topIndexItem];
                }
            }
            else
            {
                frm.idIs = request.SeriesAttribute ?? request.SeriesElement.Uid;
                frm.ShowDialog();
            }
        }
    }
}
