using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using epg123.Properties;
using epg123.SchedulesDirectAPI;
using epg123.Task;

namespace epg123
{
    public partial class frmMain : Form
    {
        private enum LineupColumn
        {
            CallSign = 0,
            Channel = 1,
            StationID = 2,
            Name = 3
        }
        private List<string> _lineups;
        private readonly epgTaskScheduler _task = new epgTaskScheduler();
        private HashSet<string> _newLineups = new HashSet<string>();
        private readonly ImageList _imageList = new ImageList();
        private bool _newLogin = true;
        public bool RestartAsAdmin;

        public epgConfig Config = new epgConfig();
        private epgConfig _oldConfig = new epgConfig();
        public bool Execute;
        public bool Import;
        public bool Match;
        private readonly double _dpiScaleFactor = 1.0;

        public frmMain()
        {
            // required to show UAC shield on buttons
            Application.EnableVisualStyles();

            // create form objects
            InitializeComponent();

            // adjust components for screen dpi
            using (var g = CreateGraphics())
            {
                if ((int)g.DpiX != 96 || (int)g.DpiY != 96)
                {
                    _dpiScaleFactor = g.DpiX / 96;

                    // adjust image size for list view items
                    _imageList.ImageSize = new Size((int)(g.DpiX / 6), (int)(g.DpiY / 6));

                    // adjust column widths for list views
                    ListView[] listviews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
                    foreach (var listview in listviews)
                    {
                        foreach (ColumnHeader column in listview.Columns)
                        {
                            column.Width = (int)(column.Width * _dpiScaleFactor);
                        }
                    }
                }
            }

            toolStrip1.ImageScalingSize = toolStrip2.ImageScalingSize = toolStrip3.ImageScalingSize = toolStrip4.ImageScalingSize = toolStrip5.ImageScalingSize = new Size((int)(_dpiScaleFactor * 16), (int)(_dpiScaleFactor * 16));
        }
        private void ConfigForm_Load(object sender, EventArgs e)
        {
            // copy over window size and location from previous version if needed
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            // initialize the schedules direct api
            sdApi.Initialize("EPG123", Helper.SdGrabberVersion);

            // complete the title bar label with version number
            Text += $" v{Helper.Epg123Version}";

            // check for updates
            var veresp = sdApi.SdCheckVersion();
            if (veresp != null && veresp.Version != Helper.SdGrabberVersion)
            {
                lblUpdate.Text = $"UPDATE AVAILABLE (v{veresp.Version})";
            }

            // set imagelist for listviews
            lvL1Lineup.SmallImageList = lvL1Lineup.LargeImageList = _imageList;
            lvL2Lineup.SmallImageList = lvL2Lineup.LargeImageList = _imageList;
            lvL3Lineup.SmallImageList = lvL3Lineup.LargeImageList = _imageList;
            lvL4Lineup.SmallImageList = lvL4Lineup.LargeImageList = _imageList;
            lvL5Lineup.SmallImageList = lvL5Lineup.LargeImageList = _imageList;

            // set the splitter distance
            splitContainer1.Panel1MinSize = (int)(splitContainer1.Panel1MinSize * _dpiScaleFactor);

            // restore window position and size
            if ((Settings.Default.WindowLocation != new Point(-1, -1)))
            {
                Location = Settings.Default.WindowLocation;
            }

            Size = Settings.Default.WindowSize;
            if (Settings.Default.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }
        private void frmMain_Shown(object sender, EventArgs e)
        {
            // get login/password info from configuration file if exists
            if (!File.Exists(Helper.Epg123CfgPath)) return;

            // login to Schedules Direct and get a token
            try
            {
                using (var stream = new StreamReader(Helper.Epg123CfgPath, Encoding.Default))
                {
                    var serializer = new XmlSerializer(typeof(epgConfig));
                    TextReader reader = new StringReader(stream.ReadToEnd());
                    Config = (epgConfig)serializer.Deserialize(reader);
                    reader.Close();

                    _oldConfig = Config.Clone();
                }

                if (!string.IsNullOrEmpty(Config.UserAccount.LoginName) && !string.IsNullOrEmpty(Config.UserAccount.PasswordHash))
                {
                    txtLoginName.Text = Config.UserAccount.LoginName;
                    txtPassword.Text = "********";

                    Refresh();
                    btnLogin_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
            }

            // if client was started as elevated to perform an action
            if (Helper.UserHasElevatedRights && File.Exists(Helper.EButtonPath))
            {
                using (var sr = new StreamReader(Helper.EButtonPath))
                {
                    var line = sr.ReadLine();
                    if (line != null && (line.Contains("createTask") || line.Contains("deleteTask")))
                    {
                        btnTask_Click(null, null);
                        tabConfigs.SelectedTab = tabTask;
                    }
                    sr.Close();
                }
                Helper.DeleteFile(Helper.EButtonPath);
            }
            Cursor = Cursors.Arrow;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // give option to save if there were changes
            RefreshConfiguration();
            if (!btnLogin.Enabled && !Config.Equals(_oldConfig) && DialogResult.Yes == MessageBox.Show("There have been changes made to your configuration. Do you wish to save changes before exiting?", "Configuration Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                btnSave_Click(sender, null);
            }

            // save the windows size and location
            if (WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowLocation = Location;
                Settings.Default.WindowSize = Size;
            }
            else
            {
                Settings.Default.WindowLocation = RestoreBounds.Location;
                Settings.Default.WindowSize = RestoreBounds.Size;
            }
            Settings.Default.WindowMaximized = (WindowState == FormWindowState.Maximized);
            Settings.Default.Save();
        }

        #region ========== Elevated Rights, Registry, and Event Log =========
        private void ElevateRights()
        {
            // save current settings
            if (!string.IsNullOrEmpty(txtAcctExpires.Text))
            {
                btnSave_Click(null, null);
            }

            // start a new process with elevated rights
            RestartAsAdmin = true;
            Application.Exit();
        }

        #endregion

        #region ========== Scheduled Task ==========
        private void UpdateTaskPanel(bool silent = false)
        {
            // get status
            _task.QueryTask(silent);

            // set task create/delete button text
            btnTask.Text = (_task.Exist || _task.ExistNoAccess) ? "Delete" : "Create";

            // update scheduled task run time
            tbSchedTime.Enabled = (!_task.Exist && !_task.ExistNoAccess);
            tbSchedTime.Text = _task.SchedTime.ToString("HH:mm");
            lblUpdateTime.Enabled = (!_task.Exist && !_task.ExistNoAccess);

            // set sheduled task wake checkbox
            cbTaskWake.Enabled = (!_task.Exist && !_task.ExistNoAccess);
            cbTaskWake.Checked = _task.Wake;

            // determine which action is the client action
            var clientIndex = -1;
            var epg123Index = -1;
            if (_task.Exist)
            {
                for (var i = 0; i < _task.Actions.Length; ++i)
                {
                    if (_task.Actions[i].Path.ToLower().Contains("epg123.exe")) epg123Index = i;
                    if (_task.Actions[i].Path.ToLower().Contains("epg123client.exe")) clientIndex = i;
                }

                // verify task configuration with respect to this executable
                if (!silent && epg123Index >= 0 && !_task.Actions[epg123Index].Path.ToLower().Replace("\"", "").Equals(Helper.Epg123ExePath.ToLower()))
                {
                    MessageBox.Show($"The location of this program file is not the same location configured in the Scheduled Task.\n\nThis program:\n{Helper.Epg123ExePath}\n\nTask program:\n{_task.Actions[epg123Index].Path}", "Configuration Warning", MessageBoxButtons.OK);
                }
            }

            // set import and automatch checkbox states
            if (!File.Exists(Helper.Epg123ClientExePath) || !File.Exists(Helper.EhshellExeFilePath))
            {
                cbImport.Enabled = cbAutomatch.Enabled = false;
                cbImport.Checked = cbAutomatch.Checked = false;
            }
            else
            {
                cbImport.Enabled = !_task.Exist && !_task.ExistNoAccess;
                cbImport.Checked = (clientIndex >= 0) || (!_task.Exist && Config.AutoImport);
                cbAutomatch.Enabled = !_task.Exist && !_task.ExistNoAccess && cbImport.Checked;
                cbAutomatch.Checked = ((clientIndex >= 0) && _task.Actions[clientIndex].Arguments.ToLower().Contains("-match")) || (!_task.Exist && Config.Automatch);
            }

            // update status string
            if (_task.Exist && (epg123Index >= 0))
            {
                lblSchedStatus.Text = _task.StatusString;
                lblSchedStatus.ForeColor = Color.Black;
            }
            else if (_task.Exist && (clientIndex >= 0))
            {
                lblSchedStatus.Text = "### Client Mode ONLY - Guide will not be downloaded. ###";
                lblSchedStatus.ForeColor = Color.Red;
            }
            else
            {
                lblSchedStatus.Text = _task.StatusString;
                lblSchedStatus.ForeColor = Color.Red;
            }
        }
        private void btnTask_Click(object sender, EventArgs e)
        {
            if (sender != null) // null sender means we restarted to finish in administrator mode
            {
                // create new task if file location is valid
                if (!_task.Exist)
                {
                    // create task using epg123.exe & epg123Client.exe
                    if (cbImport.Checked)
                    {
                        var actions = new epgTaskScheduler.TaskActions[2];
                        actions[0].Path = Helper.Epg123ExePath;
                        actions[0].Arguments = "-update";
                        actions[1].Path = Helper.Epg123ClientExePath;
                        actions[1].Arguments = "-i \"" + Helper.Epg123MxfPath + "\"" + ((cbAutomatch.Checked) ? " -match" : null);
                        _task.CreateTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                    }
                    // create task using epg123.exe
                    else
                    {
                        var actions = new epgTaskScheduler.TaskActions[1];
                        actions[0].Path = Helper.Epg123ExePath;
                        actions[0].Arguments = "-update";
                        _task.CreateTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                    }
                    btnSave_Click(null, null);
                }
            }

            // check for elevated rights and open new process if necessary
            if (!Helper.UserHasElevatedRights)
            {
                if (_task.Exist || _task.ExistNoAccess)
                {
                    Helper.WriteEButtonFile("deleteTask");
                }
                else
                {
                    Helper.WriteEButtonFile("createTask");
                }
                ElevateRights();
                return;
            }

            if (_task.Exist)
            {
                _task.DeleteTask();
            }
            else
            {
                _task.ImportTask();
            }

            // update panel with current information
            UpdateTaskPanel();
        }
        #endregion

        #region ========== Login ==========
        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnLogin_Click(null, null);
            }
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // disable input fields while trying to login
            Cursor = Cursors.WaitCursor;
            txtLoginName.Enabled = txtPassword.Enabled = btnLogin.Enabled = false;

            if ((Config.UserAccount != null) && !string.IsNullOrEmpty(Config.UserAccount.LoginName) && !string.IsNullOrEmpty(Config.UserAccount.PasswordHash))
            {
                // use the new username/password combination, otherwise use the stored username/password
                if (!txtLoginName.Text.ToLower().Equals(Config.UserAccount.LoginName.ToLower()) || !txtPassword.Text.Equals("********"))
                {
                    Config.UserAccount = new SdUserAccount
                    {
                        LoginName = txtLoginName.Text,
                        Password = txtPassword.Text
                    };
                }
                else
                {
                    _newLogin = false;
                }
            }
            else if (!string.IsNullOrEmpty(txtLoginName.Text) && !string.IsNullOrEmpty(txtPassword.Text))
            {
                Config.UserAccount = new SdUserAccount
                {
                    LoginName = txtLoginName.Text,
                    Password = txtPassword.Text
                };
            }
            else
            {
                Config.UserAccount = new SdUserAccount
                {
                    LoginName = string.Empty,
                    Password = string.Empty
                };
            }

            txtLoginName.Enabled = txtPassword.Enabled = btnLogin.Enabled = !LoginUser();
            Cursor = Cursors.Arrow;
        }
        private bool LoginUser()
        {
            bool ret;
            var errorString = string.Empty;
            if (ret = sdApi.SdGetToken(Config.UserAccount.LoginName, Config.UserAccount.PasswordHash, ref errorString))
            {
                // get membership expiration
                GetUserStatus();

                // create 2 lists : stations to be downloaded and stations not to be downloaded
                // stations that do not exist in either list are NEW
                if (Config.StationId != null)
                {
                    PopulateIncludedExcludedStations(Config.StationId);
                }

                // populate the listviews with lineup channels
                BuildLineupTabs();

                // set configuration options
                if (Config.DaysToDownload <= 0)
                {
                    numDays.Value = 14;
                    cbTVDB.Checked = true;
                    cbOadOverride.Checked = true;
                    cbTMDb.Checked = true;
                    cbSdLogos.Checked = true;
                    cbAddNewStations.Checked = true;
                }
                else
                {
                    numDays.Value = Math.Min(Config.DaysToDownload, numDays.Maximum);
                    cbPrefixTitle.Checked = Config.PrefixEpisodeTitle;
                    cbAppendDescription.Checked = Config.AppendEpisodeDesc;
                    cbOadOverride.Checked = Config.OadOverride;
                    cbTMDb.Checked = Config.TMDbCoverArt;
                    cbSdLogos.Checked = Config.IncludeSdLogos;
                    
                    cbTVDB.Checked = Config.TheTvdbNumbers;
                    cbPrefixDescription.Checked = Config.PrefixEpisodeDescription;
                    cbAlternateSEFormat.Checked = Config.AlternateSEFormat;
                    cbAddNewStations.Checked = Config.AutoAddNew;
                    cbSeriesPosterArt.Checked = Config.SeriesPosterArt;
                    cbModernMedia.Checked = Config.ModernMediaUiPlusSupport;
                    cmbPreferredLogos.SelectedIndex = (int)(Helper.PreferredLogos)Enum.Parse(typeof(Helper.PreferredLogos), Config.PreferredLogoStyle, true);
                    cmbAlternateLogos.SelectedIndex = (int)(Helper.PreferredLogos)Enum.Parse(typeof(Helper.PreferredLogos), Config.AlternateLogoStyle, true);
                    ckChannelNumbers.Checked = Config.XmltvIncludeChannelNumbers;
                    ckChannelLogos.Checked = !string.IsNullOrEmpty(Config.XmltvIncludeChannelLogos) && (Config.XmltvIncludeChannelLogos != "false");
                    ckLocalLogos.Checked = (Config.XmltvIncludeChannelLogos == "local") || (Config.XmltvIncludeChannelLogos == "substitute");
                    ckUrlLogos.Checked = (Config.XmltvIncludeChannelLogos == "url");
                    ckSubstitutePath.Checked = (Config.XmltvIncludeChannelLogos == "substitute");
                    txtSubstitutePath.Text = Config.XmltvLogoSubstitutePath;
                    ckXmltvFillerData.Checked = Config.XmltvAddFillerData;
                    ckXmltvExtendedInfo.Checked = Config.XmltvExtendedInfoInTitleDescriptions;
                    numFillerDuration.Value = Config.XmltvFillerProgramLength;
                    rtbFillerDescription.Text = Config.XmltvFillerProgramDescription;
                    tbXmltvOutput.Text = Config.XmltvOutputFile ?? Helper.Epg123XmltvPath;
                    cbBrandLogo.Checked = !Config.BrandLogoImage.Equals("none");

                    cbXmltv.Checked = Config.CreateXmltv;
                }

                // get persistent cfg values
                if (!_task.Exist && !_task.ExistNoAccess && File.Exists(Helper.Epg123ClientExePath))
                {
                    if (File.Exists(Helper.Epg123CfgPath))
                    {
                        cbImport.Checked = cbAutomatch.Enabled = Config.AutoImport;
                        cbAutomatch.Checked = Config.Automatch;
                    }
                    else
                    {
                        cbImport.Checked = cbAutomatch.Enabled = true;
                        cbAutomatch.Checked = true;
                    }
                }

                // enable form controls
                tabLineups.Enabled = true;
                tabConfigs.Enabled = true;
                btnSave.Enabled = true;
                btnExecute.Enabled = true;
                btnClientLineups.Enabled = true;

                // update the task panel
                UpdateTaskPanel();

                // automatically save a .cfg file with account info if first login or password change
                if (_newLogin)
                {
                    btnSave_Click(null, null);
                }
            }
            else
            {
                MessageBox.Show(errorString, "Login Failed");
            }

            return ret;
        }
        private void GetUserStatus()
        {
            var status = sdApi.SdGetStatus();
            if (status == null)
            {
                txtAcctExpires.Text = "Unknown";
            }
            else
            {
                txtAcctExpires.Text = status.Account.Expires.ToLocalTime().ToString();
                if (status.Account.Expires - DateTime.Now < TimeSpan.FromDays(14.0))
                {
                    // weird fact: the text color of a read-only textbox will only change after you set the backcolor
                    txtAcctExpires.ForeColor = Color.Red;
                    txtAcctExpires.BackColor = txtAcctExpires.BackColor;
                }

                if (status.Lineups != null && status.Lineups.Count != 0) return;
                MessageBox.Show("There are no lineups in your SD-JSON account. You must\nadd at least one lineup to proceed.", "No Lineups in Account", MessageBoxButtons.OK);
                btnClientConfig_Click(null, null);
            }
        }
        #endregion

        #region ========== Setup Lineup ListViews and Tabs ==========
        private void BuildLineupTabs()
        {
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup, lvL5Lineup };
            ToolStripLabel[] listViewLabels = { lblL1Lineup, lblL2Lineup, lblL3Lineup, lblL4Lineup };
            ToolStrip[] toolStrips = { toolStrip1, toolStrip2, toolStrip3, toolStrip4, toolStrip5 };

            // focus on first tab
            tabLineups.SelectedIndex = 0;

            // temporarily disable item check
            _disableItemCheck = true;

            // clear lineup listviews and title
            for (var i = 0; i < listViews.Length; ++i)
            {
                listViews[i].Items.Clear();
                listViews[i].ListViewItemSorter = null;
                if (i < 4) listViewLabels[i].Text = string.Empty;
                toolStrips[i].Enabled = false;
                ExcludeLineup(i);
            }

            // populate the listviews with channels/services
            BuildListViewChannels();
            BuildCustomListViewChannels();

            // assign a listviewcolumnsorter to a listview
            AssignColumnSorters();

            // re-enable item check
            _disableItemCheck = false;
        }
        private void BuildCustomListViewChannels()
        {
            btnCustomLineup.DropDownItems.Clear();
            if (File.Exists(Helper.Epg123CustomLineupsXmlPath))
            {
                CustomLineups customLineups;
                using (var stream = new StreamReader(Helper.Epg123CustomLineupsXmlPath, Encoding.Default))
                {
                    var serializer = new XmlSerializer(typeof(CustomLineups));
                    TextReader reader = new StringReader(stream.ReadToEnd());
                    customLineups = (CustomLineups)serializer.Deserialize(reader);
                    reader.Close();
                }

                foreach (var lineup in customLineups.CustomLineup)
                {
                    btnCustomLineup.DropDownItems.Add($"{lineup.Name} ({lineup.Location})").Tag = lineup;
                }
                toolStrip5.Enabled = true;
            }
            else
            {
                btnCustomLineup.Text = "Click here to manage custom lineups.";
                btnCustomLineup.Tag = string.Empty;
                toolStrip5.Enabled = false;
            }

            if (btnCustomLineup.DropDownItems.Count <= 0) return;
            if (!_newLogin)
            {
                foreach (ToolStripItem item in btnCustomLineup.DropDownItems)
                {
                    if (!Config.IncludedLineup.Contains(((CustomLineup) item.Tag).Lineup)) continue;
                    item.PerformClick();
                    L5includeToolStripMenuItem.PerformClick();
                    return;
                }
            }
            btnCustomLineup.DropDownItems[0].PerformClick();
        }
        private CustomStation PrimaryOrAlternateStation(CustomStation station)
        {
            var ret = new CustomStation
            {
                Alternate = station.Alternate,
                Callsign = station.Callsign,
                Name = station.Name,
                Number = station.Number,
                StationId = station.StationId,
                Subnumber = station.Subnumber
            };

            ListView[] listviews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
            foreach (var listView in listviews)
            {
                foreach (ListViewItem item in listView.Items)
                {
                    if (item.SubItems[(int)LineupColumn.StationID].Text.Equals(station.StationId))
                    {
                        return station;
                    }

                    if (!item.SubItems[(int) LineupColumn.StationID].Text.Equals(station.Alternate)) continue;
                    ret.Callsign = item.SubItems[(int)LineupColumn.CallSign].Text;
                    ret.Name = item.SubItems[(int)LineupColumn.Name].Text;
                    ret.StationId = item.SubItems[(int)LineupColumn.StationID].Text;
                }
            }
            return ret;
        }
        private void btnCustomLineup_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!(e.ClickedItem.Tag is CustomLineup lineup)) return;
            btnCustomLineup.Text = e.ClickedItem.Text;
            btnCustomLineup.Tag = lineup.Lineup;

            if (lvL5Lineup.Items.Count > 0)
            {
                lvL5Lineup.Items.Clear();
            }

            var items = new List<ListViewItem>();
            foreach (var station in lineup.Station)
            {
                var stationItem = PrimaryOrAlternateStation(station);

                var channel = stationItem.Number.ToString();
                channel += (stationItem.Subnumber != 0) ? "." + stationItem.Subnumber : string.Empty;
                items.Add(new ListViewItem(
                    new[]
                    {
                        stationItem.Callsign,
                        channel,
                        stationItem.StationId,
                        stationItem.Name
                    })
                {
                    Checked = _allStationIDs.Contains(station.StationId) || _allStationIDs.Contains(station.Alternate),
                    ForeColor = _allStationIDs.Contains(stationItem.StationId) ? SystemColors.WindowText : SystemColors.GrayText
                });
            }

            _lockCustomCheckboxes = false;
            lvL5Lineup.Items.AddRange(items.ToArray());
            _lockCustomCheckboxes = true;
        }

        private bool _lockCustomCheckboxes;
        private HashSet<string> _allStationIDs;
        private void BuildListViewChannels()
        {
            _allStationIDs = new HashSet<string>();
            _lineups = new List<string>();
            _sdlogos = new Dictionary<string, string>();
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
            ToolStripLabel[] listViewLabels = { lblL1Lineup, lblL2Lineup, lblL3Lineup, lblL4Lineup };
            ToolStrip[] toolStrips = { toolStrip1, toolStrip2, toolStrip3, toolStrip4 };

            // retrieve lineups from SD
            var clientLineups = sdApi.SdGetLineups();
            if (clientLineups == null) return;

            // build listviews with lineups and channels
            var processedLineup = 0;
            foreach (var clientLineup in clientLineups.Lineups)
            {
                // initialize an array of listviewitems
                var listViewItems = new List<ListViewItem>();

                // record lineup unique id
                _lineups.Add(clientLineup.Lineup);

                // process the lineup map
                if (!clientLineup.IsDeleted)
                {
                    // set the include globe state with checkmark and update label
                    if (((Config.IncludedLineup != null) && Config.IncludedLineup.Contains(clientLineup.Lineup)) || _newLineups.Contains(clientLineup.Lineup)) IncludeLineup(processedLineup);
                    if (processedLineup < listViews.Length) listViewLabels[processedLineup].Text = $"{clientLineup.Name} ({clientLineup.Location})";

                    // request the lineup's station maps
                    var lineupMap = sdApi.SdGetStationMaps(clientLineup.Lineup);
                    if (lineupMap == null) continue;

                    // match station with mapping for lineup number and subnumbers
                    foreach (var station in lineupMap.Stations)
                    {
                        if (station == null) continue;
                        var stationLanguage = string.Empty;
                        if (station.BroadcastLanguage != null)
                        {
                            stationLanguage = station.BroadcastLanguage[0];
                            if (station.BroadcastLanguage.Length > 1)
                            {
                                foreach (var lang in station.BroadcastLanguage)
                                {
                                    if (lang.Equals("en", StringComparison.OrdinalIgnoreCase)) continue;
                                    stationLanguage = lang;
                                    break;
                                }
                            }
                        }

                        // use hashset to make sure we don't duplicate channel entries
                        var channelNumbers = new HashSet<string>();
                        _allStationIDs.Add(station.StationId);

                        foreach (var map in lineupMap.Map)
                        {
                            if (!map.StationId.Equals(station.StationId)) continue;

                            var number = -1;
                            var subnumber = 0;

                            // create what will be the ListViewItem Tag
                            var dlStation = new SdChannelDownload
                            {
                                CallSign = station.Callsign,
                                StationId = station.StationId,
                                CustomCallSign = CheckCustomCallsign(station.StationId),
                                CustomServiceName = CheckCustomServicename(station.StationId),
                                HdOverride = CheckHdOverride(station.StationId),
                                SdOverride = CheckSdOverride(station.StationId)
                            };

                            // QAM
                            if (map.ChannelMajor > 0)
                            {
                                number = map.ChannelMajor;
                                subnumber = map.ChannelMinor;
                            }

                            // ATSC or NTSC
                            else if (map.AtscMajor > 0)
                            {
                                number = map.AtscMajor;
                                subnumber = map.AtscMinor;
                            }
                            else if (map.UhfVhf > 0)
                            {
                                number = map.UhfVhf;
                            }

                            // Cable or Satellite
                            else if (!string.IsNullOrEmpty(map.Channel))
                            {
                                //subnumber = 0;
                                if (Regex.Match(map.Channel, @"[A-Za-z]{1}[\d]{4}").Length > 0)
                                {
                                    // 4dtv has channels starting with 2 character satellite identifier
                                    number = int.Parse(map.Channel.Substring(2));
                                }
                                else if (!int.TryParse(Regex.Replace(map.Channel, "[^0-9.]", ""), out number))
                                {
                                    // if channel number is not a whole number, must be a decimal number
                                    var numbers = Regex.Replace(map.Channel, "[^0-9.]", "").Replace('_', '.').Replace("-", ".").Split('.');
                                    if (numbers.Length == 2)
                                    {
                                        number = int.Parse(numbers[0]);
                                        subnumber = int.Parse(numbers[1]);
                                    }
                                }
                            }

                            var channelNumber = number + ((subnumber > 0) ? "." + subnumber : null);
                            if (!channelNumbers.Add(channelNumber + ":" + station.StationId)) continue;
                            listViewItems.Add(AddListviewChannel(dlStation.CustomCallSign ?? station.Callsign, channelNumber, station.StationId, dlStation.CustomServiceName ?? station.Name, dlStation, stationLanguage));

                            // URIs to channel logos are here ... store them for use if needed
                            string dummy;
                            if ((station.StationLogos != null) && station.StationLogos.Count > 0)
                            {
                                for (var i = 0; i < station.StationLogos.Count; ++i)
                                {
                                    if (!_sdlogos.TryGetValue(station.Callsign + "-" + (i + 1), out dummy))
                                    {
                                        _sdlogos.Add(station.Callsign + "-" + (i + 1), station.StationLogos[i].Url);
                                    }
                                }
                            }
                            else if ((station.Logo != null) && !_sdlogos.TryGetValue(station.Callsign, out dummy))
                            {
                                _sdlogos.Add(station.Callsign, station.Logo.Url);
                            }
                        }
                    }

                    // add all the listview items to the listview
                    if (listViewItems.Count > 0 && processedLineup < listViews.Length)
                    {
                        listViews[processedLineup].Items.AddRange(listViewItems.ToArray());
                        toolStrips[processedLineup].Enabled = true;
                    }
                }
                else if (processedLineup < listViews.Length)
                {
                    listViewLabels[processedLineup].Text = $"[Deleted] ({clientLineup.Lineup})";
                }
                ++processedLineup;
            }
        }
        private ListViewItem AddListviewChannel(string callsign, string number, string stationid, string name, SdChannelDownload dlstation, string language)
        {
            var channelIsNew = !_includedStations.Contains(stationid) && !_excludedStations.Contains(stationid) && (_includedStations.Count + _excludedStations.Count > 0);
            return new ListViewItem(
                new[]
                {
                    callsign,
                    number,
                    stationid,
                    name
                })
            {
                Tag = dlstation,
                Checked = _includedStations.Contains(stationid) || (Config.AutoAddNew && !_excludedStations.Contains(stationid) && !btnClientLineups.Enabled),
                ImageKey = GetLanguageIcon(language),
                BackColor = channelIsNew ? Color.Pink : default
            };
        }
        private string GetLanguageIcon(string language)
        {
            if (string.IsNullOrEmpty(language)) language = "zz";

            language = language.ToLower().Substring(0, 2);
            if (_imageList.Images.Keys.Contains(language))
            {
                return language;
            }

            _imageList.Images.Add(language, DrawText(language, new Font(lvL1Lineup.Font.Name, 16, FontStyle.Bold, lvL1Lineup.Font.Unit)));
            return language;
        }
        private static Image DrawText(string text, Font font)
        {
            byte[] textBytes;
            try
            {
                textBytes = Encoding.ASCII.GetBytes(new CultureInfo(text).ThreeLetterISOLanguageName);
            }
            catch
            {
                textBytes = Encoding.ASCII.GetBytes("zaa");
                //Logger.WriteError(string.Format("{0} not supported.", text));
            }

            // establish backColor based on language identifier
            const int bitWeight = 8;
            const int colorBase = 0x7A - 0xFF / bitWeight;
            if (textBytes.Length < 3) { Array.Resize(ref textBytes, 3); textBytes[2] = textBytes[0]; }
            var backColor = Color.FromArgb((textBytes[0] - colorBase) * bitWeight,
                                             (textBytes[1] - colorBase) * bitWeight,
                                             (textBytes[2] - colorBase) * bitWeight);

            // determine best textColor
            const int threshold = 140;
            var brightness = (int)Math.Sqrt(backColor.R * backColor.R * 0.299 +
                                            backColor.G * backColor.G * 0.587 +
                                            backColor.B * backColor.B * 0.114);
            var textColor = (brightness < threshold) ? Color.White : Color.Black;

            // determine size of text with font
            SizeF textSize;
            using (Image img = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(img))
                {
                    textSize = g.MeasureString(text, font);
                }
            }

            // create the text image in a box
            Image image = new Bitmap((int)textSize.Width + 1, (int)textSize.Height + 1);
            using (var g = Graphics.FromImage(image))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                // paint the background
                g.Clear(backColor);

                // draw a box around the border
                g.DrawRectangle(Pens.Black, 0, 0, image.Width - 2, image.Height - 2);

                // draw the text in the box
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(text, font, textBrush, 0, 0);
                }
            }

            return image;
        }
        #endregion

        #region ========== ListView Tab Widgets ==========

        private bool _disableItemCheck;
        private void btnAll_Click(object sender, EventArgs e)
        {
            ToolStripButton[] btn = { btnL1All, btnL2All, btnL3All, btnL4All };
            ListView[] lv = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };

            // determine which lineup the button click is for
            for (var i = 0; i < btn.Length; ++i)
            {
                if (!btn[i].Equals(sender)) continue;
                // escape if the listview is not enabled
                if (!lv[i].Enabled) break;

                _disableItemCheck = true;
                var addStations = new HashSet<string>();

                // scan through lineup and check any unchecked items
                foreach (ListViewItem item in lv[i].Items)
                {
                    if (item.Checked) continue;
                    var stationId = item.SubItems[2].Text;
                    addStations.Add(stationId);
                    _includedStations.Add(stationId);
                    _excludedStations.Remove(stationId);
                    item.Checked = true;
                }

                // if there were no items to check, stop
                if (addStations.Count == 0) break;

                // scan through all lineups and check the affected stations
                foreach (var listview in lv)
                {
                    foreach (ListViewItem item in listview.Items)
                    {
                        if (!item.Checked && addStations.Contains(item.SubItems[2].Text)) item.Checked = true;
                    }
                }

                _disableItemCheck = false;
            }
        }
        private void btnNone_Click(object sender, EventArgs e)
        {
            ToolStripButton[] btn = { btnL1None, btnL2None, btnL3None, btnL4None };
            ListView[] lv = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };

            // determine which lineup the button click is for
            for (var i = 0; i < btn.Length; ++i)
            {
                if (!btn[i].Equals(sender)) continue;
                // escape if listview is not enabled
                if (!lv[i].Enabled) break;

                _disableItemCheck = true;
                var removeStations = new HashSet<string>();

                // scan through lineup and uncheck any checked items
                foreach (ListViewItem item in lv[i].Items)
                {
                    if (!item.Checked) continue;
                    var stationId = item.SubItems[2].Text;
                    removeStations.Add(stationId);
                    _includedStations.Remove(stationId);
                    _excludedStations.Add(stationId);
                    item.Checked = false;
                }

                // if there were no items to uncheck, stop
                if (removeStations.Count == 0) break;

                // scan through all lineups and uncheck the affected stations
                foreach (var listview in lv)
                {
                    foreach (ListViewItem item in listview.Items)
                    {
                        if (item.Checked && removeStations.Contains(item.SubItems[2].Text)) item.Checked = false;
                    }
                }

                _disableItemCheck = false;
            }
        }
        private void lvLineup_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_disableItemCheck) return;

            // Ignore item checks when selecting multiple items
            if ((ModifierKeys & (Keys.Shift | Keys.Control)) > 0)
            {
                e.NewValue = e.CurrentValue;
            }

            // temporarily disable item check
            _disableItemCheck = true;

            ListView[] listviews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
            var stationId = string.Empty;

            foreach (var listview in listviews)
            {
                // if not the sender, go to next listview
                if (!sender.Equals(listview)) continue;

                // if listview is not focused, then this is draw event, not click event
                if (!listview.Focused)
                {
                    _disableItemCheck = false;
                    return;
                }

                // determine what station id it is
                stationId = listview.Items[e.Index].SubItems[(int)LineupColumn.StationID].Text;

                // update the include/exclude hashsets
                if (e.NewValue == CheckState.Checked)
                {
                    _includedStations.Add(stationId);
                    _excludedStations.Remove(stationId);
                }
                else
                {
                    _includedStations.Remove(stationId);
                    _excludedStations.Add(stationId);
                }
                break;
            }

            // scan all the listviews to change check state
            foreach (var listview in listviews)
            {
                foreach (ListViewItem item in listview.Items)
                {
                    if ((item != null) && item.SubItems[(int)LineupColumn.StationID].Text.Equals(stationId))
                    {
                        item.Checked = (e.NewValue == CheckState.Checked);
                    }
                }
            }

            // re-enable item check
            _disableItemCheck = false;
        }
        #endregion

        #region ========== Buttons & Links ==========
        private void RefreshConfiguration()
        {
            ToolStripMenuItem[] items = { L1includeToolStripMenuItem, L2includeToolStripMenuItem, L3includeToolStripMenuItem, L4includeToolStripMenuItem };
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
            var stations = new HashSet<string>();
            var expectedStationIds = new HashSet<string>();

            // reset included lineups and stations
            Config.IncludedLineup = new List<string>();
            Config.StationId = new List<SdChannelDownload>();

            // add all station includes and excludes to configuration file
            for (var i = 0; i < items.Length; ++i)
            {
                // flag that lineup is included for download and add to inluded lineups
                var included = items[i].Checked;
                if (included) Config.IncludedLineup.Add(_lineups[i]);

                foreach (ListViewItem listviewitem in listViews[i].Items)
                {
                    var station = (SdChannelDownload)listviewitem.Tag;
                    station.StationId = station.StationId.Replace("-", "");

                    if (!stations.Contains(listviewitem.SubItems[(int)LineupColumn.StationID].Text))
                    {
                        stations.Add(listviewitem.SubItems[(int)LineupColumn.StationID].Text);
                        if (!listviewitem.Checked)
                        {
                            station.StationId = "-" + station.StationId;
                        }
                        Config.StationId.Add(station);
                    }

                    if (included && listviewitem.Checked && !expectedStationIds.Contains(station.StationId))
                    {
                        expectedStationIds.Add(station.StationId);
                    }
                }
            }
            if (L5includeToolStripMenuItem.Checked && (btnCustomLineup.DropDown.Items.Count > 0))
            {
                Config.IncludedLineup.Add((string)btnCustomLineup.Tag);
                foreach (ListViewItem item in lvL5Lineup.Items)
                {
                    if (item.Checked) expectedStationIds.Add(item.SubItems[(int)LineupColumn.StationID].Text);
                }
            }
            Config.ExpectedServicecount = expectedStationIds.Count;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            RefreshConfiguration();

            // sanity checks
            if ((Config.ExpectedServicecount  == 0) && (sender != null))
            {
                if (MessageBox.Show("There are no INCLUDED lineups and/or no stations selected for download.\n\nDo you wish to commit these changes?",
                                    "No Stations to Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            else if ((Config.ExpectedServicecount != _oldConfig.ExpectedServicecount) && (Config.ExpectedServicecount > 0))
            {
                var prompt = $"The number of stations to download has {((_oldConfig.ExpectedServicecount > Config.ExpectedServicecount) ? "decreased" : "increased")} from {_oldConfig.ExpectedServicecount} to {Config.ExpectedServicecount} from the previous configuration.\n\nDo you wish to commit these changes?";
                if (MessageBox.Show(prompt, "Change in Expected Services Count", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            // commit the updated config file if there are changes
            if (_newLogin || !Config.Equals(_oldConfig))
            {
                // save the file and determine flags for execution if selected
                try
                {
                    // save configuration file
                    using (var stream = new StreamWriter(Helper.Epg123CfgPath, false, Encoding.UTF8))
                    {
                        Config.Version = Helper.Epg123Version;
                        var serializer = new XmlSerializer(typeof(epgConfig));
                        var ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        TextWriter writer = stream;
                        serializer.Serialize(writer, Config, ns);
                    }
                    _oldConfig = Config.Clone();

                    _newLogin = false;
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex.Message);
                }
            }

            Import = cbImport.Checked;
            Match = cbAutomatch.Checked;
            if ((sender != null) && (Execute = sender.Equals(btnExecute))) Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnClientConfig_Click(object sender, EventArgs e)
        {
            var gui = new frmLineups();
            gui.ShowDialog();
            if (gui.Cancel) return;
            _newLineups = gui.NewLineups;
            BuildLineupTabs();
        }
        private void btnViewLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(Helper.Epg123TraceLogPath))
            {
                Process.Start(Helper.Epg123TraceLogPath);
            }
        }
        private void btnClearCache_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (Directory.Exists(Helper.Epg123CacheFolder))
            {
                var failed = Directory.GetFiles(Helper.Epg123CacheFolder).Count(file => !Helper.DeleteFile(file));
                if (failed > 0)
                {
                    Logger.WriteError("Failed to delete all files from the cache folder.");
                }
            }
            Helper.DeleteFile(Helper.Epg123GuideImagesXmlPath);
            Helper.DeleteFile(Helper.Epg123MmuiplusJsonPath);

            MessageBox.Show("Cache files have been removed and will be rebuilt on next update.", "Operation Complete", MessageBoxButtons.OK);
            Cursor = Cursors.Arrow;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start("http://epg123.garyan2.net");
        }
        #endregion

        #region ========== Column Sorters ==========
        private void AssignColumnSorters()
        {
            ListView[] listviews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup, lvL5Lineup };
            foreach (var listview in listviews)
            {
                // create and assign listview item sorter
                listview.ListViewItemSorter = new ListViewColumnSorter
                {
                    SortColumn = (int)LineupColumn.CallSign,
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

        #region ========== Include/Exclude Stations and Lineups ==========

        readonly HashSet<string> _excludedStations = new HashSet<string>();
        readonly HashSet<string> _includedStations = new HashSet<string>();
        private void PopulateIncludedExcludedStations(IEnumerable<SdChannelDownload> list)
        {
            foreach (var station in list)
            {
                if (station.StationId.StartsWith("-"))
                {
                    _excludedStations.Add(station.StationId.Replace("-", ""));
                }
                else
                {
                    _includedStations.Add(station.StationId);
                }
            }
        }
        private void LineupEnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image[] stopLight = { Resources.GreenLight.ToBitmap(), Resources.RedLight.ToBitmap() };
            ToolStripDropDownButton[] btns = { L1IncludeExclude, L2IncludeExclude, L3IncludeExclude, L4IncludeExclude, L5IncludeExclude };
            ToolStripMenuItem[] items = { L1includeToolStripMenuItem , L2includeToolStripMenuItem , L3includeToolStripMenuItem , L4includeToolStripMenuItem , L5includeToolStripMenuItem,
                                          L1excludeToolStripMenuItem , L2excludeToolStripMenuItem , L3excludeToolStripMenuItem , L4excludeToolStripMenuItem , L5excludeToolStripMenuItem };
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup, lvL5Lineup };
            var mid = items.Length / 2;
            for (var i = 0; i < items.Length; ++i)
            {
                // determine which menuitem was clicked
                if (!items[i].Equals((ToolStripMenuItem) sender)) continue;
                items[i].Checked = true;
                items[(i + mid) % items.Length].Checked = false;
                btns[i % mid].Image = stopLight[i / btns.Length];

                listViews[i % listViews.Length].Enabled = (i < 5);
                listViews[i % listViews.Length].ForeColor = (i < 5) ? DefaultForeColor : Color.LightGray;
                break;
            }
        }
        private void IncludeLineup(int lineup)
        {
            ToolStripDropDownButton[] btns = { L1IncludeExclude, L2IncludeExclude, L3IncludeExclude, L4IncludeExclude, L5IncludeExclude };
            ToolStripMenuItem[] items = { L1includeToolStripMenuItem , L2includeToolStripMenuItem , L3includeToolStripMenuItem , L4includeToolStripMenuItem , L5includeToolStripMenuItem,
                                          L1excludeToolStripMenuItem , L2excludeToolStripMenuItem , L3excludeToolStripMenuItem , L4excludeToolStripMenuItem , L5excludeToolStripMenuItem };
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup, lvL5Lineup };
            if (lineup >= btns.Length) return;

            var mid = items.Length / 2;
            btns[lineup].Image = Resources.GreenLight.ToBitmap();
            listViews[lineup].Enabled = items[lineup].Checked = true;
            items[lineup + mid].Checked = false;
            listViews[lineup].ForeColor = DefaultForeColor;
        }
        private void ExcludeLineup(int lineup)
        {
            ToolStripDropDownButton[] btns = { L1IncludeExclude, L2IncludeExclude, L3IncludeExclude, L4IncludeExclude, L5IncludeExclude };
            ToolStripMenuItem[] items = { L1includeToolStripMenuItem , L2includeToolStripMenuItem , L3includeToolStripMenuItem , L4includeToolStripMenuItem , L5includeToolStripMenuItem,
                                          L1excludeToolStripMenuItem , L2excludeToolStripMenuItem , L3excludeToolStripMenuItem , L4excludeToolStripMenuItem , L5excludeToolStripMenuItem};
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup, lvL5Lineup };
            if (lineup >= btns.Length) return;

            var mid = items.Length / 2;
            btns[lineup].Image = Resources.RedLight.ToBitmap();
            listViews[lineup].Enabled = items[lineup].Checked = false;
            items[lineup + mid].Checked = true;
            listViews[lineup].ForeColor = Color.LightGray;
        }
        #endregion

        #region ===== Configuration Tabs =====
        #region ========== TAB: XMLTV ==========
        private void ckXmltvConfigs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(cbXmltv))
            {
                Config.CreateXmltv = ckChannelNumbers.Enabled = ckChannelLogos.Enabled = ckXmltvFillerData.Enabled = ckXmltvExtendedInfo.Enabled =
                    lblXmltvOutput.Enabled = tbXmltvOutput.Enabled = btnXmltvOutput.Enabled = lblXmltvLogosNote.Enabled = cbXmltv.Checked;
                if (!cbXmltv.Checked)
                {
                    ckUrlLogos.Enabled = ckLocalLogos.Enabled = ckSubstitutePath.Enabled = txtSubstitutePath.Enabled = false;
                    numFillerDuration.Enabled = lblFillerDuration.Enabled = lblFillerDescription.Enabled = rtbFillerDescription.Enabled = false;
                }
                else
                {
                    ckUrlLogos.Enabled = ckLocalLogos.Enabled = ckChannelLogos.Checked;
                    ckSubstitutePath.Enabled = (ckLocalLogos.Checked && ckChannelLogos.Checked);
                    txtSubstitutePath.Enabled = (ckSubstitutePath.Checked && ckLocalLogos.Checked && ckChannelLogos.Checked);
                    numFillerDuration.Enabled = lblFillerDuration.Enabled = lblFillerDescription.Enabled = rtbFillerDescription.Enabled = ckXmltvFillerData.Checked;
                }
            }
            else if (sender.Equals(ckChannelNumbers))
            {
                Config.XmltvIncludeChannelNumbers = ckChannelNumbers.Checked;
            }
            else if (sender.Equals(ckChannelLogos))
            {
                ckUrlLogos.Enabled = ckLocalLogos.Enabled = ckChannelLogos.Checked;
                ckSubstitutePath.Enabled = ckLocalLogos.Checked && ckChannelLogos.Checked;
                txtSubstitutePath.Enabled = ckSubstitutePath.Checked && ckLocalLogos.Checked && ckChannelLogos.Checked;

                if (!ckChannelLogos.Checked) Config.XmltvIncludeChannelLogos = "false";

                Config.XmltvIncludeChannelLogos = !ckChannelLogos.Checked ? "false" : ckUrlLogos.Checked ? "url" : !ckSubstitutePath.Checked ? "local" : "substitute";
            }
            else if (sender.Equals(ckUrlLogos))
            {
                ckLocalLogos.Checked = !ckUrlLogos.Checked;
                Config.XmltvIncludeChannelLogos = "url";
            }
            else if (sender.Equals(ckLocalLogos))
            {
                ckUrlLogos.Checked = !ckLocalLogos.Checked;
                ckSubstitutePath.Enabled = ckLocalLogos.Checked && cbXmltv.Checked;
                txtSubstitutePath.Enabled = ckSubstitutePath.Checked && ckLocalLogos.Checked && cbXmltv.Checked;
                if (!ckUrlLogos.Checked)
                {
                    Config.XmltvIncludeChannelLogos = (ckSubstitutePath.Checked && ckLocalLogos.Checked) ? "substitute" : "local";
                }
            }
            else if (sender.Equals(ckSubstitutePath))
            {
                txtSubstitutePath.Enabled = ckSubstitutePath.Checked && ckLocalLogos.Checked && cbXmltv.Checked;
                if (!Config.XmltvIncludeChannelLogos.Equals("url") && !Config.XmltvIncludeChannelLogos.Equals("false"))
                {
                    Config.XmltvIncludeChannelLogos = (ckSubstitutePath.Checked && ckLocalLogos.Checked) ? "substitute" : "local";
                }
            }
            else if (sender.Equals(txtSubstitutePath))
            {
                Config.XmltvLogoSubstitutePath = txtSubstitutePath.Text;
            }
            else if (sender.Equals(ckXmltvFillerData))
            {
                numFillerDuration.Enabled = lblFillerDuration.Enabled = lblFillerDescription.Enabled = rtbFillerDescription.Enabled = ckXmltvFillerData.Checked && cbXmltv.Checked;
                Config.XmltvAddFillerData = ckXmltvFillerData.Checked;
            }
            else if (sender.Equals(numFillerDuration))
            {
                Config.XmltvFillerProgramLength = (int)numFillerDuration.Value;
            }
            else if (sender.Equals(rtbFillerDescription))
            {
                Config.XmltvFillerProgramDescription = rtbFillerDescription.Text;
            }
            else if (sender.Equals(ckXmltvExtendedInfo))
            {
                Config.XmltvExtendedInfoInTitleDescriptions = ckXmltvExtendedInfo.Checked;
            }
        }
        private void btnXmltvOutput_Click(object sender, EventArgs e)
        {
            var fileInfo = new FileInfo(tbXmltvOutput.Text);
            saveFileDialog1.InitialDirectory = fileInfo.DirectoryName;
            saveFileDialog1.FileName = fileInfo.Name;

            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                Config.XmltvOutputFile = tbXmltvOutput.Text = saveFileDialog1.FileName;
            }
        }
        #endregion
        #region ========== TAB: Images ==========

        private Dictionary<string, string> _sdlogos = new Dictionary<string, string>();
        private void imageConfigs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(cbSeriesPosterArt))
            {
                Config.SeriesPosterArt = cbSeriesPosterArt.Checked;
            }
            else if (sender.Equals(cbTMDb))
            {
                Config.TMDbCoverArt = cbTMDb.Checked;
            }
            else if (sender.Equals(cbSdLogos))
            {
                Config.IncludeSdLogos = lblPreferredLogos.Enabled = cmbPreferredLogos.Enabled = lblAlternateLogos.Enabled = cmbAlternateLogos.Enabled = cbSdLogos.Checked;
            }
            else if (sender.Equals(cmbPreferredLogos))
            {
                Config.PreferredLogoStyle = ((Helper.PreferredLogos)cmbPreferredLogos.SelectedIndex).ToString();
                if (Config.PreferredLogoStyle.Equals("none"))
                {
                    cmbAlternateLogos.SelectedIndex = cmbAlternateLogos.Items.Count - 1;
                }
            }
            else if (sender.Equals(cmbAlternateLogos))
            {
                Config.AlternateLogoStyle = ((Helper.PreferredLogos)cmbAlternateLogos.SelectedIndex).ToString();
            }
        }
        private void btnSdLogos_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            // open form and handle the sd logo downloads and image crop/resize
            var dl = new frmDownloadLogos(_sdlogos);
            dl.ShowDialog();

            Cursor = Cursors.Arrow;
        }
        #endregion
        #region ========== TAB: Config ==========
        private void configs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(numDays))
            {
                Config.DaysToDownload = (int)numDays.Value;
            }
            else if (sender.Equals(cbTVDB))
            {
                Config.TheTvdbNumbers = cbTVDB.Checked;
            }
            else if (sender.Equals(cbPrefixTitle))
            {
                Config.PrefixEpisodeTitle = cbPrefixTitle.Checked;
                cbAlternateSEFormat.Enabled = (Config.PrefixEpisodeTitle || Config.PrefixEpisodeDescription);
            }
            else if (sender.Equals(cbPrefixDescription))
            {
                Config.PrefixEpisodeDescription = cbPrefixDescription.Checked;
                cbAlternateSEFormat.Enabled = (Config.PrefixEpisodeTitle || Config.PrefixEpisodeDescription);
            }
            else if (sender.Equals(cbAlternateSEFormat))
            {
                Config.AlternateSEFormat = cbAlternateSEFormat.Checked;
            }
            else if (sender.Equals(cbAppendDescription))
            {
                Config.AppendEpisodeDesc = cbAppendDescription.Checked;
            }
            else if (sender.Equals(cbOadOverride))
            {
                Config.OadOverride = cbOadOverride.Checked;
            }
            else if (sender.Equals(cbAddNewStations))
            {
                Config.AutoAddNew = cbAddNewStations.Checked;
            }
            else if (sender.Equals(cbBrandLogo))
            {
                if (cbBrandLogo.Checked)
                {
                    if (!Config.PreferredLogoStyle.Equals("light") && !Config.AlternateLogoStyle.Equals("light"))
                    {
                        Config.BrandLogoImage = "light";
                    }
                    else Config.BrandLogoImage = "dark";
                }
                else Config.BrandLogoImage = "none";
            }
            else if (sender.Equals(cbModernMedia))
            {
                Config.ModernMediaUiPlusSupport = cbModernMedia.Checked;
            }
        }
        #endregion
        #region ========== TAB: Task ==========
        private void configTask_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(cbImport))
            {
                Config.AutoImport = cbAutomatch.Enabled = cbImport.Checked;
            }
            else if (sender.Equals(cbAutomatch))
            {
                Config.Automatch = cbAutomatch.Checked;
            }
        }
        #endregion
        #endregion

        private void lvL5Lineup_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_lockCustomCheckboxes && lvL5Lineup.Focused) e.NewValue = e.CurrentValue;
        }

        private void btnCustomLineup_ButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show($"This feature is not yet implemented. You can manually edit the custom lineup file \"{Helper.Epg123CustomLineupsXmlPath}\".",
                "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lineupMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
            listViews[tabLineups.SelectedIndex].SelectedIndices.Clear();
            if (listViews[tabLineups.SelectedIndex].Items.Count == 0) { e.Cancel = true;
            }
        }

        private void copyToClipboardMenuItem_Click(object sender, EventArgs e)
        {
            ListView[] listViews = { lvL1Lineup, lvL2Lineup, lvL3Lineup, lvL4Lineup };
            ToolStripLabel[] labels = { lblL1Lineup, lblL2Lineup, lblL3Lineup, lblL4Lineup };

            var textToAdd = $"Lineup: {labels[tabLineups.SelectedIndex].Text}\r\n";
            textToAdd += "Call Sign\tChannel\tStationID\tName\r\n";
            textToAdd = listViews[tabLineups.SelectedIndex].Items.Cast<ListViewItem>().Aggregate(textToAdd, (current, listViewItem) => current + $"{listViewItem.SubItems[0].Text}\t{listViewItem.SubItems[1].Text}\t{listViewItem.SubItems[2].Text}\t{listViewItem.SubItems[3].Text}\r\n");
            Clipboard.SetText(textToAdd);
        }

        private bool CheckHdOverride(string stationId)
        {
            return (from station in Config.StationId ?? new List<SdChannelDownload>() where station.StationId == stationId select station.HdOverride).FirstOrDefault();
        }
        private bool CheckSdOverride(string stationId)
        {
            return (from station in Config.StationId ?? new List<SdChannelDownload>() where station.StationId == stationId select station.SdOverride).FirstOrDefault();
        }

        private string CheckCustomCallsign(string stationId)
        {
            return (from station in Config.StationId where station.StationId == stationId && !string.IsNullOrEmpty(station.CustomCallSign) select station.CustomCallSign).FirstOrDefault();
        }
        private string CheckCustomServicename(string stationId)
        {
            return (from station in Config.StationId where station.StationId == stationId && !string.IsNullOrEmpty(station.CustomServiceName) select station.CustomServiceName).FirstOrDefault();
        }

        private void tabTask_Enter(object sender, EventArgs e)
        {
            UpdateTaskPanel(true);
        }
    }
}