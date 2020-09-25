namespace epg123
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabConfigs = new System.Windows.Forms.TabControl();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.cbBrandLogo = new System.Windows.Forms.CheckBox();
            this.cbAlternateSEFormat = new System.Windows.Forms.CheckBox();
            this.lblAlternateLogos = new System.Windows.Forms.Label();
            this.cmbAlternateLogos = new System.Windows.Forms.ComboBox();
            this.btnSdLogos = new System.Windows.Forms.Button();
            this.lblPreferredLogos = new System.Windows.Forms.Label();
            this.cmbPreferredLogos = new System.Windows.Forms.ComboBox();
            this.cbSeriesPosterArt = new System.Windows.Forms.CheckBox();
            this.cbTMDb = new System.Windows.Forms.CheckBox();
            this.cbSdLogos = new System.Windows.Forms.CheckBox();
            this.cbTVDB = new System.Windows.Forms.CheckBox();
            this.cbPrefixTitle = new System.Windows.Forms.CheckBox();
            this.cbPrefixDescription = new System.Windows.Forms.CheckBox();
            this.cbAppendDescription = new System.Windows.Forms.CheckBox();
            this.cbModernMedia = new System.Windows.Forms.CheckBox();
            this.numDays = new System.Windows.Forms.NumericUpDown();
            this.lblDaysDownload = new System.Windows.Forms.Label();
            this.cbOadOverride = new System.Windows.Forms.CheckBox();
            this.cbAddNewStations = new System.Windows.Forms.CheckBox();
            this.tabXmltv = new System.Windows.Forms.TabPage();
            this.ckXmltvExtendedInfo = new System.Windows.Forms.CheckBox();
            this.lblXmltvLogosNote = new System.Windows.Forms.Label();
            this.lblXmltvOutput = new System.Windows.Forms.Label();
            this.btnXmltvOutput = new System.Windows.Forms.Button();
            this.tbXmltvOutput = new System.Windows.Forms.TextBox();
            this.rtbFillerDescription = new System.Windows.Forms.RichTextBox();
            this.lblFillerDescription = new System.Windows.Forms.Label();
            this.lblFillerDuration = new System.Windows.Forms.Label();
            this.numFillerDuration = new System.Windows.Forms.NumericUpDown();
            this.ckXmltvFillerData = new System.Windows.Forms.CheckBox();
            this.txtSubstitutePath = new System.Windows.Forms.TextBox();
            this.cbXmltv = new System.Windows.Forms.CheckBox();
            this.ckSubstitutePath = new System.Windows.Forms.CheckBox();
            this.ckLocalLogos = new System.Windows.Forms.CheckBox();
            this.ckUrlLogos = new System.Windows.Forms.CheckBox();
            this.ckChannelLogos = new System.Windows.Forms.CheckBox();
            this.ckChannelNumbers = new System.Windows.Forms.CheckBox();
            this.tabTask = new System.Windows.Forms.TabPage();
            this.cbAutomatch = new System.Windows.Forms.CheckBox();
            this.lblSchedStatus = new System.Windows.Forms.Label();
            this.btnTask = new epg123.ElevatedButton();
            this.cbImport = new System.Windows.Forms.CheckBox();
            this.cbTaskWake = new System.Windows.Forms.CheckBox();
            this.tbSchedTime = new System.Windows.Forms.MaskedTextBox();
            this.lblUpdateTime = new System.Windows.Forms.Label();
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
            this.tabL1 = new System.Windows.Forms.TabPage();
            this.lvL1Lineup = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lineupMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToClipboardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.L1IncludeExclude = new System.Windows.Forms.ToolStripDropDownButton();
            this.L1includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.L1excludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnL1All = new System.Windows.Forms.ToolStripButton();
            this.btnL1None = new System.Windows.Forms.ToolStripButton();
            this.lblL1Lineup = new System.Windows.Forms.ToolStripLabel();
            this.tabL2 = new System.Windows.Forms.TabPage();
            this.lvL2Lineup = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.L2IncludeExclude = new System.Windows.Forms.ToolStripDropDownButton();
            this.L2includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.L2excludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnL2All = new System.Windows.Forms.ToolStripButton();
            this.btnL2None = new System.Windows.Forms.ToolStripButton();
            this.lblL2Lineup = new System.Windows.Forms.ToolStripLabel();
            this.tabL3 = new System.Windows.Forms.TabPage();
            this.lvL3Lineup = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.L3IncludeExclude = new System.Windows.Forms.ToolStripDropDownButton();
            this.L3includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.L3excludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnL3All = new System.Windows.Forms.ToolStripButton();
            this.btnL3None = new System.Windows.Forms.ToolStripButton();
            this.lblL3Lineup = new System.Windows.Forms.ToolStripLabel();
            this.tabL4 = new System.Windows.Forms.TabPage();
            this.lvL4Lineup = new System.Windows.Forms.ListView();
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip4 = new System.Windows.Forms.ToolStrip();
            this.L4IncludeExclude = new System.Windows.Forms.ToolStripDropDownButton();
            this.L4includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.L4excludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnL4All = new System.Windows.Forms.ToolStripButton();
            this.btnL4None = new System.Windows.Forms.ToolStripButton();
            this.lblL4Lineup = new System.Windows.Forms.ToolStripLabel();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lvL5Lineup = new System.Windows.Forms.ListView();
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip5 = new System.Windows.Forms.ToolStrip();
            this.L5IncludeExclude = new System.Windows.Forms.ToolStripDropDownButton();
            this.L5includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.L5excludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCustomLineup = new System.Windows.Forms.ToolStripSplitButton();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnClearCache = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabConfigs.SuspendLayout();
            this.tabConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDays)).BeginInit();
            this.tabXmltv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFillerDuration)).BeginInit();
            this.tabTask.SuspendLayout();
            this.grpAccount.SuspendLayout();
            this.tabLineups.SuspendLayout();
            this.tabL1.SuspendLayout();
            this.lineupMenuStrip.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabL2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabL3.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.tabL4.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.toolStrip5.SuspendLayout();
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
            this.splitContainer1.Panel1MinSize = 340;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabLineups);
            this.splitContainer1.Panel2MinSize = 340;
            this.splitContainer1.Size = new System.Drawing.Size(784, 526);
            this.splitContainer1.SplitterDistance = 340;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabConfigs
            // 
            this.tabConfigs.Controls.Add(this.tabConfig);
            this.tabConfigs.Controls.Add(this.tabXmltv);
            this.tabConfigs.Controls.Add(this.tabTask);
            this.tabConfigs.Enabled = false;
            this.tabConfigs.HotTrack = true;
            this.tabConfigs.Location = new System.Drawing.Point(12, 105);
            this.tabConfigs.Multiline = true;
            this.tabConfigs.Name = "tabConfigs";
            this.tabConfigs.SelectedIndex = 0;
            this.tabConfigs.Size = new System.Drawing.Size(317, 421);
            this.tabConfigs.TabIndex = 10;
            // 
            // tabConfig
            // 
            this.tabConfig.BackColor = System.Drawing.SystemColors.Control;
            this.tabConfig.Controls.Add(this.cbBrandLogo);
            this.tabConfig.Controls.Add(this.cbAlternateSEFormat);
            this.tabConfig.Controls.Add(this.lblAlternateLogos);
            this.tabConfig.Controls.Add(this.cmbAlternateLogos);
            this.tabConfig.Controls.Add(this.btnSdLogos);
            this.tabConfig.Controls.Add(this.lblPreferredLogos);
            this.tabConfig.Controls.Add(this.cmbPreferredLogos);
            this.tabConfig.Controls.Add(this.cbSeriesPosterArt);
            this.tabConfig.Controls.Add(this.cbTMDb);
            this.tabConfig.Controls.Add(this.cbSdLogos);
            this.tabConfig.Controls.Add(this.cbTVDB);
            this.tabConfig.Controls.Add(this.cbPrefixTitle);
            this.tabConfig.Controls.Add(this.cbPrefixDescription);
            this.tabConfig.Controls.Add(this.cbAppendDescription);
            this.tabConfig.Controls.Add(this.cbModernMedia);
            this.tabConfig.Controls.Add(this.numDays);
            this.tabConfig.Controls.Add(this.lblDaysDownload);
            this.tabConfig.Controls.Add(this.cbOadOverride);
            this.tabConfig.Controls.Add(this.cbAddNewStations);
            this.tabConfig.Location = new System.Drawing.Point(4, 22);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfig.Size = new System.Drawing.Size(309, 395);
            this.tabConfig.TabIndex = 2;
            this.tabConfig.Text = "Configuration";
            // 
            // cbBrandLogo
            // 
            this.cbBrandLogo.AutoSize = true;
            this.cbBrandLogo.Location = new System.Drawing.Point(6, 345);
            this.cbBrandLogo.Name = "cbBrandLogo";
            this.cbBrandLogo.Size = new System.Drawing.Size(294, 17);
            this.cbBrandLogo.TabIndex = 37;
            this.cbBrandLogo.Text = "Add status logo in channel guide (viewable by extenders)";
            this.cbBrandLogo.UseVisualStyleBackColor = true;
            this.cbBrandLogo.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbAlternateSEFormat
            // 
            this.cbAlternateSEFormat.AutoSize = true;
            this.cbAlternateSEFormat.Enabled = false;
            this.cbAlternateSEFormat.Location = new System.Drawing.Point(6, 101);
            this.cbAlternateSEFormat.Name = "cbAlternateSEFormat";
            this.cbAlternateSEFormat.Size = new System.Drawing.Size(295, 17);
            this.cbAlternateSEFormat.TabIndex = 36;
            this.cbAlternateSEFormat.Text = "Use season/episode format \"S1:E2\" instead of \"s01e02\"";
            this.cbAlternateSEFormat.UseVisualStyleBackColor = true;
            this.cbAlternateSEFormat.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // lblAlternateLogos
            // 
            this.lblAlternateLogos.AutoSize = true;
            this.lblAlternateLogos.Location = new System.Drawing.Point(15, 292);
            this.lblAlternateLogos.Name = "lblAlternateLogos";
            this.lblAlternateLogos.Size = new System.Drawing.Size(98, 13);
            this.lblAlternateLogos.TabIndex = 35;
            this.lblAlternateLogos.Text = "Alternate SD logos:";
            // 
            // cmbAlternateLogos
            // 
            this.cmbAlternateLogos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAlternateLogos.FormattingEnabled = true;
            this.cmbAlternateLogos.Items.AddRange(new object[] {
            "white logos",
            "gray logos",
            "logos for dark backgrounds",
            "logos for light backgrounds",
            "none"});
            this.cmbAlternateLogos.Location = new System.Drawing.Point(119, 289);
            this.cmbAlternateLogos.Name = "cmbAlternateLogos";
            this.cmbAlternateLogos.Size = new System.Drawing.Size(160, 21);
            this.cmbAlternateLogos.TabIndex = 34;
            this.cmbAlternateLogos.SelectedIndexChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // btnSdLogos
            // 
            this.btnSdLogos.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSdLogos.FlatAppearance.BorderSize = 2;
            this.btnSdLogos.Location = new System.Drawing.Point(6, 316);
            this.btnSdLogos.Name = "btnSdLogos";
            this.btnSdLogos.Size = new System.Drawing.Size(297, 23);
            this.btnSdLogos.TabIndex = 33;
            this.btnSdLogos.Text = "Collect all station logos for subscribed lineups from SD";
            this.toolTip1.SetToolTip(this.btnSdLogos, "Download all logos to .\\sdlogos folder");
            this.btnSdLogos.UseVisualStyleBackColor = true;
            this.btnSdLogos.Click += new System.EventHandler(this.btnSdLogos_Click);
            // 
            // lblPreferredLogos
            // 
            this.lblPreferredLogos.AutoSize = true;
            this.lblPreferredLogos.Location = new System.Drawing.Point(14, 265);
            this.lblPreferredLogos.Name = "lblPreferredLogos";
            this.lblPreferredLogos.Size = new System.Drawing.Size(99, 13);
            this.lblPreferredLogos.TabIndex = 32;
            this.lblPreferredLogos.Text = "Preferred SD logos:";
            // 
            // cmbPreferredLogos
            // 
            this.cmbPreferredLogos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPreferredLogos.FormattingEnabled = true;
            this.cmbPreferredLogos.Items.AddRange(new object[] {
            "white logos",
            "gray logos",
            "logos for dark backgrounds",
            "logos for light backgrounds",
            "do not download SD logos"});
            this.cmbPreferredLogos.Location = new System.Drawing.Point(119, 262);
            this.cmbPreferredLogos.Name = "cmbPreferredLogos";
            this.cmbPreferredLogos.Size = new System.Drawing.Size(160, 21);
            this.cmbPreferredLogos.TabIndex = 31;
            this.cmbPreferredLogos.SelectedIndexChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // cbSeriesPosterArt
            // 
            this.cbSeriesPosterArt.AutoSize = true;
            this.cbSeriesPosterArt.Location = new System.Drawing.Point(6, 193);
            this.cbSeriesPosterArt.Name = "cbSeriesPosterArt";
            this.cbSeriesPosterArt.Size = new System.Drawing.Size(252, 17);
            this.cbSeriesPosterArt.TabIndex = 30;
            this.cbSeriesPosterArt.Text = "Use 2x3 posters for series images instead of 4x3";
            this.cbSeriesPosterArt.UseVisualStyleBackColor = true;
            this.cbSeriesPosterArt.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // cbTMDb
            // 
            this.cbTMDb.AutoSize = true;
            this.cbTMDb.Location = new System.Drawing.Point(6, 216);
            this.cbTMDb.Name = "cbTMDb";
            this.cbTMDb.Size = new System.Drawing.Size(249, 17);
            this.cbTMDb.TabIndex = 28;
            this.cbTMDb.Text = "Use themoviedb.org for missing movie cover art";
            this.cbTMDb.UseVisualStyleBackColor = true;
            this.cbTMDb.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // cbSdLogos
            // 
            this.cbSdLogos.AutoSize = true;
            this.cbSdLogos.Checked = true;
            this.cbSdLogos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSdLogos.Location = new System.Drawing.Point(6, 239);
            this.cbSdLogos.Name = "cbSdLogos";
            this.cbSdLogos.Size = new System.Drawing.Size(199, 17);
            this.cbSdLogos.TabIndex = 29;
            this.cbSdLogos.Text = "Include station logos in .\\logos folder";
            this.cbSdLogos.UseVisualStyleBackColor = true;
            this.cbSdLogos.CheckedChanged += new System.EventHandler(this.imageConfigs_Changed);
            // 
            // cbTVDB
            // 
            this.cbTVDB.AutoSize = true;
            this.cbTVDB.Location = new System.Drawing.Point(6, 32);
            this.cbTVDB.Name = "cbTVDB";
            this.cbTVDB.Size = new System.Drawing.Size(289, 17);
            this.cbTVDB.TabIndex = 27;
            this.cbTVDB.Text = "Use TheTVDB season and episode numbers if provided";
            this.cbTVDB.UseVisualStyleBackColor = true;
            this.cbTVDB.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbPrefixTitle
            // 
            this.cbPrefixTitle.AutoSize = true;
            this.cbPrefixTitle.Location = new System.Drawing.Point(6, 55);
            this.cbPrefixTitle.Name = "cbPrefixTitle";
            this.cbPrefixTitle.Size = new System.Drawing.Size(274, 17);
            this.cbPrefixTitle.TabIndex = 24;
            this.cbPrefixTitle.Text = "Prefix episode title with season and episode numbers";
            this.cbPrefixTitle.UseVisualStyleBackColor = true;
            this.cbPrefixTitle.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbPrefixDescription
            // 
            this.cbPrefixDescription.AutoSize = true;
            this.cbPrefixDescription.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbPrefixDescription.Location = new System.Drawing.Point(6, 78);
            this.cbPrefixDescription.Name = "cbPrefixDescription";
            this.cbPrefixDescription.Size = new System.Drawing.Size(281, 17);
            this.cbPrefixDescription.TabIndex = 26;
            this.cbPrefixDescription.Text = "Prefix episode desc with season and episode numbers";
            this.cbPrefixDescription.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbPrefixDescription.UseVisualStyleBackColor = true;
            this.cbPrefixDescription.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbAppendDescription
            // 
            this.cbAppendDescription.AutoSize = true;
            this.cbAppendDescription.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbAppendDescription.Location = new System.Drawing.Point(6, 124);
            this.cbAppendDescription.Name = "cbAppendDescription";
            this.cbAppendDescription.Size = new System.Drawing.Size(292, 17);
            this.cbAppendDescription.TabIndex = 25;
            this.cbAppendDescription.Text = "Append episode desc with season and episode numbers";
            this.cbAppendDescription.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbAppendDescription.UseVisualStyleBackColor = true;
            this.cbAppendDescription.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbModernMedia
            // 
            this.cbModernMedia.AutoSize = true;
            this.cbModernMedia.Location = new System.Drawing.Point(6, 368);
            this.cbModernMedia.Name = "cbModernMedia";
            this.cbModernMedia.Size = new System.Drawing.Size(199, 17);
            this.cbModernMedia.TabIndex = 23;
            this.cbModernMedia.Text = "Create ModernMedia UI+ support file";
            this.cbModernMedia.UseVisualStyleBackColor = true;
            this.cbModernMedia.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // numDays
            // 
            this.numDays.Location = new System.Drawing.Point(6, 6);
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
            this.lblDaysDownload.Location = new System.Drawing.Point(52, 8);
            this.lblDaysDownload.Name = "lblDaysDownload";
            this.lblDaysDownload.Size = new System.Drawing.Size(172, 13);
            this.lblDaysDownload.TabIndex = 2;
            this.lblDaysDownload.Text = "days of schedule data to download";
            // 
            // cbOadOverride
            // 
            this.cbOadOverride.AutoSize = true;
            this.cbOadOverride.Location = new System.Drawing.Point(6, 147);
            this.cbOadOverride.Name = "cbOadOverride";
            this.cbOadOverride.Size = new System.Drawing.Size(232, 17);
            this.cbOadOverride.TabIndex = 12;
            this.cbOadOverride.Text = "Allow NEW flag to override Original Air Date";
            this.cbOadOverride.UseVisualStyleBackColor = true;
            this.cbOadOverride.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // cbAddNewStations
            // 
            this.cbAddNewStations.AutoSize = true;
            this.cbAddNewStations.Location = new System.Drawing.Point(6, 170);
            this.cbAddNewStations.Name = "cbAddNewStations";
            this.cbAddNewStations.Size = new System.Drawing.Size(246, 17);
            this.cbAddNewStations.TabIndex = 13;
            this.cbAddNewStations.Text = "Automatically download new stations in lineups";
            this.cbAddNewStations.UseVisualStyleBackColor = true;
            this.cbAddNewStations.CheckedChanged += new System.EventHandler(this.configs_Changed);
            // 
            // tabXmltv
            // 
            this.tabXmltv.BackColor = System.Drawing.SystemColors.Control;
            this.tabXmltv.Controls.Add(this.ckXmltvExtendedInfo);
            this.tabXmltv.Controls.Add(this.lblXmltvLogosNote);
            this.tabXmltv.Controls.Add(this.lblXmltvOutput);
            this.tabXmltv.Controls.Add(this.btnXmltvOutput);
            this.tabXmltv.Controls.Add(this.tbXmltvOutput);
            this.tabXmltv.Controls.Add(this.rtbFillerDescription);
            this.tabXmltv.Controls.Add(this.lblFillerDescription);
            this.tabXmltv.Controls.Add(this.lblFillerDuration);
            this.tabXmltv.Controls.Add(this.numFillerDuration);
            this.tabXmltv.Controls.Add(this.ckXmltvFillerData);
            this.tabXmltv.Controls.Add(this.txtSubstitutePath);
            this.tabXmltv.Controls.Add(this.cbXmltv);
            this.tabXmltv.Controls.Add(this.ckSubstitutePath);
            this.tabXmltv.Controls.Add(this.ckLocalLogos);
            this.tabXmltv.Controls.Add(this.ckUrlLogos);
            this.tabXmltv.Controls.Add(this.ckChannelLogos);
            this.tabXmltv.Controls.Add(this.ckChannelNumbers);
            this.tabXmltv.Location = new System.Drawing.Point(4, 22);
            this.tabXmltv.Name = "tabXmltv";
            this.tabXmltv.Padding = new System.Windows.Forms.Padding(3);
            this.tabXmltv.Size = new System.Drawing.Size(309, 395);
            this.tabXmltv.TabIndex = 3;
            this.tabXmltv.Text = "XMLTV";
            // 
            // ckXmltvExtendedInfo
            // 
            this.ckXmltvExtendedInfo.AutoSize = true;
            this.ckXmltvExtendedInfo.Location = new System.Drawing.Point(6, 284);
            this.ckXmltvExtendedInfo.Name = "ckXmltvExtendedInfo";
            this.ckXmltvExtendedInfo.Size = new System.Drawing.Size(199, 17);
            this.ckXmltvExtendedInfo.TabIndex = 29;
            this.ckXmltvExtendedInfo.Text = "Add extended info before description\r\n";
            this.ckXmltvExtendedInfo.UseVisualStyleBackColor = true;
            this.ckXmltvExtendedInfo.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // lblXmltvLogosNote
            // 
            this.lblXmltvLogosNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblXmltvLogosNote.Location = new System.Drawing.Point(6, 362);
            this.lblXmltvLogosNote.Name = "lblXmltvLogosNote";
            this.lblXmltvLogosNote.Size = new System.Drawing.Size(297, 30);
            this.lblXmltvLogosNote.TabIndex = 28;
            this.lblXmltvLogosNote.Text = "* The option to \'Include station logos in .\\logos folder\' must be enabled to incl" +
    "ude channel logos in the XMLTV file.";
            // 
            // lblXmltvOutput
            // 
            this.lblXmltvOutput.AutoSize = true;
            this.lblXmltvOutput.Location = new System.Drawing.Point(6, 309);
            this.lblXmltvOutput.Name = "lblXmltvOutput";
            this.lblXmltvOutput.Size = new System.Drawing.Size(58, 13);
            this.lblXmltvOutput.TabIndex = 27;
            this.lblXmltvOutput.Text = "Output file:";
            // 
            // btnXmltvOutput
            // 
            this.btnXmltvOutput.Location = new System.Drawing.Point(280, 325);
            this.btnXmltvOutput.Name = "btnXmltvOutput";
            this.btnXmltvOutput.Size = new System.Drawing.Size(26, 22);
            this.btnXmltvOutput.TabIndex = 26;
            this.btnXmltvOutput.Text = "...";
            this.btnXmltvOutput.UseVisualStyleBackColor = true;
            this.btnXmltvOutput.Click += new System.EventHandler(this.btnXmltvOutput_Click);
            // 
            // tbXmltvOutput
            // 
            this.tbXmltvOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbXmltvOutput.Cursor = System.Windows.Forms.Cursors.Default;
            this.tbXmltvOutput.Location = new System.Drawing.Point(6, 326);
            this.tbXmltvOutput.Name = "tbXmltvOutput";
            this.tbXmltvOutput.ReadOnly = true;
            this.tbXmltvOutput.Size = new System.Drawing.Size(268, 20);
            this.tbXmltvOutput.TabIndex = 25;
            this.tbXmltvOutput.TabStop = false;
            // 
            // rtbFillerDescription
            // 
            this.rtbFillerDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbFillerDescription.Location = new System.Drawing.Point(24, 232);
            this.rtbFillerDescription.Name = "rtbFillerDescription";
            this.rtbFillerDescription.Size = new System.Drawing.Size(280, 46);
            this.rtbFillerDescription.TabIndex = 24;
            this.rtbFillerDescription.Text = "This program was generated by EPG123 to provide filler data for stations that did" +
    " not receive any guide listings from the upstream source.";
            this.rtbFillerDescription.TextChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // lblFillerDescription
            // 
            this.lblFillerDescription.AutoSize = true;
            this.lblFillerDescription.Location = new System.Drawing.Point(21, 216);
            this.lblFillerDescription.Name = "lblFillerDescription";
            this.lblFillerDescription.Size = new System.Drawing.Size(126, 13);
            this.lblFillerDescription.TabIndex = 23;
            this.lblFillerDescription.Text = "Filler program description:";
            // 
            // lblFillerDuration
            // 
            this.lblFillerDuration.AutoSize = true;
            this.lblFillerDuration.Location = new System.Drawing.Point(65, 195);
            this.lblFillerDuration.Name = "lblFillerDuration";
            this.lblFillerDuration.Size = new System.Drawing.Size(143, 13);
            this.lblFillerDuration.TabIndex = 22;
            this.lblFillerDuration.Text = "hour duration of filler program";
            // 
            // numFillerDuration
            // 
            this.numFillerDuration.Location = new System.Drawing.Point(24, 193);
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
            this.numFillerDuration.TabIndex = 21;
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
            this.ckXmltvFillerData.Location = new System.Drawing.Point(6, 170);
            this.ckXmltvFillerData.Name = "ckXmltvFillerData";
            this.ckXmltvFillerData.Size = new System.Drawing.Size(294, 17);
            this.ckXmltvFillerData.TabIndex = 13;
            this.ckXmltvFillerData.Text = "Create filler programs for stations that have no guide data";
            this.ckXmltvFillerData.UseVisualStyleBackColor = true;
            this.ckXmltvFillerData.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // txtSubstitutePath
            // 
            this.txtSubstitutePath.Location = new System.Drawing.Point(41, 144);
            this.txtSubstitutePath.Name = "txtSubstitutePath";
            this.txtSubstitutePath.Size = new System.Drawing.Size(263, 20);
            this.txtSubstitutePath.TabIndex = 12;
            this.txtSubstitutePath.TextChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // cbXmltv
            // 
            this.cbXmltv.AutoSize = true;
            this.cbXmltv.Checked = true;
            this.cbXmltv.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbXmltv.Location = new System.Drawing.Point(6, 6);
            this.cbXmltv.Name = "cbXmltv";
            this.cbXmltv.Size = new System.Drawing.Size(112, 17);
            this.cbXmltv.TabIndex = 20;
            this.cbXmltv.Text = "Create XMLTV file";
            this.cbXmltv.UseVisualStyleBackColor = true;
            this.cbXmltv.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckSubstitutePath
            // 
            this.ckSubstitutePath.AutoSize = true;
            this.ckSubstitutePath.Checked = true;
            this.ckSubstitutePath.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckSubstitutePath.Location = new System.Drawing.Point(41, 121);
            this.ckSubstitutePath.Margin = new System.Windows.Forms.Padding(37, 3, 3, 3);
            this.ckSubstitutePath.Name = "ckSubstitutePath";
            this.ckSubstitutePath.Size = new System.Drawing.Size(191, 17);
            this.ckSubstitutePath.TabIndex = 11;
            this.ckSubstitutePath.Text = "Substitute path to logos folder with:";
            this.ckSubstitutePath.UseVisualStyleBackColor = true;
            this.ckSubstitutePath.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckLocalLogos
            // 
            this.ckLocalLogos.AutoSize = true;
            this.ckLocalLogos.Checked = true;
            this.ckLocalLogos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckLocalLogos.Location = new System.Drawing.Point(24, 98);
            this.ckLocalLogos.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.ckLocalLogos.Name = "ckLocalLogos";
            this.ckLocalLogos.Size = new System.Drawing.Size(194, 17);
            this.ckLocalLogos.TabIndex = 10;
            this.ckLocalLogos.Text = "Use local images from .\\logos folder";
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
            this.ckUrlLogos.TabIndex = 9;
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
            this.ckChannelLogos.Size = new System.Drawing.Size(134, 17);
            this.ckChannelLogos.TabIndex = 8;
            this.ckChannelLogos.Text = "Include channel logos*";
            this.ckChannelLogos.UseVisualStyleBackColor = true;
            this.ckChannelLogos.CheckedChanged += new System.EventHandler(this.ckXmltvConfigs_Changed);
            // 
            // ckChannelNumbers
            // 
            this.ckChannelNumbers.AutoSize = true;
            this.ckChannelNumbers.Location = new System.Drawing.Point(6, 29);
            this.ckChannelNumbers.Name = "ckChannelNumbers";
            this.ckChannelNumbers.Size = new System.Drawing.Size(145, 17);
            this.ckChannelNumbers.TabIndex = 7;
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
            this.tabTask.Size = new System.Drawing.Size(309, 395);
            this.tabTask.TabIndex = 4;
            this.tabTask.Text = "Scheduled Task";
            this.tabTask.Enter += new System.EventHandler(this.tabTask_Enter);
            // 
            // cbAutomatch
            // 
            this.cbAutomatch.AutoSize = true;
            this.cbAutomatch.Location = new System.Drawing.Point(21, 51);
            this.cbAutomatch.Name = "cbAutomatch";
            this.cbAutomatch.Size = new System.Drawing.Size(217, 17);
            this.cbAutomatch.TabIndex = 23;
            this.cbAutomatch.Text = "Automatically match stations to channels";
            this.cbAutomatch.UseVisualStyleBackColor = true;
            this.cbAutomatch.CheckedChanged += new System.EventHandler(this.configTask_Changed);
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
            // btnTask
            // 
            this.btnTask.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnTask.Location = new System.Drawing.Point(228, 87);
            this.btnTask.Name = "btnTask";
            this.btnTask.Size = new System.Drawing.Size(75, 23);
            this.btnTask.TabIndex = 21;
            this.btnTask.Text = "Create";
            this.btnTask.UseVisualStyleBackColor = true;
            this.btnTask.Click += new System.EventHandler(this.btnTask_Click);
            // 
            // cbImport
            // 
            this.cbImport.AutoSize = true;
            this.cbImport.Location = new System.Drawing.Point(6, 30);
            this.cbImport.Name = "cbImport";
            this.cbImport.Size = new System.Drawing.Size(222, 17);
            this.cbImport.TabIndex = 22;
            this.cbImport.Text = "Automatically import guide data into WMC";
            this.cbImport.UseVisualStyleBackColor = true;
            this.cbImport.CheckedChanged += new System.EventHandler(this.configTask_Changed);
            // 
            // cbTaskWake
            // 
            this.cbTaskWake.AutoSize = true;
            this.cbTaskWake.Location = new System.Drawing.Point(175, 8);
            this.cbTaskWake.Name = "cbTaskWake";
            this.cbTaskWake.Size = new System.Drawing.Size(55, 17);
            this.cbTaskWake.TabIndex = 20;
            this.cbTaskWake.Text = "Wake";
            this.cbTaskWake.UseVisualStyleBackColor = true;
            // 
            // tbSchedTime
            // 
            this.tbSchedTime.Location = new System.Drawing.Point(131, 6);
            this.tbSchedTime.Mask = "00:00";
            this.tbSchedTime.Name = "tbSchedTime";
            this.tbSchedTime.Size = new System.Drawing.Size(38, 20);
            this.tbSchedTime.TabIndex = 19;
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
            this.grpAccount.Size = new System.Drawing.Size(317, 96);
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
            this.btnClientLineups.TabIndex = 7;
            this.btnClientLineups.Text = "Lineups";
            this.btnClientLineups.UseVisualStyleBackColor = true;
            this.btnClientLineups.Click += new System.EventHandler(this.btnClientConfig_Click);
            // 
            // txtAcctExpires
            // 
            this.txtAcctExpires.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAcctExpires.Location = new System.Drawing.Point(80, 72);
            this.txtAcctExpires.Name = "txtAcctExpires";
            this.txtAcctExpires.ReadOnly = true;
            this.txtAcctExpires.Size = new System.Drawing.Size(150, 13);
            this.txtAcctExpires.TabIndex = 6;
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
            this.btnLogin.TabIndex = 4;
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
            this.txtLoginName.Location = new System.Drawing.Point(80, 17);
            this.txtLoginName.Name = "txtLoginName";
            this.txtLoginName.Size = new System.Drawing.Size(150, 20);
            this.txtLoginName.TabIndex = 0;
            this.txtLoginName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLogin_KeyPress);
            // 
            // tabLineups
            // 
            this.tabLineups.Controls.Add(this.tabL1);
            this.tabLineups.Controls.Add(this.tabL2);
            this.tabLineups.Controls.Add(this.tabL3);
            this.tabLineups.Controls.Add(this.tabL4);
            this.tabLineups.Controls.Add(this.tabPage1);
            this.tabLineups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLineups.Location = new System.Drawing.Point(0, 0);
            this.tabLineups.Name = "tabLineups";
            this.tabLineups.SelectedIndex = 0;
            this.tabLineups.Size = new System.Drawing.Size(440, 526);
            this.tabLineups.TabIndex = 0;
            // 
            // tabL1
            // 
            this.tabL1.Controls.Add(this.lvL1Lineup);
            this.tabL1.Controls.Add(this.toolStrip1);
            this.tabL1.Location = new System.Drawing.Point(4, 22);
            this.tabL1.Name = "tabL1";
            this.tabL1.Padding = new System.Windows.Forms.Padding(3);
            this.tabL1.Size = new System.Drawing.Size(432, 500);
            this.tabL1.TabIndex = 0;
            this.tabL1.Text = "Lineup 1";
            // 
            // lvL1Lineup
            // 
            this.lvL1Lineup.CheckBoxes = true;
            this.lvL1Lineup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader13});
            this.lvL1Lineup.ContextMenuStrip = this.lineupMenuStrip;
            this.lvL1Lineup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvL1Lineup.FullRowSelect = true;
            this.lvL1Lineup.HideSelection = false;
            this.lvL1Lineup.Location = new System.Drawing.Point(3, 28);
            this.lvL1Lineup.Name = "lvL1Lineup";
            this.lvL1Lineup.Size = new System.Drawing.Size(426, 469);
            this.lvL1Lineup.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvL1Lineup.TabIndex = 1;
            this.lvL1Lineup.UseCompatibleStateImageBehavior = false;
            this.lvL1Lineup.View = System.Windows.Forms.View.Details;
            this.lvL1Lineup.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            this.lvL1Lineup.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvLineup_ItemCheck);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "CallSign";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Channel";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "StationID";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Name";
            this.columnHeader13.Width = 175;
            // 
            // lineupMenuStrip
            // 
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
            // toolStrip1
            // 
            this.toolStrip1.Enabled = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L1IncludeExclude,
            this.btnL1All,
            this.btnL1None,
            this.lblL1Lineup});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(426, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // L1IncludeExclude
            // 
            this.L1IncludeExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.L1IncludeExclude.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L1includeToolStripMenuItem,
            this.L1excludeToolStripMenuItem});
            this.L1IncludeExclude.Image = ((System.Drawing.Image)(resources.GetObject("L1IncludeExclude.Image")));
            this.L1IncludeExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.L1IncludeExclude.Name = "L1IncludeExclude";
            this.L1IncludeExclude.Size = new System.Drawing.Size(29, 22);
            this.L1IncludeExclude.Text = "Include/Exclude";
            this.L1IncludeExclude.ToolTipText = "Include/Exclude Lineup";
            // 
            // L1includeToolStripMenuItem
            // 
            this.L1includeToolStripMenuItem.CheckOnClick = true;
            this.L1includeToolStripMenuItem.Name = "L1includeToolStripMenuItem";
            this.L1includeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L1includeToolStripMenuItem.Text = "Include";
            this.L1includeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // L1excludeToolStripMenuItem
            // 
            this.L1excludeToolStripMenuItem.Checked = true;
            this.L1excludeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.L1excludeToolStripMenuItem.Name = "L1excludeToolStripMenuItem";
            this.L1excludeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L1excludeToolStripMenuItem.Text = "Exclude";
            this.L1excludeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // btnL1All
            // 
            this.btnL1All.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL1All.Image = ((System.Drawing.Image)(resources.GetObject("btnL1All.Image")));
            this.btnL1All.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL1All.Name = "btnL1All";
            this.btnL1All.Size = new System.Drawing.Size(25, 22);
            this.btnL1All.Text = "All";
            this.btnL1All.ToolTipText = "Select All";
            this.btnL1All.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnL1None
            // 
            this.btnL1None.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL1None.Image = ((System.Drawing.Image)(resources.GetObject("btnL1None.Image")));
            this.btnL1None.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL1None.Name = "btnL1None";
            this.btnL1None.Size = new System.Drawing.Size(40, 22);
            this.btnL1None.Text = "None";
            this.btnL1None.ToolTipText = "Select None";
            this.btnL1None.Click += new System.EventHandler(this.btnNone_Click);
            // 
            // lblL1Lineup
            // 
            this.lblL1Lineup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblL1Lineup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblL1Lineup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblL1Lineup.Name = "lblL1Lineup";
            this.lblL1Lineup.Size = new System.Drawing.Size(0, 22);
            // 
            // tabL2
            // 
            this.tabL2.Controls.Add(this.lvL2Lineup);
            this.tabL2.Controls.Add(this.toolStrip2);
            this.tabL2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabL2.Location = new System.Drawing.Point(4, 22);
            this.tabL2.Name = "tabL2";
            this.tabL2.Padding = new System.Windows.Forms.Padding(3);
            this.tabL2.Size = new System.Drawing.Size(432, 500);
            this.tabL2.TabIndex = 1;
            this.tabL2.Text = "Lineup 2";
            this.tabL2.UseVisualStyleBackColor = true;
            // 
            // lvL2Lineup
            // 
            this.lvL2Lineup.CheckBoxes = true;
            this.lvL2Lineup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader14});
            this.lvL2Lineup.ContextMenuStrip = this.lineupMenuStrip;
            this.lvL2Lineup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvL2Lineup.FullRowSelect = true;
            this.lvL2Lineup.HideSelection = false;
            this.lvL2Lineup.Location = new System.Drawing.Point(3, 28);
            this.lvL2Lineup.Name = "lvL2Lineup";
            this.lvL2Lineup.Size = new System.Drawing.Size(426, 469);
            this.lvL2Lineup.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvL2Lineup.TabIndex = 5;
            this.lvL2Lineup.UseCompatibleStateImageBehavior = false;
            this.lvL2Lineup.View = System.Windows.Forms.View.Details;
            this.lvL2Lineup.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            this.lvL2Lineup.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvLineup_ItemCheck);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "CallSign";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Channel";
            this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "StationID";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Name";
            this.columnHeader14.Width = 175;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Enabled = false;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L2IncludeExclude,
            this.btnL2All,
            this.btnL2None,
            this.lblL2Lineup});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(426, 25);
            this.toolStrip2.TabIndex = 4;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // L2IncludeExclude
            // 
            this.L2IncludeExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.L2IncludeExclude.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L2includeToolStripMenuItem,
            this.L2excludeToolStripMenuItem});
            this.L2IncludeExclude.Image = ((System.Drawing.Image)(resources.GetObject("L2IncludeExclude.Image")));
            this.L2IncludeExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.L2IncludeExclude.Name = "L2IncludeExclude";
            this.L2IncludeExclude.Size = new System.Drawing.Size(29, 22);
            this.L2IncludeExclude.Text = "Include/Exclude";
            this.L2IncludeExclude.ToolTipText = "Include/Exclude Lineup";
            // 
            // L2includeToolStripMenuItem
            // 
            this.L2includeToolStripMenuItem.CheckOnClick = true;
            this.L2includeToolStripMenuItem.Name = "L2includeToolStripMenuItem";
            this.L2includeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L2includeToolStripMenuItem.Text = "Include";
            this.L2includeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // L2excludeToolStripMenuItem
            // 
            this.L2excludeToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.L2excludeToolStripMenuItem.Checked = true;
            this.L2excludeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.L2excludeToolStripMenuItem.Name = "L2excludeToolStripMenuItem";
            this.L2excludeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L2excludeToolStripMenuItem.Text = "Exclude";
            this.L2excludeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // btnL2All
            // 
            this.btnL2All.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL2All.Image = ((System.Drawing.Image)(resources.GetObject("btnL2All.Image")));
            this.btnL2All.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL2All.Name = "btnL2All";
            this.btnL2All.Size = new System.Drawing.Size(25, 22);
            this.btnL2All.Text = "All";
            this.btnL2All.ToolTipText = "Select All";
            this.btnL2All.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnL2None
            // 
            this.btnL2None.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL2None.Image = ((System.Drawing.Image)(resources.GetObject("btnL2None.Image")));
            this.btnL2None.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL2None.Name = "btnL2None";
            this.btnL2None.Size = new System.Drawing.Size(40, 22);
            this.btnL2None.Text = "None";
            this.btnL2None.ToolTipText = "Select None";
            this.btnL2None.Click += new System.EventHandler(this.btnNone_Click);
            // 
            // lblL2Lineup
            // 
            this.lblL2Lineup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblL2Lineup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblL2Lineup.Name = "lblL2Lineup";
            this.lblL2Lineup.Size = new System.Drawing.Size(0, 22);
            // 
            // tabL3
            // 
            this.tabL3.Controls.Add(this.lvL3Lineup);
            this.tabL3.Controls.Add(this.toolStrip3);
            this.tabL3.Location = new System.Drawing.Point(4, 22);
            this.tabL3.Name = "tabL3";
            this.tabL3.Padding = new System.Windows.Forms.Padding(3);
            this.tabL3.Size = new System.Drawing.Size(432, 500);
            this.tabL3.TabIndex = 2;
            this.tabL3.Text = "Lineup 3";
            this.tabL3.UseVisualStyleBackColor = true;
            // 
            // lvL3Lineup
            // 
            this.lvL3Lineup.CheckBoxes = true;
            this.lvL3Lineup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader15});
            this.lvL3Lineup.ContextMenuStrip = this.lineupMenuStrip;
            this.lvL3Lineup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvL3Lineup.FullRowSelect = true;
            this.lvL3Lineup.HideSelection = false;
            this.lvL3Lineup.Location = new System.Drawing.Point(3, 28);
            this.lvL3Lineup.Name = "lvL3Lineup";
            this.lvL3Lineup.Size = new System.Drawing.Size(426, 469);
            this.lvL3Lineup.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvL3Lineup.TabIndex = 7;
            this.lvL3Lineup.UseCompatibleStateImageBehavior = false;
            this.lvL3Lineup.View = System.Windows.Forms.View.Details;
            this.lvL3Lineup.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            this.lvL3Lineup.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvLineup_ItemCheck);
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "CallSign";
            this.columnHeader7.Width = 100;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Channel";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "StationID";
            this.columnHeader9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Name";
            this.columnHeader15.Width = 175;
            // 
            // toolStrip3
            // 
            this.toolStrip3.Enabled = false;
            this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L3IncludeExclude,
            this.btnL3All,
            this.btnL3None,
            this.lblL3Lineup});
            this.toolStrip3.Location = new System.Drawing.Point(3, 3);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(426, 25);
            this.toolStrip3.TabIndex = 6;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // L3IncludeExclude
            // 
            this.L3IncludeExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.L3IncludeExclude.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L3includeToolStripMenuItem,
            this.L3excludeToolStripMenuItem});
            this.L3IncludeExclude.Image = ((System.Drawing.Image)(resources.GetObject("L3IncludeExclude.Image")));
            this.L3IncludeExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.L3IncludeExclude.Name = "L3IncludeExclude";
            this.L3IncludeExclude.Size = new System.Drawing.Size(29, 22);
            this.L3IncludeExclude.Text = "Include/Exclude";
            this.L3IncludeExclude.ToolTipText = "Include/Exclude Lineup";
            // 
            // L3includeToolStripMenuItem
            // 
            this.L3includeToolStripMenuItem.CheckOnClick = true;
            this.L3includeToolStripMenuItem.Name = "L3includeToolStripMenuItem";
            this.L3includeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L3includeToolStripMenuItem.Text = "Include";
            this.L3includeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // L3excludeToolStripMenuItem
            // 
            this.L3excludeToolStripMenuItem.Checked = true;
            this.L3excludeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.L3excludeToolStripMenuItem.Name = "L3excludeToolStripMenuItem";
            this.L3excludeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L3excludeToolStripMenuItem.Text = "Exclude";
            this.L3excludeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // btnL3All
            // 
            this.btnL3All.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL3All.Image = ((System.Drawing.Image)(resources.GetObject("btnL3All.Image")));
            this.btnL3All.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL3All.Name = "btnL3All";
            this.btnL3All.Size = new System.Drawing.Size(25, 22);
            this.btnL3All.Text = "All";
            this.btnL3All.ToolTipText = "Select All";
            this.btnL3All.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnL3None
            // 
            this.btnL3None.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL3None.Image = ((System.Drawing.Image)(resources.GetObject("btnL3None.Image")));
            this.btnL3None.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL3None.Name = "btnL3None";
            this.btnL3None.Size = new System.Drawing.Size(40, 22);
            this.btnL3None.Text = "None";
            this.btnL3None.ToolTipText = "Select None";
            this.btnL3None.Click += new System.EventHandler(this.btnNone_Click);
            // 
            // lblL3Lineup
            // 
            this.lblL3Lineup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblL3Lineup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblL3Lineup.Name = "lblL3Lineup";
            this.lblL3Lineup.Size = new System.Drawing.Size(0, 22);
            // 
            // tabL4
            // 
            this.tabL4.Controls.Add(this.lvL4Lineup);
            this.tabL4.Controls.Add(this.toolStrip4);
            this.tabL4.Location = new System.Drawing.Point(4, 22);
            this.tabL4.Name = "tabL4";
            this.tabL4.Padding = new System.Windows.Forms.Padding(3);
            this.tabL4.Size = new System.Drawing.Size(432, 500);
            this.tabL4.TabIndex = 3;
            this.tabL4.Text = "Lineup 4";
            this.tabL4.UseVisualStyleBackColor = true;
            // 
            // lvL4Lineup
            // 
            this.lvL4Lineup.CheckBoxes = true;
            this.lvL4Lineup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader16});
            this.lvL4Lineup.ContextMenuStrip = this.lineupMenuStrip;
            this.lvL4Lineup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvL4Lineup.FullRowSelect = true;
            this.lvL4Lineup.HideSelection = false;
            this.lvL4Lineup.Location = new System.Drawing.Point(3, 28);
            this.lvL4Lineup.Name = "lvL4Lineup";
            this.lvL4Lineup.Size = new System.Drawing.Size(426, 469);
            this.lvL4Lineup.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvL4Lineup.TabIndex = 7;
            this.lvL4Lineup.UseCompatibleStateImageBehavior = false;
            this.lvL4Lineup.View = System.Windows.Forms.View.Details;
            this.lvL4Lineup.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            this.lvL4Lineup.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvLineup_ItemCheck);
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "CallSign";
            this.columnHeader10.Width = 100;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Channel";
            this.columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "StationID";
            this.columnHeader12.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Name";
            this.columnHeader16.Width = 175;
            // 
            // toolStrip4
            // 
            this.toolStrip4.Enabled = false;
            this.toolStrip4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L4IncludeExclude,
            this.btnL4All,
            this.btnL4None,
            this.lblL4Lineup});
            this.toolStrip4.Location = new System.Drawing.Point(3, 3);
            this.toolStrip4.Name = "toolStrip4";
            this.toolStrip4.Size = new System.Drawing.Size(426, 25);
            this.toolStrip4.TabIndex = 6;
            this.toolStrip4.Text = "toolStrip4";
            // 
            // L4IncludeExclude
            // 
            this.L4IncludeExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.L4IncludeExclude.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L4includeToolStripMenuItem,
            this.L4excludeToolStripMenuItem});
            this.L4IncludeExclude.Image = ((System.Drawing.Image)(resources.GetObject("L4IncludeExclude.Image")));
            this.L4IncludeExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.L4IncludeExclude.Name = "L4IncludeExclude";
            this.L4IncludeExclude.Size = new System.Drawing.Size(29, 22);
            this.L4IncludeExclude.Text = "Include/Exclude";
            this.L4IncludeExclude.ToolTipText = "Include/Exclude Lineup";
            // 
            // L4includeToolStripMenuItem
            // 
            this.L4includeToolStripMenuItem.CheckOnClick = true;
            this.L4includeToolStripMenuItem.Name = "L4includeToolStripMenuItem";
            this.L4includeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L4includeToolStripMenuItem.Text = "Include";
            this.L4includeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // L4excludeToolStripMenuItem
            // 
            this.L4excludeToolStripMenuItem.Checked = true;
            this.L4excludeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.L4excludeToolStripMenuItem.Name = "L4excludeToolStripMenuItem";
            this.L4excludeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L4excludeToolStripMenuItem.Text = "Exclude";
            this.L4excludeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // btnL4All
            // 
            this.btnL4All.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL4All.Image = ((System.Drawing.Image)(resources.GetObject("btnL4All.Image")));
            this.btnL4All.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL4All.Name = "btnL4All";
            this.btnL4All.Size = new System.Drawing.Size(25, 22);
            this.btnL4All.Text = "All";
            this.btnL4All.ToolTipText = "Select All";
            this.btnL4All.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnL4None
            // 
            this.btnL4None.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnL4None.Image = ((System.Drawing.Image)(resources.GetObject("btnL4None.Image")));
            this.btnL4None.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL4None.Name = "btnL4None";
            this.btnL4None.Size = new System.Drawing.Size(40, 22);
            this.btnL4None.Text = "None";
            this.btnL4None.ToolTipText = "Select None";
            this.btnL4None.Click += new System.EventHandler(this.btnNone_Click);
            // 
            // lblL4Lineup
            // 
            this.lblL4Lineup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblL4Lineup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblL4Lineup.Name = "lblL4Lineup";
            this.lblL4Lineup.Size = new System.Drawing.Size(0, 22);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lvL5Lineup);
            this.tabPage1.Controls.Add(this.toolStrip5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(432, 500);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "Custom Lineup";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lvL5Lineup
            // 
            this.lvL5Lineup.CheckBoxes = true;
            this.lvL5Lineup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader17,
            this.columnHeader18,
            this.columnHeader19,
            this.columnHeader20});
            this.lvL5Lineup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvL5Lineup.Enabled = false;
            this.lvL5Lineup.FullRowSelect = true;
            this.lvL5Lineup.HideSelection = false;
            this.lvL5Lineup.Location = new System.Drawing.Point(3, 28);
            this.lvL5Lineup.Name = "lvL5Lineup";
            this.lvL5Lineup.Size = new System.Drawing.Size(426, 469);
            this.lvL5Lineup.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvL5Lineup.TabIndex = 9;
            this.lvL5Lineup.UseCompatibleStateImageBehavior = false;
            this.lvL5Lineup.View = System.Windows.Forms.View.Details;
            this.lvL5Lineup.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            this.lvL5Lineup.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvL5Lineup_ItemCheck);
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "CallSign";
            this.columnHeader17.Width = 100;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "Channel";
            this.columnHeader18.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader19
            // 
            this.columnHeader19.Text = "StationID";
            this.columnHeader19.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader20
            // 
            this.columnHeader20.Text = "Name";
            this.columnHeader20.Width = 175;
            // 
            // toolStrip5
            // 
            this.toolStrip5.Enabled = false;
            this.toolStrip5.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L5IncludeExclude,
            this.btnCustomLineup});
            this.toolStrip5.Location = new System.Drawing.Point(3, 3);
            this.toolStrip5.Name = "toolStrip5";
            this.toolStrip5.Size = new System.Drawing.Size(426, 25);
            this.toolStrip5.TabIndex = 8;
            this.toolStrip5.Text = "toolStrip5";
            // 
            // L5IncludeExclude
            // 
            this.L5IncludeExclude.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.L5IncludeExclude.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.L5includeToolStripMenuItem,
            this.L5excludeToolStripMenuItem});
            this.L5IncludeExclude.Image = ((System.Drawing.Image)(resources.GetObject("L5IncludeExclude.Image")));
            this.L5IncludeExclude.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.L5IncludeExclude.Name = "L5IncludeExclude";
            this.L5IncludeExclude.Size = new System.Drawing.Size(29, 22);
            this.L5IncludeExclude.Text = "Include/Exclude";
            this.L5IncludeExclude.ToolTipText = "Include/Exclude Lineup";
            // 
            // L5includeToolStripMenuItem
            // 
            this.L5includeToolStripMenuItem.CheckOnClick = true;
            this.L5includeToolStripMenuItem.Name = "L5includeToolStripMenuItem";
            this.L5includeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L5includeToolStripMenuItem.Text = "Include";
            this.L5includeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // L5excludeToolStripMenuItem
            // 
            this.L5excludeToolStripMenuItem.Checked = true;
            this.L5excludeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.L5excludeToolStripMenuItem.Name = "L5excludeToolStripMenuItem";
            this.L5excludeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.L5excludeToolStripMenuItem.Text = "Exclude";
            this.L5excludeToolStripMenuItem.Click += new System.EventHandler(this.LineupEnableToolStripMenuItem_Click);
            // 
            // btnCustomLineup
            // 
            this.btnCustomLineup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnCustomLineup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCustomLineup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCustomLineup.Image = ((System.Drawing.Image)(resources.GetObject("btnCustomLineup.Image")));
            this.btnCustomLineup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCustomLineup.Name = "btnCustomLineup";
            this.btnCustomLineup.Size = new System.Drawing.Size(229, 22);
            this.btnCustomLineup.Text = "Click here to manage custom lineups.";
            this.btnCustomLineup.ToolTipText = "Click here to manage custom lineups.";
            this.btnCustomLineup.ButtonClick += new System.EventHandler(this.btnCustomLineup_ButtonClick);
            this.btnCustomLineup.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.btnCustomLineup_DropDownItemClicked);
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
            // frmMain
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
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EPG123 Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabConfigs.ResumeLayout(false);
            this.tabConfig.ResumeLayout(false);
            this.tabConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDays)).EndInit();
            this.tabXmltv.ResumeLayout(false);
            this.tabXmltv.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFillerDuration)).EndInit();
            this.tabTask.ResumeLayout(false);
            this.tabTask.PerformLayout();
            this.grpAccount.ResumeLayout(false);
            this.grpAccount.PerformLayout();
            this.tabLineups.ResumeLayout(false);
            this.tabL1.ResumeLayout(false);
            this.tabL1.PerformLayout();
            this.lineupMenuStrip.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabL2.ResumeLayout(false);
            this.tabL2.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabL3.ResumeLayout(false);
            this.tabL3.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.tabL4.ResumeLayout(false);
            this.tabL4.PerformLayout();
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.toolStrip5.ResumeLayout(false);
            this.toolStrip5.PerformLayout();
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
        private System.Windows.Forms.TabPage tabL1;
        private System.Windows.Forms.ListView lvL1Lineup;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnL1All;
        private System.Windows.Forms.ToolStripButton btnL1None;
        private System.Windows.Forms.ToolStripLabel lblL1Lineup;
        private System.Windows.Forms.TabPage tabL2;
        private System.Windows.Forms.ListView lvL2Lineup;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton btnL2All;
        private System.Windows.Forms.ToolStripButton btnL2None;
        private System.Windows.Forms.ToolStripLabel lblL2Lineup;
        private System.Windows.Forms.TabPage tabL3;
        private System.Windows.Forms.ListView lvL3Lineup;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripButton btnL3All;
        private System.Windows.Forms.ToolStripButton btnL3None;
        private System.Windows.Forms.ToolStripLabel lblL3Lineup;
        private System.Windows.Forms.TabPage tabL4;
        private System.Windows.Forms.ListView lvL4Lineup;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripButton btnL4All;
        private System.Windows.Forms.ToolStripButton btnL4None;
        private System.Windows.Forms.ToolStripLabel lblL4Lineup;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TextBox txtAcctExpires;
        private System.Windows.Forms.Label lblExpiration;
        private System.Windows.Forms.NumericUpDown numDays;
        private System.Windows.Forms.Label lblDaysDownload;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnClientLineups;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.CheckBox cbOadOverride;
        private System.Windows.Forms.CheckBox cbAddNewStations;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.ToolStripDropDownButton L1IncludeExclude;
        private System.Windows.Forms.ToolStripMenuItem L1includeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem L1excludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton L2IncludeExclude;
        private System.Windows.Forms.ToolStripMenuItem L2includeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem L2excludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton L3IncludeExclude;
        private System.Windows.Forms.ToolStripMenuItem L3includeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem L3excludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton L4IncludeExclude;
        private System.Windows.Forms.ToolStripMenuItem L4includeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem L4excludeToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.CheckBox cbXmltv;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListView lvL5Lineup;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ToolStrip toolStrip5;
        private System.Windows.Forms.ToolStripDropDownButton L5IncludeExclude;
        private System.Windows.Forms.ToolStripMenuItem L5includeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem L5excludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton btnCustomLineup;
        private System.Windows.Forms.ContextMenuStrip lineupMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardMenuItem;
        private System.Windows.Forms.TabControl tabConfigs;
        private System.Windows.Forms.TabPage tabConfig;
        private System.Windows.Forms.TabPage tabXmltv;
        private System.Windows.Forms.CheckBox ckXmltvFillerData;
        private System.Windows.Forms.TextBox txtSubstitutePath;
        private System.Windows.Forms.CheckBox ckSubstitutePath;
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
        private System.Windows.Forms.Label lblFillerDescription;
        private System.Windows.Forms.Label lblFillerDuration;
        private System.Windows.Forms.NumericUpDown numFillerDuration;
        private System.Windows.Forms.Label lblXmltvOutput;
        private System.Windows.Forms.Button btnXmltvOutput;
        private System.Windows.Forms.TextBox tbXmltvOutput;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox cbTVDB;
        private System.Windows.Forms.CheckBox cbPrefixTitle;
        private System.Windows.Forms.CheckBox cbPrefixDescription;
        private System.Windows.Forms.CheckBox cbAppendDescription;
        private System.Windows.Forms.Button btnSdLogos;
        private System.Windows.Forms.Label lblPreferredLogos;
        private System.Windows.Forms.ComboBox cmbPreferredLogos;
        private System.Windows.Forms.CheckBox cbSeriesPosterArt;
        private System.Windows.Forms.CheckBox cbTMDb;
        private System.Windows.Forms.CheckBox cbSdLogos;
        private System.Windows.Forms.Label lblAlternateLogos;
        private System.Windows.Forms.ComboBox cmbAlternateLogos;
        private System.Windows.Forms.Label lblXmltvLogosNote;
        private System.Windows.Forms.CheckBox cbAlternateSEFormat;
        private System.Windows.Forms.CheckBox cbBrandLogo;
        private System.Windows.Forms.CheckBox ckXmltvExtendedInfo;
    }
}