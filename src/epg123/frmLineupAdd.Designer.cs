namespace epg123
{
    partial class frmLineupAdd
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLineupAdd));
            this.cmbCountries = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtZipcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnFetch = new System.Windows.Forms.Button();
            this.lblExample = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // cmbCountries
            // 
            this.cmbCountries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCountries.Location = new System.Drawing.Point(85, 12);
            this.cmbCountries.Name = "cmbCountries";
            this.cmbCountries.Size = new System.Drawing.Size(160, 21);
            this.cmbCountries.TabIndex = 0;
            this.cmbCountries.SelectedIndexChanged += new System.EventHandler(this.cmbCountries_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Country:";
            // 
            // txtZipcode
            // 
            this.txtZipcode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtZipcode.Location = new System.Drawing.Point(85, 39);
            this.txtZipcode.Name = "txtZipcode";
            this.txtZipcode.Size = new System.Drawing.Size(160, 20);
            this.txtZipcode.TabIndex = 0;
            this.txtZipcode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtZipcode_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Postal Code:";
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(15, 89);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(357, 160);
            this.listBox1.TabIndex = 4;
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            this.listBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseMove);
            this.listBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseUp);
            // 
            // btnFetch
            // 
            this.btnFetch.Location = new System.Drawing.Point(251, 37);
            this.btnFetch.Name = "btnFetch";
            this.btnFetch.Size = new System.Drawing.Size(75, 23);
            this.btnFetch.TabIndex = 5;
            this.btnFetch.Text = "Fetch";
            this.btnFetch.UseVisualStyleBackColor = true;
            this.btnFetch.Click += new System.EventHandler(this.btnFetch_Click);
            // 
            // lblExample
            // 
            this.lblExample.AutoSize = true;
            this.lblExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExample.Location = new System.Drawing.Point(29, 64);
            this.lblExample.Name = "lblExample";
            this.lblExample.Size = new System.Drawing.Size(50, 13);
            this.lblExample.TabIndex = 6;
            this.lblExample.Text = "Example:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 259);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(311, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "*Right-click to preview channels.     **Double-click lineup to add.";
            // 
            // frmLineupAdd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 281);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblExample);
            this.Controls.Add(this.btnFetch);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtZipcode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbCountries);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 320);
            this.Name = "frmLineupAdd";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lineup Search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbCountries;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtZipcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btnFetch;
        private System.Windows.Forms.Label lblExample;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}