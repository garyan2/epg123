namespace epg123Transfer
{
    partial class frmTransfer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTransfer));
            this.lvWmcRecordings = new System.Windows.Forms.ListView();
            this.clmType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.lvMxfRecordings = new System.Windows.Forms.ListView();
            this.clmOldType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmOldDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.matchVerifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddRecordings = new System.Windows.Forms.Button();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOpenBackup = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvWmcRecordings
            // 
            this.lvWmcRecordings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvWmcRecordings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmType,
            this.clmDescription});
            this.lvWmcRecordings.ContextMenuStrip = this.contextMenuStrip1;
            this.lvWmcRecordings.FullRowSelect = true;
            this.lvWmcRecordings.GridLines = true;
            this.lvWmcRecordings.HideSelection = false;
            this.lvWmcRecordings.Location = new System.Drawing.Point(426, 41);
            this.lvWmcRecordings.Name = "lvWmcRecordings";
            this.lvWmcRecordings.Size = new System.Drawing.Size(346, 479);
            this.lvWmcRecordings.TabIndex = 0;
            this.lvWmcRecordings.UseCompatibleStateImageBehavior = false;
            this.lvWmcRecordings.View = System.Windows.Forms.View.Details;
            this.lvWmcRecordings.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            // 
            // clmType
            // 
            this.clmType.Text = "Type";
            // 
            // clmDescription
            // 
            this.clmDescription.Text = "Title / Keyword(s)";
            this.clmDescription.Width = 250;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCancel});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(111, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItemCancel
            // 
            this.toolStripMenuItemCancel.Name = "toolStripMenuItemCancel";
            this.toolStripMenuItemCancel.Size = new System.Drawing.Size(110, 22);
            this.toolStripMenuItemCancel.Text = "Cancel";
            this.toolStripMenuItemCancel.Click += new System.EventHandler(this.toolStripMenuItemCancel_Click);
            // 
            // lvMxfRecordings
            // 
            this.lvMxfRecordings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvMxfRecordings.CheckBoxes = true;
            this.lvMxfRecordings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmOldType,
            this.clmOldDescription});
            this.lvMxfRecordings.ContextMenuStrip = this.contextMenuStrip2;
            this.lvMxfRecordings.FullRowSelect = true;
            this.lvMxfRecordings.GridLines = true;
            this.lvMxfRecordings.HideSelection = false;
            this.lvMxfRecordings.Location = new System.Drawing.Point(12, 41);
            this.lvMxfRecordings.Name = "lvMxfRecordings";
            this.lvMxfRecordings.Size = new System.Drawing.Size(339, 479);
            this.lvMxfRecordings.TabIndex = 1;
            this.lvMxfRecordings.UseCompatibleStateImageBehavior = false;
            this.lvMxfRecordings.View = System.Windows.Forms.View.Details;
            this.lvMxfRecordings.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLineupSort);
            this.lvMxfRecordings.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.oldRecordingListView_ItemCheck);
            // 
            // clmOldType
            // 
            this.clmOldType.Text = "Type";
            // 
            // clmOldDescription
            // 
            this.clmOldDescription.Text = "Title / Keyword(s)";
            this.clmOldDescription.Width = 250;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.matchVerifyToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(154, 26);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // matchVerifyToolStripMenuItem
            // 
            this.matchVerifyToolStripMenuItem.Name = "matchVerifyToolStripMenuItem";
            this.matchVerifyToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.matchVerifyToolStripMenuItem.Text = "Match && Verify";
            this.matchVerifyToolStripMenuItem.Click += new System.EventHandler(this.matchVerifyToolStripMenuItem_Click);
            // 
            // btnAddRecordings
            // 
            this.btnAddRecordings.Location = new System.Drawing.Point(357, 231);
            this.btnAddRecordings.Name = "btnAddRecordings";
            this.btnAddRecordings.Size = new System.Drawing.Size(63, 23);
            this.btnAddRecordings.TabIndex = 2;
            this.btnAddRecordings.Text = "> > >";
            this.btnAddRecordings.UseVisualStyleBackColor = true;
            this.btnAddRecordings.Click += new System.EventHandler(this.btnTransfer_Click);
            // 
            // lblDateTime
            // 
            this.lblDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDateTime.AutoSize = true;
            this.lblDateTime.Location = new System.Drawing.Point(12, 523);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(141, 13);
            this.lblDateTime.TabIndex = 3;
            this.lblDateTime.Text = "Database Last Updated on: ";
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExit.Location = new System.Drawing.Point(697, 526);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(423, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Current WMC Recording Requests";
            // 
            // btnOpenBackup
            // 
            this.btnOpenBackup.Location = new System.Drawing.Point(12, 12);
            this.btnOpenBackup.Name = "btnOpenBackup";
            this.btnOpenBackup.Size = new System.Drawing.Size(339, 23);
            this.btnOpenBackup.TabIndex = 6;
            this.btnOpenBackup.Text = "Open recordings backup file";
            this.btnOpenBackup.UseVisualStyleBackColor = true;
            this.btnOpenBackup.Click += new System.EventHandler(this.btnOpenBackup_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.AddExtension = false;
            this.openFileDialog1.Filter = "All files|*.*";
            this.openFileDialog1.Title = "Open Backup Recordings File";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 539);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(292, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "* Right-click items to manually match or verify series requests";
            // 
            // frmTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOpenBackup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.lblDateTime);
            this.Controls.Add(this.btnAddRecordings);
            this.Controls.Add(this.lvMxfRecordings);
            this.Controls.Add(this.lvWmcRecordings);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(800, 10000);
            this.Name = "frmTransfer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EPG123 Series Recordings Transfer Tool";
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvWmcRecordings;
        private System.Windows.Forms.ColumnHeader clmType;
        private System.Windows.Forms.ColumnHeader clmDescription;
        private System.Windows.Forms.ListView lvMxfRecordings;
        private System.Windows.Forms.ColumnHeader clmOldType;
        private System.Windows.Forms.ColumnHeader clmOldDescription;
        private System.Windows.Forms.Button btnAddRecordings;
        private System.Windows.Forms.Label lblDateTime;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOpenBackup;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem matchVerifyToolStripMenuItem;
        private System.Windows.Forms.Label label2;
    }
}

