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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private readonly epgTaskScheduler _task = new epgTaskScheduler();
        private readonly ImageList _imageList = new ImageList();
        private bool _newLogin = true;
        public bool RestartAsAdmin;

        public epgConfig Config = new epgConfig();
        private epgConfig _oldConfig = new epgConfig();
        public bool Execute;
        public bool Import;
        public bool Match;
        private readonly double _dpiScaleFactor = 1.0;
        private bool _lockCustomCheckboxes;
        private Thread _logoThread;

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
                    ListView[] listviews = { lvLineupChannels };
                    foreach (var listview in listviews)
                    {
                        foreach (ColumnHeader column in listview.Columns)
                        {
                            column.Width = (int)(column.Width * _dpiScaleFactor);
                        }
                    }
                }
            }

            toolStrip6.ImageScalingSize = new Size((int)(_dpiScaleFactor * 16), (int)(_dpiScaleFactor * 16));
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
            lvL5Lineup.SmallImageList = lvL5Lineup.LargeImageList = _imageList;
            lvLineupChannels.SmallImageList = lvLineupChannels.LargeImageList = _imageList;

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

            // double buffer list views
            lvLineupChannels.DoubleBuffered(true);
            lvL5Lineup.DoubleBuffered(true);

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

            // end the thread if still running
            if (_logoThread?.IsAlive ?? false)
            {
                _logoThread.Interrupt();
                if (!_logoThread.Join(100)) _logoThread.Abort();
            }
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
                    cbSeriesWsArt.Checked = Config.SeriesWsArt;
                    cbModernMedia.Checked = Config.ModernMediaUiPlusSupport;
                    cbBrandLogo.Checked = !Config.BrandLogoImage.Equals("none");
                    cmbPreferredLogos.SelectedIndex = (int)(Helper.PreferredLogos)Enum.Parse(typeof(Helper.PreferredLogos), Config.PreferredLogoStyle, true);
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
                    cbNoCastCrew.Checked = Config.ExcludeCastAndCrew;
                    cbXmltvSingleImage.Checked = Config.XmltvSingleImage;
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
            ListView[] listViews = { lvLineupChannels, lvL5Lineup };

            // focus on first tab
            tabLineups.SelectedIndex = 0;

            // clear lineup listviews and title
            foreach (var t in listViews)
            {
                t.Items.Clear();
                t.ListViewItemSorter = null;
            }

            // populate the listviews with channels/services
            BuildLineupsAndStations();
            BuildCustomListViewChannels();

            // assign a listviewcolumnsorter to a listview
            AssignColumnSorters();
        }
        public void BuildLineupsAndStations()
        {
            // reset lineups, stations, and combobox
            comboLineups.Items.Clear();
            _allAvailableStations.Clear();

            // retrieve lineups from SD
            var clientLineups = sdApi.SdGetLineups();
            if (clientLineups == null) return;

            foreach (var clientLineup in clientLineups.Lineups)
            {
                // request the lineup's station maps
                var lineupMap = sdApi.SdGetStationMaps(clientLineup.Lineup);
                if (lineupMap == null) continue;

                // build the stations
                foreach (var station in lineupMap.Stations)
                {
                    _allAvailableStations.Add(station.StationId);
                    if (_allStations.ContainsKey(station.StationId)) continue;

                    var configStation = Config.StationId.SingleOrDefault(arg =>
                        arg.StationId.Replace("-", "").Equals(station.StationId));
                    _allStations.Add(station.StationId, new myStation(station)
                    {
                        CustomCallsign = configStation?.CustomCallSign,
                        CustomServiceName = configStation?.CustomServiceName,
                        HDOverride = (configStation?.HdOverride ?? false),
                        SDOverride = (configStation?.SdOverride ?? false),
                        Include = (!configStation?.StationId.StartsWith("-") ?? Config.AutoAddNew),
                    });
                }

                // build the lineup
                if (_allLineups.ContainsKey(clientLineup.Lineup)) continue;
                _allLineups.Add(clientLineup.Lineup, new myLineup(clientLineup)
                {
                    Include = (Config.IncludedLineup?.Contains(clientLineup.Lineup) ?? false),
                    Channels = lineupMap.Map.ToList()
                });
            }
            GetAllServiceLogos(false);

            // cleanup dead lineups and populate combo box
            foreach (var lineup in _allLineups)
            {
                if (clientLineups.Lineups.SingleOrDefault(arg => arg.Lineup.Equals(lineup.Key)) == null)
                {
                    lineup.Value.Include = false;
                    continue;
                }
                comboLineups.Items.Add(lineup.Value);
            }

            labelLineupCounts.Text = $"subscribed to {comboLineups.Items.Count} out of {sdApi.MaxLineups} allowed lineups";

            if (comboLineups.Items.Count > 0) comboLineups.SelectedIndex = 0;
        }
        public string GetChannelNumber(SdLineupMap map)
        {
            var number = -1;
            var subnumber = 0;

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
                // subnumber = 0;
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

            return number + (subnumber > 0 ? "." + subnumber : null);
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

            if (_allAvailableStations.Contains(station.StationId) || !_allAvailableStations.Contains(station.Alternate)) return station;

            ret.Callsign = _allStations[station.Alternate].Callsign;
            ret.Name = _allStations[station.Alternate].Name;
            ret.StationId = _allStations[station.Alternate].StationId;

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
                    Checked = _allAvailableStations.Contains(station.StationId) || _allAvailableStations.Contains(station.Alternate),
                    ForeColor = _allAvailableStations.Contains(stationItem.StationId) ? SystemColors.WindowText : SystemColors.GrayText
                });
            }

            _lockCustomCheckboxes = false;
            lvL5Lineup.Items.AddRange(items.ToArray());
            _lockCustomCheckboxes = true;
        }

        private string GetLanguageIcon(string language)
        {
            if (string.IsNullOrEmpty(language)) language = "zz";

            language = language.ToLower().Substring(0, 2);
            if (_imageList.Images.Keys.Contains(language))
            {
                return language;
            }

            _imageList.Images.Add(language, DrawText(language, new Font(lvLineupChannels.Font.Name, 16, FontStyle.Bold, lvLineupChannels.Font.Unit)));
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

        #region ========== Buttons & Links ==========
        private void RefreshConfiguration()
        {
            var stationsToExclude = new HashSet<string>();
            var stationsToDownload = new HashSet<string>();

            // reset included lineups and stations to repopulate
            Config.IncludedLineup = new List<string>();
            Config.StationId = new List<SdChannelDownload>();
            foreach (var lineup in _allLineups.Where(lineup => lineup.Value.Include))
            {
                Config.IncludedLineup.Add(lineup.Key);
                foreach (var station in lineup.Value.Channels)
                {
                    if (_allAvailableStations.Contains(station.StationId) && _allStations[station.StationId].Include)
                    {
                        stationsToDownload.Add(station.StationId);
                    }
                }
            }
            if (L5includeToolStripMenuItem.Checked && (btnCustomLineup.DropDown.Items.Count > 0))
            {
                Config.IncludedLineup.Add((string)btnCustomLineup.Tag);
                foreach (ListViewItem item in lvL5Lineup.Items)
                {
                    if (item.Checked) stationsToDownload.Add(item.SubItems[(int)LineupColumn.StationID].Text);
                }
            }

            // populate the excluded stations
            foreach (var station in _allAvailableStations)
            {
                if (!stationsToDownload.Contains(station)) stationsToExclude.Add(station);
            }

            // add all the included stations
            foreach (var include in stationsToDownload)
            {
                var station = _allStations[include];
                Config.StationId.Add(new SdChannelDownload
                {
                    HdOverride = station.HDOverride,
                    SdOverride = station.SDOverride,
                    CallSign = station.Callsign,
                    CustomCallSign = station.CustomCallsign,
                    CustomServiceName = station.CustomServiceName,
                    StationId = include
                });
            }

            // add all the excluded stations
            foreach (var exclude in stationsToExclude)
            {
                var station = _allStations[exclude];
                Config.StationId.Add(new SdChannelDownload
                {
                    HdOverride = station.HDOverride,
                    SdOverride = station.SDOverride,
                    CallSign = station.Callsign,
                    CustomCallSign = station.CustomCallsign,
                    CustomServiceName = station.CustomServiceName,
                    StationId = $"-{exclude}"
                });
            }

            Config.ExpectedServicecount = stationsToDownload.Count;
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
            BuildLineupTabs();

            foreach (var lineup in gui.NewLineups)
            {
                if (!_allLineups.ContainsKey(lineup)) continue;
                _allLineups[lineup].Include = true;
            }

            subscribedLineup_SelectedIndexChanged(null, null);
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
            ListView[] listviews = { lvL5Lineup, lvLineupChannels };
            foreach (var listview in listviews)
            {
                // create and assign listview item sorter
                listview.ListViewItemSorter = new ListViewColumnSorter
                {
                    SortColumn = (int)LineupColumn.CallSign,
                    Order = SortOrder.Ascending
                };
                foreach (ColumnHeader head in listview.Columns)
                {
                    SetSortArrow(head, SortOrder.None);
                }
                listview.Sort();
                SetSortArrow(listview.Columns[(int)LineupColumn.CallSign], SortOrder.Ascending);
                listview.Refresh();
            }
        }
        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine which column sorter this click applies to
            var lvcs = (ListViewColumnSorter)((ListView)sender).ListViewItemSorter;
            lvcs.ClickHeader();

            // Determine if clicked column is already the column that is being sorted
            if (e.Column == lvcs.SortColumn)
            {
                // Reverse the current sort direction for this column
                lvcs.Order = (lvcs.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                SetSortArrow(((ListView)sender).Columns[lvcs.SortColumn], SortOrder.None);
                lvcs.SortColumn = e.Column;
                lvcs.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            ((ListView)sender).Sort();
            SetSortArrow(((ListView)sender).Columns[e.Column], lvcs.Order);

            if (lvLineupChannels.Items.Count > 0)
                lvLineupChannels.EnsureVisible(0);
        }
        private void SetSortArrow(ColumnHeader head, SortOrder order)
        {
            const string ascArrow = "▲";
            const string descArrow = "▼";

            // remove arrow
            if (head.Text.EndsWith(ascArrow) || head.Text.EndsWith(descArrow))
                head.Text = head.Text.Substring(0, head.Text.Length - 1);

            // add arrow
            switch (order)
            {
                case SortOrder.Ascending: head.Text += ascArrow; break;
                case SortOrder.Descending: head.Text += descArrow; break;
            }
        }
        #endregion

        #region ========== Lineup ListViews ==========
        private readonly Dictionary<string, myStation> _allStations = new Dictionary<string, myStation>();
        private readonly Dictionary<string, myLineup> _allLineups = new Dictionary<string, myLineup>();
        private readonly HashSet<string> _allAvailableStations = new HashSet<string>();

        #region ===== Station Logos =====
        private void lvLineupChannels_MouseClick(object sender, MouseEventArgs e)
        {
            var lvi = ((ListView)sender).GetItemAt(e.X, e.Y);
            if (lvi == null || e.Button != MouseButtons.Left) return;

            var minX = lvi.SubItems[3].Bounds.X;
            if (e.X > minX && e.X < minX + 48)
            {
                var item = lvLineupChannels.SelectedItems[0] as myChannelLvi;
                var station = item.Station;
                var frm = new frmLogos(station.Station);
                frm.FormClosed += (o, args) =>
                {
                    _allStations[station.StationId].ServiceLogo = GetServiceBitmap(station.Callsign);
                };
                frm.Show(this);
            }
        }

        private void lvLineupChannels_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var backColor = e.SubItem.BackColor = e.Item.BackColor;
            var foreColor = e.SubItem.ForeColor = e.Item.ForeColor;
            if (e.ColumnIndex == 0 || e.Header != columnHeader24)
            {
                e.DrawDefault = true;
                return;
            }

            // set minimum column width
            var textSize = e.Graphics.MeasureString(e.SubItem.Text, lvLineupChannels.Font);
            if (e.Item.ListView.Columns[e.ColumnIndex].Width < (int)textSize.Width + 51)
            {
                e.Item.ListView.Columns[e.ColumnIndex].Width = (int)textSize.Width + 51;
                return;
            }

            if (!e.Item.ListView.Enabled)
            {
                backColor = SystemColors.Control;
                foreColor = Color.LightGray;
            }
            else if (e.Item.ListView.SelectedItems.Contains(e.Item))
            {
                if ((e.ItemState & ListViewItemStates.Selected) != 0 && e.Item.ListView.ContainsFocus)
                {
                    backColor = SystemColors.Highlight;
                    foreColor = SystemColors.HighlightText;
                }
                else
                {
                    backColor = SystemColors.Control;
                    foreColor = SystemColors.ControlText;
                }
            }

            e.DrawBackground();
            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);

            var sf = new StringFormat { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };
            e.Graphics.DrawString(e.SubItem.Text, lvLineupChannels.Font, new SolidBrush(foreColor),
                new Rectangle(e.Bounds.X + 51, e.Bounds.Y, e.Bounds.Width + 51, e.Bounds.Height), sf);

            if (((myChannelLvi)e.Item).Station.ServiceLogo != null)
            {
                var imageRect = new Rectangle(e.Bounds.X, e.Bounds.Y, 48, 16);
                e.Graphics.DrawImage(((myChannelLvi)e.Item).Station.ServiceLogo, imageRect);
            }
        }

        private Bitmap GetServiceBitmap(string callsign)
        {
            if (!Config.IncludeSdLogos) return null;

            var custom = false;
            var path = $"{Helper.Epg123LogosFolder}\\{callsign}";
            if (File.Exists($"{path}_c.png"))
            {
                path += "_c.png";
                custom = true;
            }
            else if (Config.PreferredLogoStyle.Equals("NONE", StringComparison.OrdinalIgnoreCase))
            {
                // prevent remaining logos from possibly being used
            }
            else if (File.Exists($"{path}_{Config.PreferredLogoStyle.Substring(0, 1)}.png"))
            {
                path += $"_{Config.PreferredLogoStyle.Substring(0, 1)}.png";
            }
            else if (File.Exists($"{path}_{Config.AlternateLogoStyle.Substring(0, 1)}.png"))
            {
                path += $"_{Config.AlternateLogoStyle.Substring(0, 1)}.png";
            }
            else if (File.Exists($"{path}.png"))
            {
                path += ".png";
            }
            if (!path.EndsWith(".png")) return null;

            var source = Image.FromFile(path) as Bitmap;
            var ratio = (double)source.Width / source.Height;
            var offsetX = 0;
            var offsetY = 0;
            if (ratio < 3.0)
            {
                offsetX = (source.Height * 3 - source.Width) / 2;
            }
            else
            {
                offsetY = (source.Width / 3 - source.Height) / 2;
            }

            var retBitmap = new Bitmap(source.Width + offsetX * 2, source.Height + offsetY * 2);
            retBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var g = Graphics.FromImage(retBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (Config.AlternateLogoStyle.Equals("GRAY", StringComparison.OrdinalIgnoreCase))
                {
                    g.Clear(Color.White);
                }
                else g.Clear(Color.FromArgb(255, 6, 15, 30));
                g.DrawImage(source, offsetX, offsetY);

                // draw a box around the border
                var thickness = 1 * retBitmap.Height / 16;
                if (custom) g.DrawRectangle(new Pen(Color.Red, thickness), 0, 0, retBitmap.Width - thickness, retBitmap.Height - thickness);
            }

            // free up the file for edit
            source.Dispose();
            GC.Collect();

            return new Bitmap(retBitmap, 48, 16);
        }

        private void GetAllServiceLogos(bool refresh = false)
        {
            // end logos thread if still running from earlier build
            if (_logoThread?.IsAlive ?? false)
            {
                _logoThread.Interrupt();
                if (!_logoThread.Join(100)) _logoThread.Abort();
            }

            pictureBox1.Image = Resources.RedLight.ToBitmap();
            ThreadStart starter = () =>
            {
                foreach (var station in _allStations)
                {
                    if (!refresh && station.Value.ServiceLogo != null) continue;
                    try
                    {
                        station.Value.ServiceLogo = GetServiceBitmap(station.Value.Callsign);
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            };
            starter += () =>
            {
                pictureBox1.Image = Resources.GreenLight.ToBitmap();
            };
            _logoThread = new Thread(starter);
            _logoThread.Start();
        }
        #endregion
        #region ===== Subscribed Lineups =====
        private void toolStrip6_Resize(object sender, EventArgs e)
        {
            comboLineups.Width = toolStrip6.Bounds.Width - btnIncludeExclude.Bounds.Right - 4;
            labelLineupCounts.Width = toolStrip6.Bounds.Width - toolStripSeparator1.Bounds.Right - 4;
        }

        private void menuIncludeExclude_Click(object sender, EventArgs e)
        {
            var include = false;
            var menu = (ToolStripMenuItem)sender;
            if (menu.Equals(menuInclude))
            {
                include = true;
            }

            var selectedLineup = (myLineup)comboLineups.SelectedItem;
            selectedLineup.Include = include;

            menuInclude.Checked = lvLineupChannels.Enabled = include;
            menuExclude.Checked = !include;
            btnIncludeExclude.Image = include ? Resources.GreenLight.ToBitmap() : Resources.RedLight.ToBitmap();
            lvLineupChannels.ForeColor = include ? DefaultForeColor : Color.LightGray;

            btnSelectAll.Enabled = btnSelectNone.Enabled = include;
        }

        private void subscribedLineup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLineups.SelectedIndex == -1) return;

            lvLineupChannels.Items.Clear();
            lvLineupChannels.BeginUpdate();

            var selectedLineup = (myLineup)comboLineups.SelectedItem;
            menuInclude.Checked = lvLineupChannels.Enabled = btnSelectAll.Enabled = btnSelectNone.Enabled = selectedLineup.Include;
            menuExclude.Checked = !selectedLineup.Include;
            btnIncludeExclude.Image = selectedLineup.Include ? Resources.GreenLight.ToBitmap() : Resources.RedLight.ToBitmap();
            lvLineupChannels.ForeColor = selectedLineup.Include ? DefaultForeColor : Color.LightGray;

            var channels = new List<myChannelLvi>();
            foreach (var channel in _allLineups[selectedLineup.Lineup.Lineup].Channels)
            {
                var channelIsNew = Config.StationId.SingleOrDefault(arg => arg.StationId.Replace("-", "") == channel.StationId) == null;
                channels.Add(new myChannelLvi(GetChannelNumber(channel), _allStations[channel.StationId])
                {
                    ImageKey = GetLanguageIcon(_allStations[channel.StationId].LanguageCode),
                    BackColor = channelIsNew ? Color.Pink : default
                });
            }
            lvLineupChannels.Items.AddRange(channels.ToArray());

            lvLineupChannels.EndUpdate();
        }

        private void btnSelectAllNone_Click(object sender, EventArgs e)
        {
            var enable = ((ToolStripButton)sender).Equals(btnSelectAll);

            lvLineupChannels.BeginUpdate();
            foreach (myChannelLvi item in lvLineupChannels.Items)
            {
                if (item.Checked == enable) continue;
                item.Station.Include = enable;
            }
            lvLineupChannels.EndUpdate();
        }

        private void lvLineupChannels_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Ignore item checks when selecting multiple items
            if ((ModifierKeys & (Keys.Shift | Keys.Control)) > 0)
            {
                e.NewValue = e.CurrentValue;
            }

            // determine what station id it is
            var stationId = lvLineupChannels.Items[e.Index].SubItems[(int)LineupColumn.StationID].Text;

            // set the include state
            _allStations[stationId].Include = e.NewValue == CheckState.Checked;
        }

        private void lvLineupChannels_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (e.ColumnIndex != 3)
            {
                e.DrawDefault = true;
                return;
            }

            e.DrawBackground();

            var mouse = PointToClient(Cursor.Position);
            mouse.X -= splitContainer1.SplitterDistance + splitContainer1.SplitterWidth + tabLineups.Margin.Left +
                       lvLineupChannels.Margin.Left + e.Bounds.Location.X;
            mouse.Y -= tabLineups.Margin.Top + tabLineups.ItemSize.Height + toolStrip6.Margin.Top + toolStrip6.Height + toolStrip6.Margin.Bottom + lvLineupChannels.Margin.Top;

            if (Cursor == Cursors.Default && mouse.X > 7 && mouse.X < e.Bounds.Width + 7 && mouse.Y >= 0 && mouse.Y < e.Bounds.Height)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 217, 235, 249)), e.Bounds);
            }

            e.Graphics.DrawLine(SystemPens.ControlLight, e.Bounds.Right - 1, e.Bounds.Y, e.Bounds.Right - 1, e.Bounds.Bottom);
            e.Graphics.DrawLine(SystemPens.ControlLight, e.Bounds.Left + 48, e.Bounds.Y, e.Bounds.Left + 48, e.Bounds.Bottom);

            var sf = new StringFormat { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };
            e.Graphics.DrawString(e.Header.Text, lvLineupChannels.Font, new SolidBrush(e.ForeColor),
                new Rectangle(e.Bounds.X + 51, e.Bounds.Y, e.Bounds.Width - 48, e.Bounds.Height), sf);
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString("Logo", lvLineupChannels.Font, new SolidBrush(e.ForeColor),
                new Rectangle(e.Bounds.X, e.Bounds.Y, 48, e.Bounds.Height), sf);
        }
        #endregion
        #region ===== Custom Lineup =====
        private void LineupEnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var include = L5includeToolStripMenuItem.Equals((ToolStripMenuItem)sender);

            L5includeToolStripMenuItem.Checked = include;
            L5excludeToolStripMenuItem.Checked = !include;
            L5IncludeExclude.Image = include ? Resources.GreenLight.ToBitmap() : Resources.RedLight.ToBitmap();
            lvL5Lineup.Enabled = include;
            lvL5Lineup.ForeColor = include ? DefaultForeColor : Color.LightGray;
        }

        private void btnCustomLineup_ButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show($"This feature is not yet implemented. You can manually edit the custom lineup file \"{Helper.Epg123CustomLineupsXmlPath}\".",
                "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lvL5Lineup_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_lockCustomCheckboxes && lvL5Lineup.Focused) e.NewValue = e.CurrentValue;
        }

        private void lvL5Lineup_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }
        #endregion
        #endregion

        #region ========== Configuration Tabs ==========
        #region ========== TAB: XMLTV ==========
        private void ckXmltvConfigs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(cbXmltv))
            {
                Config.CreateXmltv = ckChannelNumbers.Enabled = ckChannelLogos.Enabled = ckXmltvFillerData.Enabled = ckXmltvExtendedInfo.Enabled =
                    cbXmltvSingleImage.Enabled = lblXmltvOutput.Enabled = tbXmltvOutput.Enabled = btnXmltvOutput.Enabled = lblXmltvLogosNote.Enabled =
                        cbXmltv.Checked;
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
            else if (sender.Equals(cbXmltvSingleImage))
            {
                Config.XmltvSingleImage = cbXmltvSingleImage.Checked;
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
        private void imageConfigs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(cbSeriesPosterArt))
            {
                if (cbSeriesWsArt.Checked && cbSeriesPosterArt.Checked) cbSeriesWsArt.Checked = false;
                Config.SeriesPosterArt = cbSeriesPosterArt.Checked;
            }
            else if (sender.Equals(cbSeriesWsArt))
            {
                if (cbSeriesPosterArt.Checked && cbSeriesWsArt.Checked) cbSeriesPosterArt.Checked = false;
                Config.SeriesWsArt = cbSeriesWsArt.Checked;
            }
            else if (sender.Equals(cbTMDb))
            {
                Config.TMDbCoverArt = cbTMDb.Checked;
            }
            else if (sender.Equals(cbSdLogos))
            {
                Config.IncludeSdLogos = lblPreferredLogos.Enabled = cmbPreferredLogos.Enabled = cbSdLogos.Checked;
                GetAllServiceLogos(true);
            }
            else if (sender.Equals(cmbPreferredLogos))
            {
                Config.PreferredLogoStyle = ((Helper.PreferredLogos)cmbPreferredLogos.SelectedIndex).ToString();
                switch (Config.PreferredLogoStyle)
                {
                    case "DARK":
                        Config.AlternateLogoStyle = Helper.PreferredLogos.WHITE.ToString();
                        break;
                    case "LIGHT":
                        Config.AlternateLogoStyle = Helper.PreferredLogos.GRAY.ToString();
                        break;
                    default:
                        Config.AlternateLogoStyle = Config.PreferredLogoStyle;
                        break;
                }
                GetAllServiceLogos(true);
                configs_Changed(cbBrandLogo, null);
            }
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
                    if (!Config.PreferredLogoStyle.Equals("LIGHT", StringComparison.OrdinalIgnoreCase) && !Config.AlternateLogoStyle.Equals("LIGHT", StringComparison.OrdinalIgnoreCase))
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
            else if (sender.Equals(cbNoCastCrew))
            {
                Config.ExcludeCastAndCrew = cbNoCastCrew.Checked;
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

        private void tabTask_Enter(object sender, EventArgs e)
        {
            UpdateTaskPanel(true);
        }
        #endregion
        #endregion

        private void lineupMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (lvLineupChannels.Items.Count == 0) e.Cancel = true;
        }

        private void copyToClipboardMenuItem_Click(object sender, EventArgs e)
        {
            var textToAdd = $"Lineup: {comboLineups.Text}\r\n";
            textToAdd += "Call Sign\tChannel\tStationID\tName\r\n";
            textToAdd = lvLineupChannels.Items.Cast<ListViewItem>().Aggregate(textToAdd, (current, listViewItem) => current + $"{listViewItem.SubItems[0].Text}\t{listViewItem.SubItems[1].Text}\t{listViewItem.SubItems[2].Text}\t{listViewItem.SubItems[3].Text}\r\n");
            Clipboard.SetText(textToAdd);
        }
    }
}

public static class ControlExtensions
{
    public static void DoubleBuffered(this Control control, bool enable)
    {
        var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        doubleBufferPropertyInfo?.SetValue(control, enable, null);
    }
}

public class myLineup
{
    public override string ToString()
    {
        return $"{(Lineup.IsDeleted ? "[Deleted] " : null)}{Lineup.Name} ({Lineup.Location})";
    }

    public bool Include { get; set; }
    public SdLineup Lineup { get; private set; }
    public List<SdLineupMap> Channels { get; set; }

    public myLineup(SdLineup lineup)
    {
        Lineup = lineup;
    }
}

public class myStation
{
    public override string ToString()
    {
        return $"{Callsign} - {Name}";
    }

    private bool _include;
    public event EventHandler IncludeChanged;
    protected virtual void OnIncludeChanged(EventArgs e)
    {
        var handler = IncludeChanged;
        handler?.Invoke(this, e);
    }
    public bool Include
    {
        get => _include;
        set
        {
            if (value == _include) return;
            _include = value;
            OnIncludeChanged(EventArgs.Empty);
        }
    }

    private Bitmap _serviceLogo;
    public event EventHandler LogoChanged;
    protected virtual void OnLogoChanged(EventArgs e)
    {
        var handler = LogoChanged;
        handler?.Invoke(this, e);
    }
    public Bitmap ServiceLogo
    {
        get => _serviceLogo;
        set
        {
            _serviceLogo = value;
            OnLogoChanged(EventArgs.Empty);
        }
    }

    public bool HDOverride { get; set; }
    public bool SDOverride { get; set; }
    public string CustomCallsign { get; set; }
    public string CustomServiceName { get; set; }
    public string Callsign { get; private set; }
    public string Name { get; private set; }
    public string StationId { get; private set; }
    public string LanguageCode { get; private set; }
    public SdLineupStation Station { get; private set; }

    public myStation(SdLineupStation station)
    {
        var atsc = false;

        // determine station name for ATSC stations
        var names = station.Name.Replace("-", "").Split(' ');
        if (!string.IsNullOrEmpty(station.Affiliate) && names.Length == 2 && names[0] == station.Callsign && $"({names[0]})" == $"{names[1]}")
        {
            atsc = true;
        }

        // add callsign and station name
        StationId = station.StationId;
        Callsign = station.Callsign;
        Name = (atsc ? $"{station.Callsign} ({station.Affiliate})" : null) ?? station.Name;
        LanguageCode = station.BroadcastLanguage[0] ?? null;
        Station = station;
    }
}

public class myChannelLvi : ListViewItem
{
    public string ChannelNumber { get; set; }
    public myStation Station { get; private set; }

    public myChannelLvi(string channelNumber, myStation station) : base(new string[4])
    {
        ChannelNumber = channelNumber;
        Station = station;

        SubItems[0].Text = Station.CustomCallsign ?? Station.Callsign;
        SubItems[1].Text = ChannelNumber;
        SubItems[2].Text = Station.StationId;
        SubItems[3].Text = Station.CustomServiceName ?? Station.Name;

        Checked = Station.Include;
        ForeColor = station.Include ? SystemColors.WindowText : SystemColors.GrayText;

        Station.IncludeChanged += (sender, args) =>
        {
            Checked = Station.Include;
            ForeColor = station.Include ? SystemColors.WindowText : SystemColors.GrayText;
        };

        Station.LogoChanged += (sender, args) =>
        {
            try
            {
                if (ListView?.InvokeRequired ?? false)
                {
                    ListView?.Invoke(new Action(delegate
                    {
                        ListView?.Invalidate(Bounds);
                    }));
                }
                else
                {
                    ListView?.Invalidate(Bounds);
                }
            }
            catch
            {
                // do nothing
            }
        };
    }
}