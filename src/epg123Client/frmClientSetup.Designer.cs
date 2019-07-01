namespace epg123
{
    partial class frmClientSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmClientSetup));
            this.btnCleanStart = new System.Windows.Forms.Button();
            this.btnTvSetup = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.lblCleanStart = new System.Windows.Forms.Label();
            this.lblTvSetup = new System.Windows.Forms.Label();
            this.lblConfig = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.cbAutostep = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCleanStart
            // 
            this.btnCleanStart.Location = new System.Drawing.Point(12, 12);
            this.btnCleanStart.Name = "btnCleanStart";
            this.btnCleanStart.Size = new System.Drawing.Size(100, 40);
            this.btnCleanStart.TabIndex = 1;
            this.btnCleanStart.Text = "Step 1:\r\nClean Start";
            this.btnCleanStart.UseVisualStyleBackColor = true;
            this.btnCleanStart.Click += new System.EventHandler(this.button_Click);
            // 
            // btnTvSetup
            // 
            this.btnTvSetup.Enabled = false;
            this.btnTvSetup.Location = new System.Drawing.Point(12, 58);
            this.btnTvSetup.Name = "btnTvSetup";
            this.btnTvSetup.Size = new System.Drawing.Size(100, 40);
            this.btnTvSetup.TabIndex = 3;
            this.btnTvSetup.Text = "Step 2:\r\nTV Setup";
            this.btnTvSetup.UseVisualStyleBackColor = true;
            this.btnTvSetup.Click += new System.EventHandler(this.button_Click);
            // 
            // btnConfig
            // 
            this.btnConfig.Enabled = false;
            this.btnConfig.Location = new System.Drawing.Point(12, 104);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(100, 40);
            this.btnConfig.TabIndex = 5;
            this.btnConfig.Text = "Step 3:\r\nConfigure";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.button_Click);
            // 
            // lblCleanStart
            // 
            this.lblCleanStart.Location = new System.Drawing.Point(118, 12);
            this.lblCleanStart.Name = "lblCleanStart";
            this.lblCleanStart.Size = new System.Drawing.Size(154, 40);
            this.lblCleanStart.TabIndex = 5;
            this.lblCleanStart.Text = "Delete eHome folder contents and prepare new database";
            this.lblCleanStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTvSetup
            // 
            this.lblTvSetup.Enabled = false;
            this.lblTvSetup.Location = new System.Drawing.Point(118, 58);
            this.lblTvSetup.Name = "lblTvSetup";
            this.lblTvSetup.Size = new System.Drawing.Size(154, 40);
            this.lblTvSetup.TabIndex = 7;
            this.lblTvSetup.Text = "Perform WMC TV Setup";
            this.lblTvSetup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblConfig
            // 
            this.lblConfig.Enabled = false;
            this.lblConfig.Location = new System.Drawing.Point(118, 104);
            this.lblConfig.Name = "lblConfig";
            this.lblConfig.Size = new System.Drawing.Size(154, 40);
            this.lblConfig.TabIndex = 9;
            this.lblConfig.Text = "Configure EPG123";
            this.lblConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 167);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(284, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // cbAutostep
            // 
            this.cbAutostep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAutostep.AutoSize = true;
            this.cbAutostep.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbAutostep.Checked = true;
            this.cbAutostep.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutostep.Location = new System.Drawing.Point(84, 147);
            this.cbAutostep.Name = "cbAutostep";
            this.cbAutostep.Size = new System.Drawing.Size(188, 17);
            this.cbAutostep.TabIndex = 11;
            this.cbAutostep.Text = "Automatically proceed to next step";
            this.cbAutostep.UseVisualStyleBackColor = true;
            // 
            // frmClientSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 189);
            this.Controls.Add(this.cbAutostep);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lblConfig);
            this.Controls.Add(this.lblTvSetup);
            this.Controls.Add(this.lblCleanStart);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.btnTvSetup);
            this.Controls.Add(this.btnCleanStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 228);
            this.Name = "frmClientSetup";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Client Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmClientSetup_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCleanStart;
        private System.Windows.Forms.Button btnTvSetup;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.Label lblCleanStart;
        private System.Windows.Forms.Label lblTvSetup;
        private System.Windows.Forms.Label lblConfig;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.CheckBox cbAutostep;
    }
}