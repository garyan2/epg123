namespace epg123
{
    partial class frmProgress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProgress));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.progressBarStage = new System.Windows.Forms.ProgressBar();
            this.progressBarTask = new System.Windows.Forms.ProgressBar();
            this.lblTaskTitle = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTaskProgress = new System.Windows.Forms.Label();
            this.lblStageProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // progressBarStage
            // 
            this.progressBarStage.Location = new System.Drawing.Point(12, 40);
            this.progressBarStage.Maximum = 1300;
            this.progressBarStage.Name = "progressBarStage";
            this.progressBarStage.Size = new System.Drawing.Size(310, 23);
            this.progressBarStage.Step = 1;
            this.progressBarStage.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarStage.TabIndex = 0;
            // 
            // progressBarTask
            // 
            this.progressBarTask.Location = new System.Drawing.Point(12, 101);
            this.progressBarTask.Name = "progressBarTask";
            this.progressBarTask.Size = new System.Drawing.Size(310, 23);
            this.progressBarTask.Step = 1;
            this.progressBarTask.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarTask.TabIndex = 1;
            // 
            // lblTaskTitle
            // 
            this.lblTaskTitle.Location = new System.Drawing.Point(12, 20);
            this.lblTaskTitle.Name = "lblTaskTitle";
            this.lblTaskTitle.Size = new System.Drawing.Size(310, 17);
            this.lblTaskTitle.TabIndex = 2;
            this.lblTaskTitle.Text = "Stage";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Progress";
            // 
            // lblTaskProgress
            // 
            this.lblTaskProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTaskProgress.Location = new System.Drawing.Point(222, 127);
            this.lblTaskProgress.Name = "lblTaskProgress";
            this.lblTaskProgress.Size = new System.Drawing.Size(100, 23);
            this.lblTaskProgress.TabIndex = 5;
            this.lblTaskProgress.Text = "0/0";
            this.lblTaskProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblStageProgress
            // 
            this.lblStageProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStageProgress.Location = new System.Drawing.Point(222, 66);
            this.lblStageProgress.Name = "lblStageProgress";
            this.lblStageProgress.Size = new System.Drawing.Size(100, 23);
            this.lblStageProgress.TabIndex = 6;
            this.lblStageProgress.Text = "0/0";
            this.lblStageProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 167);
            this.Controls.Add(this.lblStageProgress);
            this.Controls.Add(this.lblTaskProgress);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblTaskTitle);
            this.Controls.Add(this.progressBarTask);
            this.Controls.Add(this.progressBarStage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmProgress";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EPG123 Update Progress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmProgress_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ProgressBar progressBarStage;
        private System.Windows.Forms.ProgressBar progressBarTask;
        private System.Windows.Forms.Label lblTaskTitle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTaskProgress;
        private System.Windows.Forms.Label lblStageProgress;
    }
}