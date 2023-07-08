namespace epg123Client
{
    partial class frmStorage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStorage));
            this.numWarning = new System.Windows.Forms.NumericUpDown();
            this.numError = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numError)).BeginInit();
            this.SuspendLayout();
            // 
            // numWarning
            // 
            this.numWarning.Location = new System.Drawing.Point(131, 16);
            this.numWarning.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numWarning.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numWarning.Name = "numWarning";
            this.numWarning.Size = new System.Drawing.Size(53, 20);
            this.numWarning.TabIndex = 0;
            this.numWarning.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numWarning.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numWarning.ValueChanged += new System.EventHandler(this.numWarning_ValueChanged);
            // 
            // numError
            // 
            this.numError.Location = new System.Drawing.Point(131, 42);
            this.numError.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numError.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numError.Name = "numError";
            this.numError.Size = new System.Drawing.Size(53, 20);
            this.numError.TabIndex = 1;
            this.numError.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numError.ValueChanged += new System.EventHandler(this.numError_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "WARNING Threshold:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "ERROR Threshold:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(190, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "GB remaining";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(190, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "GB remaining";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(495, 276);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(558, 154);
            this.label5.TabIndex = 10;
            this.label5.Text = resources.GetString("label5.Text");
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(72, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(165, 52);
            this.label6.TabIndex = 11;
            this.label6.Text = ":=  No threshold; do not report\r\n:=  Use WMC predictive results\r\n:=  Override thr" +
    "eshold in GB";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 46);
            this.label7.TabIndex = 12;
            this.label7.Text = "-1\r\n0\r\n1 - 1024";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmStorage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 311);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numError);
            this.Controls.Add(this.numWarning);
            this.Name = "frmStorage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Storage Notifications";
            ((System.ComponentModel.ISupportInitialize)(this.numWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numError)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numWarning;
        private System.Windows.Forms.NumericUpDown numError;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}