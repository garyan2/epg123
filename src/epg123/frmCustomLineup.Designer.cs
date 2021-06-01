
namespace epg123
{
    partial class frmCustomLineup
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
            this.lvAvailable = new System.Windows.Forms.ListView();
            this.cCallSign = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cStationId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvCustom = new System.Windows.Forms.ListView();
            this.ccCallSign = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ccNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ccStationId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ccName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ccMatch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cbCustom = new System.Windows.Forms.ComboBox();
            this.btnAddLineup = new System.Windows.Forms.Button();
            this.btnRemoveLineup = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvAvailable
            // 
            this.lvAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvAvailable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cCallSign,
            this.cStationId,
            this.cName});
            this.lvAvailable.FullRowSelect = true;
            this.lvAvailable.HideSelection = false;
            this.lvAvailable.Location = new System.Drawing.Point(18, 41);
            this.lvAvailable.Name = "lvAvailable";
            this.lvAvailable.Size = new System.Drawing.Size(339, 508);
            this.lvAvailable.TabIndex = 0;
            this.lvAvailable.UseCompatibleStateImageBehavior = false;
            this.lvAvailable.View = System.Windows.Forms.View.Details;
            this.lvAvailable.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvLineupSort);
            // 
            // cCallSign
            // 
            this.cCallSign.Text = "CallSign";
            // 
            // cStationId
            // 
            this.cStationId.Text = "StationID";
            // 
            // cName
            // 
            this.cName.Text = "Name";
            this.cName.Width = 175;
            // 
            // lvCustom
            // 
            this.lvCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCustom.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ccCallSign,
            this.ccNumber,
            this.ccStationId,
            this.ccName,
            this.ccMatch});
            this.lvCustom.FullRowSelect = true;
            this.lvCustom.HideSelection = false;
            this.lvCustom.Location = new System.Drawing.Point(3, 41);
            this.lvCustom.Name = "lvCustom";
            this.lvCustom.Size = new System.Drawing.Size(555, 508);
            this.lvCustom.TabIndex = 1;
            this.lvCustom.UseCompatibleStateImageBehavior = false;
            this.lvCustom.View = System.Windows.Forms.View.Details;
            this.lvCustom.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvLineupSort);
            // 
            // ccCallSign
            // 
            this.ccCallSign.Text = "CallSign";
            // 
            // ccNumber
            // 
            this.ccNumber.Text = "Number";
            // 
            // ccStationId
            // 
            this.ccStationId.Text = "StationID";
            // 
            // ccName
            // 
            this.ccName.Text = "Name";
            // 
            // ccMatch
            // 
            this.ccMatch.Text = "MatchName";
            // 
            // cbCustom
            // 
            this.cbCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCustom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCustom.FormattingEnabled = true;
            this.cbCustom.Location = new System.Drawing.Point(3, 12);
            this.cbCustom.Name = "cbCustom";
            this.cbCustom.Size = new System.Drawing.Size(497, 21);
            this.cbCustom.TabIndex = 2;
            this.cbCustom.SelectedIndexChanged += new System.EventHandler(this.cbCustom_SelectedIndexChanged);
            // 
            // btnAddLineup
            // 
            this.btnAddLineup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddLineup.Location = new System.Drawing.Point(506, 11);
            this.btnAddLineup.Name = "btnAddLineup";
            this.btnAddLineup.Size = new System.Drawing.Size(23, 23);
            this.btnAddLineup.TabIndex = 3;
            this.btnAddLineup.Text = "button1";
            this.btnAddLineup.UseVisualStyleBackColor = true;
            // 
            // btnRemoveLineup
            // 
            this.btnRemoveLineup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveLineup.Location = new System.Drawing.Point(535, 11);
            this.btnRemoveLineup.Name = "btnRemoveLineup";
            this.btnRemoveLineup.Size = new System.Drawing.Size(23, 23);
            this.btnRemoveLineup.TabIndex = 4;
            this.btnRemoveLineup.Text = "button2";
            this.btnRemoveLineup.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Available Stations from Subscribed Lineups";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lvAvailable);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1MinSize = 360;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvCustom);
            this.splitContainer1.Panel2.Controls.Add(this.btnRemoveLineup);
            this.splitContainer1.Panel2.Controls.Add(this.cbCustom);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddLineup);
            this.splitContainer1.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
            this.splitContainer1.Size = new System.Drawing.Size(934, 561);
            this.splitContainer1.SplitterDistance = 360;
            this.splitContainer1.TabIndex = 6;
            // 
            // frmCustomLineup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(934, 561);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(480, 0);
            this.Name = "frmCustomLineup";
            this.Text = "frmCustomLineup";
            this.Shown += new System.EventHandler(this.frmCustomLineup_Shown);
            this.ResizeEnd += new System.EventHandler(this.frmCustomLineup_ResizeEnd);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvAvailable;
        private System.Windows.Forms.ColumnHeader cCallSign;
        private System.Windows.Forms.ColumnHeader cStationId;
        private System.Windows.Forms.ColumnHeader cName;
        private System.Windows.Forms.ListView lvCustom;
        private System.Windows.Forms.ComboBox cbCustom;
        private System.Windows.Forms.Button btnAddLineup;
        private System.Windows.Forms.Button btnRemoveLineup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader ccCallSign;
        private System.Windows.Forms.ColumnHeader ccNumber;
        private System.Windows.Forms.ColumnHeader ccStationId;
        private System.Windows.Forms.ColumnHeader ccName;
        private System.Windows.Forms.ColumnHeader ccMatch;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}