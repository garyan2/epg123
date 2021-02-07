namespace epg123
{
    partial class frmWmcTweak
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWmcTweak));
            this.btnCommitGuideChanges = new System.Windows.Forms.Button();
            this.cbMainShowDetails = new System.Windows.Forms.CheckBox();
            this.trackMinutes = new System.Windows.Forms.TrackBar();
            this.grpMainEPG = new System.Windows.Forms.GroupBox();
            this.cbClock = new System.Windows.Forms.CheckBox();
            this.cbExpandedMovie = new System.Windows.Forms.CheckBox();
            this.cbExpandedEpg = new System.Windows.Forms.CheckBox();
            this.cbChannelName = new System.Windows.Forms.CheckBox();
            this.cbCenterLogo = new System.Windows.Forms.CheckBox();
            this.cbRemoveAnimations = new System.Windows.Forms.CheckBox();
            this.cbHideNumber = new System.Windows.Forms.CheckBox();
            this.trackColumnWidth = new System.Windows.Forms.TrackBar();
            this.trackLogoSize = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.lblColumnWidth = new System.Windows.Forms.Label();
            this.lblRowHeight = new System.Windows.Forms.Label();
            this.lblLogoSize = new System.Windows.Forms.Label();
            this.btnResetToDefault = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbAutoAdjustColumnWidth = new System.Windows.Forms.CheckBox();
            this.trackRowHeight = new System.Windows.Forms.TrackBar();
            this.label12 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCellFontSize = new System.Windows.Forms.Label();
            this.trackCellFontSize = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.lblMinutes = new System.Windows.Forms.Label();
            this.cbMiniShowDetails = new System.Windows.Forms.CheckBox();
            this.lblMiniRows = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.trackMiniRows = new System.Windows.Forms.TrackBar();
            this.lblMainRows = new System.Windows.Forms.Label();
            this.trackMainRows = new System.Windows.Forms.TrackBar();
            this.btnTunerLimit = new System.Windows.Forms.Button();
            this.grpWmcTweak = new System.Windows.Forms.GroupBox();
            this.cbNoSuccess = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.lblStatusLogoOpaque = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.rdoDark = new System.Windows.Forms.RadioButton();
            this.rdoLight = new System.Windows.Forms.RadioButton();
            this.rdoNone = new System.Windows.Forms.RadioButton();
            this.pbStatusLogo = new System.Windows.Forms.PictureBox();
            this.lblMovieGuide = new System.Windows.Forms.Label();
            this.btnMovieGuide = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numInstantReplay = new System.Windows.Forms.NumericUpDown();
            this.numSkipAhead = new System.Windows.Forms.NumericUpDown();
            this.numBuffer = new System.Windows.Forms.NumericUpDown();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.lblPatternExample = new System.Windows.Forms.Label();
            this.btnClearPattern = new System.Windows.Forms.Button();
            this.btnSetPattern = new System.Windows.Forms.Button();
            this.txtNamePattern = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnRemoveLogos = new System.Windows.Forms.Button();
            this.lblTunerLimit = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackMinutes)).BeginInit();
            this.grpMainEPG.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackColumnWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackLogoSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackRowHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackCellFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackMiniRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackMainRows)).BeginInit();
            this.grpWmcTweak.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatusLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInstantReplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSkipAhead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCommitGuideChanges
            // 
            this.btnCommitGuideChanges.Location = new System.Drawing.Point(251, 508);
            this.btnCommitGuideChanges.Name = "btnCommitGuideChanges";
            this.btnCommitGuideChanges.Size = new System.Drawing.Size(75, 23);
            this.btnCommitGuideChanges.TabIndex = 0;
            this.btnCommitGuideChanges.Text = "Update";
            this.btnCommitGuideChanges.UseVisualStyleBackColor = true;
            this.btnCommitGuideChanges.Click += new System.EventHandler(this.btnUpdateGuideConfigurations_Click);
            // 
            // cbMainShowDetails
            // 
            this.cbMainShowDetails.AutoSize = true;
            this.cbMainShowDetails.Checked = true;
            this.cbMainShowDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMainShowDetails.Location = new System.Drawing.Point(182, 209);
            this.cbMainShowDetails.Name = "cbMainShowDetails";
            this.cbMainShowDetails.Size = new System.Drawing.Size(130, 17);
            this.cbMainShowDetails.TabIndex = 1;
            this.cbMainShowDetails.Text = "Show Program Details";
            this.cbMainShowDetails.UseVisualStyleBackColor = true;
            this.cbMainShowDetails.CheckStateChanged += new System.EventHandler(this.cbMainShowDetails_CheckStateChanged);
            // 
            // trackMinutes
            // 
            this.trackMinutes.AutoSize = false;
            this.trackMinutes.LargeChange = 30;
            this.trackMinutes.Location = new System.Drawing.Point(6, 301);
            this.trackMinutes.Maximum = 240;
            this.trackMinutes.Minimum = 30;
            this.trackMinutes.Name = "trackMinutes";
            this.trackMinutes.Size = new System.Drawing.Size(170, 35);
            this.trackMinutes.SmallChange = 30;
            this.trackMinutes.TabIndex = 3;
            this.trackMinutes.TickFrequency = 30;
            this.trackMinutes.Value = 120;
            this.trackMinutes.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // grpMainEPG
            // 
            this.grpMainEPG.Controls.Add(this.cbClock);
            this.grpMainEPG.Controls.Add(this.cbExpandedMovie);
            this.grpMainEPG.Controls.Add(this.cbExpandedEpg);
            this.grpMainEPG.Controls.Add(this.cbChannelName);
            this.grpMainEPG.Controls.Add(this.cbCenterLogo);
            this.grpMainEPG.Controls.Add(this.cbRemoveAnimations);
            this.grpMainEPG.Controls.Add(this.cbHideNumber);
            this.grpMainEPG.Controls.Add(this.trackColumnWidth);
            this.grpMainEPG.Controls.Add(this.trackLogoSize);
            this.grpMainEPG.Controls.Add(this.label7);
            this.grpMainEPG.Controls.Add(this.lblColumnWidth);
            this.grpMainEPG.Controls.Add(this.lblRowHeight);
            this.grpMainEPG.Controls.Add(this.lblLogoSize);
            this.grpMainEPG.Controls.Add(this.btnResetToDefault);
            this.grpMainEPG.Controls.Add(this.label4);
            this.grpMainEPG.Controls.Add(this.cbAutoAdjustColumnWidth);
            this.grpMainEPG.Controls.Add(this.trackRowHeight);
            this.grpMainEPG.Controls.Add(this.label12);
            this.grpMainEPG.Controls.Add(this.label5);
            this.grpMainEPG.Controls.Add(this.lblCellFontSize);
            this.grpMainEPG.Controls.Add(this.trackCellFontSize);
            this.grpMainEPG.Controls.Add(this.label3);
            this.grpMainEPG.Controls.Add(this.btnCommitGuideChanges);
            this.grpMainEPG.Controls.Add(this.lblMinutes);
            this.grpMainEPG.Controls.Add(this.cbMiniShowDetails);
            this.grpMainEPG.Controls.Add(this.lblMiniRows);
            this.grpMainEPG.Controls.Add(this.trackMinutes);
            this.grpMainEPG.Controls.Add(this.label2);
            this.grpMainEPG.Controls.Add(this.label1);
            this.grpMainEPG.Controls.Add(this.cbMainShowDetails);
            this.grpMainEPG.Controls.Add(this.trackMiniRows);
            this.grpMainEPG.Controls.Add(this.lblMainRows);
            this.grpMainEPG.Controls.Add(this.trackMainRows);
            this.grpMainEPG.Location = new System.Drawing.Point(12, 12);
            this.grpMainEPG.Name = "grpMainEPG";
            this.grpMainEPG.Size = new System.Drawing.Size(332, 537);
            this.grpMainEPG.TabIndex = 4;
            this.grpMainEPG.TabStop = false;
            this.grpMainEPG.Text = "Guide Tweaks";
            // 
            // cbClock
            // 
            this.cbClock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbClock.Location = new System.Drawing.Point(234, 398);
            this.cbClock.Name = "cbClock";
            this.cbClock.Size = new System.Drawing.Size(92, 40);
            this.cbClock.TabIndex = 31;
            this.cbClock.Text = "Add Date to Clock Display";
            this.cbClock.UseVisualStyleBackColor = true;
            // 
            // cbExpandedMovie
            // 
            this.cbExpandedMovie.AutoSize = true;
            this.cbExpandedMovie.Location = new System.Drawing.Point(9, 398);
            this.cbExpandedMovie.Name = "cbExpandedMovie";
            this.cbExpandedMovie.Size = new System.Drawing.Size(166, 17);
            this.cbExpandedMovie.TabIndex = 30;
            this.cbExpandedMovie.Text = "Add Movie Years to Grid Cells";
            this.cbExpandedMovie.UseVisualStyleBackColor = true;
            // 
            // cbExpandedEpg
            // 
            this.cbExpandedEpg.AutoSize = true;
            this.cbExpandedEpg.Location = new System.Drawing.Point(9, 421);
            this.cbExpandedEpg.Name = "cbExpandedEpg";
            this.cbExpandedEpg.Size = new System.Drawing.Size(173, 17);
            this.cbExpandedEpg.TabIndex = 29;
            this.cbExpandedEpg.Text = "Add Episode Titles to Grid Cells";
            this.cbExpandedEpg.UseVisualStyleBackColor = true;
            // 
            // cbChannelName
            // 
            this.cbChannelName.AutoSize = true;
            this.cbChannelName.Location = new System.Drawing.Point(9, 444);
            this.cbChannelName.Name = "cbChannelName";
            this.cbChannelName.Size = new System.Drawing.Size(200, 17);
            this.cbChannelName.TabIndex = 28;
            this.cbChannelName.Text = "Replace Callsign with Channel Name";
            this.cbChannelName.UseVisualStyleBackColor = true;
            // 
            // cbCenterLogo
            // 
            this.cbCenterLogo.AutoSize = true;
            this.cbCenterLogo.Location = new System.Drawing.Point(9, 513);
            this.cbCenterLogo.Name = "cbCenterLogo";
            this.cbCenterLogo.Size = new System.Drawing.Size(89, 17);
            this.cbCenterLogo.TabIndex = 27;
            this.cbCenterLogo.Text = "Center Logos";
            this.cbCenterLogo.UseVisualStyleBackColor = true;
            // 
            // cbRemoveAnimations
            // 
            this.cbRemoveAnimations.AutoSize = true;
            this.cbRemoveAnimations.Location = new System.Drawing.Point(9, 467);
            this.cbRemoveAnimations.Name = "cbRemoveAnimations";
            this.cbRemoveAnimations.Size = new System.Drawing.Size(194, 17);
            this.cbRemoveAnimations.TabIndex = 26;
            this.cbRemoveAnimations.Text = "Remove Channel Focus Animations";
            this.cbRemoveAnimations.UseVisualStyleBackColor = true;
            // 
            // cbHideNumber
            // 
            this.cbHideNumber.AutoSize = true;
            this.cbHideNumber.Location = new System.Drawing.Point(9, 490);
            this.cbHideNumber.Name = "cbHideNumber";
            this.cbHideNumber.Size = new System.Drawing.Size(135, 17);
            this.cbHideNumber.TabIndex = 25;
            this.cbHideNumber.Text = "Hide Channel Numbers";
            this.cbHideNumber.UseVisualStyleBackColor = true;
            this.cbHideNumber.CheckedChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // trackColumnWidth
            // 
            this.trackColumnWidth.AutoSize = false;
            this.trackColumnWidth.LargeChange = 1;
            this.trackColumnWidth.Location = new System.Drawing.Point(6, 355);
            this.trackColumnWidth.Maximum = 450;
            this.trackColumnWidth.Name = "trackColumnWidth";
            this.trackColumnWidth.Size = new System.Drawing.Size(170, 35);
            this.trackColumnWidth.TabIndex = 12;
            this.trackColumnWidth.TickFrequency = 30;
            this.trackColumnWidth.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // trackLogoSize
            // 
            this.trackLogoSize.AutoSize = false;
            this.trackLogoSize.LargeChange = 1;
            this.trackLogoSize.Location = new System.Drawing.Point(6, 139);
            this.trackLogoSize.Maximum = 2;
            this.trackLogoSize.Name = "trackLogoSize";
            this.trackLogoSize.Size = new System.Drawing.Size(170, 35);
            this.trackLogoSize.TabIndex = 20;
            this.trackLogoSize.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(6, 69);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Guide Row Height";
            // 
            // lblColumnWidth
            // 
            this.lblColumnWidth.AutoSize = true;
            this.lblColumnWidth.Location = new System.Drawing.Point(182, 355);
            this.lblColumnWidth.Name = "lblColumnWidth";
            this.lblColumnWidth.Size = new System.Drawing.Size(70, 13);
            this.lblColumnWidth.TabIndex = 13;
            this.lblColumnWidth.Text = "Default pixels";
            // 
            // lblRowHeight
            // 
            this.lblRowHeight.AutoSize = true;
            this.lblRowHeight.Location = new System.Drawing.Point(182, 85);
            this.lblRowHeight.Name = "lblRowHeight";
            this.lblRowHeight.Size = new System.Drawing.Size(82, 13);
            this.lblRowHeight.TabIndex = 23;
            this.lblRowHeight.Text = "1.85X Font Size";
            // 
            // lblLogoSize
            // 
            this.lblLogoSize.AutoSize = true;
            this.lblLogoSize.Location = new System.Drawing.Point(182, 139);
            this.lblLogoSize.Name = "lblLogoSize";
            this.lblLogoSize.Size = new System.Drawing.Size(32, 13);
            this.lblLogoSize.TabIndex = 21;
            this.lblLogoSize.Text = "Small";
            // 
            // btnResetToDefault
            // 
            this.btnResetToDefault.Location = new System.Drawing.Point(170, 508);
            this.btnResetToDefault.Name = "btnResetToDefault";
            this.btnResetToDefault.Size = new System.Drawing.Size(75, 23);
            this.btnResetToDefault.TabIndex = 6;
            this.btnResetToDefault.Text = "Default";
            this.btnResetToDefault.UseVisualStyleBackColor = true;
            this.btnResetToDefault.Click += new System.EventHandler(this.btnUpdateGuideConfigurations_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 339);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(135, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Channel Column Width";
            // 
            // cbAutoAdjustColumnWidth
            // 
            this.cbAutoAdjustColumnWidth.AutoSize = true;
            this.cbAutoAdjustColumnWidth.Location = new System.Drawing.Point(182, 371);
            this.cbAutoAdjustColumnWidth.Name = "cbAutoAdjustColumnWidth";
            this.cbAutoAdjustColumnWidth.Size = new System.Drawing.Size(120, 17);
            this.cbAutoAdjustColumnWidth.TabIndex = 15;
            this.cbAutoAdjustColumnWidth.Text = "Automatically Adjust";
            this.cbAutoAdjustColumnWidth.UseVisualStyleBackColor = true;
            this.cbAutoAdjustColumnWidth.CheckStateChanged += new System.EventHandler(this.cbAutoAdjustColumnWidth_CheckStateChanged);
            // 
            // trackRowHeight
            // 
            this.trackRowHeight.AutoSize = false;
            this.trackRowHeight.LargeChange = 1;
            this.trackRowHeight.Location = new System.Drawing.Point(6, 85);
            this.trackRowHeight.Maximum = 100;
            this.trackRowHeight.Minimum = 50;
            this.trackRowHeight.Name = "trackRowHeight";
            this.trackRowHeight.Size = new System.Drawing.Size(170, 35);
            this.trackRowHeight.TabIndex = 22;
            this.trackRowHeight.TickFrequency = 10;
            this.trackRowHeight.Value = 85;
            this.trackRowHeight.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(6, 123);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(113, 13);
            this.label12.TabIndex = 22;
            this.label12.Text = "Channel Logo Size";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Guide Cell Font Size";
            // 
            // lblCellFontSize
            // 
            this.lblCellFontSize.AutoSize = true;
            this.lblCellFontSize.Location = new System.Drawing.Point(182, 31);
            this.lblCellFontSize.Name = "lblCellFontSize";
            this.lblCellFontSize.Size = new System.Drawing.Size(45, 13);
            this.lblCellFontSize.TabIndex = 16;
            this.lblCellFontSize.Text = "22 point";
            // 
            // trackCellFontSize
            // 
            this.trackCellFontSize.AutoSize = false;
            this.trackCellFontSize.LargeChange = 1;
            this.trackCellFontSize.Location = new System.Drawing.Point(6, 31);
            this.trackCellFontSize.Maximum = 48;
            this.trackCellFontSize.Minimum = 10;
            this.trackCellFontSize.Name = "trackCellFontSize";
            this.trackCellFontSize.Size = new System.Drawing.Size(170, 35);
            this.trackCellFontSize.TabIndex = 15;
            this.trackCellFontSize.Value = 22;
            this.trackCellFontSize.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 285);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Guide Time Span";
            // 
            // lblMinutes
            // 
            this.lblMinutes.AutoSize = true;
            this.lblMinutes.Location = new System.Drawing.Point(182, 301);
            this.lblMinutes.Name = "lblMinutes";
            this.lblMinutes.Size = new System.Drawing.Size(64, 13);
            this.lblMinutes.TabIndex = 5;
            this.lblMinutes.Text = "120 minutes";
            // 
            // cbMiniShowDetails
            // 
            this.cbMiniShowDetails.AutoSize = true;
            this.cbMiniShowDetails.Checked = true;
            this.cbMiniShowDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMiniShowDetails.Location = new System.Drawing.Point(182, 263);
            this.cbMiniShowDetails.Name = "cbMiniShowDetails";
            this.cbMiniShowDetails.Size = new System.Drawing.Size(130, 17);
            this.cbMiniShowDetails.TabIndex = 10;
            this.cbMiniShowDetails.Text = "Show Program Details";
            this.cbMiniShowDetails.UseVisualStyleBackColor = true;
            // 
            // lblMiniRows
            // 
            this.lblMiniRows.AutoSize = true;
            this.lblMiniRows.Location = new System.Drawing.Point(182, 247);
            this.lblMiniRows.Name = "lblMiniRows";
            this.lblMiniRows.Size = new System.Drawing.Size(43, 13);
            this.lblMiniRows.TabIndex = 9;
            this.lblMiniRows.Text = "2 Rows";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 231);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Mini-Guide Rows";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 177);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Primary Guide Rows";
            // 
            // trackMiniRows
            // 
            this.trackMiniRows.AutoSize = false;
            this.trackMiniRows.LargeChange = 1;
            this.trackMiniRows.Location = new System.Drawing.Point(6, 247);
            this.trackMiniRows.Maximum = 7;
            this.trackMiniRows.Minimum = 2;
            this.trackMiniRows.Name = "trackMiniRows";
            this.trackMiniRows.Size = new System.Drawing.Size(170, 35);
            this.trackMiniRows.TabIndex = 8;
            this.trackMiniRows.Value = 2;
            this.trackMiniRows.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // lblMainRows
            // 
            this.lblMainRows.AutoSize = true;
            this.lblMainRows.Location = new System.Drawing.Point(182, 193);
            this.lblMainRows.Name = "lblMainRows";
            this.lblMainRows.Size = new System.Drawing.Size(43, 13);
            this.lblMainRows.TabIndex = 7;
            this.lblMainRows.Text = "7 Rows";
            // 
            // trackMainRows
            // 
            this.trackMainRows.AutoSize = false;
            this.trackMainRows.LargeChange = 1;
            this.trackMainRows.Location = new System.Drawing.Point(6, 193);
            this.trackMainRows.Maximum = 25;
            this.trackMainRows.Minimum = 4;
            this.trackMainRows.Name = "trackMainRows";
            this.trackMainRows.Size = new System.Drawing.Size(170, 35);
            this.trackMainRows.TabIndex = 6;
            this.trackMainRows.Value = 7;
            this.trackMainRows.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // btnTunerLimit
            // 
            this.btnTunerLimit.Location = new System.Drawing.Point(251, 19);
            this.btnTunerLimit.Name = "btnTunerLimit";
            this.btnTunerLimit.Size = new System.Drawing.Size(75, 23);
            this.btnTunerLimit.TabIndex = 5;
            this.btnTunerLimit.Text = "Increase";
            this.btnTunerLimit.UseVisualStyleBackColor = true;
            this.btnTunerLimit.Click += new System.EventHandler(this.btnTunerLimit_Click);
            // 
            // grpWmcTweak
            // 
            this.grpWmcTweak.Controls.Add(this.cbNoSuccess);
            this.grpWmcTweak.Controls.Add(this.label14);
            this.grpWmcTweak.Controls.Add(this.lblStatusLogoOpaque);
            this.grpWmcTweak.Controls.Add(this.trackBar1);
            this.grpWmcTweak.Controls.Add(this.rdoDark);
            this.grpWmcTweak.Controls.Add(this.rdoLight);
            this.grpWmcTweak.Controls.Add(this.rdoNone);
            this.grpWmcTweak.Controls.Add(this.pbStatusLogo);
            this.grpWmcTweak.Controls.Add(this.lblMovieGuide);
            this.grpWmcTweak.Controls.Add(this.btnMovieGuide);
            this.grpWmcTweak.Controls.Add(this.label13);
            this.grpWmcTweak.Controls.Add(this.label11);
            this.grpWmcTweak.Controls.Add(this.label6);
            this.grpWmcTweak.Controls.Add(this.numInstantReplay);
            this.grpWmcTweak.Controls.Add(this.numSkipAhead);
            this.grpWmcTweak.Controls.Add(this.numBuffer);
            this.grpWmcTweak.Controls.Add(this.textBox2);
            this.grpWmcTweak.Controls.Add(this.lblPatternExample);
            this.grpWmcTweak.Controls.Add(this.btnClearPattern);
            this.grpWmcTweak.Controls.Add(this.btnSetPattern);
            this.grpWmcTweak.Controls.Add(this.txtNamePattern);
            this.grpWmcTweak.Controls.Add(this.textBox1);
            this.grpWmcTweak.Controls.Add(this.label10);
            this.grpWmcTweak.Controls.Add(this.label9);
            this.grpWmcTweak.Controls.Add(this.btnRemoveLogos);
            this.grpWmcTweak.Controls.Add(this.lblTunerLimit);
            this.grpWmcTweak.Controls.Add(this.btnTunerLimit);
            this.grpWmcTweak.Location = new System.Drawing.Point(349, 12);
            this.grpWmcTweak.Name = "grpWmcTweak";
            this.grpWmcTweak.Size = new System.Drawing.Size(332, 537);
            this.grpWmcTweak.TabIndex = 6;
            this.grpWmcTweak.TabStop = false;
            this.grpWmcTweak.Text = "WMC Tweaks";
            // 
            // cbNoSuccess
            // 
            this.cbNoSuccess.AutoSize = true;
            this.cbNoSuccess.Location = new System.Drawing.Point(9, 514);
            this.cbNoSuccess.Name = "cbNoSuccess";
            this.cbNoSuccess.Size = new System.Drawing.Size(240, 17);
            this.cbNoSuccess.TabIndex = 35;
            this.cbNoSuccess.Text = "Display Warnings, Errors, and Updates ONLY";
            this.cbNoSuccess.UseVisualStyleBackColor = true;
            this.cbNoSuccess.CheckedChanged += new System.EventHandler(this.cbNoSuccess_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(6, 370);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(175, 13);
            this.label14.TabIndex = 34;
            this.label14.Text = "WMC Start Menu Status Logo";
            // 
            // lblStatusLogoOpaque
            // 
            this.lblStatusLogoOpaque.BackColor = System.Drawing.Color.Transparent;
            this.lblStatusLogoOpaque.Location = new System.Drawing.Point(6, 488);
            this.lblStatusLogoOpaque.Name = "lblStatusLogoOpaque";
            this.lblStatusLogoOpaque.Size = new System.Drawing.Size(320, 23);
            this.lblStatusLogoOpaque.TabIndex = 33;
            this.lblStatusLogoOpaque.Text = "100% Opaque";
            this.lblStatusLogoOpaque.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(6, 463);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(320, 45);
            this.trackBar1.TabIndex = 32;
            this.trackBar1.TickFrequency = 5;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBar1.Value = 100;
            this.trackBar1.ValueChanged += new System.EventHandler(this.TrkOpacityChanged);
            // 
            // rdoDark
            // 
            this.rdoDark.AutoSize = true;
            this.rdoDark.Location = new System.Drawing.Point(6, 433);
            this.rdoDark.Name = "rdoDark";
            this.rdoDark.Size = new System.Drawing.Size(85, 17);
            this.rdoDark.TabIndex = 31;
            this.rdoDark.Tag = "Dark";
            this.rdoDark.Text = "Dark Accent";
            this.rdoDark.UseVisualStyleBackColor = true;
            this.rdoDark.CheckedChanged += new System.EventHandler(this.RdoCheckedChanged);
            // 
            // rdoLight
            // 
            this.rdoLight.AutoSize = true;
            this.rdoLight.Location = new System.Drawing.Point(6, 410);
            this.rdoLight.Name = "rdoLight";
            this.rdoLight.Size = new System.Drawing.Size(85, 17);
            this.rdoLight.TabIndex = 30;
            this.rdoLight.Tag = "Light";
            this.rdoLight.Text = "Light Accent";
            this.rdoLight.UseVisualStyleBackColor = true;
            this.rdoLight.CheckedChanged += new System.EventHandler(this.RdoCheckedChanged);
            // 
            // rdoNone
            // 
            this.rdoNone.AutoSize = true;
            this.rdoNone.Checked = true;
            this.rdoNone.Location = new System.Drawing.Point(6, 387);
            this.rdoNone.Name = "rdoNone";
            this.rdoNone.Size = new System.Drawing.Size(51, 17);
            this.rdoNone.TabIndex = 29;
            this.rdoNone.TabStop = true;
            this.rdoNone.Tag = "None";
            this.rdoNone.Text = "None";
            this.rdoNone.UseVisualStyleBackColor = true;
            this.rdoNone.CheckedChanged += new System.EventHandler(this.RdoCheckedChanged);
            // 
            // pbStatusLogo
            // 
            this.pbStatusLogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbStatusLogo.Location = new System.Drawing.Point(97, 387);
            this.pbStatusLogo.Name = "pbStatusLogo";
            this.pbStatusLogo.Size = new System.Drawing.Size(229, 70);
            this.pbStatusLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStatusLogo.TabIndex = 28;
            this.pbStatusLogo.TabStop = false;
            // 
            // lblMovieGuide
            // 
            this.lblMovieGuide.Location = new System.Drawing.Point(6, 82);
            this.lblMovieGuide.Name = "lblMovieGuide";
            this.lblMovieGuide.Size = new System.Drawing.Size(239, 13);
            this.lblMovieGuide.TabIndex = 27;
            this.lblMovieGuide.Text = "Enable Movie Guide (non-US/UK/CA users)";
            // 
            // btnMovieGuide
            // 
            this.btnMovieGuide.Location = new System.Drawing.Point(251, 77);
            this.btnMovieGuide.Name = "btnMovieGuide";
            this.btnMovieGuide.Size = new System.Drawing.Size(75, 23);
            this.btnMovieGuide.TabIndex = 26;
            this.btnMovieGuide.Text = "Enable";
            this.btnMovieGuide.UseVisualStyleBackColor = true;
            this.btnMovieGuide.Click += new System.EventHandler(this.UpdateRegistryValues);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(69, 342);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(144, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "second instant replay interval";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(69, 316);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(134, 13);
            this.label11.TabIndex = 24;
            this.label11.Text = "second skip ahead interval";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(69, 290);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "minute pause buffer";
            // 
            // numInstantReplay
            // 
            this.numInstantReplay.Location = new System.Drawing.Point(9, 338);
            this.numInstantReplay.Name = "numInstantReplay";
            this.numInstantReplay.Size = new System.Drawing.Size(54, 20);
            this.numInstantReplay.TabIndex = 22;
            this.numInstantReplay.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numInstantReplay.ValueChanged += new System.EventHandler(this.UpdateRegistryValues);
            // 
            // numSkipAhead
            // 
            this.numSkipAhead.Location = new System.Drawing.Point(9, 312);
            this.numSkipAhead.Name = "numSkipAhead";
            this.numSkipAhead.Size = new System.Drawing.Size(54, 20);
            this.numSkipAhead.TabIndex = 21;
            this.numSkipAhead.Value = new decimal(new int[] {
            29,
            0,
            0,
            0});
            this.numSkipAhead.ValueChanged += new System.EventHandler(this.UpdateRegistryValues);
            // 
            // numBuffer
            // 
            this.numBuffer.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numBuffer.Location = new System.Drawing.Point(9, 286);
            this.numBuffer.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.numBuffer.Minimum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numBuffer.Name = "numBuffer";
            this.numBuffer.Size = new System.Drawing.Size(54, 20);
            this.numBuffer.TabIndex = 20;
            this.numBuffer.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numBuffer.ValueChanged += new System.EventHandler(this.UpdateRegistryValues);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(144, 121);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(182, 90);
            this.textBox2.TabIndex = 19;
            this.textBox2.Text = "Home Town\r\ns03e07 Home is Where the Art Is\r\n10/06/2019 11:00:00 AM\r\n02/25/2019\r\n3" +
    "7\r\nHGTVP\r\nHome & Garden Television (Pacific)";
            // 
            // lblPatternExample
            // 
            this.lblPatternExample.Location = new System.Drawing.Point(6, 240);
            this.lblPatternExample.Name = "lblPatternExample";
            this.lblPatternExample.Size = new System.Drawing.Size(320, 43);
            this.lblPatternExample.TabIndex = 18;
            this.lblPatternExample.Text = "Home Town_HGTVP_2019_10_06_11_00_00.wtv";
            // 
            // btnClearPattern
            // 
            this.btnClearPattern.Location = new System.Drawing.Point(279, 215);
            this.btnClearPattern.Name = "btnClearPattern";
            this.btnClearPattern.Size = new System.Drawing.Size(47, 23);
            this.btnClearPattern.TabIndex = 17;
            this.btnClearPattern.Text = "Clear";
            this.btnClearPattern.UseVisualStyleBackColor = true;
            this.btnClearPattern.Click += new System.EventHandler(this.BtnUpdateFilePattern);
            // 
            // btnSetPattern
            // 
            this.btnSetPattern.Location = new System.Drawing.Point(226, 215);
            this.btnSetPattern.Name = "btnSetPattern";
            this.btnSetPattern.Size = new System.Drawing.Size(47, 23);
            this.btnSetPattern.TabIndex = 16;
            this.btnSetPattern.Text = "Set";
            this.btnSetPattern.UseVisualStyleBackColor = true;
            this.btnSetPattern.Click += new System.EventHandler(this.BtnUpdateFilePattern);
            // 
            // txtNamePattern
            // 
            this.txtNamePattern.Location = new System.Drawing.Point(9, 217);
            this.txtNamePattern.Name = "txtNamePattern";
            this.txtNamePattern.Size = new System.Drawing.Size(211, 20);
            this.txtNamePattern.TabIndex = 15;
            this.txtNamePattern.Text = "%T_%Cs_%Dt";
            this.txtNamePattern.TextChanged += new System.EventHandler(this.txtNamePattern_TextChanged);
            this.txtNamePattern.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNamePattern_KeyPress);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox1.Location = new System.Drawing.Point(9, 121);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(129, 90);
            this.textBox1.TabIndex = 14;
            this.textBox1.Text = "%T = Movie/Series Title\r\n%Et = Episode Title\r\n%Dt = Start Time\r\n%Do = Original Ai" +
    "r Date\r\n%Ch = Channel Number\r\n%Cs = Channel Call Sign\r\n%Cn = Station Name";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(6, 105);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(188, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Set recordings filename pattern.";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(6, 53);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(239, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Remove all channel logos.";
            // 
            // btnRemoveLogos
            // 
            this.btnRemoveLogos.Location = new System.Drawing.Point(251, 48);
            this.btnRemoveLogos.Name = "btnRemoveLogos";
            this.btnRemoveLogos.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveLogos.TabIndex = 7;
            this.btnRemoveLogos.Text = "Remove";
            this.btnRemoveLogos.UseVisualStyleBackColor = true;
            this.btnRemoveLogos.Click += new System.EventHandler(this.btnRemoveLogos_Click);
            // 
            // lblTunerLimit
            // 
            this.lblTunerLimit.Location = new System.Drawing.Point(6, 24);
            this.lblTunerLimit.Name = "lblTunerLimit";
            this.lblTunerLimit.Size = new System.Drawing.Size(239, 13);
            this.lblTunerLimit.TabIndex = 6;
            this.lblTunerLimit.Text = "Increase tuner limits to 32 per tuner type.";
            // 
            // frmWmcTweak
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(694, 561);
            this.Controls.Add(this.grpWmcTweak);
            this.Controls.Add(this.grpMainEPG);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(710, 600);
            this.Name = "frmWmcTweak";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tweak WMC";
            this.Load += new System.EventHandler(this.frmWmcTweak_Load);
            this.Shown += new System.EventHandler(this.frmWmcTweak_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.trackMinutes)).EndInit();
            this.grpMainEPG.ResumeLayout(false);
            this.grpMainEPG.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackColumnWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackLogoSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackRowHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackCellFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackMiniRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackMainRows)).EndInit();
            this.grpWmcTweak.ResumeLayout(false);
            this.grpWmcTweak.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatusLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInstantReplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSkipAhead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBuffer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCommitGuideChanges;
        private System.Windows.Forms.CheckBox cbMainShowDetails;
        private System.Windows.Forms.TrackBar trackMinutes;
        private System.Windows.Forms.GroupBox grpMainEPG;
        private System.Windows.Forms.Label lblMinutes;
        private System.Windows.Forms.TrackBar trackMainRows;
        private System.Windows.Forms.Label lblMainRows;
        private System.Windows.Forms.Label lblMiniRows;
        private System.Windows.Forms.TrackBar trackMiniRows;
        private System.Windows.Forms.CheckBox cbMiniShowDetails;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblColumnWidth;
        private System.Windows.Forms.TrackBar trackColumnWidth;
        private System.Windows.Forms.CheckBox cbAutoAdjustColumnWidth;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblCellFontSize;
        private System.Windows.Forms.TrackBar trackCellFontSize;
        private System.Windows.Forms.Button btnTunerLimit;
        private System.Windows.Forms.Button btnResetToDefault;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblRowHeight;
        private System.Windows.Forms.TrackBar trackRowHeight;
        private System.Windows.Forms.GroupBox grpWmcTweak;
        private System.Windows.Forms.Label lblTunerLimit;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnRemoveLogos;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnClearPattern;
        private System.Windows.Forms.Button btnSetPattern;
        private System.Windows.Forms.TextBox txtNamePattern;
        private System.Windows.Forms.Label lblPatternExample;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TrackBar trackLogoSize;
        private System.Windows.Forms.Label lblLogoSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown numBuffer;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numInstantReplay;
        private System.Windows.Forms.NumericUpDown numSkipAhead;
        private System.Windows.Forms.Label lblMovieGuide;
        private System.Windows.Forms.Button btnMovieGuide;
        private System.Windows.Forms.CheckBox cbHideNumber;
        private System.Windows.Forms.CheckBox cbRemoveAnimations;
        private System.Windows.Forms.CheckBox cbCenterLogo;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.RadioButton rdoDark;
        private System.Windows.Forms.RadioButton rdoLight;
        private System.Windows.Forms.RadioButton rdoNone;
        private System.Windows.Forms.PictureBox pbStatusLogo;
        private System.Windows.Forms.Label lblStatusLogoOpaque;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox cbChannelName;
        private System.Windows.Forms.CheckBox cbNoSuccess;
        private System.Windows.Forms.CheckBox cbExpandedEpg;
        private System.Windows.Forms.CheckBox cbExpandedMovie;
        private System.Windows.Forms.CheckBox cbClock;
    }
}