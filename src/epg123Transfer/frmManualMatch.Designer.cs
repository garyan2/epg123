namespace epg123Transfer
{
    partial class frmManualMatch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManualMatch));
            this.grpRovi = new System.Windows.Forms.GroupBox();
            this.tbRoviDescription = new System.Windows.Forms.TextBox();
            this.txtRoviTitle = new System.Windows.Forms.TextBox();
            this.grpGracenote = new System.Windows.Forms.GroupBox();
            this.tbGracenoteDescription = new System.Windows.Forms.TextBox();
            this.txtGracenoteTitle = new System.Windows.Forms.TextBox();
            this.picGracenote = new System.Windows.Forms.PictureBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpRovi.SuspendLayout();
            this.grpGracenote.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picGracenote)).BeginInit();
            this.SuspendLayout();
            // 
            // grpRovi
            // 
            this.grpRovi.Controls.Add(this.tbRoviDescription);
            this.grpRovi.Controls.Add(this.txtRoviTitle);
            this.grpRovi.Location = new System.Drawing.Point(12, 12);
            this.grpRovi.Name = "grpRovi";
            this.grpRovi.Size = new System.Drawing.Size(200, 300);
            this.grpRovi.TabIndex = 0;
            this.grpRovi.TabStop = false;
            this.grpRovi.Text = "MS/Rovi or SD/Gracenote";
            // 
            // tbRoviDescription
            // 
            this.tbRoviDescription.BackColor = System.Drawing.SystemColors.Control;
            this.tbRoviDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRoviDescription.Location = new System.Drawing.Point(6, 172);
            this.tbRoviDescription.Multiline = true;
            this.tbRoviDescription.Name = "tbRoviDescription";
            this.tbRoviDescription.ReadOnly = true;
            this.tbRoviDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbRoviDescription.Size = new System.Drawing.Size(188, 122);
            this.tbRoviDescription.TabIndex = 5;
            this.tbRoviDescription.TabStop = false;
            // 
            // txtRoviTitle
            // 
            this.txtRoviTitle.BackColor = System.Drawing.SystemColors.Control;
            this.txtRoviTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRoviTitle.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtRoviTitle.Location = new System.Drawing.Point(6, 19);
            this.txtRoviTitle.Name = "txtRoviTitle";
            this.txtRoviTitle.ReadOnly = true;
            this.txtRoviTitle.Size = new System.Drawing.Size(188, 20);
            this.txtRoviTitle.TabIndex = 3;
            this.txtRoviTitle.TabStop = false;
            // 
            // grpGracenote
            // 
            this.grpGracenote.Controls.Add(this.tbGracenoteDescription);
            this.grpGracenote.Controls.Add(this.txtGracenoteTitle);
            this.grpGracenote.Controls.Add(this.picGracenote);
            this.grpGracenote.Location = new System.Drawing.Point(218, 12);
            this.grpGracenote.Name = "grpGracenote";
            this.grpGracenote.Size = new System.Drawing.Size(200, 300);
            this.grpGracenote.TabIndex = 2;
            this.grpGracenote.TabStop = false;
            this.grpGracenote.Text = "Schedules Direct";
            // 
            // tbGracenoteDescription
            // 
            this.tbGracenoteDescription.BackColor = System.Drawing.SystemColors.Control;
            this.tbGracenoteDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbGracenoteDescription.Location = new System.Drawing.Point(6, 172);
            this.tbGracenoteDescription.Multiline = true;
            this.tbGracenoteDescription.Name = "tbGracenoteDescription";
            this.tbGracenoteDescription.ReadOnly = true;
            this.tbGracenoteDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbGracenoteDescription.Size = new System.Drawing.Size(188, 122);
            this.tbGracenoteDescription.TabIndex = 3;
            this.tbGracenoteDescription.TabStop = false;
            // 
            // txtGracenoteTitle
            // 
            this.txtGracenoteTitle.BackColor = System.Drawing.SystemColors.Control;
            this.txtGracenoteTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGracenoteTitle.Location = new System.Drawing.Point(6, 20);
            this.txtGracenoteTitle.Name = "txtGracenoteTitle";
            this.txtGracenoteTitle.ReadOnly = true;
            this.txtGracenoteTitle.Size = new System.Drawing.Size(188, 20);
            this.txtGracenoteTitle.TabIndex = 2;
            this.txtGracenoteTitle.TabStop = false;
            // 
            // picGracenote
            // 
            this.picGracenote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picGracenote.Location = new System.Drawing.Point(6, 46);
            this.picGracenote.Name = "picGracenote";
            this.picGracenote.Size = new System.Drawing.Size(188, 120);
            this.picGracenote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picGracenote.TabIndex = 1;
            this.picGracenote.TabStop = false;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(343, 318);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmManualMatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 354);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.grpGracenote);
            this.Controls.Add(this.grpRovi);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmManualMatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manual Match & Convert";
            this.Shown += new System.EventHandler(this.frmManualMatch_Shown);
            this.grpRovi.ResumeLayout(false);
            this.grpRovi.PerformLayout();
            this.grpGracenote.ResumeLayout(false);
            this.grpGracenote.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picGracenote)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpRovi;
        private System.Windows.Forms.GroupBox grpGracenote;
        private System.Windows.Forms.TextBox txtRoviTitle;
        private System.Windows.Forms.TextBox txtGracenoteTitle;
        private System.Windows.Forms.PictureBox picGracenote;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbGracenoteDescription;
        private System.Windows.Forms.TextBox tbRoviDescription;
    }
}