namespace epg123
{
    partial class frmXmltvConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmXmltvConfig));
            this.ckChannelNumbers = new System.Windows.Forms.CheckBox();
            this.ckChannelLogos = new System.Windows.Forms.CheckBox();
            this.ckUrlLogos = new System.Windows.Forms.CheckBox();
            this.ckLocalLogos = new System.Windows.Forms.CheckBox();
            this.ckSubstitutePath = new System.Windows.Forms.CheckBox();
            this.txtSubstitutePath = new System.Windows.Forms.TextBox();
            this.ckXmltvFillerData = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ckChannelNumbers
            // 
            this.ckChannelNumbers.AutoSize = true;
            this.ckChannelNumbers.Location = new System.Drawing.Point(12, 12);
            this.ckChannelNumbers.Name = "ckChannelNumbers";
            this.ckChannelNumbers.Size = new System.Drawing.Size(145, 17);
            this.ckChannelNumbers.TabIndex = 0;
            this.ckChannelNumbers.Text = "Include channel numbers";
            this.ckChannelNumbers.UseVisualStyleBackColor = true;
            this.ckChannelNumbers.CheckedChanged += new System.EventHandler(this.ckLogos_CheckedChanged);
            // 
            // ckChannelLogos
            // 
            this.ckChannelLogos.AutoSize = true;
            this.ckChannelLogos.Location = new System.Drawing.Point(12, 35);
            this.ckChannelLogos.Name = "ckChannelLogos";
            this.ckChannelLogos.Size = new System.Drawing.Size(130, 17);
            this.ckChannelLogos.TabIndex = 1;
            this.ckChannelLogos.Text = "Include channel logos";
            this.ckChannelLogos.UseVisualStyleBackColor = true;
            this.ckChannelLogos.CheckedChanged += new System.EventHandler(this.ckLogos_CheckedChanged);
            // 
            // ckUrlLogos
            // 
            this.ckUrlLogos.AutoSize = true;
            this.ckUrlLogos.Location = new System.Drawing.Point(29, 58);
            this.ckUrlLogos.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.ckUrlLogos.Name = "ckUrlLogos";
            this.ckUrlLogos.Size = new System.Drawing.Size(219, 17);
            this.ckUrlLogos.TabIndex = 2;
            this.ckUrlLogos.Text = "Use linked images from Schedules Direct";
            this.ckUrlLogos.UseVisualStyleBackColor = true;
            this.ckUrlLogos.CheckedChanged += new System.EventHandler(this.ckLogos_CheckedChanged);
            // 
            // ckLocalLogos
            // 
            this.ckLocalLogos.AutoSize = true;
            this.ckLocalLogos.Location = new System.Drawing.Point(29, 81);
            this.ckLocalLogos.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.ckLocalLogos.Name = "ckLocalLogos";
            this.ckLocalLogos.Size = new System.Drawing.Size(194, 17);
            this.ckLocalLogos.TabIndex = 3;
            this.ckLocalLogos.Text = "Use local images from .\\logos folder";
            this.ckLocalLogos.UseVisualStyleBackColor = true;
            this.ckLocalLogos.CheckedChanged += new System.EventHandler(this.ckLogos_CheckedChanged);
            // 
            // ckSubstitutePath
            // 
            this.ckSubstitutePath.AutoSize = true;
            this.ckSubstitutePath.Location = new System.Drawing.Point(46, 104);
            this.ckSubstitutePath.Margin = new System.Windows.Forms.Padding(37, 3, 3, 3);
            this.ckSubstitutePath.Name = "ckSubstitutePath";
            this.ckSubstitutePath.Size = new System.Drawing.Size(191, 17);
            this.ckSubstitutePath.TabIndex = 4;
            this.ckSubstitutePath.Text = "Substitute path to logos folder with:";
            this.ckSubstitutePath.UseVisualStyleBackColor = true;
            this.ckSubstitutePath.CheckedChanged += new System.EventHandler(this.ckLogos_CheckedChanged);
            // 
            // txtSubstitutePath
            // 
            this.txtSubstitutePath.Location = new System.Drawing.Point(46, 127);
            this.txtSubstitutePath.Name = "txtSubstitutePath";
            this.txtSubstitutePath.Size = new System.Drawing.Size(300, 20);
            this.txtSubstitutePath.TabIndex = 5;
            // 
            // ckXmltvFillerData
            // 
            this.ckXmltvFillerData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ckXmltvFillerData.AutoSize = true;
            this.ckXmltvFillerData.Location = new System.Drawing.Point(12, 153);
            this.ckXmltvFillerData.Name = "ckXmltvFillerData";
            this.ckXmltvFillerData.Size = new System.Drawing.Size(294, 17);
            this.ckXmltvFillerData.TabIndex = 6;
            this.ckXmltvFillerData.Text = "Create filler programs for stations that have no guide data";
            this.ckXmltvFillerData.UseVisualStyleBackColor = true;
            // 
            // frmXmltvConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 182);
            this.Controls.Add(this.ckXmltvFillerData);
            this.Controls.Add(this.txtSubstitutePath);
            this.Controls.Add(this.ckSubstitutePath);
            this.Controls.Add(this.ckLocalLogos);
            this.Controls.Add(this.ckUrlLogos);
            this.Controls.Add(this.ckChannelLogos);
            this.Controls.Add(this.ckChannelNumbers);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmXmltvConfig";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XMLTV Configuration";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmXmltvConfig_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ckChannelNumbers;
        private System.Windows.Forms.CheckBox ckChannelLogos;
        private System.Windows.Forms.CheckBox ckUrlLogos;
        private System.Windows.Forms.CheckBox ckLocalLogos;
        private System.Windows.Forms.CheckBox ckSubstitutePath;
        private System.Windows.Forms.TextBox txtSubstitutePath;
        private System.Windows.Forms.CheckBox ckXmltvFillerData;
    }
}