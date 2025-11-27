
namespace epg123Client
{
    partial class frmSatellites
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSatellites));
            this.btnCreateDefault = new System.Windows.Forms.Button();
            this.cbRadio = new System.Windows.Forms.CheckBox();
            this.cbEncrypted = new System.Windows.Forms.CheckBox();
            this.cbEnabled = new System.Windows.Forms.CheckBox();
            this.btnTransponders = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbData = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCreateDefault
            // 
            this.btnCreateDefault.Location = new System.Drawing.Point(402, 95);
            this.btnCreateDefault.Name = "btnCreateDefault";
            this.btnCreateDefault.Size = new System.Drawing.Size(75, 23);
            this.btnCreateDefault.TabIndex = 0;
            this.btnCreateDefault.Text = "Create";
            this.btnCreateDefault.UseVisualStyleBackColor = true;
            this.btnCreateDefault.Click += new System.EventHandler(this.btnCreateDefault_Click);
            // 
            // cbRadio
            // 
            this.cbRadio.AutoSize = true;
            this.cbRadio.Checked = true;
            this.cbRadio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRadio.Location = new System.Drawing.Point(57, 73);
            this.cbRadio.Name = "cbRadio";
            this.cbRadio.Size = new System.Drawing.Size(54, 17);
            this.cbRadio.TabIndex = 1;
            this.cbRadio.Text = "Radio";
            this.cbRadio.UseVisualStyleBackColor = true;
            // 
            // cbEncrypted
            // 
            this.cbEncrypted.AutoSize = true;
            this.cbEncrypted.Location = new System.Drawing.Point(248, 73);
            this.cbEncrypted.Name = "cbEncrypted";
            this.cbEncrypted.Size = new System.Drawing.Size(118, 17);
            this.cbEncrypted.TabIndex = 2;
            this.cbEncrypted.Text = "Encrypted/Blocked";
            this.cbEncrypted.UseVisualStyleBackColor = true;
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.Location = new System.Drawing.Point(57, 96);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(136, 17);
            this.cbEnabled.TabIndex = 3;
            this.cbEnabled.Text = "Enabled Channels Only";
            this.cbEnabled.UseVisualStyleBackColor = true;
            // 
            // btnTransponders
            // 
            this.btnTransponders.Location = new System.Drawing.Point(402, 58);
            this.btnTransponders.Name = "btnTransponders";
            this.btnTransponders.Size = new System.Drawing.Size(75, 23);
            this.btnTransponders.TabIndex = 4;
            this.btnTransponders.Text = "Update";
            this.btnTransponders.UseVisualStyleBackColor = true;
            this.btnTransponders.Click += new System.EventHandler(this.btnTransponders_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(102, 25);
            this.linkLabel1.Location = new System.Drawing.Point(6, 85);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(471, 44);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = resources.GetString("linkLabel1.Text");
            this.linkLabel1.UseCompatibleTextRendering = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(471, 33);
            this.label1.TabIndex = 6;
            this.label1.Text = "Update satellites and transponders from 21 November 2025. A Full Satellite Scan w" +
    "ill be required to add channels to the guide after TV Setup is complete.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnTransponders);
            this.groupBox1.Controls.Add(this.linkLabel1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(483, 132);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Update Satellites && Transponders";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cbData);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btnCreateDefault);
            this.groupBox2.Controls.Add(this.cbRadio);
            this.groupBox2.Controls.Add(this.cbEnabled);
            this.groupBox2.Controls.Add(this.cbEncrypted);
            this.groupBox2.Location = new System.Drawing.Point(12, 150);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(483, 124);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Create Custom DefaultSatellites MXF File";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Include:";
            // 
            // cbData
            // 
            this.cbData.AutoSize = true;
            this.cbData.Location = new System.Drawing.Point(117, 73);
            this.cbData.Name = "cbData";
            this.cbData.Size = new System.Drawing.Size(125, 17);
            this.cbData.TabIndex = 5;
            this.cbData.Text = "Interactive TV (Data)";
            this.cbData.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(471, 48);
            this.label2.TabIndex = 4;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnImport);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(12, 280);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(483, 98);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Import Prior to TV Setup";
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(402, 68);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 1;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(471, 43);
            this.label3.TabIndex = 0;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // frmSatellites
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 390);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSatellites";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Satellites";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCreateDefault;
        private System.Windows.Forms.CheckBox cbRadio;
        private System.Windows.Forms.CheckBox cbEncrypted;
        private System.Windows.Forms.CheckBox cbEnabled;
        private System.Windows.Forms.Button btnTransponders;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbData;
    }
}