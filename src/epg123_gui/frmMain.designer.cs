namespace epg123_gui
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabConfigs = new System.Windows.Forms.TabControl();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.pnlSize = new System.Windows.Forms.Panel();
            this.rdoSm = new System.Windows.Forms.RadioButton();
            this.rdoMd = new System.Windows.Forms.RadioButton();
            this.rdoLg = new System.Windows.Forms.RadioButton();
            this.pnlAspect = new System.Windows.Forms.Panel();
            this.rdo3x4 = new System.Windows.Forms.RadioButton();
            this.rdo2x3 = new System.Windows.Forms.RadioButton();
            this.rdo4x3 = new System.Windows.Forms.RadioButton();
            this.rdo16x9 = new System.Windows.Forms.RadioButton();
            this.lblAspect = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.cbBrandLogo = new System.Windows.Forms.CheckBox();
            this.btnRemoveOrphans = new System.Windows.Forms.Button();
            this.cbOadOverride = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cbSeasonEventImages = new System.Windows.Forms.CheckBox();
            this.cbNoCastCrew = new System.Windows.Forms.CheckBox();
            this.cbAlternateSEFormat = new System.Windows.Forms.CheckBox();
            this.lblPreferredLogos = new System.Windows.Forms.Label();
            this.cmbPreferredLogos = new System.Windows.Forms.ComboBox();
            this.cbSdLogos = new System.Windows.Forms.CheckBox();
            this.cbTVDB = new System.Windows.Forms.CheckBox();
            this.cbPrefixTitle = new System.Windows.Forms.CheckBox();
            this.cbPrefixDescription = new System.Windows.Forms.CheckBox();
            this.cbAppendDescription = new System.Windows.Forms.CheckBox();
            this.cbModernMedia = new System.Windows.Forms.CheckBox();
            this.numDays = new System.Windows.Forms.NumericUpDown();
            this.lblDaysDownload = new System.Windows.Forms.Label();
            this.cbAddNewStations = new System.Windows.Forms.CheckBox();
            this.tabXmltv = new System.Windows.Forms.TabPage();
            this.cbXmltvSingleImage = new System.Windows.Forms.CheckBox();
            this.ckXmltvExtendedInfo = new System.Windows.Forms.CheckBox();
            this.lblXmltvLogosNote = new System.Windows.Forms.Label();
            this.rtbFillerDescription = new System.Windows.Forms.RichTextBox();
            this.lblFillerDuration = new System.Windows.Forms.Label();
            this.numFillerDuration = new System.Windows.Forms.NumericUpDown();
            this.ckXmltvFillerData = new System.Windows.Forms.CheckBox();
            this.cbXmltv = new System.Windows.Forms.CheckBox();
            this.ckLocalLogos = new System.Windows.Forms.CheckBox();
            this.ckUrlLogos = new System.Windows.Forms.CheckBox();
            this.ckChannelLogos = new System.Windows.Forms.CheckBox();
            this.ckChannelNumbers = new System.Windows.Forms.CheckBox();
            this.tabTask = new System.Windows.Forms.TabPage();
            this.cbAutomatch = new System.Windows.Forms.CheckBox();
            this.lblSchedStatus = new System.Windows.Forms.Label();
            this.cbImport = new System.Windows.Forms.CheckBox();
            this.cbTaskWake = new System.Windows.Forms.CheckBox();
            this.tbSchedTime = new System.Windows.Forms.MaskedTextBox();
            this.lblUpdateTime = new System.Windows.Forms.Label();
            this.tabService = new System.Windows.Forms.TabPage();
            this.ckDebug = new System.Windows.Forms.CheckBox();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.btnChangeServer = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBaseArtwork = new System.Windows.Forms.TextBox();
            this.txtBaseApi = new System.Windows.Forms.TextBox();
            this.cmbIpAddresses = new System.Windows.Forms.ComboBox();
            this.ckIpAddress = new System.Windows.Forms.CheckBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbCacheRetention = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnServiceStart = new System.Windows.Forms.Button();
            this.btnServiceStop = new System.Windows.Forms.Button();
            this.tabNotifier = new System.Windows.Forms.TabPage();
            this.btnEmail = new System.Windows.Forms.Button();
            this.grpAccount = new System.Windows.Forms.GroupBox();
            this.btnClientLineups = new System.Windows.Forms.Button();
            this.txtAcctExpires = new System.Windows.Forms.TextBox();
            this.lblExpiration = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblLogin = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtLoginName = new System.Windows.Forms.TextBox();
            this.tabLineups = new System.Windows.Forms.TabControl();
            this.tabLineup = new System.Windows.Forms.TabPage();
            this.lvLineupChannels = new System.Windows.Forms.ListView();
            this.columnHeader21 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader22 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader23 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader24 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lineupMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToClipboardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip6 = new System.Windows.Forms.ToolStrip();
            this.btnIncludeExclude = new System.Windows.Forms.ToolStripDropDownButton();
            this.menuInclude = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExclude = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDiscardNumbers = new System.Windows.Forms.ToolStripMenuItem();
            this.comboLineups = new System.Windows.Forms.ToolStripComboBox();
            this.btnSelectAll = new System.Windows.Forms.ToolStripButton();
            this.btnSelectNone = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.labelLineupCounts = new System.Windows.Forms.ToolStripLabel();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnClearCache = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnTask = new epg123_gui.ElevatedButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabConfigs.SuspendLayout();
            this.tabConfig.SuspendLayout();
            this.pnlSize.SuspendLayout();
            this.pnlAspect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDays)).BeginInit();
            this.tabXmltv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFillerDuration)).BeginInit();
            this.tabTask.SuspendLayout();
            this.tabService.SuspendLayout();
            this.tabNotifier.SuspendLayout();
            this.grpAccount.SuspendLayout();
            this.tabLineups.SuspendLayout();
            this.tabLineup.SuspendLayout();
            this.lineupMenuStrip.SuspendLayout();
            this.toolStrip6.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabConfigs);
            this.splitContainer1.Panel1.Controls.Add(this.grpAccount);
            this.splitContainer1.Panel1.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer1.Panel1MinSize = 346;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabLineups);
            this.splitContainer1.Panel2MinSize = 340;
            this.splitContainer1.Size = new System.Drawing.Size(784, 526);
            this.splitContainer1.SplitterDistance = 346;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabConfigs
            // 
            this.tabConfigs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tabConfigs.Controls.Add(this.tabConfig);
            this.tabConfigs.Controls.Add(this.tabXmltv);
            this.tabConfigs.Controls.Add(this.tabTask);
            this.tabConfigs.Controls.Add(this.tabService);
            this.tabConfigs.Controls.Add(this.tabNotifier);
            this.tabConfigs.Enabled = false;
            this.tabConfigs.HotTrack = true;
            this.tabConfigs.Location = new System.Drawing.Point(12, 105);
            this.tabConfigs.Name = "tabConfigs";
            this.tabConfigs.SelectedIndex = 0;
            this.tabConfigs.Size = new System.Drawing.Size(331, 421);
            this.tabConfigs.TabIndex = 10;
            // 
            // tabConfig
            // 
            this.tabConfig.AutoScroll = true;
            this.tabConfig.BackColor = System.Drawing.SystemColors.Control;
            this.tabConfig.Controls.Add(this.pnlSize);
            this.tabConfig.Controls.Add(this.pnlAspect);
            this.tabConfig.Controls.Add(this.lblAspect);
            this.tabConfig.Controls.Add(this.lblSize);
            this.tabConfig.Controls.Add(this.cbBrandLogo);
            this.tabConfig.Controls.Add(this.btnRemoveOrphans);
            this.tabConfig.Controls.Add(this.cbOadOverride);
            this.tabConfig.Controls.Add(this.pictureBox1);
            this.tabConfig.Controls.Add(this.cbSeasonEventImages);
            this.tabConfig.Controls.Add(this.cbNoCastCrew);
            this.tabConfig.Controls.Add(this.cbAlternateSEFormat);
            this.tabConfig.Controls.Add(this.lblPreferredLogos);
            this.tabConfig.Controls.Add(this.cmbPreferredLogos);
            this.tabConfig.Controls.Add(this.cbSdLogos);
            this.tabConfig.Controls.Add(this.cbTVDB);
            this.tabConfig.Controls.Add(this.cbPrefixTitle);
            this.tabConfig.Controls.Add(this.cbPrefixDescription);
            this.tabConfig.Controls.Add(this.cbAppendDescription);
            this.tabConfig.Controls.Add(this.cbModernMedia);
            this.tabConfig.Controls.Add(this.numDays);
            this.tabConfig.Controls.Add(this.lblDaysDownload);
            this.tabConfig.Controls.Add(this.cbAddNewStations);
            this.tabConfig.Location = new System.Drawing.Point(4, 22);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfig.Size = new System.Drawing.Size(323, 395);
            this.tabConfig.TabIndex = 2;
            this.tabConfig.Text = "Configuration";
            // 
            // pnlSize
            // 
            this.pnlSize.Controls.Add(this.rdoSm);
            this.pnlSize.Controls.Add(this.rdoMd);
            this.pnlSize.Controls.Add(this.rdoLg);
            this.pnlSize.Location = new System.Drawing.Point(88, 221);
            this.pnlSize.Name = "pnlSize";
            this.pnlSize.Size = new System.Drawing.Size(214, 22);
            this.pnlSize.TabIndex = 48;
            // 
            // rdoSm
            // 
            this.rdoSm.AutoSize = true;
            this.rdoSm.Location = new System.Drawing.Point(3, 3);
            this.rdoSm.Name = "rdoSm";
            this.rdoSm.Size = new System.Drawing.Size(50, 17);
            this.rdoSm.TabIndex = 0;
            this.rdoSm.TabStop = true;
            this.rdoSm.Text = "Small";
            this.rdoSm.UseVisualStyleBackColor = true;
            this.rdoSm.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // rdoMd
            // 
            this.rdoMd.AutoSize = true;
            this.rdoMd.Location = new System.Drawing.Point(59, 3);
            this.rdoMd.Name = "rdoMd";
            this.rdoMd.Size = new System.Drawing.Size(62, 17);
            this.rdoMd.TabIndex = 1;
            this.rdoMd.TabStop = true;
            this.rdoMd.Text = "Medium";
            this.rdoMd.UseVisualStyleBackColor = true;
            this.rdoMd.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // rdoLg
            // 
            this.rdoLg.AutoSize = true;
            this.rdoLg.Location = new System.Drawing.Point(127, 3);
            this.rdoLg.Name = "rdoLg";
            this.rdoLg.Size = new System.Drawing.Size(52, 17);
            this.rdoLg.TabIndex = 2;
            this.rdoLg.TabStop = true;
            this.rdoLg.Text = "Large";
            this.rdoLg.UseVisualStyleBackColor = true;
            this.rdoLg.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // pnlAspect
            // 
            this.pnlAspect.Controls.Add(this.rdo3x4);
            this.pnlAspect.Controls.Add(this.rdo2x3);
            this.pnlAspect.Controls.Add(this.rdo4x3);
            this.pnlAspect.Controls.Add(this.rdo16x9);
            this.pnlAspect.Location = new System.Drawing.Point(88, 198);
            this.pnlAspect.Name = "pnlAspect";
            this.pnlAspect.Size = new System.Drawing.Size(214, 22);
            this.pnlAspect.TabIndex = 47;
            // 
            // rdo3x4
            // 
            this.rdo3x4.AutoSize = true;
            this.rdo3x4.Location = new System.Drawing.Point(51, 3);
            this.rdo3x4.Name = "rdo3x4";
            this.rdo3x4.Size = new System.Drawing.Size(42, 17);
            this.rdo3x4.TabIndex = 46;
            this.rdo3x4.TabStop = true;
            this.rdo3x4.Text = "3x4";
            this.rdo3x4.UseVisualStyleBackColor = true;
            // 
            // rdo2x3
            // 
            this.rdo2x3.AutoSize = true;
            this.rdo2x3.Location = new System.Drawing.Point(3, 3);
            this.rdo2x3.Name = "rdo2x3";
            this.rdo2x3.Size = new System.Drawing.Size(42, 17);
            this.rdo2x3.TabIndex = 43;
            this.rdo2x3.TabStop = true;
            this.rdo2x3.Text = "2x3";
            this.rdo2x3.UseVisualStyleBackColor = true;
            this.rdo2x3.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // rdo4x3
            // 
            this.rdo4x3.AutoSize = true;
            this.rdo4x3.Location = new System.Drawing.Point(99, 3);
            this.rdo4x3.Name = "rdo4x3";
            this.rdo4x3.Size = new System.Drawing.Size(42, 17);
            this.rdo4x3.TabIndex = 44;
            this.rdo4x3.TabStop = true;
            this.rdo4x3.Text = "4x3";
            this.rdo4x3.UseVisualStyleBackColor = true;
            this.rdo4x3.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // rdo16x9
            // 
            this.rdo16x9.AutoSize = true;
            this.rdo16x9.Location = new System.Drawing.Point(147, 3);
            this.rdo16x9.Name = "rdo16x9";
            this.rdo16x9.Size = new System.Drawing.Size(48, 17);
            this.rdo16x9.TabIndex = 45;
            this.rdo16x9.TabStop = true;
            this.rdo16x9.Text = "16x9";
            this.rdo16x9.UseVisualStyleBackColor = true;
            this.rdo16x9.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // lblAspect
            // 
            this.lblAspect.AutoSize = true;
            this.lblAspect.Location = new System.Drawing.Point(6, 203);
            this.lblAspect.Name = "lblAspect";
            this.lblAspect.Size = new System.Drawing.Size(76, 13);
            this.lblAspect.TabIndex = 46;
            this.lblAspect.Text = "Series Images:";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(6, 223);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(67, 13);
            this.lblSize.TabIndex = 42;
            this.lblSize.Text = "Image Sizes:";
            // 
            // cbBrandLogo
            // 
            this.cbBrandLogo.AutoSize = true;
            this.cbBrandLogo.Location = new System.Drawing.Point(6, 347);
            this.cbBrandLogo.Name = "cbBrandLogo";
            this.cbBrandLogo.Size = new System.Drawing.Size(294, 17);
            this.cbBrandLogo.TabIndex = 41;
            this.cbBrandLogo.Text = "Add status logo in channel guide (viewable by extenders)";
            this.cbBrandLogo.UseVisualStyleBackColor = true;
            this.cbBrandLogo.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // btnRemoveOrphans
            // 
            this.btnRemoveOrphans.Location = new System.Drawing.Point(79, 318);
            this.btnRemoveOrphans.Name = "btnRemoveOrphans";
            this.btnRemoveOrphans.Size = new System.Drawing.Size(160, 23);
            this.btnRemoveOrphans.TabIndex = 40;
            this.btnRemoveOrphans.Text = "Remove Orphaned Logos";
            this.btnRemoveOrphans.UseVisualStyleBackColor = true;
            this.btnRemoveOrphans.Click += new System.EventHandler(this.btnRemoveOrphans_Click);
            // 
            // cbOadOverride
            // 
            this.cbOadOverride.AutoSize = true;
            this.cbOadOverride.Location = new System.Drawing.Point(6, 179);
            this.cbOadOverride.Name = "cbOadOverride";
            this.cbOadOverride.Size = new System.Drawing.Size(232, 17);
            this.cbOadOverride.TabIndex = 9;
            this.cbOadOverride.Text = "Allow NEW flag to override Original Air Date";
            this.cbOadOverride.UseVisualStyleBackColor = true;
            this.cbOadOverride.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(286, 294);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 39;
            this.pictureBox1.TabStop = false;
            // 
            // cbSeasonEventImages
            // 
            this.cbSeasonEventImages.AutoSize = true;
            this.cbSeasonEventImages.Location = new System.Drawing.Point(6, 245);
            this.cbSeasonEventImages.Name = "cbSeasonEventImages";
            this.cbSeasonEventImages.Size = new System.Drawing.Size(211, 17);
            this.cbSeasonEventImages.TabIndex = 12;
            this.cbSeasonEventImages.Text = "Include season and sport event images";
            this.cbSeasonEventImages.UseVisualStyleBackColor = true;
            this.cbSeasonEventImages.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbNoCastCrew
            // 
            this.cbNoCastCrew.AccessibleName = "do not include cast and crew";
            this.cbNoCastCrew.AutoSize = true;
            this.cbNoCastCrew.Location = new System.Drawing.Point(6, 393);
            this.cbNoCastCrew.Name = "cbNoCastCrew";
            this.cbNoCastCrew.Size = new System.Drawing.Size(247, 17);
            this.cbNoCastCrew.TabIndex = 17;
            this.cbNoCastCrew.Text = "Slim MXF/XMLTV - do not include Cast && Crew";
            this.cbNoCastCrew.UseVisualStyleBackColor = true;
            this.cbNoCastCrew.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbAlternateSEFormat
            // 
            this.cbAlternateSEFormat.AutoSize = true;
            this.cbAlternateSEFormat.Enabled = false;
            this.cbAlternateSEFormat.Location = new System.Drawing.Point(6, 110);
            this.cbAlternateSEFormat.Name = "cbAlternateSEFormat";
            this.cbAlternateSEFormat.Size = new System.Drawing.Size(295, 17);
            this.cbAlternateSEFormat.TabIndex = 6;
            this.cbAlternateSEFormat.Text = "Use season/episode format \"S1:E2\" instead of \"s01e02\"";
            this.cbAlternateSEFormat.UseVisualStyleBackColor = true;
            this.cbAlternateSEFormat.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // lblPreferredLogos
            // 
            this.lblPreferredLogos.AutoSize = true;
            this.lblPreferredLogos.Location = new System.Drawing.Point(15, 294);
            this.lblPreferredLogos.Name = "lblPreferredLogos";
            this.lblPreferredLogos.Size = new System.Drawing.Size(99, 13);
            this.lblPreferredLogos.TabIndex = 32;
            this.lblPreferredLogos.Text = "Preferred SD logos:";
            // 
            // cmbPreferredLogos
            // 
            this.cmbPreferredLogos.AccessibleName = "perferred schedules direct logos";
            this.cmbPreferredLogos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPreferredLogos.FormattingEnabled = true;
            this.cmbPreferredLogos.Items.AddRange(new object[] {
            "white logos",
            "logos for dark backgrounds",
            "logos for light backgrounds",
            "gray logos",
            "none (custom logos only)"});
            this.cmbPreferredLogos.Location = new System.Drawing.Point(120, 291);
            this.cmbPreferredLogos.Name = "cmbPreferredLogos";
            this.cmbPreferredLogos.Size = new System.Drawing.Size(160, 21);
            this.cmbPreferredLogos.TabIndex = 15;
            this.cmbPreferredLogos.SelectedIndexChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // cbSdLogos
            // 
            this.cbSdLogos.AccessibleName = "include station logos in local logos folder";
            this.cbSdLogos.AutoSize = true;
            this.cbSdLogos.Location = new System.Drawing.Point(6, 268);
            this.cbSdLogos.Name = "cbSdLogos";
            this.cbSdLogos.Size = new System.Drawing.Size(199, 17);
            this.cbSdLogos.TabIndex = 14;
            this.cbSdLogos.Text = "Include station logos in .\\logos folder";
            this.cbSdLogos.UseVisualStyleBackColor = true;
            this.cbSdLogos.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // cbTVDB
            // 
            this.cbTVDB.AutoSize = true;
            this.cbTVDB.Location = new System.Drawing.Point(6, 41);
            this.cbTVDB.Name = "cbTVDB";
            this.cbTVDB.Size = new System.Drawing.Size(278, 17);
            this.cbTVDB.TabIndex = 3;
            this.cbTVDB.Text = "Use external season and episode numbers if provided";
            this.cbTVDB.UseVisualStyleBackColor = true;
            this.cbTVDB.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbPrefixTitle
            // 
            this.cbPrefixTitle.AutoSize = true;
            this.cbPrefixTitle.Location = new System.Drawing.Point(6, 64);
            this.cbPrefixTitle.Name = "cbPrefixTitle";
            this.cbPrefixTitle.Size = new System.Drawing.Size(274, 17);
            this.cbPrefixTitle.TabIndex = 4;
            this.cbPrefixTitle.Text = "Prefix episode title with season and episode numbers";
            this.cbPrefixTitle.UseVisualStyleBackColor = true;
            this.cbPrefixTitle.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbPrefixDescription
            // 
            this.cbPrefixDescription.AccessibleName = "prefix episode description with season and episode numbers";
            this.cbPrefixDescription.AutoSize = true;
            this.cbPrefixDescription.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbPrefixDescription.Location = new System.Drawing.Point(6, 87);
            this.cbPrefixDescription.Name = "cbPrefixDescription";
            this.cbPrefixDescription.Size = new System.Drawing.Size(281, 17);
            this.cbPrefixDescription.TabIndex = 5;
            this.cbPrefixDescription.Text = "Prefix episode desc with season and episode numbers";
            this.cbPrefixDescription.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbPrefixDescription.UseVisualStyleBackColor = true;
            this.cbPrefixDescription.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbAppendDescription
            // 
            this.cbAppendDescription.AccessibleName = "append episode description with season and episode numbers";
            this.cbAppendDescription.AutoSize = true;
            this.cbAppendDescription.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbAppendDescription.Location = new System.Drawing.Point(6, 133);
            this.cbAppendDescription.Name = "cbAppendDescription";
            this.cbAppendDescription.Size = new System.Drawing.Size(292, 17);
            this.cbAppendDescription.TabIndex = 7;
            this.cbAppendDescription.Text = "Append episode desc with season and episode numbers";
            this.cbAppendDescription.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbAppendDescription.UseVisualStyleBackColor = true;
            this.cbAppendDescription.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbModernMedia
            // 
            this.cbModernMedia.AutoSize = true;
            this.cbModernMedia.Location = new System.Drawing.Point(6, 370);
            this.cbModernMedia.Name = "cbModernMedia";
            this.cbModernMedia.Size = new System.Drawing.Size(199, 17);
            this.cbModernMedia.TabIndex = 16;
            this.cbModernMedia.Text = "Create ModernMedia UI+ support file";
            this.cbModernMedia.UseVisualStyleBackColor = true;
            this.cbModernMedia.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // numDays
            // 
            this.numDays.AccessibleDescription = "";
            this.numDays.AccessibleName = "days to download";
            this.numDays.Location = new System.Drawing.Point(6, 15);
            this.numDays.Maximum = new decimal(new int[] {
            21,
            0,
            0,
            0});
            this.numDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDays.Name = "numDays";
            this.numDays.Size = new System.Drawing.Size(40, 20);
            this.numDays.TabIndex = 1;
            this.numDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numDays.Value = new decimal(new int[] {
            14,
            0,
            0,
            0});
            this.numDays.ValueChanged += new System.EventHandler(this.configs_Changed);
            // 
            // lblDaysDownload
            // 
            this.lblDaysDownload.AutoSize = true;
            this.lblDaysDownload.Location = new System.Drawing.Point(52, 17);
            this.lblDaysDownload.Name = "lblDaysDownload";
            this.lblDaysDownload.Size = new System.Drawing.Size(172, 13);
            this.lblDaysDownload.TabIndex = 2;
            this.lblDaysDownload.Text = "days of schedule data to download";
            // 
            // cbAddNewStations
            // 
            this.cbAddNewStations.AutoSize = true;
            this.cbAddNewStations.Location = new System.Drawing.Point(6, 156);
            this.cbAddNewStations.Name = "cbAddNewStations";
            this.cbAddNewStations.Size = new System.Drawing.Size(246, 17);
            this.cbAddNewStations.TabIndex = 8;
            this.cbAddNewStations.Text = "Automatically download new stations in lineups";
            this.cbAddNewStations.UseVisualStyleBackColor = true;
            this.cbAddNewStations.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // tabXmltv
            // 
            this.tabXmltv.BackColor = System.Drawing.SystemColors.Control;
            this.tabXmltv.Controls.Add(this.cbXmltvSingleImage);
            this.tabXmltv.Controls.Add(this.ckXmltvExtendedInfo);
            this.tabXmltv.Controls.Add(this.lblXmltvLogosNote);
            this.tabXmltv.Controls.Add(this.rtbFillerDescription);
            this.tabXmltv.Controls.Add(this.lblFillerDuration);
            this.tabXmltv.Controls.Add(this.numFillerDuration);
            this.tabXmltv.Controls.Add(this.ckXmltvFillerData);
            this.tabXmltv.Controls.Add(this.cbXmltv);
            this.tabXmltv.Controls.Add(this.ckLocalLogos);
            this.tabXmltv.Controls.Add(this.ckUrlLogos);
            this.tabXmltv.Controls.Add(this.ckChannelLogos);
            this.tabXmltv.Controls.Add(this.ckChannelNumbers);
            this.tabXmltv.Location = new System.Drawing.Point(4, 22);
            this.tabXmltv.Name = "tabXmltv";
            this.tabXmltv.Padding = new System.Windows.Forms.Padding(3);
            this.tabXmltv.Size = new System.Drawing.Size(323, 395);
            this.tabXmltv.TabIndex = 3;
            this.tabXmltv.Text = "XMLTV";
            // 
            // cbXmltvSingleImage
            // 
            this.cbXmltvSingleImage.AccessibleName = "do not include additional image formats";
            this.cbXmltvSingleImage.AutoSize = true;
            this.cbXmltvSingleImage.Location = new System.Drawing.Point(6, 245);
            this.cbXmltvSingleImage.Name = "cbXmltvSingleImage";
            this.cbXmltvSingleImage.Size = new System.Drawing.Size(277, 17);
            this.cbXmltvSingleImage.TabIndex = 9;
            this.cbXmltvSingleImage.Text = "Trim XMLTV - do not include additional image formats";
            this.cbXmltvSingleImage.UseVisualStyleBackColor = true;
            this.cbXmltvSingleImage.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckXmltvExtendedInfo
            // 
            this.ckXmltvExtendedInfo.AutoSize = true;
            this.ckXmltvExtendedInfo.Location = new System.Drawing.Point(6, 222);
            this.ckXmltvExtendedInfo.Name = "ckXmltvExtendedInfo";
            this.ckXmltvExtendedInfo.Size = new System.Drawing.Size(199, 17);
            this.ckXmltvExtendedInfo.TabIndex = 8;
            this.ckXmltvExtendedInfo.Text = "Add extended info before description\r\n";
            this.ckXmltvExtendedInfo.UseVisualStyleBackColor = true;
            this.ckXmltvExtendedInfo.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // lblXmltvLogosNote
            // 
            this.lblXmltvLogosNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblXmltvLogosNote.Location = new System.Drawing.Point(7, 355);
            this.lblXmltvLogosNote.Name = "lblXmltvLogosNote";
            this.lblXmltvLogosNote.Size = new System.Drawing.Size(297, 30);
            this.lblXmltvLogosNote.TabIndex = 28;
            this.lblXmltvLogosNote.Text = "* The option to \'Include station logos in .\\logos folder\' must be enabled in the " +
    "Configuration tab.";
            // 
            // rtbFillerDescription
            // 
            this.rtbFillerDescription.AccessibleName = "filler program description edit";
            this.rtbFillerDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbFillerDescription.Location = new System.Drawing.Point(23, 170);
            this.rtbFillerDescription.Name = "rtbFillerDescription";
            this.rtbFillerDescription.Size = new System.Drawing.Size(280, 46);
            this.rtbFillerDescription.TabIndex = 7;
            this.rtbFillerDescription.Text = "This program was generated by EPG123 to provide filler data for stations that did" +
    " not receive any guide listings from the upstream source.";
            this.rtbFillerDescription.TextChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // lblFillerDuration
            // 
            this.lblFillerDuration.AutoSize = true;
            this.lblFillerDuration.Location = new System.Drawing.Point(65, 146);
            this.lblFillerDuration.Name = "lblFillerDuration";
            this.lblFillerDuration.Size = new System.Drawing.Size(143, 13);
            this.lblFillerDuration.TabIndex = 22;
            this.lblFillerDuration.Text = "hour duration of filler program";
            // 
            // numFillerDuration
            // 
            this.numFillerDuration.AccessibleName = "hour duration of filler programs";
            this.numFillerDuration.Location = new System.Drawing.Point(24, 144);
            this.numFillerDuration.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numFillerDuration.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numFillerDuration.Name = "numFillerDuration";
            this.numFillerDuration.Size = new System.Drawing.Size(35, 20);
            this.numFillerDuration.TabIndex = 6;
            this.numFillerDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numFillerDuration.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numFillerDuration.ValueChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckXmltvFillerData
            // 
            this.ckXmltvFillerData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ckXmltvFillerData.AutoSize = true;
            this.ckXmltvFillerData.Checked = true;
            this.ckXmltvFillerData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckXmltvFillerData.Location = new System.Drawing.Point(6, 121);
            this.ckXmltvFillerData.Name = "ckXmltvFillerData";
            this.ckXmltvFillerData.Size = new System.Drawing.Size(294, 17);
            this.ckXmltvFillerData.TabIndex = 5;
            this.ckXmltvFillerData.Text = "Create filler programs for stations that have no guide data";
            this.ckXmltvFillerData.UseVisualStyleBackColor = true;
            this.ckXmltvFillerData.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // cbXmltv
            // 
            this.cbXmltv.AutoSize = true;
            this.cbXmltv.Checked = true;
            this.cbXmltv.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbXmltv.Location = new System.Drawing.Point(6, 6);
            this.cbXmltv.Name = "cbXmltv";
            this.cbXmltv.Size = new System.Drawing.Size(112, 17);
            this.cbXmltv.TabIndex = 0;
            this.cbXmltv.Text = "Create XMLTV file";
            this.cbXmltv.UseVisualStyleBackColor = true;
            this.cbXmltv.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckLocalLogos
            // 
            this.ckLocalLogos.AccessibleName = "use local images from local logos folder";
            this.ckLocalLogos.AutoSize = true;
            this.ckLocalLogos.Location = new System.Drawing.Point(24, 98);
            this.ckLocalLogos.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.ckLocalLogos.Name = "ckLocalLogos";
            this.ckLocalLogos.Size = new System.Drawing.Size(201, 17);
            this.ckLocalLogos.TabIndex = 4;
            this.ckLocalLogos.Text = "Use local images from .\\logos folder *";
            this.ckLocalLogos.UseVisualStyleBackColor = true;
            this.ckLocalLogos.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckUrlLogos
            // 
            this.ckUrlLogos.AutoSize = true;
            this.ckUrlLogos.Location = new System.Drawing.Point(24, 75);
            this.ckUrlLogos.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.ckUrlLogos.Name = "ckUrlLogos";
            this.ckUrlLogos.Size = new System.Drawing.Size(219, 17);
            this.ckUrlLogos.TabIndex = 3;
            this.ckUrlLogos.Text = "Use linked images from Schedules Direct";
            this.ckUrlLogos.UseVisualStyleBackColor = true;
            this.ckUrlLogos.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckChannelLogos
            // 
            this.ckChannelLogos.AutoSize = true;
            this.ckChannelLogos.Checked = true;
            this.ckChannelLogos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckChannelLogos.Location = new System.Drawing.Point(6, 52);
            this.ckChannelLogos.Name = "ckChannelLogos";
            this.ckChannelLogos.Size = new System.Drawing.Size(130, 17);
            this.ckChannelLogos.TabIndex = 2;
            this.ckChannelLogos.Text = "Include channel logos";
            this.ckChannelLogos.UseVisualStyleBackColor = true;
            this.ckChannelLogos.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckChannelNumbers
            // 
            this.ckChannelNumbers.AutoSize = true;
            this.ckChannelNumbers.Location = new System.Drawing.Point(6, 29);
            this.ckChannelNumbers.Name = "ckChannelNumbers";
            this.ckChannelNumbers.Size = new System.Drawing.Size(145, 17);
            this.ckChannelNumbers.TabIndex = 1;
            this.ckChannelNumbers.Text = "Include channel numbers";
            this.ckChannelNumbers.UseVisualStyleBackColor = true;
            this.ckChannelNumbers.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // tabTask
            // 
            this.tabTask.BackColor = System.Drawing.SystemColors.Control;
            this.tabTask.Controls.Add(this.cbAutomatch);
            this.tabTask.Controls.Add(this.lblSchedStatus);
            this.tabTask.Controls.Add(this.btnTask);
            this.tabTask.Controls.Add(this.cbImport);
            this.tabTask.Controls.Add(this.cbTaskWake);
            this.tabTask.Controls.Add(this.tbSchedTime);
            this.tabTask.Controls.Add(this.lblUpdateTime);
            this.tabTask.Location = new System.Drawing.Point(4, 22);
            this.tabTask.Name = "tabTask";
            this.tabTask.Padding = new System.Windows.Forms.Padding(3);
            this.tabTask.Size = new System.Drawing.Size(323, 395);
            this.tabTask.TabIndex = 4;
            this.tabTask.Text = "Scheduled Task";
            // 
            // cbAutomatch
            // 
            this.cbAutomatch.AutoSize = true;
            this.cbAutomatch.Location = new System.Drawing.Point(21, 51);
            this.cbAutomatch.Name = "cbAutomatch";
            this.cbAutomatch.Size = new System.Drawing.Size(217, 17);
            this.cbAutomatch.TabIndex = 3;
            this.cbAutomatch.Text = "Automatically match stations to channels";
            this.cbAutomatch.UseVisualStyleBackColor = true;
            // 
            // lblSchedStatus
            // 
            this.lblSchedStatus.AutoSize = true;
            this.lblSchedStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSchedStatus.Location = new System.Drawing.Point(6, 71);
            this.lblSchedStatus.Name = "lblSchedStatus";
            this.lblSchedStatus.Size = new System.Drawing.Size(64, 13);
            this.lblSchedStatus.TabIndex = 4;
            this.lblSchedStatus.Text = "Task Status";
            // 
            // cbImport
            // 
            this.cbImport.AutoSize = true;
            this.cbImport.Location = new System.Drawing.Point(6, 30);
            this.cbImport.Name = "cbImport";
            this.cbImport.Size = new System.Drawing.Size(222, 17);
            this.cbImport.TabIndex = 2;
            this.cbImport.Text = "Automatically import guide data into WMC";
            this.cbImport.UseVisualStyleBackColor = true;
            // 
            // cbTaskWake
            // 
            this.cbTaskWake.AccessibleName = "wake computer to run update task";
            this.cbTaskWake.AutoSize = true;
            this.cbTaskWake.Location = new System.Drawing.Point(175, 8);
            this.cbTaskWake.Name = "cbTaskWake";
            this.cbTaskWake.Size = new System.Drawing.Size(55, 17);
            this.cbTaskWake.TabIndex = 1;
            this.cbTaskWake.Text = "Wake";
            this.cbTaskWake.UseVisualStyleBackColor = true;
            // 
            // tbSchedTime
            // 
            this.tbSchedTime.AccessibleName = "scheduled update time";
            this.tbSchedTime.Location = new System.Drawing.Point(131, 6);
            this.tbSchedTime.Mask = "00:00";
            this.tbSchedTime.Name = "tbSchedTime";
            this.tbSchedTime.Size = new System.Drawing.Size(38, 20);
            this.tbSchedTime.TabIndex = 0;
            this.tbSchedTime.TabStop = false;
            this.tbSchedTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbSchedTime.ValidatingType = typeof(System.DateTime);
            // 
            // lblUpdateTime
            // 
            this.lblUpdateTime.AutoSize = true;
            this.lblUpdateTime.Location = new System.Drawing.Point(6, 9);
            this.lblUpdateTime.Name = "lblUpdateTime";
            this.lblUpdateTime.Size = new System.Drawing.Size(119, 13);
            this.lblUpdateTime.TabIndex = 18;
            this.lblUpdateTime.Text = "Scheduled update time:";
            // 
            // tabService
            // 
            this.tabService.BackColor = System.Drawing.SystemColors.Control;
            this.tabService.Controls.Add(this.ckDebug);
            this.tabService.Controls.Add(this.linkLabel3);
            this.tabService.Controls.Add(this.btnChangeServer);
            this.tabService.Controls.Add(this.label5);
            this.tabService.Controls.Add(this.label4);
            this.tabService.Controls.Add(this.txtBaseArtwork);
            this.tabService.Controls.Add(this.txtBaseApi);
            this.tabService.Controls.Add(this.cmbIpAddresses);
            this.tabService.Controls.Add(this.ckIpAddress);
            this.tabService.Controls.Add(this.linkLabel2);
            this.tabService.Controls.Add(this.label3);
            this.tabService.Controls.Add(this.label2);
            this.tabService.Controls.Add(this.cbCacheRetention);
            this.tabService.Controls.Add(this.label1);
            this.tabService.Controls.Add(this.btnServiceStart);
            this.tabService.Controls.Add(this.btnServiceStop);
            this.tabService.Location = new System.Drawing.Point(4, 22);
            this.tabService.Name = "tabService";
            this.tabService.Padding = new System.Windows.Forms.Padding(3);
            this.tabService.Size = new System.Drawing.Size(323, 395);
            this.tabService.TabIndex = 5;
            this.tabService.Text = "Service";
            // 
            // ckDebug
            // 
            this.ckDebug.AutoSize = true;
            this.ckDebug.Location = new System.Drawing.Point(6, 84);
            this.ckDebug.Name = "ckDebug";
            this.ckDebug.Size = new System.Drawing.Size(304, 17);
            this.ckDebug.TabIndex = 17;
            this.ckDebug.Text = "Debug Mode. Use if directed to do so by Schedules Direct.";
            this.ckDebug.UseVisualStyleBackColor = true;
            this.ckDebug.CheckedChanged += new System.EventHandler(this.ckDebug_CheckedChanged);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(9, 285);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(147, 13);
            this.linkLabel3.TabIndex = 16;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "View Server Status Webpage";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // btnChangeServer
            // 
            this.btnChangeServer.Location = new System.Drawing.Point(178, 366);
            this.btnChangeServer.Name = "btnChangeServer";
            this.btnChangeServer.Size = new System.Drawing.Size(125, 23);
            this.btnChangeServer.TabIndex = 15;
            this.btnChangeServer.Text = "Change Server";
            this.btnChangeServer.UseVisualStyleBackColor = true;
            this.btnChangeServer.Click += new System.EventHandler(this.btnChangeServer_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "SD Base Artwork URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "SD Base API URL:";
            // 
            // txtBaseArtwork
            // 
            this.txtBaseArtwork.Location = new System.Drawing.Point(6, 58);
            this.txtBaseArtwork.Name = "txtBaseArtwork";
            this.txtBaseArtwork.Size = new System.Drawing.Size(256, 20);
            this.txtBaseArtwork.TabIndex = 11;
            this.txtBaseArtwork.TextChanged += new System.EventHandler(this.txtBaseArtwork_TextChanged);
            this.txtBaseArtwork.Validating += new System.ComponentModel.CancelEventHandler(this.txtBaseArtwork_Validating);
            // 
            // txtBaseApi
            // 
            this.txtBaseApi.Location = new System.Drawing.Point(6, 19);
            this.txtBaseApi.Name = "txtBaseApi";
            this.txtBaseApi.Size = new System.Drawing.Size(256, 20);
            this.txtBaseApi.TabIndex = 10;
            this.txtBaseApi.TextChanged += new System.EventHandler(this.txtBaseApi_TextChanged);
            this.txtBaseApi.Validating += new System.ComponentModel.CancelEventHandler(this.txtBaseApi_Validating);
            // 
            // cmbIpAddresses
            // 
            this.cmbIpAddresses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIpAddresses.FormattingEnabled = true;
            this.cmbIpAddresses.Location = new System.Drawing.Point(26, 191);
            this.cmbIpAddresses.Name = "cmbIpAddresses";
            this.cmbIpAddresses.Size = new System.Drawing.Size(199, 21);
            this.cmbIpAddresses.TabIndex = 9;
            this.cmbIpAddresses.SelectedIndexChanged += new System.EventHandler(this.cbIpAddresses_SelectedIndexChanged);
            // 
            // ckIpAddress
            // 
            this.ckIpAddress.AutoSize = true;
            this.ckIpAddress.Checked = true;
            this.ckIpAddress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckIpAddress.Location = new System.Drawing.Point(6, 168);
            this.ckIpAddress.Name = "ckIpAddress";
            this.ckIpAddress.Size = new System.Drawing.Size(270, 17);
            this.ckIpAddress.TabIndex = 8;
            this.ckIpAddress.Text = "Use IP address rather than host name in image links";
            this.ckIpAddress.UseVisualStyleBackColor = true;
            this.ckIpAddress.CheckedChanged += new System.EventHandler(this.cbIpAddress_CheckedChanged);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(9, 260);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(90, 13);
            this.linkLabel2.TabIndex = 5;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "View Service Log";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "after last access.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Retain cached images";
            // 
            // cbCacheRetention
            // 
            this.cbCacheRetention.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCacheRetention.FormattingEnabled = true;
            this.cbCacheRetention.Items.AddRange(new object[] {
            "Do not cache",
            "7 days",
            "14 days",
            "30 days",
            "60 days",
            "90 days",
            "180 days",
            "365 days",
            "indefinitely"});
            this.cbCacheRetention.Location = new System.Drawing.Point(128, 122);
            this.cbCacheRetention.Name = "cbCacheRetention";
            this.cbCacheRetention.Size = new System.Drawing.Size(97, 21);
            this.cbCacheRetention.TabIndex = 3;
            this.cbCacheRetention.SelectedIndexChanged += new System.EventHandler(this.cbCacheRetention_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 228);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "EPG123 Server Service";
            // 
            // btnServiceStart
            // 
            this.btnServiceStart.Location = new System.Drawing.Point(150, 223);
            this.btnServiceStart.Name = "btnServiceStart";
            this.btnServiceStart.Size = new System.Drawing.Size(75, 23);
            this.btnServiceStart.TabIndex = 0;
            this.btnServiceStart.Text = "Start";
            this.btnServiceStart.UseVisualStyleBackColor = true;
            this.btnServiceStart.Click += new System.EventHandler(this.btnServiceStartStop_Click);
            // 
            // btnServiceStop
            // 
            this.btnServiceStop.Location = new System.Drawing.Point(231, 223);
            this.btnServiceStop.Name = "btnServiceStop";
            this.btnServiceStop.Size = new System.Drawing.Size(75, 23);
            this.btnServiceStop.TabIndex = 1;
            this.btnServiceStop.Text = "Stop";
            this.btnServiceStop.UseVisualStyleBackColor = true;
            this.btnServiceStop.Click += new System.EventHandler(this.btnServiceStartStop_Click);
            // 
            // tabNotifier
            // 
            this.tabNotifier.BackColor = System.Drawing.SystemColors.Control;
            this.tabNotifier.Controls.Add(this.btnEmail);
            this.tabNotifier.Location = new System.Drawing.Point(4, 22);
            this.tabNotifier.Name = "tabNotifier";
            this.tabNotifier.Padding = new System.Windows.Forms.Padding(3);
            this.tabNotifier.Size = new System.Drawing.Size(323, 395);
            this.tabNotifier.TabIndex = 6;
            this.tabNotifier.Text = "Notifications";
            // 
            // btnEmail
            // 
            this.btnEmail.Location = new System.Drawing.Point(6, 6);
            this.btnEmail.Name = "btnEmail";
            this.btnEmail.Size = new System.Drawing.Size(99, 23);
            this.btnEmail.TabIndex = 0;
            this.btnEmail.Text = "Setup E-mail";
            this.btnEmail.UseVisualStyleBackColor = true;
            this.btnEmail.Click += new System.EventHandler(this.btnEmail_Click);
            // 
            // grpAccount
            // 
            this.grpAccount.Controls.Add(this.btnClientLineups);
            this.grpAccount.Controls.Add(this.txtAcctExpires);
            this.grpAccount.Controls.Add(this.lblExpiration);
            this.grpAccount.Controls.Add(this.btnLogin);
            this.grpAccount.Controls.Add(this.lblPassword);
            this.grpAccount.Controls.Add(this.lblLogin);
            this.grpAccount.Controls.Add(this.txtPassword);
            this.grpAccount.Controls.Add(this.txtLoginName);
            this.grpAccount.Location = new System.Drawing.Point(12, 3);
            this.grpAccount.Name = "grpAccount";
            this.grpAccount.Size = new System.Drawing.Size(327, 96);
            this.grpAccount.TabIndex = 0;
            this.grpAccount.TabStop = false;
            this.grpAccount.Text = "Schedules Direct Account";
            // 
            // btnClientLineups
            // 
            this.btnClientLineups.Enabled = false;
            this.btnClientLineups.Location = new System.Drawing.Point(236, 67);
            this.btnClientLineups.Name = "btnClientLineups";
            this.btnClientLineups.Size = new System.Drawing.Size(75, 23);
            this.btnClientLineups.TabIndex = 4;
            this.btnClientLineups.Text = "Lineups";
            this.btnClientLineups.UseVisualStyleBackColor = true;
            this.btnClientLineups.Click += new System.EventHandler(this.btnClientConfig_Click);
            // 
            // txtAcctExpires
            // 
            this.txtAcctExpires.AccessibleName = "account expires";
            this.txtAcctExpires.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAcctExpires.Location = new System.Drawing.Point(80, 72);
            this.txtAcctExpires.Name = "txtAcctExpires";
            this.txtAcctExpires.ReadOnly = true;
            this.txtAcctExpires.Size = new System.Drawing.Size(150, 13);
            this.txtAcctExpires.TabIndex = 10;
            this.txtAcctExpires.TabStop = false;
            // 
            // lblExpiration
            // 
            this.lblExpiration.AutoSize = true;
            this.lblExpiration.Location = new System.Drawing.Point(30, 72);
            this.lblExpiration.Name = "lblExpiration";
            this.lblExpiration.Size = new System.Drawing.Size(44, 13);
            this.lblExpiration.TabIndex = 5;
            this.lblExpiration.Text = "Expires:";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(236, 17);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(75, 46);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(18, 46);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password:";
            // 
            // lblLogin
            // 
            this.lblLogin.AutoSize = true;
            this.lblLogin.Location = new System.Drawing.Point(7, 20);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(67, 13);
            this.lblLogin.TabIndex = 2;
            this.lblLogin.Text = "Login Name:";
            // 
            // txtPassword
            // 
            this.txtPassword.AccessibleName = "login password";
            this.txtPassword.Location = new System.Drawing.Point(80, 43);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(150, 20);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.WordWrap = false;
            this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLogin_KeyPress);
            // 
            // txtLoginName
            // 
            this.txtLoginName.AccessibleName = "login username";
            this.txtLoginName.Location = new System.Drawing.Point(80, 17);
            this.txtLoginName.Name = "txtLoginName";
            this.txtLoginName.Size = new System.Drawing.Size(150, 20);
            this.txtLoginName.TabIndex = 0;
            this.txtLoginName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLogin_KeyPress);
            // 
            // tabLineups
            // 
            this.tabLineups.Controls.Add(this.tabLineup);
            this.tabLineups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLineups.Location = new System.Drawing.Point(0, 0);
            this.tabLineups.Name = "tabLineups";
            this.tabLineups.SelectedIndex = 0;
            this.tabLineups.Size = new System.Drawing.Size(434, 526);
            this.tabLineups.TabIndex = 0;
            // 
            // tabLineup
            // 
            this.tabLineup.Controls.Add(this.lvLineupChannels);
            this.tabLineup.Controls.Add(this.toolStrip6);
            this.tabLineup.Location = new System.Drawing.Point(4, 22);
            this.tabLineup.Name = "tabLineup";
            this.tabLineup.Size = new System.Drawing.Size(426, 500);
            this.tabLineup.TabIndex = 5;
            this.tabLineup.Text = "Subscribed Lineups";
            this.tabLineup.UseVisualStyleBackColor = true;
            // 
            // lvLineupChannels
            // 
            this.lvLineupChannels.CheckBoxes = true;
            this.lvLineupChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader21,
            this.columnHeader22,
            this.columnHeader23,
            this.columnHeader24});
            this.lvLineupChannels.ContextMenuStrip = this.lineupMenuStrip;
            this.lvLineupChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLineupChannels.FullRowSelect = true;
            this.lvLineupChannels.HideSelection = false;
            this.lvLineupChannels.Location = new System.Drawing.Point(0, 46);
            this.lvLineupChannels.Name = "lvLineupChannels";
            this.lvLineupChannels.OwnerDraw = true;
            this.lvLineupChannels.Size = new System.Drawing.Size(426, 454);
            this.lvLineupChannels.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvLineupChannels.TabIndex = 3;
            this.lvLineupChannels.UseCompatibleStateImageBehavior = false;
            this.lvLineupChannels.View = System.Windows.Forms.View.Details;
            this.lvLineupChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvLineupSort);
            this.lvLineupChannels.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvLineupChannels_DrawColumnHeader);
            this.lvLineupChannels.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.lvLineupChannels_DrawSubItem);
            this.lvLineupChannels.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvLineupChannels_ItemCheck);
            this.lvLineupChannels.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvLineupChannels_MouseClick);
            // 
            // columnHeader21
            // 
            this.columnHeader21.Text = "CallSign";
            this.columnHeader21.Width = 100;
            // 
            // columnHeader22
            // 
            this.columnHeader22.Text = "Channel";
            this.columnHeader22.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader22.Width = 65;
            // 
            // columnHeader23
            // 
            this.columnHeader23.Text = "StationID";
            this.columnHeader23.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader23.Width = 70;
            // 
            // columnHeader24
            // 
            this.columnHeader24.Text = "Name";
            this.columnHeader24.Width = 180;
            // 
            // lineupMenuStrip
            // 
            this.lineupMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.lineupMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToClipboardMenuItem});
            this.lineupMenuStrip.Name = "contextMenuStrip1";
            this.lineupMenuStrip.Size = new System.Drawing.Size(172, 26);
            this.lineupMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.lineupMenuStrip_Opening);
            // 
            // copyToClipboardMenuItem
            // 
            this.copyToClipboardMenuItem.Name = "copyToClipboardMenuItem";
            this.copyToClipboardMenuItem.Size = new System.Drawing.Size(171, 22);
            this.copyToClipboardMenuItem.Text = "Copy to Clipboard";
            this.copyToClipboardMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.copyToClipboardMenuItem.Click += new System.EventHandler(this.copyToClipboardMenuItem_Click);
            // 
            // toolStrip6
            // 
            this.toolStrip6.CanOverflow = false;
            this.toolStrip6.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnIncludeExclude,
            this.comboLineups,
            this.btnSelectAll,
            this.btnSelectNone,
            this.toolStripSeparator1,
            this.labelLineupCounts});
            this.toolStrip6.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip6.Location = new System.Drawing.Point(0, 0);
            this.toolStrip6.MaximumSize = new System.Drawing.Size(0, 46);
            this.toolStrip6.MinimumSize = new System.Drawing.Size(0, 46);
            this.toolStrip6.Name = "toolStrip6";
            this.toolStrip6.Size = new System.Drawing.Size(0, 46);
            this.toolStrip6.Stretch = true;
            this.toolStrip6.TabIndex = 2;
            this.toolStrip6.Text = "toolStrip6";
            this.toolStrip6.Resize += new System.EventHandler(this.toolStrip6_Resize);
            // 
            // btnIncludeExclude
            // 
            this.btnIncludeExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnIncludeExclude.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuInclude,
            this.menuExclude,
            this.menuDiscardNumbers});
            this.btnIncludeExclude.Image = ((System.Drawing.Image)(resources.GetObject("btnIncludeExclude.Image")));
            this.btnIncludeExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIncludeExclude.Name = "btnIncludeExclude";
            this.btnIncludeExclude.Size = new System.Drawing.Size(29, 20);
            this.btnIncludeExclude.Text = "Include/Exclude";
            this.btnIncludeExclude.ToolTipText = "Include/Exclude Lineup";
            // 
            // menuInclude
            // 
            this.menuInclude.CheckOnClick = true;
            this.menuInclude.Name = "menuInclude";
            this.menuInclude.Size = new System.Drawing.Size(178, 22);
            this.menuInclude.Text = "Include";
            this.menuInclude.ToolTipText = "Include lineup in downloads.";
            this.menuInclude.Click += new System.EventHandler(this.menuIncludeExclude_Click);
            // 
            // menuExclude
            // 
            this.menuExclude.Checked = true;
            this.menuExclude.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuExclude.Name = "menuExclude";
            this.menuExclude.Size = new System.Drawing.Size(178, 22);
            this.menuExclude.Text = "Exclude";
            this.menuExclude.ToolTipText = "Exclude lineup from downloads.";
            this.menuExclude.Click += new System.EventHandler(this.menuIncludeExclude_Click);
            // 
            // menuDiscardNumbers
            // 
            this.menuDiscardNumbers.Name = "menuDiscardNumbers";
            this.menuDiscardNumbers.Size = new System.Drawing.Size(178, 22);
            this.menuDiscardNumbers.Text = "Discard Channel #\'s";
            this.menuDiscardNumbers.ToolTipText = "Enable if you cannot use automatch with this lineup.";
            this.menuDiscardNumbers.Click += new System.EventHandler(this.menuDiscardNumbers_Click);
            // 
            // comboLineups
            // 
            this.comboLineups.AutoSize = false;
            this.comboLineups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLineups.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.comboLineups.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.comboLineups.MaxDropDownItems = 16;
            this.comboLineups.Name = "comboLineups";
            this.comboLineups.Size = new System.Drawing.Size(394, 23);
            this.comboLineups.Sorted = true;
            this.comboLineups.SelectedIndexChanged += new System.EventHandler(this.subscribedLineup_SelectedIndexChanged);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll.Image")));
            this.btnSelectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(25, 19);
            this.btnSelectAll.Text = "All";
            this.btnSelectAll.ToolTipText = "Select All";
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAllNone_Click);
            // 
            // btnSelectNone
            // 
            this.btnSelectNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSelectNone.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectNone.Image")));
            this.btnSelectNone.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new System.Drawing.Size(40, 19);
            this.btnSelectNone.Text = "None";
            this.btnSelectNone.ToolTipText = "Select None";
            this.btnSelectNone.Click += new System.EventHandler(this.btnSelectAllNone_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // labelLineupCounts
            // 
            this.labelLineupCounts.AutoSize = false;
            this.labelLineupCounts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.labelLineupCounts.Font = new System.Drawing.Font("Segoe UI", 8.5F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline))));
            this.labelLineupCounts.Name = "labelLineupCounts";
            this.labelLineupCounts.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.labelLineupCounts.Size = new System.Drawing.Size(340, 19);
            // 
            // lblUpdate
            // 
            this.lblUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdate.ForeColor = System.Drawing.Color.Red;
            this.lblUpdate.Location = new System.Drawing.Point(174, 533);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(231, 22);
            this.lblUpdate.TabIndex = 8;
            this.lblUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(510, 532);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(411, 537);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(92, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Website / Donate";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(697, 532);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Exit";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(591, 532);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(100, 23);
            this.btnExecute.TabIndex = 6;
            this.btnExecute.Text = "Save && Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHelp.Location = new System.Drawing.Point(12, 532);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 23);
            this.btnHelp.TabIndex = 7;
            this.btnHelp.Text = "View Log";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnViewLog_Click);
            // 
            // btnClearCache
            // 
            this.btnClearCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearCache.Location = new System.Drawing.Point(93, 532);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(75, 23);
            this.btnClearCache.TabIndex = 8;
            this.btnClearCache.Text = "Clear Cache";
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // btnTask
            // 
            this.btnTask.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnTask.Location = new System.Drawing.Point(228, 87);
            this.btnTask.Name = "btnTask";
            this.btnTask.Size = new System.Drawing.Size(75, 23);
            this.btnTask.TabIndex = 4;
            this.btnTask.Text = "Create";
            this.btnTask.UseVisualStyleBackColor = true;
            this.btnTask.Click += new System.EventHandler(this.btnTask_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.btnClearCache);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lblUpdate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EPG123 Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.Shown += new System.EventHandler(this.ConfigForm_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabConfigs.ResumeLayout(false);
            this.tabConfig.ResumeLayout(false);
            this.tabConfig.PerformLayout();
            this.pnlSize.ResumeLayout(false);
            this.pnlSize.PerformLayout();
            this.pnlAspect.ResumeLayout(false);
            this.pnlAspect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDays)).EndInit();
            this.tabXmltv.ResumeLayout(false);
            this.tabXmltv.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFillerDuration)).EndInit();
            this.tabTask.ResumeLayout(false);
            this.tabTask.PerformLayout();
            this.tabService.ResumeLayout(false);
            this.tabService.PerformLayout();
            this.tabNotifier.ResumeLayout(false);
            this.grpAccount.ResumeLayout(false);
            this.grpAccount.PerformLayout();
            this.tabLineups.ResumeLayout(false);
            this.tabLineup.ResumeLayout(false);
            this.tabLineup.PerformLayout();
            this.lineupMenuStrip.ResumeLayout(false);
            this.toolStrip6.ResumeLayout(false);
            this.toolStrip6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox grpAccount;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtLoginName;
        private System.Windows.Forms.TabControl tabLineups;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TextBox txtAcctExpires;
        private System.Windows.Forms.Label lblExpiration;
        private System.Windows.Forms.NumericUpDown numDays;
        private System.Windows.Forms.Label lblDaysDownload;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnClientLineups;
        private System.Windows.Forms.CheckBox cbSeasonEventImages;
        private System.Windows.Forms.CheckBox cbAddNewStations;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.CheckBox cbXmltv;
        private System.Windows.Forms.ContextMenuStrip lineupMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardMenuItem;
        private System.Windows.Forms.TabControl tabConfigs;
        private System.Windows.Forms.TabPage tabConfig;
        private System.Windows.Forms.TabPage tabXmltv;
        private System.Windows.Forms.CheckBox ckXmltvFillerData;
        private System.Windows.Forms.CheckBox ckLocalLogos;
        private System.Windows.Forms.CheckBox ckUrlLogos;
        private System.Windows.Forms.CheckBox ckChannelLogos;
        private System.Windows.Forms.CheckBox ckChannelNumbers;
        private System.Windows.Forms.CheckBox cbModernMedia;
        private System.Windows.Forms.TabPage tabTask;
        private System.Windows.Forms.CheckBox cbAutomatch;
        private System.Windows.Forms.Label lblSchedStatus;
        private ElevatedButton btnTask;
        private System.Windows.Forms.CheckBox cbImport;
        private System.Windows.Forms.CheckBox cbTaskWake;
        private System.Windows.Forms.MaskedTextBox tbSchedTime;
        private System.Windows.Forms.Label lblUpdateTime;
        private System.Windows.Forms.RichTextBox rtbFillerDescription;
        private System.Windows.Forms.Label lblFillerDuration;
        private System.Windows.Forms.NumericUpDown numFillerDuration;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox cbTVDB;
        private System.Windows.Forms.CheckBox cbPrefixTitle;
        private System.Windows.Forms.CheckBox cbPrefixDescription;
        private System.Windows.Forms.CheckBox cbAppendDescription;
        private System.Windows.Forms.Label lblPreferredLogos;
        private System.Windows.Forms.ComboBox cmbPreferredLogos;
        private System.Windows.Forms.CheckBox cbSdLogos;
        private System.Windows.Forms.CheckBox cbAlternateSEFormat;
        private System.Windows.Forms.CheckBox ckXmltvExtendedInfo;
        private System.Windows.Forms.Label lblXmltvLogosNote;
        private System.Windows.Forms.TabPage tabLineup;
        private System.Windows.Forms.ListView lvLineupChannels;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader22;
        private System.Windows.Forms.ColumnHeader columnHeader23;
        private System.Windows.Forms.ColumnHeader columnHeader24;
        private System.Windows.Forms.CheckBox cbNoCastCrew;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox cbXmltvSingleImage;
        private System.Windows.Forms.ToolStrip toolStrip6;
        private System.Windows.Forms.ToolStripDropDownButton btnIncludeExclude;
        private System.Windows.Forms.ToolStripMenuItem menuInclude;
        private System.Windows.Forms.ToolStripMenuItem menuExclude;
        private System.Windows.Forms.ToolStripComboBox comboLineups;
        private System.Windows.Forms.ToolStripButton btnSelectAll;
        private System.Windows.Forms.ToolStripButton btnSelectNone;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel labelLineupCounts;
        private System.Windows.Forms.TabPage tabService;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnServiceStart;
        private System.Windows.Forms.Button btnServiceStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbCacheRetention;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.CheckBox cbOadOverride;
        private System.Windows.Forms.CheckBox ckIpAddress;
        private System.Windows.Forms.ComboBox cmbIpAddresses;
        private System.Windows.Forms.ToolStripMenuItem menuDiscardNumbers;
        private System.Windows.Forms.Button btnRemoveOrphans;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBaseArtwork;
        private System.Windows.Forms.TextBox txtBaseApi;
        private System.Windows.Forms.Button btnChangeServer;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.CheckBox cbBrandLogo;
        private System.Windows.Forms.TabPage tabNotifier;
        private System.Windows.Forms.Button btnEmail;
        private System.Windows.Forms.CheckBox ckDebug;
        private System.Windows.Forms.RadioButton rdoLg;
        private System.Windows.Forms.RadioButton rdoMd;
        private System.Windows.Forms.RadioButton rdoSm;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblAspect;
        private System.Windows.Forms.RadioButton rdo16x9;
        private System.Windows.Forms.RadioButton rdo4x3;
        private System.Windows.Forms.RadioButton rdo2x3;
        private System.Windows.Forms.Panel pnlAspect;
        private System.Windows.Forms.RadioButton rdo3x4;
        private System.Windows.Forms.Panel pnlSize;
    }
}