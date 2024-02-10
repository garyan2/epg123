using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using GaRyan2.WmcUtilities;
using Microsoft.MediaCenter.Pvr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace epg123Transfer
{
    public partial class frmTransfer : Form
    {
        private string _oldWmcFile;
        private MXF _oldRecordings;

        public frmTransfer(string file)
        {
            _oldWmcFile = file;
            InitializeComponent();
            AssignColumnSorter(lvMxfRecordings);
            AssignColumnSorter(lvWmcRecordings);

            BuildListViews();

            if (string.IsNullOrEmpty(_oldWmcFile)) return;
            if (lvMxfRecordings.Items.Count > 0)
            {
                btnTransfer_Click(btnAddRecordings, null);
            }
            this.Close();
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
                    using (var stream = CompressXmlFiles.GetBackupFileStream("recordings.mxf", _oldWmcFile))
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(MXF));
                            _oldRecordings = (MXF)serializer.Deserialize(stream);
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
                    try
                    {
                        _oldRecordings = Helper.ReadXmlFile(_oldWmcFile, typeof(MXF));
                    }
                    catch
                    {
                        MessageBox.Show("Not a valid mxf file containing recording requests.", "Invalid File", MessageBoxButtons.OK);
                        _oldWmcFile = string.Empty;
                    }
                }
                if (_oldRecordings.ManualRequest.Count + _oldRecordings.OneTimeRequest.Count + _oldRecordings.SeriesRequest.Count + _oldRecordings.WishListRequest.Count == 0)
                {
                    //MessageBox.Show("There are no scheduled recording requests in the backup file to restore.", "Empty Requests", MessageBoxButtons.OK);
                }
            }

            PopulateWmcRecordings();
            LvLineupSort(lvWmcRecordings, null);
            AdjustColumnWidths(lvWmcRecordings);

            PopulateOldWmcRecordings();
            LvLineupSort(lvMxfRecordings, null);
            AdjustColumnWidths(lvMxfRecordings);
        }

        #region ========== Previous WMC Recordings ==========
        private void PopulateOldWmcRecordings()
        {
            if (_oldRecordings == null) return;
            var listViewItems = new List<ListViewItem>();
            var keywords = new Dictionary<string, string>();
            var services = new Dictionary<string, string>();

            // collect services and keywords based on IDs
            var allRequests = new List<MxfRequest>(_oldRecordings.ManualRequest.Count + _oldRecordings.OneTimeRequest.Count +
                                                   _oldRecordings.SeriesRequest.Count + _oldRecordings.WishListRequest.Count);
            allRequests.AddRange(_oldRecordings.ManualRequest);
            allRequests.AddRange(_oldRecordings.OneTimeRequest);
            allRequests.AddRange(_oldRecordings.SeriesRequest);
            allRequests.AddRange(_oldRecordings.WishListRequest);
            foreach (var request in allRequests)
            {
                if (request.Channel != null && request.PrototypicalService != null && !services.ContainsKey(request.Channel)) services.Add(request.Channel, request.PrototypicalService);
                if (!(request.categories?.Count > 0)) continue;
                foreach (var keyword in request.categories.Where(keyword => keyword.Id != null))
                {
                    keywords.Add(keyword.Id, keyword.Word);
                }

            }

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

                var background = Color.LightPink;
                if (series.StartsWith("!Series!") && !_wmcRecording.Contains(series))
                {
                    // write over existing Uid with gracenote Uid
                    if (!string.IsNullOrEmpty(request.SeriesAttribute)) request.SeriesAttribute = series;
                    else request.SeriesElement.Uid = series;
                    background = Color.LightGreen;
                }
                else if (series.StartsWith("!GenericSeries!"))
                {
                    background = Color.MediumSeaGreen;
                }

                // make sure we get the service id
                var service = request.PrototypicalService;
                if (request.Channel != null && request.PrototypicalService == null) services.TryGetValue(request.Channel, out service);

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "Series",
                        title + $" [{RunTypeString(request.RunType)}, {(!request.AnyChannel.Equals("true") ? $"{request.PrototypicalChannelNumber}{ServiceCallsign(service)}" : $"{ContentQualityString(request.ContentQualityPreference)}")}]"
                    })
                {
                    BackColor = background,
                    Checked = (background == Color.LightGreen),
                    Tag = request
                });
            }

            // add wishlist requests
            foreach (var request in _oldRecordings.WishListRequest)
            {
                if (request.Complete) continue;

                // determine text for entry
                var title = "";
                if (request.KeywordType != 0) title += $"{(KeywordType)request.KeywordType}: ";
                title += request.Keywords;
                if (request.categories?.Count > 0)
                {
                    title += " (";
                    foreach (var keyword in request.categories.Where(keyword => keyword.IdRef != null))
                    {
                        keywords.TryGetValue(keyword.IdRef, out var word);
                        keyword.Word = word;
                    }
                    title = request.categories.OrderBy(cat => cat.Word).Aggregate(title, (current, keyword) => current + $"{keyword.Word}, ");
                    title = title.TrimEnd(',', ' ') + ")";
                }
                if (_wmcRecording.Contains(title)) continue;

                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "WishList",
                        title + $" [{RunTypeString(request.RunType)}]"
                    })
                {
                    BackColor = Color.LightGreen,
                    Checked = true,
                    Tag = request
                });
            }

            // add onetime requests
            foreach (var request in _oldRecordings.OneTimeRequest)
            {
                if (request.Complete || _wmcRecording.Contains($"{request.PrototypicalTitle} {request.PrototypicalStartTime}")) continue;

                // make sure we get the service id
                var service = request.PrototypicalService;
                if (request.Channel != null && request.PrototypicalService == null) services.TryGetValue(request.Channel, out service);

                var epg123 = request.PrototypicalProgram.StartsWith("!Program!EP") ||
                             request.PrototypicalProgram.StartsWith("!Program!SH") ||
                             request.PrototypicalProgram.StartsWith("!Program!MV") ||
                             request.PrototypicalProgram.StartsWith("!Program!SP");
                var title = $"{request.PrototypicalTitle ?? request.Title}{EpisodeTitle(request.PrototypicalProgram)} [{request.PrototypicalChannelNumber}{ServiceCallsign(service)}, {request.PrototypicalStartTime.ToLocalTime()}]";
                var background = request.PrototypicalStartTime > DateTime.UtcNow ? Color.LightGreen : Color.MediumSeaGreen;

                var dupe = listViewItems.SingleOrDefault(arg => arg.SubItems[1].Text == title);
                if (dupe != null) continue;

                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "OneTime",
                        title
                    })
                {
                    BackColor = epg123 ? background : Color.LightPink,
                    Checked = request.PrototypicalStartTime > DateTime.UtcNow,
                    Tag = request
                });
            }

            // add manual requests
            foreach (var request in _oldRecordings.ManualRequest)
            {
                if (request.Complete || (request.IsRecurring.Equals("false") && request.PrototypicalStartTime < DateTime.UtcNow) ||
                    _wmcRecording.Contains(request.Title + " " + request.PrototypicalStartTime + " " + request.PrototypicalChannelNumber)) continue;

                // make sure we get the service id
                var service = request.PrototypicalService;
                if (request.Channel != null && request.PrototypicalService == null) services.TryGetValue(request.Channel, out service);

                var title = request.PrototypicalTitle;
                if (request.IsRecurring.Equals("false"))
                {
                    title += $" [{request.PrototypicalChannelNumber}{ServiceCallsign(service)}, {request.PrototypicalStartTime.ToLocalTime()} - {request.PrototypicalStartTime.ToLocalTime() + ConvertPDTHSM(request.PrototypicalDuration)}]";
                }
                else
                {
                    var daysOfWeek = (DaysOfWeek)int.Parse(request.DayOfWeekMask);
                    title += " [Every";
                    if (daysOfWeek == DaysOfWeek.All) title += "day,";
                    else if (daysOfWeek != DaysOfWeek.None)
                    {
                        title += " ";
                        title += (daysOfWeek & DaysOfWeek.Monday) != DaysOfWeek.None ? $"{DaysOfWeek.Monday}," : "";
                        title += (daysOfWeek & DaysOfWeek.Tuesday) != DaysOfWeek.None ? $"{DaysOfWeek.Tuesday}," : "";
                        title += (daysOfWeek & DaysOfWeek.Wednesday) != DaysOfWeek.None ? $"{DaysOfWeek.Wednesday}," : "";
                        title += (daysOfWeek & DaysOfWeek.Thursday) != DaysOfWeek.None ? $"{DaysOfWeek.Thursday}," : "";
                        title += (daysOfWeek & DaysOfWeek.Friday) != DaysOfWeek.None ? $"{DaysOfWeek.Friday}," : "";
                        title += (daysOfWeek & DaysOfWeek.Saturday) != DaysOfWeek.None ? $"{DaysOfWeek.Saturday}," : "";
                        title += (daysOfWeek & DaysOfWeek.Sunday) != DaysOfWeek.None ? $"{DaysOfWeek.Sunday}," : "";
                    }
                    title += $" {request.PrototypicalChannelNumber}{ServiceCallsign(service)}, {request.PrototypicalStartTime.ToLocalTime():h:mm tt} - {request.PrototypicalStartTime.ToLocalTime() + ConvertPDTHSM(request.PrototypicalDuration):h:mm tt}]";
                }

                var dupe = listViewItems.SingleOrDefault(arg => arg.SubItems[1].Text == title);
                if (dupe != null) continue;

                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "Manual",
                        title
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
            else if (((ListView)sender).Items[e.Index].BackColor == Color.LightPink)
            {
                e.NewValue = CheckState.Unchecked;
            }
        }

        private void btnOpenBackup_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Helper.Epg123BackupFolder;
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
            foreach (SeriesRequest request in new SeriesRequests(WmcStore.WmcObjectStore).Cast<SeriesRequest>())
            {
                // do not display archived/completed entries
                if (request.Complete) continue;

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "Series",
                        request.Title + $" [{RunTypeString(request.RunType)}, {(!request.AnyChannel ? $"{request.Channel?.ChannelNumber} {request.Channel?.CallSign}" : $"{ContentQualityString(request.ContentQualityPreference)}")}]"
                    })
                {
                    BackColor = !request.Series.GetUIdValue().Contains("!Series!") ? Color.Pink : IsScheduled(request) ? Color.LightGreen : Color.MediumSeaGreen,
                    Tag = request
                });

                // add the series Uid to the hashset
                _wmcRecording.Add(request.Series.GetUIdValue());
            }

            foreach (ManualRequest request in new ManualRequests(WmcStore.WmcObjectStore).Cast<ManualRequest>())
            {
                // do not display archived/completed entries
                if (request.Complete || (!request.IsRecurring && (request.RequestedProgram?.IsRequestFilled ?? false))) continue;

                // add the manual recording title, starttime, and channel number
                _wmcRecording.Add(request.Title + " " + request.StartTime + " " + request.Channel?.ChannelNumber?.Number + "." + request.Channel?.ChannelNumber?.SubNumber);

                // what to display?
                var title = request.PrototypicalProgram.ToString().StartsWith("Manual Recording") ? request.Title : request.PrototypicalProgram.Title;
                if (!request.IsRecurring)
                {
                    title += $" [{request.Channel?.ChannelNumber} {request.Channel?.CallSign}, {request.StartTime.ToLocalTime()} - {request.StartTime.ToLocalTime() + request.Duration}]";
                }
                else
                {
                    title += " [Every";
                    if (request.DaysOfWeek == DaysOfWeek.All) title += "day,";
                    else if (request.DaysOfWeek != DaysOfWeek.None)
                    {
                        title += " ";
                        title += (request.DaysOfWeek & DaysOfWeek.Monday) != DaysOfWeek.None ? $"{DaysOfWeek.Monday}," : "";
                        title += (request.DaysOfWeek & DaysOfWeek.Tuesday) != DaysOfWeek.None ? $"{DaysOfWeek.Tuesday}," : "";
                        title += (request.DaysOfWeek & DaysOfWeek.Wednesday) != DaysOfWeek.None ? $"{DaysOfWeek.Wednesday}," : "";
                        title += (request.DaysOfWeek & DaysOfWeek.Thursday) != DaysOfWeek.None ? $"{DaysOfWeek.Thursday}," : "";
                        title += (request.DaysOfWeek & DaysOfWeek.Friday) != DaysOfWeek.None ? $"{DaysOfWeek.Friday}," : "";
                        title += (request.DaysOfWeek & DaysOfWeek.Saturday) != DaysOfWeek.None ? $"{DaysOfWeek.Saturday}," : "";
                        title += (request.DaysOfWeek & DaysOfWeek.Sunday) != DaysOfWeek.None ? $"{DaysOfWeek.Sunday}," : "";
                    }
                    title += $" {request.Channel?.ChannelNumber} {request.Channel?.CallSign}, {request.StartTime.ToLocalTime():h:mm tt} - {request.StartTime.ToLocalTime() + request.Duration:h:mm tt}]";
                }

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "Manual",
                        title
                    })
                {
                    BackColor = IsScheduled(request) ? Color.LightGreen : Color.MediumSeaGreen,
                    Tag = request
                });
            }

            foreach (WishListRequest request in new WishListRequests(WmcStore.WmcObjectStore).Cast<WishListRequest>())
            {
                // do not display archived/completed entries
                if (request.Complete) continue;

                // determine text for entry
                var title = "";
                if (request.KeywordType != KeywordType.None) title += $"{request.KeywordType}: ";
                title += request.Title;
                if (!request.Categories.Empty)
                {
                    title += " (";
                    title = request.Categories.OrderBy(cat => cat.Word).Aggregate(title, (current, keyword) => current + $"{keyword.Word}, ");
                    title = title.TrimEnd(',', ' ') + ")";
                }

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "WishList",
                        title + $" [{RunTypeString(request.RunType)}]"
                    })
                {
                    BackColor = IsScheduled(request) ? Color.LightGreen : Color.MediumSeaGreen,
                    Tag = request
                });

                // add keywords to hashset
                _wmcRecording.Add(title);
            }

            foreach (OneTimeRequest request in new OneTimeRequests(WmcStore.WmcObjectStore).Cast<OneTimeRequest>())
            {
                // do not display archived/completed entries
                if (request.Complete || (request.RequestedProgram?.IsRequestFilled ?? false)) continue;

                // add the manual recording title, starttime, and channel number
                _wmcRecording.Add($"{request.Title} {request.StartTime}");

                // create ListViewItem
                listViewItems.Add(new ListViewItem(
                    new[]
                    {
                        "OneTime",
                        $"{request.Title}{(request.PrototypicalProgram.EpisodeTitle == "" ? null : $" : {request.PrototypicalProgram.EpisodeTitle}")} [{request.Channel?.ChannelNumber} {request.Channel?.CallSign}, {request.StartTime.ToLocalTime()}]"
                    })
                {
                    BackColor = IsScheduled(request) ? Color.LightGreen : Color.MediumSeaGreen,
                    Tag = request
                });
            }

            if (listViewItems.Count > 0)
            {
                lvWmcRecordings.Items.AddRange(listViewItems.ToArray());
            }

            if (WmcStore.WmcMergedLineup?.UncachedChannels?.Empty ?? true) btnAddRecordings.Enabled = false;
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
        private void AssignColumnSorter(ListView listview)
        {
            listview.ListViewItemSorter = new ListViewColumnSorter
            {
                SortColumn = 1,
                Order = SortOrder.Ascending
            };
        }

        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine which column sorter this click applies to
            var lvcs = (ListViewColumnSorter)((ListView)sender).ListViewItemSorter;

            // Determine if clicked column is already the column that is being sorted
            if (e == null)
            {
                e = new ColumnClickEventArgs(lvcs.SortColumn);
            }
            else if (e.Column == lvcs.SortColumn)
            {
                // Reverse the current sort direction for this column
                lvcs.Order = (lvcs.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvcs.Order = SortOrder.Ascending;
            }

            // always have secondary sort to second column (title/keyword)
            if (e.Column == 0)
            {
                lvcs.SortColumn = 1;
                ((ListView)sender).Sort();
            }

            // Perform the sort with these new sort options.
            lvcs.SortColumn = e.Column;
            ((ListView)sender).Sort();
        }

        private void AdjustColumnWidths(ListView listView)
        {
            var dpiScaleFactor = 1.0;
            using (var g = CreateGraphics())
            {
                if ((int)g.DpiX != 96)
                {
                    dpiScaleFactor = g.DpiX / 96;
                }
            }

            listView.Columns[0].Width = -1;
            listView.Columns[0].Width = Math.Max(listView.Columns[0].Width, (int)(60 * dpiScaleFactor));
            listView.Columns[1].Width = -1;
            listView.Columns[1].Width = Math.Max(listView.Width - listView.Columns[0].Width - (int)(23 * dpiScaleFactor), listView.Columns[1].Width);
        }

        #endregion

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            if (lvMxfRecordings.Items.Count <= 0) return;

            this.Cursor = Cursors.WaitCursor;

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
                ++checkedItems;

                if (((MxfRequest)item.Tag).AnyChannel.Equals("false"))
                {
                    if (ServiceCallsign(((MxfRequest)item.Tag).PrototypicalService) == null)
                    {
                        ((MxfRequest)item.Tag).AnyChannel = "true";
                        ((MxfRequest)item.Tag).ContentQualityPreference = $"{(int)ContentQualityPreference.PreferHD}";
                    }
                }

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

            if (checkedItems == 0)
            {
                this.Cursor = Cursors.Arrow;
                return;
            }

            try
            {
                // create temporary mxf file
                var mxfFilepath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mxf");
                Helper.WriteXmlFile(_oldRecordings, mxfFilepath);

                // import file and run pvr indexing
                if (WmcStore.ImportMxfFile(mxfFilepath))
                    WmcStore.ReindexPvrSchedule();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import the old recording requests.\n\n{Helper.ReportExceptionMessages(ex)}", "Failed to Import", MessageBoxButtons.OK);
            }

            BuildListViews();
            this.Cursor = Cursors.Arrow;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            WmcStore.Close();
            Close();
        }

        private TimeSpan ConvertPDTHSM(string pdthsm)
        {
            var expression = new Regex(@"P((?<days>\d{1,2})D)*T((?<hours>\d{1,2})H)*((?<minutes>\d{1,2})M)*((?<seconds>\d{1,2})S)*");
            var match = expression.Match(pdthsm);
            if (!match.Success) return new TimeSpan(0);
            int.TryParse(match.Groups["days"].Value, out var days);
            int.TryParse(match.Groups["hours"].Value, out var hours);
            int.TryParse(match.Groups["minutes"].Value, out var minutes);
            int.TryParse(match.Groups["seconds"].Value, out var seconds);
            return new TimeSpan(days, hours, minutes, seconds);
        }

        private static string RunTypeString(string value)
        {
            return RunTypeString((RunType)int.Parse(value));
        }

        private static string RunTypeString(RunType type)
        {
            switch (type)
            {
                case RunType.FirstRunOnly:
                    return "New";
                case RunType.LiveOnly:
                    return "Live";
                default:
                    return "New/Repeat";
            }
        }

        private static string ServiceCallsign(string protoService)
        {
            if (!(WmcStore.WmcObjectStore.UIds[protoService]?.Target is Microsoft.MediaCenter.Guide.Service service)) return null;
            return service.CallSign == "" ? null : $" {service.CallSign}";
        }

        private static string EpisodeTitle(string protoProgram)
        {
            if (!(WmcStore.WmcObjectStore.UIds[protoProgram]?.Target is Microsoft.MediaCenter.Guide.Program program)) return null;
            return program.EpisodeTitle == "" ? null : $" : {program.EpisodeTitle}";
        }

        private static string ContentQualityString(string preference)
        {
            return ContentQualityString((ContentQualityPreference)int.Parse(preference));
        }

        private static string ContentQualityString(ContentQualityPreference preference)
        {
            switch (preference)
            {
                case ContentQualityPreference.OnlyHD:
                    return "HD Only";
                case ContentQualityPreference.PreferHD:
                    return "HD Preferred";
                case ContentQualityPreference.OnlySD:
                    return "SD Only";
                case ContentQualityPreference.PreferSD:
                    return "SD Preferred";
                default:
                    return "SD/HD";
            }
        }

        private static bool IsScheduled(Request request)
        {
            return request.RequestedPrograms.Any(program => program.ScheduledRecording != null);
            //return !request.Complete && request.RequestedPrograms.Any(program => program.IsRequested && !program.IsRequestFilled);
        }

        private void frmTransfer_Shown(object sender, EventArgs e)
        {
            AdjustColumnWidths(lvMxfRecordings);
            AdjustColumnWidths(lvWmcRecordings);
        }
    }
}