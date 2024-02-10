using epg123;
using epg123_gui.Properties;
using GaRyan2.SchedulesDirectAPI;
using GaRyan2.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using github = GaRyan2.Github;
using SdApi = GaRyan2.SchedulesDirect;

namespace epg123_gui
{
    public partial class ConfigForm : Form
    {
        private enum LineupColumn
        {
            CallSign = 0,
            Channel = 1,
            StationID = 2,
            Name = 3
        }
        private readonly EpgTaskScheduler _task = new EpgTaskScheduler();
        private readonly ImageList _imageList = new ImageList();
        private bool _newLogin;

        private epgConfig Config;
        private epgConfig _originalConfig;
        private Thread _logoThread;
        private string _BaseServerAddress => Settings.Default.CfgLocation.Replace("epg123/epg123.cfg", "");

        #region ========== Form Opening/Closing ==========
        public ConfigForm()
        {
            // required to show UAC shield on buttons
            Application.EnableVisualStyles();

            // create form objects
            InitializeComponent();

            // adjust components for screen dpi
            double _dpiScaleFactor = 1.0;
            using (var g = CreateGraphics())
            {
                if ((int)g.DpiX != 96 || (int)g.DpiY != 96)
                {
                    _dpiScaleFactor = g.DpiX / 96;

                    // adjust image size for list view items
                    _imageList.ImageSize = new Size((int)(g.DpiX / 6), (int)(g.DpiY / 6));

                    // adjust column widths for list views
                    foreach (ColumnHeader column in lvLineupChannels.Columns)
                    {
                        column.Width = (int)(column.Width * _dpiScaleFactor);
                    }
                }
            }
            toolStrip6.ImageScalingSize = new Size((int)(_dpiScaleFactor * 16), (int)(_dpiScaleFactor * 16));
            splitContainer1.Panel1MinSize = (int)(splitContainer1.Panel1MinSize * _dpiScaleFactor);

            // double buffer list view
            lvLineupChannels.DoubleBuffered(true);

            // initialize the logger
            Logger.Initialize(Helper.Epg123TraceLogPath, "Activating the configuration GUI", false);
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

            // restore window position and size
            Size = Settings.Default.WindowSize;
            if (Settings.Default.WindowLocation != new Point(-1, -1)) Location = Settings.Default.WindowLocation;
            if (Settings.Default.WindowMaximized) WindowState = FormWindowState.Maximized;

            // set imagelist for listviews
            lvLineupChannels.SmallImageList = lvLineupChannels.LargeImageList = _imageList;

            // check for updates
            github.Initialize($"EPG123/{Helper.Epg123Version}", "epg123");
            if (github.UpdateAvailable()) lblUpdate.Text = "UPDATE AVAILABLE";
        }

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            // ensure all components are shown
            Refresh();

            // load configuration file and set component states/values
            LoadConfigurationFile(false);

            // complete the title bar label with version number
            var info = string.Empty;
            if (Helper.InstallMethod == Helper.Installation.PORTABLE) info = " (PORTABLE)";
            else if (Helper.InstallMethod == Helper.Installation.CLIENT) info = $" (REMOTE: {_BaseServerAddress})";
            Text = $"EPG123 Configurator v{Helper.Epg123Version}{info}";

            // check if in debug mode
            if (Config.UseDebug && (DialogResult.Yes == MessageBox.Show("You are currently connecting with Schedules Direct in Debug Mode. Do you wish to change back to Normal mode?",
                                                                        "SD Debug Mode", MessageBoxButtons.YesNo, MessageBoxIcon.Question)))
            {
                _originalConfig.UseDebug = Config.UseDebug = ckDebug.Checked = false;
                if (Helper.InstallMethod != Helper.Installation.CLIENT) Helper.WriteXmlFile(Config, Helper.Epg123CfgPath);
                else
                {
                    var baseApi = $"{_BaseServerAddress}epg123/";
                    SdApi.Initialize($"EPG123/{Helper.Epg123Version}", baseApi, Config.BaseArtworkUrl, !Config.UseDebug);
                    SdApi.UploadConfiguration(Settings.Default.CfgLocation, Config);
                    Application.Restart();
                    Environment.Exit(0);
                }
            }

            // initialize the schedules direct api
            if (Helper.InstallMethod != Helper.Installation.PORTABLE)
            {
                var baseApi = $"{_BaseServerAddress}epg123/";
                SdApi.Initialize($"EPG123/{Helper.Epg123Version}", baseApi, Config.BaseArtworkUrl, Config.UseDebug);
            }
            else SdApi.Initialize($"EPG123/{Helper.Epg123Version}", Config.BaseApiUrl, Config.BaseArtworkUrl, Config.UseDebug);

            // login to Schedules Direct and get a token
            if (Login(Config.UserAccount?.LoginName, Config.UserAccount?.PasswordHash))
            {
                BuildLineupsAndStations();
                _newLogin = false;
            }
        }

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // give option to save if there were changes
            RefreshExpectedCounts();
            if (!btnLogin.Text.Equals("Login") && !Config.Equals(_originalConfig) && DialogResult.Yes == MessageBox.Show("There have been changes made to your configuration. Do you wish to save changes before exiting?", "Configuration Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
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

            // save sorting preference
            if (lvLineupChannels.Items.Count > 0)
            {
                var lvcs = (ListViewColumnSorter)lvLineupChannels.ListViewItemSorter;
                Settings.Default.LineupTableSort = lvcs.SortColumn * 10 + (lvcs.GroupOrder ? 3 : (int)lvcs.Order);
            }
            Settings.Default.Save();

            // end the thread if still running
            if (_logoThread?.IsAlive ?? false)
            {
                _logoThread.Interrupt();
                if (!_logoThread.Join(100)) _logoThread.Abort();
            }
        }

        private void LoadConfigurationFile(bool reload)
        {
            // start service if needed
            if (Helper.InstallMethod <= Helper.Installation.SERVER) UdpFunctions.StartService();

            // determine path to cfg file
            if (reload) Settings.Default.CfgLocation = null;
            if (string.IsNullOrEmpty(Settings.Default.CfgLocation) ||
               (Helper.InstallMethod == Helper.Installation.PORTABLE && !Settings.Default.CfgLocation.Contains(Helper.Epg123ProgramDataFolder)) ||
               (Helper.InstallMethod != Helper.Installation.CLIENT && !Settings.Default.CfgLocation.Contains("localhost")) ||
               (Helper.InstallMethod == Helper.Installation.CLIENT && (Settings.Default.CfgLocation.Contains(Helper.Epg123ProgramDataFolder) || Settings.Default.CfgLocation.Contains("localhost"))))
            {
                switch (Helper.InstallMethod)
                {
                    case Helper.Installation.FULL:
                    case Helper.Installation.SERVER:
                        Settings.Default.CfgLocation = $"http://localhost:{Helper.TcpUdpPort}/epg123/epg123.cfg";
                        break;
                    case Helper.Installation.CLIENT:
                        var frmRemote = new frmRemoteServers();
                        frmRemote.ShowDialog();
                        if (string.IsNullOrEmpty(frmRemote.cfgPath)) MessageBox.Show("No server configuration files identified. Closing application.", "Exiting");
                        else Settings.Default.CfgLocation = frmRemote.cfgPath;
                        break;
                    case Helper.Installation.PORTABLE:
                        Settings.Default.CfgLocation = Helper.Epg123CfgPath;
                        break;
                    default: // UNKNOWN
                        Settings.Default.CfgLocation = null;
                        MessageBox.Show("Unknown installation method. Closing application.", "Exiting");
                        break;
                }
                if (string.IsNullOrEmpty(Settings.Default.CfgLocation)) this.Close();
            }

            // load cfg file
            if (Settings.Default.CfgLocation.StartsWith("http://"))
            {
                try
                {
                    // download config file
                    using (var wc = new WebClient())
                    {
                        var cfg = wc.DownloadString(Settings.Default.CfgLocation);
                        var serializer = new XmlSerializer(typeof(epgConfig));
                        var reader = new StringReader(cfg);
                        Config = (epgConfig)serializer.Deserialize(reader);
                        Logger.WriteInformation($"Successfully downloaded configuration file from {Settings.Default.CfgLocation}.");
                    }

                    // if new config file on remote server, ensure UseIpAddress is set
                    if (Helper.InstallMethod == Helper.Installation.CLIENT && string.IsNullOrEmpty(Config.UseIpAddress))
                    {
                        Config.UseIpAddress = Settings.Default.CfgLocation.Substring("http://".Length).Split(':')[0];
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"Failed to download configuration file from {Settings.Default.CfgLocation}. Exception:{Helper.ReportExceptionMessages(ex)}");
                    if (!reload)
                    {
                        LoadConfigurationFile(true);
                        return;
                    }
                }
            }
            else Config = (epgConfig)Helper.ReadXmlFile(Settings.Default.CfgLocation, typeof(epgConfig));
            if (Config == null) Config = new epgConfig();
            _originalConfig = Config.Clone();

            // set control states and values
            txtLoginName.Text = Config.UserAccount?.LoginName;
            txtPassword.Text = !string.IsNullOrEmpty(Config.UserAccount?.PasswordHash) ? "********" : "";

            numDays.Value = Math.Min(Config.DaysToDownload, numDays.Maximum);
            cbTVDB.Checked = Config.TheTvdbNumbers;
            cbPrefixTitle.Checked = Config.PrefixEpisodeTitle;
            cbPrefixDescription.Checked = Config.PrefixEpisodeDescription;
            cbAlternateSEFormat.Checked = Config.AlternateSEFormat;
            cbAppendDescription.Checked = Config.AppendEpisodeDesc;
            cbAddNewStations.Checked = Config.AutoAddNew;
            cbOadOverride.Checked = Config.OadOverride;
            if (Config.SeriesPosterAspect.Equals("2x3") || Config.SeriesPosterArt) rdo2x3.Checked = true;
            else if (Config.SeriesPosterAspect.Equals("16x9") || Config.SeriesWsArt) rdo16x9.Checked = true;
            else if (Config.SeriesPosterAspect.Equals("3x4")) rdo3x4.Checked = true;
            else rdo4x3.Checked = true;
            rdoSm.Checked = Config.ArtworkSize.Equals("Sm");
            rdoMd.Checked = Config.ArtworkSize.Equals("Md");
            rdoLg.Checked = Config.ArtworkSize.Equals("Lg");
            cbSeasonEventImages.Checked = Config.SeasonEventImages;
            cbSdLogos.Checked = Config.IncludeSdLogos;
            cmbPreferredLogos.SelectedIndex = (int)(Helper.PreferredLogos)Enum.Parse(typeof(Helper.PreferredLogos), Config.PreferredLogoStyle, true);
            cbBrandLogo.Checked = !Config.BrandLogoImage?.Equals("none") ?? false;
            cbModernMedia.Checked = Config.ModernMediaUiPlusSupport;
            cbNoCastCrew.Checked = Config.ExcludeCastAndCrew;

            cbXmltv.Checked = Config.CreateXmltv;
            ckChannelNumbers.Checked = Config.XmltvIncludeChannelNumbers;
            ckChannelLogos.Checked = !string.IsNullOrEmpty(Config.XmltvIncludeChannelLogos) && (Config.XmltvIncludeChannelLogos != "false");
            ckUrlLogos.Checked = Config.XmltvIncludeChannelLogos == "url";
            ckLocalLogos.Checked = Config.XmltvIncludeChannelLogos == "local";
            ckXmltvFillerData.Checked = Config.XmltvAddFillerData;
            numFillerDuration.Value = Config.XmltvFillerProgramLength;
            rtbFillerDescription.Text = Config.XmltvFillerProgramDescription;
            ckXmltvExtendedInfo.Checked = Config.XmltvExtendedInfoInTitleDescriptions;
            cbXmltvSingleImage.Checked = Config.XmltvSingleImage;

            txtBaseApi.Text = Config.BaseApiUrl;
            txtBaseArtwork.Text = Config.BaseArtworkUrl;
            ckDebug.Checked = Config.UseDebug;
            var index = Array.FindIndex(daysRetention, arg => arg == Config.CacheRetention);
            cbCacheRetention.Text = (string)cbCacheRetention.Items[index];
            if (Helper.InstallMethod != Helper.Installation.CLIENT)
            {
                // gather local network interface ip addresses for combobox
                var addresses = Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(arg => arg.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .OrderBy(arg => arg.ToString());
                foreach (var address in addresses) cmbIpAddresses.Items.Add(address.ToString());
                if (!string.IsNullOrEmpty(Config.UseIpAddress) && !cmbIpAddresses.Items.Contains(Config.UseIpAddress))
                    cmbIpAddresses.Items.Add(Config.UseIpAddress);
            }
            else cmbIpAddresses.Items.Add(Config.UseIpAddress);
            ckIpAddress.Checked = !string.IsNullOrEmpty(Config.UseIpAddress);
            cmbIpAddresses.Text = Config.UseIpAddress;
            btnChangeServer.Visible = Helper.InstallMethod == Helper.Installation.CLIENT;

            // disable components as needed
            if (Helper.InstallMethod == Helper.Installation.CLIENT)
            {
                btnRemoveOrphans.Enabled = false;
                tabConfigs.TabPages.Remove(tabTask);
                btnServiceStart.Enabled = false;
                btnServiceStop.Enabled = false;
                label1.Enabled = false;
                ckIpAddress.Enabled = false;
                cmbIpAddresses.Enabled = false;
                tabConfigs.TabPages.Remove(tabNotifier);
            }
            else
            {
                UpdateTaskPanel();
                UpdateServiceTab();
            }

            if (Helper.InstallMethod == Helper.Installation.PORTABLE)
            {
                cbCacheRetention.Enabled = ckIpAddress.Enabled = cmbIpAddresses.Enabled = btnServiceStart.Enabled = btnServiceStop.Enabled = linkLabel2.Enabled = linkLabel3.Enabled = false;
                label1.Enabled = label2.Enabled = label3.Enabled = false;
                cbSeasonEventImages.Enabled = false; cbSeasonEventImages.Font = new Font(cbSeasonEventImages.Font, cbSeasonEventImages.Font.Style | FontStyle.Strikeout);
                lblAspect.Enabled = lblSize.Enabled = pnlAspect.Enabled = pnlSize.Enabled = false;
                lblAspect.Font = lblSize.Font = rdo2x3.Font = rdo3x4.Font = rdo4x3.Font = rdo16x9.Font = rdoSm.Font = rdoMd.Font = rdoLg.Font =
                    new Font(lblAspect.Font, lblAspect.Font.Style | FontStyle.Strikeout);
            }
        }
        #endregion

        #region ========== Scheduled Task ==========
        private void UpdateTaskPanel(bool silent = false)
        {
            // get status
            _task.QueryTask(silent);

            // set .Enabled flags for components
            tbSchedTime.Enabled = lblUpdateTime.Enabled = cbTaskWake.Enabled = !_task.Exist && !_task.ExistNoAccess;

            // set task create/delete button text
            btnTask.Text = _task.Exist || _task.ExistNoAccess ? "Delete" : "Create";

            // update scheduled task run time
            tbSchedTime.Text = _task.SchedTime.ToString("HH:mm");

            // set scheduled task wake checkbox
            cbTaskWake.Checked = _task.Wake;

            // determine which action is the client action
            var clientIndex = -1;
            var epg123Index = -1;
            if (_task.Exist)
            {
                for (var i = 0; i < _task.Actions.Length; ++i)
                {
                    if (_task.Actions[i].Path.ToLower().Contains(Helper.Epg123ExePath.ToLower())) epg123Index = i;
                    else if (_task.Actions[i].Path.ToLower().Contains(Helper.Epg123ClientExePath.ToLower())) clientIndex = i;
                }

                // display task status
                if (epg123Index >= 0)
                {
                    lblSchedStatus.Text = _task.StatusString;
                    lblSchedStatus.ForeColor = Color.Black;
                }
                else if (clientIndex >= 0)
                {
                    lblSchedStatus.Text = "### Client Mode ONLY - Guide will not be downloaded. ###";
                    lblSchedStatus.ForeColor = Color.Red;
                }

                // verify task configuration with respect to this executable
                if (epg123Index >= 0 || clientIndex >= 0)
                {
                    // update import checkbox
                    cbImport.Enabled = false;
                    cbImport.Checked = clientIndex >= 0;

                    // update automatch checkbox
                    cbAutomatch.Enabled = false;
                    cbAutomatch.Checked = clientIndex >= 0 && _task.Actions[clientIndex].Arguments.ToLower().Contains("-match");

                    return;
                }
                lblSchedStatus.Text = "### Existing task does not point to this installation. ###";
                lblSchedStatus.ForeColor = Color.Red;
                cbImport.Enabled = cbAutomatch.Enabled = false;
            }
            else
            {
                // set import and automatch checkbox states
                cbImport.Enabled = cbImport.Checked = cbAutomatch.Enabled = File.Exists(Helper.Epg123ClientExePath) && File.Exists(Helper.EhshellExeFilePath);
                lblSchedStatus.Text = _task.Exist || _task.ExistNoAccess ? string.Empty : _task.StatusString;
                lblSchedStatus.ForeColor = Color.Red;
            }
        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            // create new task if file location is valid
            if (!_task.Exist)
            {
                // create task using epg123.exe & epg123Client.exe
                if (cbImport.Checked)
                {
                    var actions = new EpgTaskScheduler.TaskActions[2];
                    actions[0].Path = Helper.Epg123ExePath;
                    actions[1].Path = Helper.Epg123ClientExePath;
                    actions[1].Arguments = $"-i \"{Helper.Epg123MxfPath}\"{(cbAutomatch.Checked ? " -match" : null)}";
                    _task.CreateTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                }
                // create task using epg123.exe
                else
                {
                    var actions = new EpgTaskScheduler.TaskActions[1];
                    actions[0].Path = Helper.Epg123ExePath;
                    _task.CreateTask(cbTaskWake.Checked, tbSchedTime.Text, actions);
                }
                if (!Helper.UserHasElevatedRights) _task.ImportTask();
            }
            else _task.DeleteTask();

            // update panel with current information
            UpdateTaskPanel();
        }
        #endregion

        #region ========== Login ==========
        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) btnLogin_Click(null, null);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // logout user for a fresh login
            if (sender != null && ((Button)sender).Text == "Logout")
            {
                // enable username/password fields
                txtLoginName.Enabled = true;
                txtPassword.Enabled = true;
                btnLogin.Text = "Login";
                txtPassword.Text = "";

                // enable form controls
                tabLineups.Enabled = false;
                tabConfigs.Enabled = false;
                btnSave.Enabled = false;
                btnExecute.Enabled = false;
                btnClientLineups.Enabled = false;

                return;
            }

            if (string.IsNullOrEmpty(txtLoginName.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Login username and/or password is blank.", "Missing Login Information", MessageBoxButtons.OK);
                return;
            }

            // disable input fields while trying to login
            Cursor = Cursors.WaitCursor;
            txtLoginName.Enabled = txtPassword.Enabled = false;
            if (!Login(txtLoginName.Text, txtPassword.Text)) return;
            BuildLineupsAndStations();
            Cursor = Cursors.Arrow;
        }

        private bool Login(string username, string passwordHash)
        {
            btnLogin.Focus();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordHash)) return false;
            if (username != Config.UserAccount?.LoginName || passwordHash != Config.UserAccount?.PasswordHash)
            {
                Config.UserAccount = new SdUserAccount
                {
                    LoginName = username,
                    Password = passwordHash
                };
                passwordHash = Config.UserAccount.PasswordHash;
                _newLogin = true;
            }

            if (SdApi.GetToken(username, passwordHash, _newLogin))
            {
                // enable form controls
                tabLineups.Enabled = true;
                tabConfigs.Enabled = true;
                btnSave.Enabled = true;
                btnExecute.Enabled = Helper.InstallMethod != Helper.Installation.CLIENT;
                btnClientLineups.Enabled = true;

                // disable username/password fields
                txtLoginName.Enabled = false;
                txtPassword.Enabled = false;
                btnLogin.Text = "Logout";

                // get user status
                UserStatus status;
                if ((status = SdApi.GetUserStatus()) != null)
                {
                    txtAcctExpires.Text = status.Account.Expires.ToLocalTime().ToString();
                    if (status.Account.Expires - DateTime.UtcNow < TimeSpan.FromDays(14.0))
                    {
                        // weird fact: the text color of a read-only textbox will only change after you set the backcolor
                        txtAcctExpires.ForeColor = Color.Red;
                        txtAcctExpires.BackColor = txtAcctExpires.BackColor;
                    }

                    if ((status.Lineups?.Count ?? 0) > 0) return true;
                    MessageBox.Show("There are no lineups in your SD-JSON account. You must\nadd at least one lineup to proceed.", "No Lineups in Account", MessageBoxButtons.OK);
                    btnClientConfig_Click(null, null);
                    return true;
                }
            }
            else
            {
                MessageBox.Show(SdApi.ErrorMessage ?? "Failed to get token. Check trace.log and/or server.log file for details.", "Failed to Login");
                txtLoginName.Enabled = txtPassword.Enabled = true;
                txtPassword.Text = string.Empty;
                Cursor = Cursors.Arrow;
            }
            return false;
        }
        #endregion

        #region ========== Setup Lineup Stations ListView and Combobox ==========
        private readonly Dictionary<string, MemberLineup> _allLineups = new Dictionary<string, MemberLineup>();
        private readonly Dictionary<string, MemberStation> _allStations = new Dictionary<string, MemberStation>();

        public bool BuildLineupsAndStations(HashSet<string> newLineup = null)
        {
            // reset listview
            lvLineupChannels.Items.Clear();
            lvLineupChannels.ListViewItemSorter = null;
            AssignColumnSorter();

            // reset lineups, stations, and combobox
            comboLineups.Items.Clear();
            _allLineups.Clear();
            _allStations.Clear();

            // retrieve lineups from SD
            var clientLineups = SdApi.GetSubscribedLineups();
            if (clientLineups == null) return false;

            foreach (var clientLineup in clientLineups.Lineups)
            {
                // request the lineup's station maps
                var lineupMap = SdApi.GetStationChannelMap(clientLineup.Lineup);
                if (lineupMap == null) continue;

                // build the lineup
                _allLineups.Add(clientLineup.Lineup, new MemberLineup(clientLineup)
                {
                    Include = (newLineup?.Contains(clientLineup.Lineup) ?? false) ||
                              (Config.IncludedLineup?.Contains(clientLineup.Lineup) ?? false),
                    DiscardNumbers = Config.DiscardChanNumbers?.Contains(clientLineup.Lineup) ?? false,
                    Channels = lineupMap.Map.ToList()
                });

                // build the stations
                foreach (var station in lineupMap.Stations)
                {
                    if (_allStations.ContainsKey(station.StationId)) continue;

                    var configStation = Config.StationId.SingleOrDefault(arg => arg.StationId.Replace("-", "").Equals(station.StationId));
                    _allStations.Add(station.StationId, new MemberStation(station, configStation, cbAddNewStations));
                }
            }

            // build the lineup combobox
            foreach (var lineup in _allLineups.OrderBy(arg => arg.Value.Lineup.Name).ThenBy(arg => arg.Value.Lineup.Location)) { _ = comboLineups.Items.Add(lineup.Value); }
            labelLineupCounts.Text = $"subscribed to {comboLineups.Items.Count} out of {SdApi.MaxLineups} allowed lineups";
            labelLineupCounts.Width = toolStrip6.Bounds.Width - toolStripSeparator1.Bounds.Right - 5;

            // build the imageList for all stations
            if (_allStations.Count > 0)
            {
                if (Helper.InstallMethod == Helper.Installation.CLIENT)
                {
                    _RemoteCustomLogos = SdApi.GetCustomLogosFromServer($"{_BaseServerAddress}logos/custom");
                }
                BuildLanguageIcons();
                GetAllServiceLogos();
            }

            if (comboLineups.Items.Count > 0) comboLineups.SelectedIndex = 0;
            return comboLineups.Items.Count > 0;
        }

        #region ========== Column Sorter ==========
        private void AssignColumnSorter()
        {
            var colSort = Settings.Default.LineupTableSort / 10;
            var order = Settings.Default.LineupTableSort % 10;
            lvLineupChannels.ListViewItemSorter = new ListViewColumnSorter
            {
                SortColumn = colSort,
                Order = order > 2 ? SortOrder.Ascending : (SortOrder)order,
                GroupOrder = (order > 2)
            };
            foreach (ColumnHeader head in lvLineupChannels.Columns)
            {
                SetSortArrow(head, SortOrder.None);
            }
            SetSortArrow(lvLineupChannels.Columns[colSort], (SortOrder)(order > 2 ? 1 : order));
        }

        private void LvLineupSort(object sender, ColumnClickEventArgs e)
        {
            // Determine which column sorter this click applies to
            var listview = (ListView)sender;
            var sorter = (ListViewColumnSorter)listview.ListViewItemSorter;

            // remove sort indicator from column if new
            if (e.Column != sorter.SortColumn) SetSortArrow(listview.Columns[sorter.SortColumn], SortOrder.None);

            // Perform the sort with these new sort options.
            sorter.ClickHeader(e.Column);
            listview.Sort();
            SetSortArrow(listview.Columns[e.Column], sorter.Order);

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

        #region ===== Station Logo Generation =====
        private readonly ConcurrentDictionary<string, Bitmap> _Bitmaps = new ConcurrentDictionary<string, Bitmap>();
        private List<string> _RemoteCustomLogos = new List<string>();
        private readonly object _bitmapLock = new object();
        private void GetAllServiceLogos()
        {
            // end logos thread if still running from earlier build
            if (_logoThread?.IsAlive ?? false)
            {
                _logoThread.Interrupt();
                if (!_logoThread.Join(100)) _logoThread.Abort();
            }
            if (_allStations.Count == 0) return;

            pictureBox1.Image = Resources.RedLight.ToBitmap();
            ThreadStart starter = () =>
            {
                try
                {
                    Parallel.For(0, _allStations.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, i =>
                    {
                        _allStations.ElementAt(i).Value.ServiceLogo = GetServiceBitmap(_allStations.ElementAt(i).Value.Station);
                    });
                }
                catch { }
            };
            starter += () =>
            {
                pictureBox1.Image = Resources.GreenLight.ToBitmap();
            };
            _logoThread = new Thread(starter);
            _logoThread.Start();
        }

        private Bitmap GetServiceBitmap(LineupStation station)
        {
            if (!Config.IncludeSdLogos) return null;

            // custom logo file check
            var filename = $"{station.Callsign}_c.png";
            var path = $"{Helper.Epg123LogosFolder}{filename}";
            if (Helper.InstallMethod != Helper.Installation.CLIENT && File.Exists(path))
            {
                if (!_Bitmaps.ContainsKey(filename)) _Bitmaps.TryAdd(filename, ResizeLogoBitmap(Image.FromFile(path) as Bitmap, true));
                return AddLogoBackground(_Bitmaps[filename]);
            }

            if (Helper.InstallMethod == Helper.Installation.CLIENT && _RemoteCustomLogos.Contains(filename))
            {
                path = $"{_BaseServerAddress}logos/{filename}";
                if (!_Bitmaps.ContainsKey(filename))
                {
                    using (var client = new WebClient())
                    using (var ms = new MemoryStream(client.DownloadData(path)))
                    {
                        _Bitmaps.TryAdd(filename, ResizeLogoBitmap(new Bitmap(ms), true));
                    }
                }
                return AddLogoBackground(_Bitmaps[filename]);
            }
            if (Config.PreferredLogoStyle.Equals("none", StringComparison.OrdinalIgnoreCase)) return null;

            // primary and alternate file check
            var priLogo = station.StationLogos?.FirstOrDefault(arg => arg.Category.Equals(Config.PreferredLogoStyle.ToLower()))?.Md5;
            var altLogo = station.StationLogos?.FirstOrDefault(arg => arg.Category.Equals(Config.AlternateLogoStyle.ToLower()))?.Md5;

            // change logo uri's into file path
            if (priLogo != null) priLogo = $"{priLogo}.png";
            if (altLogo != null) altLogo = $"{altLogo}.png";
            if (priLogo == null && altLogo == null && station.Logo != null) priLogo = $"{station.Logo?.Md5}.png";
            if ((filename = priLogo ?? altLogo) == null) return null;

            // use primary or alternate file to display
            path = $"{Helper.Epg123LogosFolder}{filename}";
            if (Helper.InstallMethod != Helper.Installation.CLIENT && File.Exists(path))
            {
                if (!_Bitmaps.ContainsKey(filename)) _Bitmaps.TryAdd(filename, ResizeLogoBitmap(Image.FromFile(path) as Bitmap));
                return AddLogoBackground(_Bitmaps[filename]);
            }

            if (Helper.InstallMethod == Helper.Installation.CLIENT && _RemoteCustomLogos.Contains(filename))
            {
                path = $"{_BaseServerAddress}logos/{filename}";
                if (!_Bitmaps.ContainsKey(filename))
                {
                    using (var client = new WebClient())
                    using (var ms = new MemoryStream(client.DownloadData(path)))
                    {
                        _Bitmaps.TryAdd(filename, ResizeLogoBitmap(new Bitmap(ms), false));
                    }
                }
                return AddLogoBackground(_Bitmaps[filename]);
            }

            // no logo file available so download it
            priLogo = station.StationLogos?.FirstOrDefault(arg => arg.Category.Equals(Config.PreferredLogoStyle.ToLower()))?.Url;
            altLogo = station.StationLogos?.FirstOrDefault(arg => arg.Category.Equals(Config.AlternateLogoStyle.ToLower()))?.Url;
            path = priLogo ?? altLogo ?? station.Logo?.Url;
            if (path == null) return null;
            if (_Bitmaps.ContainsKey(path)) return AddLogoBackground(_Bitmaps[path]);

            try
            {
                using (var client = new WebClient())
                using (var ms = new MemoryStream(client.DownloadData(path)))
                {
                    _Bitmaps.TryAdd(path, ResizeLogoBitmap(Helper.CropAndResizeImage(new Bitmap(ms))));
                    return AddLogoBackground(_Bitmaps[path]);
                }
            }
            catch { return null; }
        }

        private Bitmap ResizeLogoBitmap(Bitmap source, bool custom = false)
        {
            var ratio = (double)source.Width / source.Height;
            var offsetX = 0;
            var offsetY = 0;
            if (ratio < 3.0) offsetX = (source.Height * 3 - source.Width) / 2;
            else offsetY = (source.Width / 3 - source.Height) / 2;

            var retBitmap = new Bitmap(source.Width + offsetX * 2, source.Height + offsetY * 2);
            retBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var g = Graphics.FromImage(retBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(source, offsetX, offsetY);

                var thickness = 1 * retBitmap.Height / 16;
                if (custom) g.DrawRectangle(new Pen(Color.Red, thickness), 0, 0, retBitmap.Width - thickness, retBitmap.Height - thickness);
            }
            source.Dispose();
            return new Bitmap(retBitmap, 48, 16);
        }

        private Bitmap AddLogoBackground(Bitmap source)
        {
            Bitmap clone;
            lock (_bitmapLock) { clone = source.Clone() as Bitmap; }
            var retBitmap = new Bitmap(clone.Width, clone.Height);
            retBitmap.SetResolution(clone.HorizontalResolution, clone.VerticalResolution);
            using (var g = Graphics.FromImage(retBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (Config.AlternateLogoStyle.Equals("GRAY", StringComparison.OrdinalIgnoreCase))
                {
                    g.Clear(Color.White);
                }
                else g.Clear(Color.FromArgb(255, 6, 15, 30));
                g.DrawImage(clone, 0, 0);
            }
            return retBitmap;
        }
        #endregion

        #region ========== Build Language Icons for ListView items ==========
        private void BuildLanguageIcons()
        {
            foreach (var language in _allStations.Select(arg => arg.Value.LanguageCode).Distinct())
            {
                if (_imageList.Images.Keys.Contains(language)) continue;
                _imageList.Images.Add(language, DrawText(language, new Font(lvLineupChannels.Font.Name, 16, FontStyle.Bold, lvLineupChannels.Font.Unit)));
            }
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
        #endregion

        #region ========== Buttons & Links ==========
        private void RefreshExpectedCounts()
        {
            if (Config == null) return;
            Config.IncludedLineup = _allLineups.Values.Where(arg => arg.Include).OrderBy(arg => arg.Lineup.Lineup).Select(arg => arg.Lineup.Lineup).ToList();
            Config.DiscardChanNumbers = _allLineups.Values.Where(arg => arg.Include && arg.DiscardNumbers).OrderBy(arg => arg.Lineup.Lineup).Select(arg => arg.Lineup.Lineup).ToList();

            var includedStations = _allLineups.Values.Where(arg => arg.Include).SelectMany(arg => arg.Channels.Select(ch => ch.StationId)).Distinct().ToList();
            Config.StationId = _allStations.Values.Where(arg => includedStations.Contains(arg.StationId)).Select(arg => arg.StationOptions).OrderBy(arg => arg.CallSign).ToList();
            Config.ExpectedServicecount = Config.StationId.Count(arg => !arg.StationId.StartsWith("-"));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            RefreshExpectedCounts();

            // sanity checks
            if ((Config.ExpectedServicecount == 0) && (sender != null))
            {
                MessageBox.Show("There are no INCLUDED lineups and/or no stations selected for download.\n\nConfiguration will not be saved.",
                                "No Stations to Download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (Config.ExpectedServicecount != _originalConfig.ExpectedServicecount)
            {
                var prompt = $"The number of stations to download has {((_originalConfig.ExpectedServicecount > Config.ExpectedServicecount) ? "decreased" : "increased")} from {_originalConfig.ExpectedServicecount} to {Config.ExpectedServicecount} from the previous configuration.\n\nDo you wish to commit these changes?";
                if (MessageBox.Show(prompt, "Change in Expected Services Count", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            // commit the updated config file if there are changes
            if (_newLogin || !Config.Equals(_originalConfig))
            {
                // check if image size has changed
                if (!Config.ArtworkSize.Equals(_originalConfig.ArtworkSize))
                {
                    if (Helper.InstallMethod == Helper.Installation.CLIENT)
                    {
                        try
                        {
                            using (var wc = new WebClient())
                            {
                                _ = wc.DownloadData($"{_BaseServerAddress}epg123/clearCache");
                                Logger.WriteInformation("Cache successfully cleared on server.");
                            }
                        }
                        catch { Logger.WriteError("Failed to clear json cache on server."); }
                    }
                    else if (Helper.DeleteFile(Helper.Epg123CacheJsonPath))
                    {
                        Logger.WriteInformation("Successfully deleted json cache file.");
                    }
                }

                // save configuration file
                Config.Version = Helper.Epg123Version;
                if (Helper.InstallMethod != Helper.Installation.CLIENT) Helper.WriteXmlFile(Config, Helper.Epg123CfgPath);
                else SdApi.UploadConfiguration(Settings.Default.CfgLocation, Config);

                // update the original config with the new for comparison if needed
                _originalConfig = Config.Clone();

                // clear new station flags
                foreach (var station in _allStations.Where(arg => arg.Value.IsNew)) station.Value.IsNew = false;
                subscribedLineup_SelectedIndexChanged(null, null);
                _newLogin = false;
            }

            if (sender?.Equals(btnExecute) ?? false)
            {
                // run epg123 to create mxf file
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = Helper.Epg123ExePath,
                    Arguments = $"-p{(cbImport.Checked ? " -import" : "")}{(cbAutomatch.Checked ? " -match" : "")}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // close gui and wait for exit
                Close();
                proc.WaitForExit();
                Logger.Status = proc.ExitCode;
            }
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

            BuildLineupsAndStations(gui.NewLineups);
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            if (Helper.InstallMethod == Helper.Installation.CLIENT)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Helper.LogViewer,
                    Arguments = $"{_BaseServerAddress}trace.log"
                });
            }
            else Helper.ViewLogFile();
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            bool success = false;
            if (Helper.InstallMethod == Helper.Installation.CLIENT)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        _ = wc.DownloadData($"{_BaseServerAddress}epg123/clearCache");
                        Logger.WriteInformation("Cache successfully cleared on server.");
                        success = true;
                    }
                }
                catch { Logger.WriteError("Failed to clear cache on server."); }
            }
            else if (success = Helper.DeleteFile(Helper.Epg123CacheJsonPath) && Helper.DeleteFile(Helper.Epg123MmuiplusJsonPath))
            {
                Logger.WriteInformation("Successfully deleted all cache files.");
            }
            else Logger.WriteError("Failed to delete all cache files.");
            if (success) MessageBox.Show("Cache files have been removed and will be rebuilt on next update.", "Operation Complete", MessageBoxButtons.OK);
            else MessageBox.Show("A problem occurred attempting to delete cache files.", "Operation Failed");
            Cursor = Cursors.Arrow;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start("http://garyan2.github.io/download.html");
        }
        #endregion

        #region ========== Lineup ListViews ==========
        private void lvLineupChannels_MouseClick(object sender, MouseEventArgs e)
        {
            var lvi = ((ListView)sender).GetItemAt(e.X, e.Y);
            if (lvi == null || e.Button != MouseButtons.Left) return;

            var minX = lvi.SubItems[3].Bounds.X;
            if (e.X > minX && e.X < minX + 48)
            {
                var item = lvLineupChannels.SelectedItems[0] as MemberListViewItem;
                var station = item.Station;
                var frm = new frmLogos(station.Station);
                frm.FormClosed += (o, args) =>
                {
                    if (frm.LogoChanged) _RemoteCustomLogos = SdApi.GetCustomLogosFromServer($"{_BaseServerAddress}logos/custom");
                    lock (_bitmapLock) _Bitmaps.TryRemove($"{station.Station.Callsign}_c.png", out _);
                    _allStations[station.StationId].ServiceLogo = GetServiceBitmap(station.Station);
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

            if (((MemberListViewItem)e.Item).Station.ServiceLogo != null)
            {
                var imageRect = new Rectangle(e.Bounds.X, e.Bounds.Y, 48, 16);
                e.Graphics.DrawImage(((MemberListViewItem)e.Item).Station.ServiceLogo, imageRect);
            }
        }

        private void btnRemoveOrphans_Click(object sender, EventArgs e)
        {
            if (DialogResult.No == MessageBox.Show("This will delete all orphaned logos from your .\\logos folder based on your current logo options. No custom logos will be deleted. Do you wish to proceed?",
                "Logo Cleanup", MessageBoxButtons.YesNo)) return;

            var logos = Directory.Exists(Helper.Epg123LogosFolder) ? Directory.GetFiles(Helper.Epg123LogosFolder) : new string[0];
            var candidates = new List<string>();
            var activeLogos = new HashSet<string>();

            // gather active logos
            foreach (var station in _allStations.Where(arg => arg.Value.Station.StationLogos != null))
            {
                if (!station.Value.Include) continue;
                if (File.Exists($"{Helper.Epg123LogosFolder}{station.Value.CallSign}_c.png")) continue;
                var priLogo = station.Value.Station.StationLogos.FirstOrDefault(arg => arg.Category.Equals(Config.PreferredLogoStyle.ToLower()))?.Md5;
                var altLogo = station.Value.Station.StationLogos.FirstOrDefault(arg => arg.Category.Equals(Config.AlternateLogoStyle.ToLower()))?.Md5;
                if (priLogo != null) activeLogos.Add($"{Helper.Epg123LogosFolder}{priLogo}.png");
                else if (altLogo != null) activeLogos.Add($"{Helper.Epg123LogosFolder}{altLogo}.png");
                else if (station.Value.Station.Logo != null) activeLogos.Add($"{Helper.Epg123LogosFolder}{station.Value.Station.Logo.Md5}.png");
            }

            // collect candidate deletions
            foreach (var logo in logos)
            {
                if (logo.ToLower().EndsWith("_c.png")) continue;
                if (!activeLogos.Contains(logo)) candidates.Add(logo);
            }

            // delete files
            foreach (var candidate in candidates)
            {
                Helper.DeleteFile(candidate);
            }

            MessageBox.Show($"There were {candidates.Count} orphaned logos deleted from the .\\logos folder.", "Logo Cleanup");
        }

        #region ===== Subscribed Lineups =====
        private void toolStrip6_Resize(object sender, EventArgs e)
        {
            comboLineups.Width = toolStrip6.Bounds.Width - btnIncludeExclude.Bounds.Right - 5;
            labelLineupCounts.Width = toolStrip6.Bounds.Width - toolStripSeparator1.Bounds.Right - 5;
        }

        private void menuIncludeExclude_Click(object sender, EventArgs e)
        {
            var include = false;
            var menu = (ToolStripMenuItem)sender;
            if (menu.Equals(menuInclude))
            {
                include = true;
            }

            var selectedLineup = (MemberLineup)comboLineups.SelectedItem;
            selectedLineup.Include = include;

            menuInclude.Checked = lvLineupChannels.Enabled = include;
            menuExclude.Checked = !include;
            btnIncludeExclude.Image = include ? Resources.GreenLight.ToBitmap() : Resources.RedLight.ToBitmap();
            lvLineupChannels.ForeColor = include ? DefaultForeColor : Color.LightGray;

            btnSelectAll.Enabled = btnSelectNone.Enabled = include;
        }

        private void menuDiscardNumbers_Click(object sender, EventArgs e)
        {
            var selectedLineup = (MemberLineup)comboLineups.SelectedItem;
            selectedLineup.DiscardNumbers = menuDiscardNumbers.Checked = !selectedLineup.DiscardNumbers;
        }

        private void subscribedLineup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLineups.SelectedIndex == -1) return;

            lvLineupChannels.Items.Clear();
            lvLineupChannels.BeginUpdate();

            var selectedLineup = (MemberLineup)comboLineups.SelectedItem;
            menuInclude.Checked = lvLineupChannels.Enabled = btnSelectAll.Enabled = btnSelectNone.Enabled = selectedLineup.Include;
            menuExclude.Checked = !selectedLineup.Include;
            menuDiscardNumbers.Checked = selectedLineup.DiscardNumbers;
            btnIncludeExclude.Image = selectedLineup.Include ? Resources.GreenLight.ToBitmap() : Resources.RedLight.ToBitmap();
            lvLineupChannels.ForeColor = selectedLineup.Include ? DefaultForeColor : Color.LightGray;

            var channels = new List<MemberListViewItem>();
            foreach (var channel in _allLineups[selectedLineup.Lineup.Lineup].Channels)
            {
                channels.Add(new MemberListViewItem(channel.ChannelNumber, _allStations[channel.StationId]));
            }
            lvLineupChannels.Items.AddRange(channels.ToArray());
            lvLineupChannels.EndUpdate();
        }

        private void btnSelectAllNone_Click(object sender, EventArgs e)
        {
            var enable = ((ToolStripButton)sender).Equals(btnSelectAll);

            lvLineupChannels.BeginUpdate();
            foreach (MemberListViewItem item in lvLineupChannels.Items)
            {
                if (item.Checked == enable) continue;
                item.Station.Include = enable;
            }
            lvLineupChannels.EndUpdate();
        }

        private void lvLineupChannels_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Ignore item checks when selecting multiple items
            if ((ModifierKeys & (Keys.Shift | Keys.Control)) > 0) e.NewValue = e.CurrentValue;

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
        #endregion

        #region ========== Configuration Tabs ==========
        #region ========== TAB: XMLTV ==========
        private void ckXmltvConfigs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(cbXmltv))
            {
                Config.CreateXmltv = ckChannelNumbers.Enabled = ckChannelLogos.Enabled = ckXmltvFillerData.Enabled = ckXmltvExtendedInfo.Enabled =
                    cbXmltvSingleImage.Enabled = lblXmltvLogosNote.Enabled = cbXmltv.Checked;
                if (!cbXmltv.Checked)
                {
                    ckUrlLogos.Enabled = ckLocalLogos.Enabled = false;
                    numFillerDuration.Enabled = lblFillerDuration.Enabled = rtbFillerDescription.Enabled = false;
                }
                else
                {
                    ckUrlLogos.Enabled = ckLocalLogos.Enabled = ckChannelLogos.Checked;
                    numFillerDuration.Enabled = lblFillerDuration.Enabled = rtbFillerDescription.Enabled = ckXmltvFillerData.Checked;
                }
            }
            else if (sender.Equals(ckChannelNumbers)) Config.XmltvIncludeChannelNumbers = ckChannelNumbers.Checked;
            else if (sender.Equals(ckChannelLogos))
            {
                ckUrlLogos.Enabled = ckLocalLogos.Enabled = ckChannelLogos.Checked;
                if (!ckChannelLogos.Checked) Config.XmltvIncludeChannelLogos = "false";
                Config.XmltvIncludeChannelLogos = !ckChannelLogos.Checked ? "false" : ckUrlLogos.Checked ? "url" : "local";
            }
            else if (sender.Equals(ckUrlLogos))
            {
                ckLocalLogos.Checked = !ckUrlLogos.Checked;
                Config.XmltvIncludeChannelLogos = "url";
            }
            else if (sender.Equals(ckLocalLogos))
            {
                ckUrlLogos.Checked = !ckLocalLogos.Checked;
                Config.XmltvIncludeChannelLogos = "local";
            }
            else if (sender.Equals(ckXmltvFillerData))
            {
                numFillerDuration.Enabled = lblFillerDuration.Enabled = rtbFillerDescription.Enabled = ckXmltvFillerData.Checked && cbXmltv.Checked;
                Config.XmltvAddFillerData = ckXmltvFillerData.Checked;
            }
            else if (sender.Equals(numFillerDuration)) Config.XmltvFillerProgramLength = (int)numFillerDuration.Value;
            else if (sender.Equals(rtbFillerDescription)) Config.XmltvFillerProgramDescription = rtbFillerDescription.Text;
            else if (sender.Equals(ckXmltvExtendedInfo)) Config.XmltvExtendedInfoInTitleDescriptions = ckXmltvExtendedInfo.Checked;
            else if (sender.Equals(cbXmltvSingleImage)) Config.XmltvSingleImage = cbXmltvSingleImage.Checked;
        }
        #endregion
        #region ========== TAB: Images ==========
        private void imageConfigs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(rdo2x3) && rdo2x3.Checked)
            {
                Config.SeriesPosterAspect = "2x3";
            }
            else if (sender.Equals(rdo3x4) && rdo3x4.Checked)
            {
                Config.SeriesPosterAspect = "3x4";
            }
            else if (sender.Equals(rdo4x3) && rdo4x3.Checked)
            {
                Config.SeriesPosterAspect = "4x3";
            }
            else if (sender.Equals(rdo16x9) && rdo16x9.Checked)
            {
                Config.SeriesPosterAspect = "16x9";
            }
            else if (sender.Equals(rdoSm) && rdoSm.Checked)
            {
                Config.ArtworkSize = "Sm";
            }
            else if (sender.Equals(rdoMd) && rdoMd.Checked)
            {
                Config.ArtworkSize = "Md";
            }
            else if (sender.Equals(rdoLg) && rdoLg.Checked)
            {
                Config.ArtworkSize = "Lg";
            }
            else if (sender.Equals(cbSdLogos))
            {
                Config.IncludeSdLogos = cbSdLogos.Checked;
                GetAllServiceLogos();
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
                GetAllServiceLogos();
            }
        }
        #endregion
        #region ========== TAB: Config ==========
        private void configs_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(numDays)) Config.DaysToDownload = (int)numDays.Value;
            else if (sender.Equals(cbTVDB)) Config.TheTvdbNumbers = cbTVDB.Checked;
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
            else if (sender.Equals(cbAlternateSEFormat)) Config.AlternateSEFormat = cbAlternateSEFormat.Checked;
            else if (sender.Equals(cbAppendDescription)) Config.AppendEpisodeDesc = cbAppendDescription.Checked;
            else if (sender.Equals(cbOadOverride)) Config.OadOverride = cbOadOverride.Checked;
            else if (sender.Equals(cbSeasonEventImages)) Config.SeasonEventImages = cbSeasonEventImages.Checked;
            else if (sender.Equals(cbAddNewStations)) Config.AutoAddNew = cbAddNewStations.Checked;
            else if (sender.Equals(cbModernMedia)) Config.ModernMediaUiPlusSupport = cbModernMedia.Checked;
            else if (sender.Equals(cbNoCastCrew)) Config.ExcludeCastAndCrew = cbNoCastCrew.Checked;
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
        }
        #endregion
        #region ========== TAB: Service ==========
        private readonly int[] daysRetention = new[] { 0, 7, 14, 30, 60, 90, 180, 365, 10958 };

        private void UpdateServiceTab()
        {
            var index = Array.FindIndex(daysRetention, arg => arg == Config.CacheRetention);
            cbCacheRetention.Text = (string)cbCacheRetention.Items[index];
            btnServiceStartStop_Click(null, null);
        }

        private void cbCacheRetention_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.CacheRetention = daysRetention[cbCacheRetention.SelectedIndex];
        }

        private void btnServiceStartStop_Click(object sender, EventArgs e)
        {
            if ((Button)sender == btnServiceStart) UdpFunctions.StartService();
            else if ((Button)sender == btnServiceStop) UdpFunctions.StopService();
            btnServiceStop.Enabled = !(btnServiceStart.Enabled = !UdpFunctions.ServiceRunning());
        }

        private void cbIpAddress_CheckedChanged(object sender, EventArgs e)
        {
            cmbIpAddresses.Enabled = ckIpAddress.Checked;
            if (ckIpAddress.Checked && cmbIpAddresses.SelectedIndex < 0 && cmbIpAddresses.Items.Count > 0)
            {
                cmbIpAddresses.SelectedIndex = 0;
                Config.UseIpAddress = cmbIpAddresses.SelectedItem.ToString();
            }
            else Config.UseIpAddress = null;
        }

        private void cbIpAddresses_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.UseIpAddress = cmbIpAddresses.SelectedItem.ToString();
        }

        private void txtBaseApi_TextChanged(object sender, EventArgs e)
        {
            Config.BaseApiUrl = txtBaseApi.Text;
        }

        private void txtBaseArtwork_TextChanged(object sender, EventArgs e)
        {
            Config.BaseArtworkUrl = txtBaseArtwork.Text;
        }

        private void ckDebug_CheckedChanged(object sender, EventArgs e)
        {
            Config.UseDebug = ckDebug.Checked;
        }

        private void btnChangeServer_Click(object sender, EventArgs e)
        {
            // load configuration file and set component states/values
            LoadConfigurationFile(true);

            // complete the title bar label with version number
            var info = string.Empty;
            if (Helper.InstallMethod == Helper.Installation.PORTABLE) info = " (PORTABLE)";
            else if (Helper.InstallMethod == Helper.Installation.CLIENT) info = $" (REMOTE: {_BaseServerAddress})";
            Text += $" v{Helper.Epg123Version}{info}";

            // initialize the schedules direct api
            if (Helper.InstallMethod != Helper.Installation.PORTABLE)
            {
                var baseApi = $"{_BaseServerAddress}epg123/";
                SdApi.Initialize($"EPG123/{Helper.Epg123Version}", baseApi, Config.BaseArtworkUrl, Config.UseDebug);
            }
            else SdApi.Initialize($"EPG123/{Helper.Epg123Version}", Config.BaseApiUrl, Config.BaseArtworkUrl, Config.UseDebug);

            // login to Schedules Direct and get a token
            if (Login(Config.UserAccount?.LoginName, Config.UserAccount?.PasswordHash))
            {
                BuildLineupsAndStations();
                _newLogin = false;
            }
        }

        private void txtBaseApi_Validating(object sender, CancelEventArgs e)
        {
            if (txtBaseApi.Text == _originalConfig.BaseApiUrl) return;
            if (string.IsNullOrEmpty(txtBaseApi.Text)) { txtBaseApi.Text = _originalConfig.BaseApiUrl; return; }
            if (!(Uri.TryCreate(txtBaseApi.Text, UriKind.Absolute, out Uri baseUri) && (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps)))
            {
                MessageBox.Show("Not a valid http address. Don't forget the leading \"http\" or \"https\".", "Input Error");
                e.Cancel = true;
                return;
            }
            if (!TryHttpAddress(txtBaseApi.Text))
            {
                MessageBox.Show("Not a valid Schedules Direct address.", "URL Incorrect");
                txtBaseApi.Text = _originalConfig.BaseApiUrl;
                e.Cancel = true;
            }
        }

        private void txtBaseArtwork_Validating(object sender, CancelEventArgs e)
        {
            if (txtBaseArtwork.Text == _originalConfig.BaseArtworkUrl) return;
            if (string.IsNullOrEmpty(txtBaseArtwork.Text)) { txtBaseArtwork.Text = _originalConfig.BaseArtworkUrl; return; }
            if (!(Uri.TryCreate(txtBaseArtwork.Text, UriKind.Absolute, out Uri baseUri) && (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps)))
            {
                MessageBox.Show("Not a valid http address. Don't forget the leading \"http\" or \"https\".", "Input Error");
                e.Cancel = true;
                return;
            }
            if (!TryHttpAddress(txtBaseArtwork.Text))
            {
                MessageBox.Show("Not a valid Schedules Direct address.", "URL Incorrect");
                txtBaseArtwork.Text = _originalConfig.BaseArtworkUrl;
                e.Cancel = true;
            }
        }

        private bool TryHttpAddress(string url)
        {
            try
            {
                var request = WebRequest.Create($"{url}available");
                request.GetResponse();
                return true;
            }
            catch { }
            return false;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel2.LinkVisited = true;
            Process.Start($"{_BaseServerAddress}server.log");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel3.LinkVisited = true;
            Process.Start(_BaseServerAddress);
        }
        #endregion

        private void btnEmail_Click(object sender, EventArgs e)
        {
            var emailForm = new frmEmail();
            emailForm.ShowDialog();
        }
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
