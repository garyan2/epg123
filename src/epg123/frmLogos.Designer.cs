namespace epg123
{
    partial class frmLogos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogos));
            this.pbCustomLocal = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuDeleteCustom = new System.Windows.Forms.ToolStripMenuItem();
            this.pbDarkLocal = new System.Windows.Forms.PictureBox();
            this.pbWhiteLocal = new System.Windows.Forms.PictureBox();
            this.pbLightLocal = new System.Windows.Forms.PictureBox();
            this.pbGrayLocal = new System.Windows.Forms.PictureBox();
            this.pbDefaultLocal = new System.Windows.Forms.PictureBox();
            this.pbDarkRemote = new System.Windows.Forms.PictureBox();
            this.pbWhiteRemote = new System.Windows.Forms.PictureBox();
            this.pbLightRemote = new System.Windows.Forms.PictureBox();
            this.pbGrayRemote = new System.Windows.Forms.PictureBox();
            this.pbDefaultRemote = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pbCustomLocal)).BeginInit();
            this.contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDarkLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWhiteLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLightLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrayLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDarkRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWhiteRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLightRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrayRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultRemote)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCustomLocal
            // 
            this.pbCustomLocal.BackColor = System.Drawing.SystemColors.Control;
            this.pbCustomLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbCustomLocal.ContextMenuStrip = this.contextMenuStrip2;
            this.pbCustomLocal.Location = new System.Drawing.Point(187, 12);
            this.pbCustomLocal.Name = "pbCustomLocal";
            this.pbCustomLocal.Size = new System.Drawing.Size(120, 90);
            this.pbCustomLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbCustomLocal.TabIndex = 0;
            this.pbCustomLocal.TabStop = false;
            this.pbCustomLocal.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragDrop);
            this.pbCustomLocal.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragEnter);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuDeleteCustom});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(181, 48);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // menuDeleteCustom
            // 
            this.menuDeleteCustom.Name = "menuDeleteCustom";
            this.menuDeleteCustom.Size = new System.Drawing.Size(180, 22);
            this.menuDeleteCustom.Text = "Delete local file...";
            this.menuDeleteCustom.Click += new System.EventHandler(this.menuDeleteLocal_Click);
            // 
            // pbDarkLocal
            // 
            this.pbDarkLocal.BackColor = System.Drawing.SystemColors.Control;
            this.pbDarkLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbDarkLocal.ContextMenuStrip = this.contextMenuStrip2;
            this.pbDarkLocal.Location = new System.Drawing.Point(187, 108);
            this.pbDarkLocal.Name = "pbDarkLocal";
            this.pbDarkLocal.Size = new System.Drawing.Size(120, 90);
            this.pbDarkLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDarkLocal.TabIndex = 1;
            this.pbDarkLocal.TabStop = false;
            this.pbDarkLocal.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragDrop);
            this.pbDarkLocal.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragEnter);
            this.pbDarkLocal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbWhiteLocal
            // 
            this.pbWhiteLocal.BackColor = System.Drawing.SystemColors.Control;
            this.pbWhiteLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbWhiteLocal.ContextMenuStrip = this.contextMenuStrip2;
            this.pbWhiteLocal.Location = new System.Drawing.Point(187, 204);
            this.pbWhiteLocal.Name = "pbWhiteLocal";
            this.pbWhiteLocal.Size = new System.Drawing.Size(120, 90);
            this.pbWhiteLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbWhiteLocal.TabIndex = 2;
            this.pbWhiteLocal.TabStop = false;
            this.pbWhiteLocal.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragDrop);
            this.pbWhiteLocal.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragEnter);
            this.pbWhiteLocal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbLightLocal
            // 
            this.pbLightLocal.BackColor = System.Drawing.SystemColors.Control;
            this.pbLightLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbLightLocal.ContextMenuStrip = this.contextMenuStrip2;
            this.pbLightLocal.Location = new System.Drawing.Point(187, 300);
            this.pbLightLocal.Name = "pbLightLocal";
            this.pbLightLocal.Size = new System.Drawing.Size(120, 90);
            this.pbLightLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLightLocal.TabIndex = 3;
            this.pbLightLocal.TabStop = false;
            this.pbLightLocal.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragDrop);
            this.pbLightLocal.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragEnter);
            this.pbLightLocal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbGrayLocal
            // 
            this.pbGrayLocal.BackColor = System.Drawing.SystemColors.Control;
            this.pbGrayLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbGrayLocal.ContextMenuStrip = this.contextMenuStrip2;
            this.pbGrayLocal.Location = new System.Drawing.Point(187, 396);
            this.pbGrayLocal.Name = "pbGrayLocal";
            this.pbGrayLocal.Size = new System.Drawing.Size(120, 90);
            this.pbGrayLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbGrayLocal.TabIndex = 4;
            this.pbGrayLocal.TabStop = false;
            this.pbGrayLocal.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragDrop);
            this.pbGrayLocal.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragEnter);
            this.pbGrayLocal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbDefaultLocal
            // 
            this.pbDefaultLocal.BackColor = System.Drawing.SystemColors.Control;
            this.pbDefaultLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbDefaultLocal.ContextMenuStrip = this.contextMenuStrip2;
            this.pbDefaultLocal.Location = new System.Drawing.Point(187, 492);
            this.pbDefaultLocal.Name = "pbDefaultLocal";
            this.pbDefaultLocal.Size = new System.Drawing.Size(120, 90);
            this.pbDefaultLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDefaultLocal.TabIndex = 5;
            this.pbDefaultLocal.TabStop = false;
            this.pbDefaultLocal.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragDrop);
            this.pbDefaultLocal.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBox_DragEnter);
            this.pbDefaultLocal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbDarkRemote
            // 
            this.pbDarkRemote.BackColor = System.Drawing.SystemColors.Control;
            this.pbDarkRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbDarkRemote.Location = new System.Drawing.Point(61, 108);
            this.pbDarkRemote.Name = "pbDarkRemote";
            this.pbDarkRemote.Size = new System.Drawing.Size(120, 90);
            this.pbDarkRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDarkRemote.TabIndex = 6;
            this.pbDarkRemote.TabStop = false;
            this.pbDarkRemote.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbWhiteRemote
            // 
            this.pbWhiteRemote.BackColor = System.Drawing.SystemColors.Control;
            this.pbWhiteRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbWhiteRemote.Location = new System.Drawing.Point(61, 204);
            this.pbWhiteRemote.Name = "pbWhiteRemote";
            this.pbWhiteRemote.Size = new System.Drawing.Size(120, 90);
            this.pbWhiteRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbWhiteRemote.TabIndex = 7;
            this.pbWhiteRemote.TabStop = false;
            this.pbWhiteRemote.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbLightRemote
            // 
            this.pbLightRemote.BackColor = System.Drawing.SystemColors.Control;
            this.pbLightRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbLightRemote.Location = new System.Drawing.Point(61, 300);
            this.pbLightRemote.Name = "pbLightRemote";
            this.pbLightRemote.Size = new System.Drawing.Size(120, 90);
            this.pbLightRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLightRemote.TabIndex = 8;
            this.pbLightRemote.TabStop = false;
            this.pbLightRemote.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbGrayRemote
            // 
            this.pbGrayRemote.BackColor = System.Drawing.SystemColors.Control;
            this.pbGrayRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbGrayRemote.Location = new System.Drawing.Point(61, 396);
            this.pbGrayRemote.Name = "pbGrayRemote";
            this.pbGrayRemote.Size = new System.Drawing.Size(120, 90);
            this.pbGrayRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbGrayRemote.TabIndex = 9;
            this.pbGrayRemote.TabStop = false;
            this.pbGrayRemote.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // pbDefaultRemote
            // 
            this.pbDefaultRemote.BackColor = System.Drawing.SystemColors.Control;
            this.pbDefaultRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbDefaultRemote.Location = new System.Drawing.Point(61, 492);
            this.pbDefaultRemote.Name = "pbDefaultRemote";
            this.pbDefaultRemote.Size = new System.Drawing.Size(120, 90);
            this.pbDefaultRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDefaultRemote.TabIndex = 10;
            this.pbDefaultRemote.TabStop = false;
            this.pbDefaultRemote.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDragSource_MouseDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(133, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Custom";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(21, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Dark";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(15, 204);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "White";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(20, 300);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Light";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(22, 396);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Gray";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(7, 492);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Default";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(10, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(171, 68);
            this.label7.TabIndex = 18;
            this.label7.Text = "label7";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = resources.GetString("openFileDialog1.Filter");
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // frmLogos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(326, 597);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbDefaultRemote);
            this.Controls.Add(this.pbGrayRemote);
            this.Controls.Add(this.pbLightRemote);
            this.Controls.Add(this.pbWhiteRemote);
            this.Controls.Add(this.pbDarkRemote);
            this.Controls.Add(this.pbDefaultLocal);
            this.Controls.Add(this.pbGrayLocal);
            this.Controls.Add(this.pbLightLocal);
            this.Controls.Add(this.pbWhiteLocal);
            this.Controls.Add(this.pbDarkLocal);
            this.Controls.Add(this.pbCustomLocal);
            this.Name = "frmLogos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Station Logos";
            this.Load += new System.EventHandler(this.frmLogos_Load);
            this.Shown += new System.EventHandler(this.frmLogos_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pbCustomLocal)).EndInit();
            this.contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbDarkLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWhiteLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLightLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrayLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDarkRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWhiteRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLightRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrayRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultRemote)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbCustomLocal;
        private System.Windows.Forms.PictureBox pbDarkLocal;
        private System.Windows.Forms.PictureBox pbWhiteLocal;
        private System.Windows.Forms.PictureBox pbLightLocal;
        private System.Windows.Forms.PictureBox pbGrayLocal;
        private System.Windows.Forms.PictureBox pbDefaultLocal;
        private System.Windows.Forms.PictureBox pbDarkRemote;
        private System.Windows.Forms.PictureBox pbWhiteRemote;
        private System.Windows.Forms.PictureBox pbLightRemote;
        private System.Windows.Forms.PictureBox pbGrayRemote;
        private System.Windows.Forms.PictureBox pbDefaultRemote;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem menuDeleteCustom;
    }
}