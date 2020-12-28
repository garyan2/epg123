using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.MediaCenter.Pvr;

namespace epg123Transfer
{
    public partial class frmTransfer : Form
    {
        private string _oldWmcFile;
        mxf _oldRecordings = new mxf()
        {
            SeriesRequest = new List<MxfRequest>(),
            ManualRequest = new List<MxfRequest>(),
            OneTimeRequest = new List<MxfRequest>(),
            WishListRequest = new List<MxfRequest>()
        };

        public frmTransfer(string file)
        {
            _oldWmcFile = file;
            InitializeComponent();

            ImportBinFile();
            AssignColumnSorters();
            BuildListViews();

            if (string.IsNullOrEmpty(file)) return;
            if (lvMxfRecordings.Items.Count > 0)
            {
                btnTransfer_Click(btnAddRecordings, null);
            }
        }

        private void BuildListViews()
        {
            _wmcRecording = new HashSet<string>();
            lvMxfRecordings.Items.Clear();
            lvWmcRecordings.Items.Clear();

            if (!string.IsNullOrEmpty(_oldWmcFile))
            {
                if (_oldWmcFile.ToLower().EndsWith(".zip"))
                {
                    using (var stream = epg123.CompressXmlFiles.GetBackupFileStream("recordings.mxf", _oldWmcFile))
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(mxf));
                            _oldRecordings = (mxf)serializer.Deserialize(stream);
                        }
                        catch
                        {
                            MessageBox.Show("This zip file does not contain a recordings.mxf file with recording requests.", "Invalid File", MessageBoxButtons.OK);
                            _oldWmcFile = string.Empty;
                        }
                    }
                }
                else
                {
                    using (var stream = new StreamReader(_oldWmcFile, Encoding.Default))
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(mxf));
                            TextReader reader = new StringReader(stream.ReadToEnd());
                            _oldRecordings = (mxf)serializer.Deserialize(reader);
                            reader.Close();
                        }
                        catch
                        {
                            MessageBox.Show("Not a valid mxf file containing recording requests.", "Invalid File", MessageBoxButtons.OK);
                            _oldWmcFile = string.Empty;
                        }
                    }
                }
                if (_oldRecordings.ManualRequest.Count + _oldRecordings.OneTimeRequest.Count + _oldRecordings.SeriesRequest.Count + _oldRecordings.WishListRequest.Count == 0)
                {
                    //MessageBox.Show("There are no scheduled recording requests in the backup file to restore.", "Empty Requests", MessageBoxButtons.OK);
                }
            }

            PopulateWmcRecordings();
            PopulateOldWmcRecordings();
        }

        #region ========= Binary Table File ==========
        readonly Dictionary<string, string> _idTable = new Dictionary<string, string>();
        private void ImportBinFile()
        {
            const string url = @"http://epg123.garyan2.net/downloads/seriesXfer.bin";

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(WebRequest.Create(url).GetResponse().GetResponseStream(), memoryStream);
                    memoryStream.Position = 0;

                    using (var reader = new BinaryReader(memoryStream))
                    {
                        var timestamp = DateTime.FromBinary(reader.ReadInt64());
                        lblDateTime.Text += timestamp.ToLocalTime().ToString();

                        while (memoryStream.Position < memoryStream.Length)
                        {
                            _idTable.Add($"!MCSeries!{reader.ReadInt32()}", $"!Series!{reader.ReadInt32():D8}");
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
            var buffer = new byte[16 * 1024];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
        #endregion

        #region ========== Previous WMC Recordings ==========
        private void PopulateOldWmcRecordings()
        {
            var listViewItems = new List<ListViewItem>();
            foreach (var request in _oldRecordings.SeriesRequest)
            {
                // determine title & series and whether to display or not
                var title = string.IsNullOrEmpty(request.PrototypicalTitle) ? string.IsNullOrEmpty(request.Title) ? (request.SeriesElement == null) ? string.Empty : request.SeriesElement.Title : request.Title : request.PrototypicalTitle;
                var series = string.IsNullOrEmpty(request.SeriesAttribute) ? string.IsNullOrEmpty(request.SeriesElement.Uid) ? string.Empty : request.SeriesElement.Uid : request.SeriesAttribute;

                // if title is empty, then probably recreated a previously cancelled series... need to get title from that one
                if (string.IsNullOrEmpty(title) && !request.Complete)
                {
                    foreach (var iRequest in from iRequest in _oldRecordings.SeriesRequest let iSeries = string.IsNullOrEmpty(iRequest.SeriesAttribute) ? string.IsNullOrEmpty(iRequest.SeriesElement.Uid) ? string.Empty : iRequest.SeriesElement.Uid : iRequest.SeriesAttribute where iSeries == series select iRequest)
                    {
                        request.PrototypicalTitle = title = string.IsNullOrEmpty(iRequest.PrototypicalTitle) ? string.IsNullOrEmpty(iRequest.Title) ? (iRequest.SeriesElement == null) ? string.Empty : iRequest.SeriesElement.Title : iRequest.Title : iRequest.PrototypicalTitle;
                        if (!string.IsNullOrEmpty(title)) break;
                    }
                }

                if (request.Complete || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(series))
                {
                    continue;
                }

                var epgSeries = series;
                var background = Color.LightPink;
                if (series.StartsWith("!Series!") || _idTable.TryGetValue(series, out epgSeries))
                {
                    // if series already exists in wmc, don't display
                    if (_wmcRecording.Contains(epgSeries)) continue;

                    // write over existing Uid with gracenote Uid
                    if (!string.IsNullOrEmpty(request.SeriesAttribute)) request.SeriesAttribute = epgSeries;
                    else request.SeriesElement.Uid = epgSeries;
                    background = Color.LightGreen;

                    if (!series.StartsWith("!Series!")) request.AnyChannel = "true";
                }

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
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

            // add wishlist requests
            listViewItems.AddRange(from request in _oldRecordings.WishListRequest where !request.Complete && !_wmcRecording.Contains(request.Keywords) select new ListViewItem(new[] {"WishList", request.Keywords}) {BackColor = Color.LightGreen, Checked = true, Tag = request});

            // add onetime requests
            listViewItems.AddRange(from request in _oldRecordings.OneTimeRequest where !request.Complete && (request.PrototypicalStartTime >= DateTime.UtcNow) && !_wmcRecording.Contains(request.PrototypicalTitle + " " + request.PrototypicalStartTime) let epg123 = request.PrototypicalService.StartsWith("!Service!EPG123") && (request.PrototypicalProgram.StartsWith("!Program!SH") || request.PrototypicalProgram.StartsWith("!Program!EP") || request.PrototypicalProgram.StartsWith("!Program!MV") || request.PrototypicalProgram.StartsWith("!Program!SP")) select new ListViewItem(new[] {"OneTime", request.PrototypicalTitle ?? request.Title}) {BackColor = epg123 ? Color.LightGreen : Color.LightSalmon, Checked = epg123, Tag = request});

            // add manual requests
            listViewItems.AddRange(from request in _oldRecordings.ManualRequest where !request.Complete && (request.PrototypicalStartTime >= DateTime.Now) && !_wmcRecording.Contains(request.Title + " " + request.PrototypicalStartTime + " " + request.PrototypicalChannelNumber) select new ListViewItem(new[] {"Manual", request.Title ?? request.PrototypicalTitle}) {BackColor = Color.LightGreen, Checked = true, Tag = request});

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
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            _oldWmcFile = openFileDialog1.FileName;
            BuildListViews();
        }
        #endregion

        #region ========== Current WMC Recordings ==========
        private HashSet<string> _wmcRecording = new HashSet<string>();
        private void PopulateWmcRecordings()
        {
            var listViewItems = new List<ListViewItem>();
            foreach (SeriesRequest request in new SeriesRequests(epg123Client.WmcStore.WmcObjectStore))
            {
                // do not display archived/completed entries
                if (request.Complete) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "Series",
                        request.ToString().Substring(7).Replace(" - Complete: False", "")
                    })
                {
                    BackColor = !request.Series.GetUIdValue().Contains("!Series!") ? Color.Pink : Color.LightGreen,
                    Tag = request
                });

                // add the series Uid to the hashset
                _wmcRecording.Add(request.Series.GetUIdValue());
            }

            foreach (ManualRequest request in new ManualRequests(epg123Client.WmcStore.WmcObjectStore))
            {
                // do not display archived/completed entries
                if (request.Complete || (request.StartTime < DateTime.UtcNow)) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "Manual",
                        request.ToString().Substring(7)
                    })
                {
                    BackColor = Color.LightGreen,
                    Tag = request
                });

                // add the manaul recording title, starttime, and channel number
                _wmcRecording.Add(request.Title + " " + request.StartTime + " " + request.Channel.ChannelNumber.Number + "." + request.Channel.ChannelNumber.SubNumber);
            }

            foreach (WishListRequest request in new WishListRequests(epg123Client.WmcStore.WmcObjectStore))
            {
                // do not display archived/completed entries
                if (request.Complete) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "WishList",
                        request.ToString().Substring(9)
                    })
                {
                    BackColor = Color.LightGreen,
                    Tag = request
                });

                // add keywords to hashset
                _wmcRecording.Add(request.Keywords);
            }

            foreach (OneTimeRequest request in new OneTimeRequests(epg123Client.WmcStore.WmcObjectStore))
            {
                // do not display archived/completed entries
                if (request.Complete || (request.StartTime < DateTime.UtcNow)) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "OneTime",
                        request.Title
                    })
                {
                    BackColor = Color.LightGreen,
                    Tag = request
                });

                // add the manaul recording title, starttime, and channel number
                _wmcRecording.Add(request.Title + " " + request.StartTime);
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
                var request = (Request)item.Tag;
                request.Cancel();
                request.Update();
            }
            BuildListViews();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (lvWmcRecordings.SelectedItems.Count <= 0) e.Cancel = true;
        }
        #endregion

        #region ========== Column Sorting ==========
        private void AssignColumnSorters()
        {
            ListView[] listviews = { lvMxfRecordings, lvWmcRecordings };
            foreach (var listview in listviews)
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

        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine which column sorter this click applies to
            var lvcs = (ListViewColumnSorter)((ListView)sender).ListViewItemSorter;

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
            _oldRecordings.ManualRequest = new List<MxfRequest>();
            _oldRecordings.OneTimeRequest = new List<MxfRequest>();
            _oldRecordings.SeriesRequest = new List<MxfRequest>();
            _oldRecordings.WishListRequest = new List<MxfRequest>();

            // set version of mxf file to Win7 in order to import into any WMC
            _oldRecordings.Assembly[0].Version = "6.1.0.0";
            _oldRecordings.Assembly[1].Version = "6.1.0.0";

            // populate the good stuff from the listview
            var checkedItems = 0;
            foreach (ListViewItem item in lvMxfRecordings.Items)
            {
                if (!item.Checked) continue;
                else ++checkedItems;

                switch (item.Text)
                {
                    case "Series":
                        _oldRecordings.SeriesRequest.Add((MxfRequest)item.Tag);
                        break;
                    case "WishList":
                        _oldRecordings.WishListRequest.Add((MxfRequest)item.Tag);
                        break;
                    case "OneTime":
                        _oldRecordings.OneTimeRequest.Add((MxfRequest)item.Tag);
                        break;
                    case "Manual":
                        _oldRecordings.ManualRequest.Add((MxfRequest)item.Tag);
                        break;
                }
            }
            if (checkedItems == 0) return;

            try
            {
                var mxfFilepath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mxf");
                using (var stream = new StreamWriter(mxfFilepath, false))
                {
                    var serializer = new XmlSerializer(typeof(mxf));
                    TextWriter writer = stream;
                    serializer.Serialize(writer, _oldRecordings);
                }

                // import recordings
                var startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\ehome\loadmxf.exe"),
                    Arguments = $"-i \"{mxfFilepath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(startInfo))
                {
                    process.StandardOutput.ReadToEnd();
                    process.WaitForExit(30000);
                }

                // kick off the pvr schedule task
                startInfo = new ProcessStartInfo()
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%WINDIR%\ehome\mcupdate.exe"),
                    Arguments = "-PvrSchedule -nogc",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var proc = Process.Start(startInfo);
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import the old recording requests.\n\n" + ex.Message, "Failed to Import", MessageBoxButtons.OK);
            }

            BuildListViews();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            epg123Client.WmcStore.Close();
            Close();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            if (lvMxfRecordings.SelectedItems.Count != 1) e.Cancel = true;

            var lvi = lvMxfRecordings.SelectedItems[0];
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
            var request = (MxfRequest)lvMxfRecordings.SelectedItems[0].Tag;
            var frm = new frmManualMatch(request);
            if (((ToolStripMenuItem)sender).Text.Equals("Match"))
            {
                if (frm.ShowDialog() != DialogResult.OK) return;
                var topIndexItem = lvMxfRecordings.TopItem.Index;
                _idTable.Add(frm.IdWas, frm.IdIs);
                BuildListViews();
                lvMxfRecordings.TopItem = lvMxfRecordings.Items[topIndexItem];
            }
            else
            {
                frm.IdIs = request.SeriesAttribute ?? request.SeriesElement.Uid;
                frm.ShowDialog();
            }
        }
    }
}