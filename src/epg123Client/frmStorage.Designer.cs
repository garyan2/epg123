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
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.numConflictWarning = new System.Windows.Forms.NumericUpDown();
            this.numConflictError = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConflictWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConflictError)).BeginInit();
            this.SuspendLayout();
            // 
            // numWarning
            // 
            this.numWarning.Location = new System.Drawing.Point(131, 38);
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
            this.numError.Location = new System.Drawing.Point(131, 64);
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
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "WARNING Threshold:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "ERROR Threshold:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(190, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "GB remaining";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(190, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "GB remaining";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(495, 303);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(558, 154);
            this.label5.TabIndex = 10;
            this.label5.Text = resources.GetString("label5.Text");
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(72, 87);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(165, 52);
            this.label6.TabIndex = 11;
            this.label6.Text = ":=  No threshold; do not report\r\n:=  Use WMC predictive results\r\n:=  Override thr" +
    "eshold in GB";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 46);
            this.label7.TabIndex = 12;
            this.label7.Text = "-1\r\n0\r\n1 - 1024";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(492, 66);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "days";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(492, 40);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "days";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(348, 66);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "ERROR if within:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(334, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(101, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "WARNING if within:";
            // 
            // numConflictWarning
            // 
            this.numConflictWarning.Location = new System.Drawing.Point(441, 36);
            this.numConflictWarning.Maximum = new decimal(new int[] {
            17,
            0,
            0,
            0});
            this.numConflictWarning.Name = "numConflictWarning";
            this.numConflictWarning.Size = new System.Drawing.Size(45, 20);
            this.numConflictWarning.TabIndex = 14;
            this.numConflictWarning.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numConflictWarning.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numConflictWarning.ValueChanged += new System.EventHandler(this.numConflictWarning_ValueChanged);
            // 
            // numConflictError
            // 
            this.numConflictError.Location = new System.Drawing.Point(441, 62);
            this.numConflictError.Maximum = new decimal(new int[] {
            17,
            0,
            0,
            0});
            this.numConflictError.Name = "numConflictError";
            this.numConflictError.Size = new System.Drawing.Size(45, 20);
            this.numConflictError.TabIndex = 13;
            this.numConflictError.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numConflictError.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numConflictError.ValueChanged += new System.EventHandler(this.numConflictError_ValueChanged);
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(12, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(248, 23);
            this.label12.TabIndex = 19;
            this.label12.Text = "Recorder Storage";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(322, 9);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(248, 23);
            this.label13.TabIndex = 20;
            this.label13.Text = "Tuner Conflicts";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(334, 87);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(54, 46);
            this.label14.TabIndex = 22;
            this.label14.Text = "0\r\n1 - 17";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(394, 87);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(165, 52);
            this.label15.TabIndex = 21;
            this.label15.Text = ":=  No threshold; do not report\r\n:=  Maximum days to report";
            // 
            // frmStorage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 338);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.numConflictWarning);
            this.Controls.Add(this.numConflictError);
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
            this.Text = "Storage and Tuner Conflict Notifications";
            ((System.ComponentModel.ISupportInitialize)(this.numWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConflictWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConflictError)).EndInit();
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
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numConflictWarning;
        private System.Windows.Forms.NumericUpDown numConflictError;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
    }
}