namespace epg123Client
{
    partial class frmAddChannel
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
            this.cmbScannedLineups = new System.Windows.Forms.ComboBox();
            this.tabTunerSpace = new System.Windows.Forms.TabControl();
            this.tabChannelTuningInfo = new System.Windows.Forms.TabPage();
            this.chnTiModulationType = new System.Windows.Forms.ComboBox();
            this.lblChnTiModulationType = new System.Windows.Forms.Label();
            this.lblChnTiSubnumber = new System.Windows.Forms.Label();
            this.chnTiPhysicalNumber = new System.Windows.Forms.NumericUpDown();
            this.lblChnTiPhysicalNumber = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chnTiSubnumber = new System.Windows.Forms.NumericUpDown();
            this.chnTiNumber = new System.Windows.Forms.NumericUpDown();
            this.chnTiCallsign = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddChannelTuningInfo = new System.Windows.Forms.Button();
            this.tabDvbTuningInfo = new System.Windows.Forms.TabPage();
            this.tabUnsupported = new System.Windows.Forms.TabPage();
            this.lblUnsupported = new System.Windows.Forms.Label();
            this.tabGhostTuner = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chnNtiSubnumber = new System.Windows.Forms.NumericUpDown();
            this.chnNtiServiceType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chnNtiNumber = new System.Windows.Forms.NumericUpDown();
            this.chnNtiCallsign = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnNtiAddChannel = new System.Windows.Forms.Button();
            this.lvDevices = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rtbChannelAddHistory = new System.Windows.Forms.RichTextBox();
            this.tabTunerSpace.SuspendLayout();
            this.tabChannelTuningInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chnTiPhysicalNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chnTiSubnumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chnTiNumber)).BeginInit();
            this.tabUnsupported.SuspendLayout();
            this.tabGhostTuner.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chnNtiSubnumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chnNtiNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbScannedLineups
            // 
            this.cmbScannedLineups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScannedLineups.FormattingEnabled = true;
            this.cmbScannedLineups.Location = new System.Drawing.Point(12, 12);
            this.cmbScannedLineups.Name = "cmbScannedLineups";
            this.cmbScannedLineups.Size = new System.Drawing.Size(350, 21);
            this.cmbScannedLineups.Sorted = true;
            this.cmbScannedLineups.TabIndex = 0;
            this.cmbScannedLineups.SelectedIndexChanged += new System.EventHandler(this.cmbScannedLineups_SelectedIndexChanged);
            // 
            // tabTunerSpace
            // 
            this.tabTunerSpace.Controls.Add(this.tabChannelTuningInfo);
            this.tabTunerSpace.Controls.Add(this.tabDvbTuningInfo);
            this.tabTunerSpace.Controls.Add(this.tabUnsupported);
            this.tabTunerSpace.Controls.Add(this.tabGhostTuner);
            this.tabTunerSpace.Location = new System.Drawing.Point(12, 134);
            this.tabTunerSpace.Name = "tabTunerSpace";
            this.tabTunerSpace.SelectedIndex = 0;
            this.tabTunerSpace.Size = new System.Drawing.Size(350, 206);
            this.tabTunerSpace.TabIndex = 2;
            // 
            // tabChannelTuningInfo
            // 
            this.tabChannelTuningInfo.Controls.Add(this.chnTiModulationType);
            this.tabChannelTuningInfo.Controls.Add(this.lblChnTiModulationType);
            this.tabChannelTuningInfo.Controls.Add(this.lblChnTiSubnumber);
            this.tabChannelTuningInfo.Controls.Add(this.chnTiPhysicalNumber);
            this.tabChannelTuningInfo.Controls.Add(this.lblChnTiPhysicalNumber);
            this.tabChannelTuningInfo.Controls.Add(this.label6);
            this.tabChannelTuningInfo.Controls.Add(this.chnTiSubnumber);
            this.tabChannelTuningInfo.Controls.Add(this.chnTiNumber);
            this.tabChannelTuningInfo.Controls.Add(this.chnTiCallsign);
            this.tabChannelTuningInfo.Controls.Add(this.label1);
            this.tabChannelTuningInfo.Controls.Add(this.btnAddChannelTuningInfo);
            this.tabChannelTuningInfo.Location = new System.Drawing.Point(4, 22);
            this.tabChannelTuningInfo.Name = "tabChannelTuningInfo";
            this.tabChannelTuningInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabChannelTuningInfo.Size = new System.Drawing.Size(342, 180);
            this.tabChannelTuningInfo.TabIndex = 0;
            this.tabChannelTuningInfo.Text = "ChannelTuningInfo";
            this.tabChannelTuningInfo.UseVisualStyleBackColor = true;
            // 
            // chnTiModulationType
            // 
            this.chnTiModulationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chnTiModulationType.FormattingEnabled = true;
            this.chnTiModulationType.Items.AddRange(new object[] {
            "QAM 16",
            "QAM 32",
            "QAM 64",
            "QAM 80",
            "QAM 96",
            "QAM 112",
            "QAM 128",
            "QAM 160",
            "QAM 192",
            "QAM 224",
            "QAM 256",
            "QAM 320",
            "QAM 384",
            "QAM 448",
            "QAM 512",
            "QAM 640",
            "QAM 768",
            "QAM 896",
            "QAM 1024"});
            this.chnTiModulationType.Location = new System.Drawing.Point(171, 46);
            this.chnTiModulationType.Name = "chnTiModulationType";
            this.chnTiModulationType.Size = new System.Drawing.Size(123, 21);
            this.chnTiModulationType.TabIndex = 24;
            // 
            // lblChnTiModulationType
            // 
            this.lblChnTiModulationType.AutoSize = true;
            this.lblChnTiModulationType.Location = new System.Drawing.Point(76, 49);
            this.lblChnTiModulationType.Name = "lblChnTiModulationType";
            this.lblChnTiModulationType.Size = new System.Drawing.Size(89, 13);
            this.lblChnTiModulationType.TabIndex = 23;
            this.lblChnTiModulationType.Text = "Modulation Type:";
            this.lblChnTiModulationType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblChnTiSubnumber
            // 
            this.lblChnTiSubnumber.AutoSize = true;
            this.lblChnTiSubnumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChnTiSubnumber.Location = new System.Drawing.Point(227, 22);
            this.lblChnTiSubnumber.Name = "lblChnTiSubnumber";
            this.lblChnTiSubnumber.Size = new System.Drawing.Size(11, 13);
            this.lblChnTiSubnumber.TabIndex = 12;
            this.lblChnTiSubnumber.Text = "-";
            // 
            // chnTiPhysicalNumber
            // 
            this.chnTiPhysicalNumber.Location = new System.Drawing.Point(171, 73);
            this.chnTiPhysicalNumber.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.chnTiPhysicalNumber.Name = "chnTiPhysicalNumber";
            this.chnTiPhysicalNumber.Size = new System.Drawing.Size(50, 20);
            this.chnTiPhysicalNumber.TabIndex = 11;
            this.chnTiPhysicalNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblChnTiPhysicalNumber
            // 
            this.lblChnTiPhysicalNumber.AutoSize = true;
            this.lblChnTiPhysicalNumber.Location = new System.Drawing.Point(74, 75);
            this.lblChnTiPhysicalNumber.Name = "lblChnTiPhysicalNumber";
            this.lblChnTiPhysicalNumber.Size = new System.Drawing.Size(91, 13);
            this.lblChnTiPhysicalNumber.TabIndex = 10;
            this.lblChnTiPhysicalNumber.Text = "Physical Channel:";
            this.lblChnTiPhysicalNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(171, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Channel:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chnTiSubnumber
            // 
            this.chnTiSubnumber.Location = new System.Drawing.Point(244, 20);
            this.chnTiSubnumber.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.chnTiSubnumber.Name = "chnTiSubnumber";
            this.chnTiSubnumber.Size = new System.Drawing.Size(50, 20);
            this.chnTiSubnumber.TabIndex = 8;
            this.chnTiSubnumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chnTiNumber
            // 
            this.chnTiNumber.Location = new System.Drawing.Point(171, 20);
            this.chnTiNumber.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.chnTiNumber.Name = "chnTiNumber";
            this.chnTiNumber.Size = new System.Drawing.Size(50, 20);
            this.chnTiNumber.TabIndex = 7;
            this.chnTiNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chnTiCallsign
            // 
            this.chnTiCallsign.Location = new System.Drawing.Point(6, 19);
            this.chnTiCallsign.Name = "chnTiCallsign";
            this.chnTiCallsign.Size = new System.Drawing.Size(110, 20);
            this.chnTiCallsign.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Call Sign:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddChannelTuningInfo
            // 
            this.btnAddChannelTuningInfo.Location = new System.Drawing.Point(236, 151);
            this.btnAddChannelTuningInfo.Name = "btnAddChannelTuningInfo";
            this.btnAddChannelTuningInfo.Size = new System.Drawing.Size(100, 23);
            this.btnAddChannelTuningInfo.TabIndex = 4;
            this.btnAddChannelTuningInfo.Text = "Add Channel";
            this.btnAddChannelTuningInfo.UseVisualStyleBackColor = true;
            this.btnAddChannelTuningInfo.Click += new System.EventHandler(this.btnAddChannel_Click);
            // 
            // tabDvbTuningInfo
            // 
            this.tabDvbTuningInfo.Location = new System.Drawing.Point(4, 22);
            this.tabDvbTuningInfo.Name = "tabDvbTuningInfo";
            this.tabDvbTuningInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabDvbTuningInfo.Size = new System.Drawing.Size(342, 180);
            this.tabDvbTuningInfo.TabIndex = 3;
            this.tabDvbTuningInfo.Text = "DvbTuningInfo";
            this.tabDvbTuningInfo.UseVisualStyleBackColor = true;
            // 
            // tabUnsupported
            // 
            this.tabUnsupported.Controls.Add(this.lblUnsupported);
            this.tabUnsupported.Location = new System.Drawing.Point(4, 22);
            this.tabUnsupported.Name = "tabUnsupported";
            this.tabUnsupported.Padding = new System.Windows.Forms.Padding(3);
            this.tabUnsupported.Size = new System.Drawing.Size(342, 180);
            this.tabUnsupported.TabIndex = 2;
            this.tabUnsupported.Text = "Unsupported";
            this.tabUnsupported.UseVisualStyleBackColor = true;
            // 
            // lblUnsupported
            // 
            this.lblUnsupported.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUnsupported.Location = new System.Drawing.Point(3, 3);
            this.lblUnsupported.Name = "lblUnsupported";
            this.lblUnsupported.Size = new System.Drawing.Size(336, 174);
            this.lblUnsupported.TabIndex = 0;
            this.lblUnsupported.Text = "lblUnsupported";
            this.lblUnsupported.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabGhostTuner
            // 
            this.tabGhostTuner.Controls.Add(this.label7);
            this.tabGhostTuner.Controls.Add(this.label5);
            this.tabGhostTuner.Controls.Add(this.chnNtiSubnumber);
            this.tabGhostTuner.Controls.Add(this.chnNtiServiceType);
            this.tabGhostTuner.Controls.Add(this.label2);
            this.tabGhostTuner.Controls.Add(this.label3);
            this.tabGhostTuner.Controls.Add(this.chnNtiNumber);
            this.tabGhostTuner.Controls.Add(this.chnNtiCallsign);
            this.tabGhostTuner.Controls.Add(this.label4);
            this.tabGhostTuner.Controls.Add(this.btnNtiAddChannel);
            this.tabGhostTuner.Location = new System.Drawing.Point(4, 22);
            this.tabGhostTuner.Name = "tabGhostTuner";
            this.tabGhostTuner.Padding = new System.Windows.Forms.Padding(3);
            this.tabGhostTuner.Size = new System.Drawing.Size(342, 180);
            this.tabGhostTuner.TabIndex = 4;
            this.tabGhostTuner.Text = "NonTuner";
            this.tabGhostTuner.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(6, 70);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(330, 78);
            this.label7.TabIndex = 34;
            this.label7.Text = "\r\n*Only Service Type \'TV\' will be visible in the EPG123 Client Guide Tool. All ot" +
    "hers will still be visible in WMC.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(227, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 13);
            this.label5.TabIndex = 33;
            this.label5.Text = "-";
            // 
            // chnNtiSubnumber
            // 
            this.chnNtiSubnumber.Location = new System.Drawing.Point(244, 20);
            this.chnNtiSubnumber.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.chnNtiSubnumber.Name = "chnNtiSubnumber";
            this.chnNtiSubnumber.Size = new System.Drawing.Size(50, 20);
            this.chnNtiSubnumber.TabIndex = 32;
            this.chnNtiSubnumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chnNtiServiceType
            // 
            this.chnNtiServiceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chnNtiServiceType.FormattingEnabled = true;
            this.chnNtiServiceType.Items.AddRange(new object[] {
            "Unknown",
            "TV",
            "Audio",
            "Interactive TV",
            "ISDB Bookmark",
            "ISDB Engineering"});
            this.chnNtiServiceType.Location = new System.Drawing.Point(171, 46);
            this.chnNtiServiceType.Name = "chnNtiServiceType";
            this.chnNtiServiceType.Size = new System.Drawing.Size(123, 21);
            this.chnNtiServiceType.TabIndex = 31;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(88, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Service Type*:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(171, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Channel:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chnNtiNumber
            // 
            this.chnNtiNumber.Location = new System.Drawing.Point(171, 20);
            this.chnNtiNumber.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.chnNtiNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.chnNtiNumber.Name = "chnNtiNumber";
            this.chnNtiNumber.Size = new System.Drawing.Size(50, 20);
            this.chnNtiNumber.TabIndex = 28;
            this.chnNtiNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chnNtiNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // chnNtiCallsign
            // 
            this.chnNtiCallsign.Location = new System.Drawing.Point(6, 19);
            this.chnNtiCallsign.Name = "chnNtiCallsign";
            this.chnNtiCallsign.Size = new System.Drawing.Size(110, 20);
            this.chnNtiCallsign.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Call Sign:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnNtiAddChannel
            // 
            this.btnNtiAddChannel.Location = new System.Drawing.Point(236, 151);
            this.btnNtiAddChannel.Name = "btnNtiAddChannel";
            this.btnNtiAddChannel.Size = new System.Drawing.Size(100, 23);
            this.btnNtiAddChannel.TabIndex = 25;
            this.btnNtiAddChannel.Text = "Add Channel";
            this.btnNtiAddChannel.UseVisualStyleBackColor = true;
            this.btnNtiAddChannel.Click += new System.EventHandler(this.btnAddChannel_Click);
            // 
            // lvDevices
            // 
            this.lvDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvDevices.FullRowSelect = true;
            this.lvDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvDevices.HideSelection = false;
            this.lvDevices.Location = new System.Drawing.Point(12, 37);
            this.lvDevices.Name = "lvDevices";
            this.lvDevices.Size = new System.Drawing.Size(350, 91);
            this.lvDevices.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvDevices.TabIndex = 3;
            this.lvDevices.UseCompatibleStateImageBehavior = false;
            this.lvDevices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Devices";
            this.columnHeader1.Width = 320;
            // 
            // rtbChannelAddHistory
            // 
            this.rtbChannelAddHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbChannelAddHistory.Cursor = System.Windows.Forms.Cursors.No;
            this.rtbChannelAddHistory.Location = new System.Drawing.Point(368, 12);
            this.rtbChannelAddHistory.Name = "rtbChannelAddHistory";
            this.rtbChannelAddHistory.Size = new System.Drawing.Size(284, 328);
            this.rtbChannelAddHistory.TabIndex = 4;
            this.rtbChannelAddHistory.Text = "";
            // 
            // frmAddChannel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 352);
            this.Controls.Add(this.rtbChannelAddHistory);
            this.Controls.Add(this.lvDevices);
            this.Controls.Add(this.tabTunerSpace);
            this.Controls.Add(this.cmbScannedLineups);
            this.Name = "frmAddChannel";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Channel to Lineup";
            this.tabTunerSpace.ResumeLayout(false);
            this.tabChannelTuningInfo.ResumeLayout(false);
            this.tabChannelTuningInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chnTiPhysicalNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chnTiSubnumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chnTiNumber)).EndInit();
            this.tabUnsupported.ResumeLayout(false);
            this.tabGhostTuner.ResumeLayout(false);
            this.tabGhostTuner.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chnNtiSubnumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chnNtiNumber)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbScannedLineups;
        private System.Windows.Forms.TabControl tabTunerSpace;
        private System.Windows.Forms.TabPage tabChannelTuningInfo;
        private System.Windows.Forms.ListView lvDevices;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button btnAddChannelTuningInfo;
        private System.Windows.Forms.TabPage tabUnsupported;
        private System.Windows.Forms.TextBox chnTiCallsign;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown chnTiSubnumber;
        private System.Windows.Forms.NumericUpDown chnTiNumber;
        private System.Windows.Forms.NumericUpDown chnTiPhysicalNumber;
        private System.Windows.Forms.Label lblChnTiPhysicalNumber;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblChnTiSubnumber;
        private System.Windows.Forms.ComboBox chnTiModulationType;
        private System.Windows.Forms.Label lblChnTiModulationType;
        private System.Windows.Forms.Label lblUnsupported;
        private System.Windows.Forms.TabPage tabDvbTuningInfo;
        private System.Windows.Forms.TabPage tabGhostTuner;
        private System.Windows.Forms.ComboBox chnNtiServiceType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown chnNtiNumber;
        private System.Windows.Forms.TextBox chnNtiCallsign;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnNtiAddChannel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown chnNtiSubnumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RichTextBox rtbChannelAddHistory;
    }
}