using epg123;

namespace epg123Client
{
    partial class clientForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(clientForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.lvEditTextBox = new System.Windows.Forms.TextBox();
            this.mergedChannelListView = new System.Windows.Forms.ListView();
            this.columnCallSign = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnServiceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSubscribedLineup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnScannedSources = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnTuningInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnGuideEndTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.subscribeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unsubscribeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.renameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renumberMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.clipboardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearListingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.lblMergedChannelListView = new System.Windows.Forms.ToolStripLabel();
            this.btnLablesDisplay = new System.Windows.Forms.ToolStripButton();
            this.mergedChannelToolStrip = new System.Windows.Forms.ToolStrip();
            this.lblMatchBy = new System.Windows.Forms.ToolStripLabel();
            this.btnAutoNumber = new System.Windows.Forms.ToolStripButton();
            this.btnAutoCallsign = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmbSources = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnChannelDisplay = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDeleteChannel = new System.Windows.Forms.ToolStripButton();
            this.btnAddChannels = new System.Windows.Forms.ToolStripButton();
            this.btnUndelete = new System.Windows.Forms.ToolStripButton();
            this.btnStoreExplorer = new System.Windows.Forms.ToolStripButton();
            this.btnExportMxf = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer2 = new System.Windows.Forms.ToolStripContainer();
            this.lineupChannelListView = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.lineupChannelToolStrip = new System.Windows.Forms.ToolStrip();
            this.lblLineupCombobox = new System.Windows.Forms.ToolStripLabel();
            this.cmbObjectStoreLineups = new System.Windows.Forms.ToolStripComboBox();
            this.btnRefreshLineups = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDeleteLineup = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lvItemsProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.lblToolStripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.grpClientConfig = new System.Windows.Forms.GroupBox();
            this.btnViewLog = new System.Windows.Forms.Button();
            this.btnTransferTool = new System.Windows.Forms.Button();
            this.lblDatabaseUtilities = new System.Windows.Forms.Label();
            this.btnTweakWmc = new System.Windows.Forms.Button();
            this.btnBackup = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.grpScheduledTask = new System.Windows.Forms.GroupBox();
            this.cbAutomatch = new System.Windows.Forms.CheckBox();
            this.rdoClientMode = new System.Windows.Forms.RadioButton();
            this.tbTaskInfo = new System.Windows.Forms.TextBox();
            this.cbTaskWake = new System.Windows.Forms.CheckBox();
            this.rdoFullMode = new System.Windows.Forms.RadioButton();
            this.lblSchedStatus = new System.Windows.Forms.Label();
            this.tbSchedTime = new System.Windows.Forms.MaskedTextBox();
            this.lblUpdateTime = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ebtnRestore = new epg123.ElevatedButton();
            this.ebtnRebuild = new epg123.ElevatedButton();
            this.ebtnSetup = new epg123.ElevatedButton();
            this.btnTask = new epg123.ElevatedButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.mergedChannelToolStrip.SuspendLayout();
            this.toolStripContainer2.ContentPanel.SuspendLayout();
            this.toolStripContainer2.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.lineupChannelToolStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.grpClientConfig.SuspendLayout();
            this.grpScheduledTask.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Enabled = false;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolStripContainer1);
            this.splitContainer1.Panel1MinSize = 415;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.toolStripContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(784, 428);
            this.splitContainer1.SplitterDistance = 415;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.lvEditTextBox);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.mergedChannelListView);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(415, 378);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(415, 428);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mergedChannelToolStrip);
            // 
            // lvEditTextBox
            // 
            this.lvEditTextBox.Location = new System.Drawing.Point(309, 3);
            this.lvEditTextBox.Name = "lvEditTextBox";
            this.lvEditTextBox.Size = new System.Drawing.Size(100, 20);
            this.lvEditTextBox.TabIndex = 1;
            this.lvEditTextBox.Visible = false;
            this.lvEditTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lvEditTextBox_KeyPress);
            // 
            // mergedChannelListView
            // 
            this.mergedChannelListView.CheckBoxes = true;
            this.mergedChannelListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnCallSign,
            this.columnNumber,
            this.columnServiceName,
            this.columnSubscribedLineup,
            this.columnScannedSources,
            this.columnTuningInfo,
            this.columnGuideEndTime});
            this.mergedChannelListView.ContextMenuStrip = this.contextMenuStrip1;
            this.mergedChannelListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mergedChannelListView.FullRowSelect = true;
            this.mergedChannelListView.HideSelection = false;
            this.mergedChannelListView.Location = new System.Drawing.Point(0, 0);
            this.mergedChannelListView.Name = "mergedChannelListView";
            this.mergedChannelListView.OwnerDraw = true;
            this.mergedChannelListView.Size = new System.Drawing.Size(415, 378);
            this.mergedChannelListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.mergedChannelListView.TabIndex = 0;
            this.mergedChannelListView.UseCompatibleStateImageBehavior = false;
            this.mergedChannelListView.View = System.Windows.Forms.View.Details;
            this.mergedChannelListView.VirtualMode = true;
            this.mergedChannelListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.mergedChannelListView_AfterLabelEdit);
            this.mergedChannelListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvLineupSort);
            this.mergedChannelListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.mergedChannelListView_DrawColumnHeader);
            this.mergedChannelListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.mergedChannelListView_DrawItem);
            this.mergedChannelListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.mergedChannelListView_DrawSubItem);
            this.mergedChannelListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.mergedChannelListView_RetrieveVirtualItem);
            this.mergedChannelListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mergedChannelListView_KeyDown);
            this.mergedChannelListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mergedChannelListView_MouseClick);
            this.mergedChannelListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mergedChannelListView_MouseDoubleClick);
            // 
            // columnCallSign
            // 
            this.columnCallSign.Text = "Call Sign";
            // 
            // columnNumber
            // 
            this.columnNumber.Text = "Number";
            // 
            // columnServiceName
            // 
            this.columnServiceName.Text = "Service Name";
            this.columnServiceName.Width = 100;
            // 
            // columnSubscribedLineup
            // 
            this.columnSubscribedLineup.Text = "Subscribed Lineup";
            this.columnSubscribedLineup.Width = 100;
            // 
            // columnScannedSources
            // 
            this.columnScannedSources.Text = "Scanned Source(s)";
            // 
            // columnTuningInfo
            // 
            this.columnTuningInfo.Text = "TuningInfo";
            // 
            // columnGuideEndTime
            // 
            this.columnGuideEndTime.Text = "Guide End Time";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.subscribeMenuItem,
            this.unsubscribeMenuItem,
            this.toolStripSeparator2,
            this.renameMenuItem,
            this.renumberMenuItem,
            this.toolStripSeparator6,
            this.clipboardMenuItem,
            this.clearListingsMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(184, 148);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // subscribeMenuItem
            // 
            this.subscribeMenuItem.Name = "subscribeMenuItem";
            this.subscribeMenuItem.Size = new System.Drawing.Size(183, 22);
            this.subscribeMenuItem.Text = "Subscribe";
            this.subscribeMenuItem.Click += new System.EventHandler(this.subscribeMenuItem_Click);
            // 
            // unsubscribeMenuItem
            // 
            this.unsubscribeMenuItem.Name = "unsubscribeMenuItem";
            this.unsubscribeMenuItem.Size = new System.Drawing.Size(183, 22);
            this.unsubscribeMenuItem.Text = "Unsubscribe";
            this.unsubscribeMenuItem.Click += new System.EventHandler(this.subscribeMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(180, 6);
            // 
            // renameMenuItem
            // 
            this.renameMenuItem.Name = "renameMenuItem";
            this.renameMenuItem.Size = new System.Drawing.Size(183, 22);
            this.renameMenuItem.Text = "Rename";
            this.renameMenuItem.Click += new System.EventHandler(this.renameMenuItem_Click);
            // 
            // renumberMenuItem
            // 
            this.renumberMenuItem.Name = "renumberMenuItem";
            this.renumberMenuItem.Size = new System.Drawing.Size(183, 22);
            this.renumberMenuItem.Text = "Renumber";
            this.renumberMenuItem.Click += new System.EventHandler(this.renumberMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(180, 6);
            // 
            // clipboardMenuItem
            // 
            this.clipboardMenuItem.Name = "clipboardMenuItem";
            this.clipboardMenuItem.Size = new System.Drawing.Size(183, 22);
            this.clipboardMenuItem.Text = "Copy to clipboard...";
            this.clipboardMenuItem.Click += new System.EventHandler(this.clipboardMenuItem_Click);
            // 
            // clearListingsMenuItem
            // 
            this.clearListingsMenuItem.Name = "clearListingsMenuItem";
            this.clearListingsMenuItem.Size = new System.Drawing.Size(183, 22);
            this.clearListingsMenuItem.Text = "Clear guide listings...";
            this.clearListingsMenuItem.Click += new System.EventHandler(this.BtnClearScheduleEntries);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMergedChannelListView,
            this.btnLablesDisplay});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(415, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 1;
            // 
            // lblMergedChannelListView
            // 
            this.lblMergedChannelListView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblMergedChannelListView.Name = "lblMergedChannelListView";
            this.lblMergedChannelListView.Size = new System.Drawing.Size(116, 22);
            this.lblMergedChannelListView.Text = "Guide Channels with";
            // 
            // btnLablesDisplay
            // 
            this.btnLablesDisplay.BackColor = System.Drawing.Color.OrangeRed;
            this.btnLablesDisplay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLablesDisplay.Image = ((System.Drawing.Image)(resources.GetObject("btnLablesDisplay.Image")));
            this.btnLablesDisplay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLablesDisplay.Name = "btnLablesDisplay";
            this.btnLablesDisplay.Size = new System.Drawing.Size(89, 22);
            this.btnLablesDisplay.Text = "Custom Labels";
            this.btnLablesDisplay.Click += new System.EventHandler(this.btnCustomDisplay_Click);
            // 
            // mergedChannelToolStrip
            // 
            this.mergedChannelToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mergedChannelToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mergedChannelToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMatchBy,
            this.btnAutoNumber,
            this.btnAutoCallsign,
            this.toolStripSeparator1,
            this.cmbSources,
            this.toolStripSeparator3,
            this.btnChannelDisplay,
            this.toolStripSeparator4,
            this.btnDeleteChannel,
            this.btnAddChannels,
            this.btnUndelete,
            this.btnStoreExplorer,
            this.btnExportMxf});
            this.mergedChannelToolStrip.Location = new System.Drawing.Point(0, 25);
            this.mergedChannelToolStrip.Name = "mergedChannelToolStrip";
            this.mergedChannelToolStrip.Size = new System.Drawing.Size(415, 25);
            this.mergedChannelToolStrip.Stretch = true;
            this.mergedChannelToolStrip.TabIndex = 0;
            // 
            // lblMatchBy
            // 
            this.lblMatchBy.Name = "lblMatchBy";
            this.lblMatchBy.Size = new System.Drawing.Size(60, 22);
            this.lblMatchBy.Text = "Match by:";
            // 
            // btnAutoNumber
            // 
            this.btnAutoNumber.Image = ((System.Drawing.Image)(resources.GetObject("btnAutoNumber.Image")));
            this.btnAutoNumber.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAutoNumber.Name = "btnAutoNumber";
            this.btnAutoNumber.Size = new System.Drawing.Size(71, 22);
            this.btnAutoNumber.Text = "Number";
            this.btnAutoNumber.ToolTipText = "Match by Channel Number";
            this.btnAutoNumber.Click += new System.EventHandler(this.btnAutoMatch_Click);
            // 
            // btnAutoCallsign
            // 
            this.btnAutoCallsign.Image = ((System.Drawing.Image)(resources.GetObject("btnAutoCallsign.Image")));
            this.btnAutoCallsign.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAutoCallsign.Name = "btnAutoCallsign";
            this.btnAutoCallsign.Size = new System.Drawing.Size(73, 22);
            this.btnAutoCallsign.Text = "Call Sign";
            this.btnAutoCallsign.ToolTipText = "Match by Channel Call Sign";
            this.btnAutoCallsign.Click += new System.EventHandler(this.btnAutoMatch_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // cmbSources
            // 
            this.cmbSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSources.DropDownWidth = 200;
            this.cmbSources.Name = "cmbSources";
            this.cmbSources.Size = new System.Drawing.Size(180, 25);
            this.cmbSources.SelectedIndexChanged += new System.EventHandler(this.cmbSources_SelectedIndexChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnChannelDisplay
            // 
            this.btnChannelDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnChannelDisplay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnChannelDisplay.Image = ((System.Drawing.Image)(resources.GetObject("btnChannelDisplay.Image")));
            this.btnChannelDisplay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnChannelDisplay.Name = "btnChannelDisplay";
            this.btnChannelDisplay.Size = new System.Drawing.Size(23, 20);
            this.btnChannelDisplay.Text = "toolStripButton1";
            this.btnChannelDisplay.ToolTipText = "Display Enabled Channels Only";
            this.btnChannelDisplay.Click += new System.EventHandler(this.btnChannelDisplay_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnDeleteChannel
            // 
            this.btnDeleteChannel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteChannel.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteChannel.Image")));
            this.btnDeleteChannel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteChannel.Name = "btnDeleteChannel";
            this.btnDeleteChannel.Size = new System.Drawing.Size(23, 20);
            this.btnDeleteChannel.Text = "Delete Channel(s)";
            this.btnDeleteChannel.Click += new System.EventHandler(this.btnDeleteChannel_Click);
            // 
            // btnAddChannels
            // 
            this.btnAddChannels.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddChannels.Image = ((System.Drawing.Image)(resources.GetObject("btnAddChannels.Image")));
            this.btnAddChannels.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddChannels.Name = "btnAddChannels";
            this.btnAddChannels.Size = new System.Drawing.Size(23, 20);
            this.btnAddChannels.Text = "toolStripButton1";
            this.btnAddChannels.ToolTipText = "Add Tuner Channel(s)";
            this.btnAddChannels.Click += new System.EventHandler(this.btnAddChannels_Click);
            // 
            // btnUndelete
            // 
            this.btnUndelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUndelete.Image = ((System.Drawing.Image)(resources.GetObject("btnUndelete.Image")));
            this.btnUndelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUndelete.Name = "btnUndelete";
            this.btnUndelete.Size = new System.Drawing.Size(23, 20);
            this.btnUndelete.Text = "Undelete";
            this.btnUndelete.ToolTipText = "Restore deleted scanned channels";
            this.btnUndelete.Click += new System.EventHandler(this.btnUndelete_Click);
            // 
            // btnStoreExplorer
            // 
            this.btnStoreExplorer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStoreExplorer.Image = ((System.Drawing.Image)(resources.GetObject("btnStoreExplorer.Image")));
            this.btnStoreExplorer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStoreExplorer.Name = "btnStoreExplorer";
            this.btnStoreExplorer.Size = new System.Drawing.Size(23, 20);
            this.btnStoreExplorer.Text = "Store Explorer";
            this.btnStoreExplorer.Click += new System.EventHandler(this.btnStoreExplorer_Click);
            // 
            // btnExportMxf
            // 
            this.btnExportMxf.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExportMxf.Image = ((System.Drawing.Image)(resources.GetObject("btnExportMxf.Image")));
            this.btnExportMxf.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportMxf.Name = "btnExportMxf";
            this.btnExportMxf.Size = new System.Drawing.Size(23, 20);
            this.btnExportMxf.Text = "Export Store to MXF";
            this.btnExportMxf.Click += new System.EventHandler(this.btnExportMxf_Click);
            // 
            // toolStripContainer2
            // 
            // 
            // toolStripContainer2.ContentPanel
            // 
            this.toolStripContainer2.ContentPanel.Controls.Add(this.lineupChannelListView);
            this.toolStripContainer2.ContentPanel.Size = new System.Drawing.Size(365, 378);
            this.toolStripContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer2.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer2.Name = "toolStripContainer2";
            this.toolStripContainer2.Size = new System.Drawing.Size(365, 428);
            this.toolStripContainer2.TabIndex = 1;
            this.toolStripContainer2.Text = "toolStripContainer2";
            // 
            // toolStripContainer2.TopToolStripPanel
            // 
            this.toolStripContainer2.TopToolStripPanel.Controls.Add(this.toolStrip2);
            this.toolStripContainer2.TopToolStripPanel.Controls.Add(this.lineupChannelToolStrip);
            // 
            // lineupChannelListView
            // 
            this.lineupChannelListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lineupChannelListView.ContextMenuStrip = this.contextMenuStrip1;
            this.lineupChannelListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineupChannelListView.FullRowSelect = true;
            this.lineupChannelListView.HideSelection = false;
            this.lineupChannelListView.Location = new System.Drawing.Point(0, 0);
            this.lineupChannelListView.MultiSelect = false;
            this.lineupChannelListView.Name = "lineupChannelListView";
            this.lineupChannelListView.Size = new System.Drawing.Size(365, 378);
            this.lineupChannelListView.TabIndex = 0;
            this.lineupChannelListView.UseCompatibleStateImageBehavior = false;
            this.lineupChannelListView.View = System.Windows.Forms.View.Details;
            this.lineupChannelListView.VirtualMode = true;
            this.lineupChannelListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvLineupSort);
            this.lineupChannelListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lineupChannelListView_RetrieveVirtualItem);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Call Sign";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Number";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Service Name";
            this.columnHeader6.Width = 100;
            // 
            // toolStrip2
            // 
            this.toolStrip2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel4});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(365, 25);
            this.toolStrip2.Stretch = true;
            this.toolStrip2.TabIndex = 1;
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(88, 22);
            this.toolStripLabel4.Text = "Lineup Services";
            // 
            // lineupChannelToolStrip
            // 
            this.lineupChannelToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.lineupChannelToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.lineupChannelToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblLineupCombobox,
            this.cmbObjectStoreLineups,
            this.btnRefreshLineups,
            this.toolStripSeparator5,
            this.btnDeleteLineup});
            this.lineupChannelToolStrip.Location = new System.Drawing.Point(0, 25);
            this.lineupChannelToolStrip.Name = "lineupChannelToolStrip";
            this.lineupChannelToolStrip.Size = new System.Drawing.Size(365, 25);
            this.lineupChannelToolStrip.Stretch = true;
            this.lineupChannelToolStrip.TabIndex = 0;
            // 
            // lblLineupCombobox
            // 
            this.lblLineupCombobox.Name = "lblLineupCombobox";
            this.lblLineupCombobox.Size = new System.Drawing.Size(46, 22);
            this.lblLineupCombobox.Text = "Lineup:";
            // 
            // cmbObjectStoreLineups
            // 
            this.cmbObjectStoreLineups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbObjectStoreLineups.DropDownWidth = 300;
            this.cmbObjectStoreLineups.MaxDropDownItems = 16;
            this.cmbObjectStoreLineups.Name = "cmbObjectStoreLineups";
            this.cmbObjectStoreLineups.Size = new System.Drawing.Size(270, 25);
            this.cmbObjectStoreLineups.Sorted = true;
            this.cmbObjectStoreLineups.SelectedIndexChanged += new System.EventHandler(this.lineupComboBox_SelectedIndexChanged);
            // 
            // btnRefreshLineups
            // 
            this.btnRefreshLineups.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefreshLineups.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshLineups.Image")));
            this.btnRefreshLineups.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefreshLineups.Name = "btnRefreshLineups";
            this.btnRefreshLineups.Size = new System.Drawing.Size(23, 22);
            this.btnRefreshLineups.Text = "Refresh Lineups";
            this.btnRefreshLineups.Click += new System.EventHandler(this.btnRefreshLineups_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // btnDeleteLineup
            // 
            this.btnDeleteLineup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteLineup.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteLineup.Image")));
            this.btnDeleteLineup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteLineup.Name = "btnDeleteLineup";
            this.btnDeleteLineup.Size = new System.Drawing.Size(23, 20);
            this.btnDeleteLineup.Text = "Delete Lineup";
            this.btnDeleteLineup.Click += new System.EventHandler(this.BtnDeleteLineupClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lvItemsProgressBar,
            this.lblToolStripStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lvItemsProgressBar
            // 
            this.lvItemsProgressBar.AutoSize = false;
            this.lvItemsProgressBar.Name = "lvItemsProgressBar";
            this.lvItemsProgressBar.Size = new System.Drawing.Size(0, 16);
            // 
            // lblToolStripStatus
            // 
            this.lblToolStripStatus.Name = "lblToolStripStatus";
            this.lblToolStripStatus.Size = new System.Drawing.Size(70, 17);
            this.lblToolStripStatus.Text = "Initializing...";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Enabled = false;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.grpClientConfig);
            this.splitContainer2.Panel1.Controls.Add(this.grpScheduledTask);
            this.splitContainer2.Panel1MinSize = 107;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer2.Panel2MinSize = 107;
            this.splitContainer2.Size = new System.Drawing.Size(784, 539);
            this.splitContainer2.SplitterDistance = 107;
            this.splitContainer2.TabIndex = 2;
            // 
            // grpClientConfig
            // 
            this.grpClientConfig.Controls.Add(this.btnViewLog);
            this.grpClientConfig.Controls.Add(this.btnTransferTool);
            this.grpClientConfig.Controls.Add(this.lblDatabaseUtilities);
            this.grpClientConfig.Controls.Add(this.btnTweakWmc);
            this.grpClientConfig.Controls.Add(this.btnBackup);
            this.grpClientConfig.Controls.Add(this.ebtnRestore);
            this.grpClientConfig.Controls.Add(this.ebtnRebuild);
            this.grpClientConfig.Controls.Add(this.ebtnSetup);
            this.grpClientConfig.Controls.Add(this.btnImport);
            this.grpClientConfig.Location = new System.Drawing.Point(421, 3);
            this.grpClientConfig.Name = "grpClientConfig";
            this.grpClientConfig.Size = new System.Drawing.Size(360, 101);
            this.grpClientConfig.TabIndex = 6;
            this.grpClientConfig.TabStop = false;
            this.grpClientConfig.Text = "Client Configuration && Actions";
            // 
            // btnViewLog
            // 
            this.btnViewLog.Location = new System.Drawing.Point(105, 44);
            this.btnViewLog.Name = "btnViewLog";
            this.btnViewLog.Size = new System.Drawing.Size(93, 23);
            this.btnViewLog.TabIndex = 24;
            this.btnViewLog.Text = "View Log";
            this.btnViewLog.UseVisualStyleBackColor = true;
            this.btnViewLog.Click += new System.EventHandler(this.btnViewLog_Click);
            // 
            // btnTransferTool
            // 
            this.btnTransferTool.Location = new System.Drawing.Point(105, 19);
            this.btnTransferTool.Name = "btnTransferTool";
            this.btnTransferTool.Size = new System.Drawing.Size(93, 23);
            this.btnTransferTool.TabIndex = 23;
            this.btnTransferTool.Text = "Transfer Tool";
            this.btnTransferTool.UseVisualStyleBackColor = true;
            this.btnTransferTool.Click += new System.EventHandler(this.btnTransferTool_Click);
            // 
            // lblDatabaseUtilities
            // 
            this.lblDatabaseUtilities.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDatabaseUtilities.Location = new System.Drawing.Point(204, 26);
            this.lblDatabaseUtilities.Name = "lblDatabaseUtilities";
            this.lblDatabaseUtilities.Size = new System.Drawing.Size(147, 15);
            this.lblDatabaseUtilities.TabIndex = 22;
            this.lblDatabaseUtilities.Text = "Database Utilities";
            this.lblDatabaseUtilities.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTweakWmc
            // 
            this.btnTweakWmc.Location = new System.Drawing.Point(6, 44);
            this.btnTweakWmc.Name = "btnTweakWmc";
            this.btnTweakWmc.Size = new System.Drawing.Size(93, 23);
            this.btnTweakWmc.TabIndex = 21;
            this.btnTweakWmc.Text = "Tweak WMC";
            this.btnTweakWmc.UseVisualStyleBackColor = true;
            this.btnTweakWmc.Click += new System.EventHandler(this.btnTweakWmc_Click);
            // 
            // btnBackup
            // 
            this.btnBackup.Location = new System.Drawing.Point(204, 44);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(72, 23);
            this.btnBackup.TabIndex = 20;
            this.btnBackup.Text = "Backup";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // btnImport
            // 
            this.btnImport.Enabled = false;
            this.btnImport.Location = new System.Drawing.Point(6, 70);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(93, 23);
            this.btnImport.TabIndex = 9;
            this.btnImport.Text = "Manual Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // grpScheduledTask
            // 
            this.grpScheduledTask.Controls.Add(this.cbAutomatch);
            this.grpScheduledTask.Controls.Add(this.rdoClientMode);
            this.grpScheduledTask.Controls.Add(this.btnTask);
            this.grpScheduledTask.Controls.Add(this.tbTaskInfo);
            this.grpScheduledTask.Controls.Add(this.cbTaskWake);
            this.grpScheduledTask.Controls.Add(this.rdoFullMode);
            this.grpScheduledTask.Controls.Add(this.lblSchedStatus);
            this.grpScheduledTask.Controls.Add(this.tbSchedTime);
            this.grpScheduledTask.Controls.Add(this.lblUpdateTime);
            this.grpScheduledTask.Location = new System.Drawing.Point(3, 3);
            this.grpScheduledTask.Name = "grpScheduledTask";
            this.grpScheduledTask.Size = new System.Drawing.Size(412, 101);
            this.grpScheduledTask.TabIndex = 5;
            this.grpScheduledTask.TabStop = false;
            this.grpScheduledTask.Text = "Scheduled Task";
            // 
            // cbAutomatch
            // 
            this.cbAutomatch.AutoSize = true;
            this.cbAutomatch.Location = new System.Drawing.Point(9, 74);
            this.cbAutomatch.Name = "cbAutomatch";
            this.cbAutomatch.Size = new System.Drawing.Size(77, 17);
            this.cbAutomatch.TabIndex = 13;
            this.cbAutomatch.Text = "Automatch";
            this.cbAutomatch.UseVisualStyleBackColor = true;
            // 
            // rdoClientMode
            // 
            this.rdoClientMode.AutoSize = true;
            this.rdoClientMode.Location = new System.Drawing.Point(9, 47);
            this.rdoClientMode.Name = "rdoClientMode";
            this.rdoClientMode.Size = new System.Drawing.Size(81, 17);
            this.rdoClientMode.TabIndex = 12;
            this.rdoClientMode.Text = "Client Mode";
            this.rdoClientMode.UseVisualStyleBackColor = true;
            this.rdoClientMode.CheckedChanged += new System.EventHandler(this.rdoMode_CheckedChanged);
            // 
            // tbTaskInfo
            // 
            this.tbTaskInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTaskInfo.Cursor = System.Windows.Forms.Cursors.Default;
            this.tbTaskInfo.Location = new System.Drawing.Point(99, 72);
            this.tbTaskInfo.Name = "tbTaskInfo";
            this.tbTaskInfo.ReadOnly = true;
            this.tbTaskInfo.Size = new System.Drawing.Size(307, 20);
            this.tbTaskInfo.TabIndex = 7;
            this.tbTaskInfo.TabStop = false;
            this.tbTaskInfo.Click += new System.EventHandler(this.tbTaskInfo_Click);
            // 
            // cbTaskWake
            // 
            this.cbTaskWake.AutoSize = true;
            this.cbTaskWake.Location = new System.Drawing.Point(265, 22);
            this.cbTaskWake.Name = "cbTaskWake";
            this.cbTaskWake.Size = new System.Drawing.Size(55, 17);
            this.cbTaskWake.TabIndex = 5;
            this.cbTaskWake.Text = "Wake";
            this.cbTaskWake.UseVisualStyleBackColor = true;
            // 
            // rdoFullMode
            // 
            this.rdoFullMode.AutoSize = true;
            this.rdoFullMode.Location = new System.Drawing.Point(9, 22);
            this.rdoFullMode.Name = "rdoFullMode";
            this.rdoFullMode.Size = new System.Drawing.Size(71, 17);
            this.rdoFullMode.TabIndex = 11;
            this.rdoFullMode.Text = "Full Mode";
            this.rdoFullMode.UseVisualStyleBackColor = true;
            this.rdoFullMode.CheckedChanged += new System.EventHandler(this.rdoMode_CheckedChanged);
            // 
            // lblSchedStatus
            // 
            this.lblSchedStatus.AutoSize = true;
            this.lblSchedStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSchedStatus.Location = new System.Drawing.Point(96, 49);
            this.lblSchedStatus.Name = "lblSchedStatus";
            this.lblSchedStatus.Size = new System.Drawing.Size(64, 13);
            this.lblSchedStatus.TabIndex = 4;
            this.lblSchedStatus.Text = "Task Status";
            // 
            // tbSchedTime
            // 
            this.tbSchedTime.Location = new System.Drawing.Point(221, 20);
            this.tbSchedTime.Mask = "00:00";
            this.tbSchedTime.Name = "tbSchedTime";
            this.tbSchedTime.Size = new System.Drawing.Size(38, 20);
            this.tbSchedTime.TabIndex = 3;
            this.tbSchedTime.TabStop = false;
            this.tbSchedTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbSchedTime.ValidatingType = typeof(System.DateTime);
            // 
            // lblUpdateTime
            // 
            this.lblUpdateTime.AutoSize = true;
            this.lblUpdateTime.Location = new System.Drawing.Point(96, 24);
            this.lblUpdateTime.Name = "lblUpdateTime";
            this.lblUpdateTime.Size = new System.Drawing.Size(119, 13);
            this.lblUpdateTime.TabIndex = 2;
            this.lblUpdateTime.Text = "Scheduled update time:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // ebtnRestore
            // 
            this.ebtnRestore.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ebtnRestore.Location = new System.Drawing.Point(282, 44);
            this.ebtnRestore.Name = "ebtnRestore";
            this.ebtnRestore.Size = new System.Drawing.Size(72, 23);
            this.ebtnRestore.TabIndex = 16;
            this.ebtnRestore.Text = "Restore";
            this.ebtnRestore.UseVisualStyleBackColor = true;
            this.ebtnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // ebtnRebuild
            // 
            this.ebtnRebuild.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ebtnRebuild.Location = new System.Drawing.Point(204, 70);
            this.ebtnRebuild.Name = "ebtnRebuild";
            this.ebtnRebuild.Size = new System.Drawing.Size(150, 23);
            this.ebtnRebuild.TabIndex = 14;
            this.ebtnRebuild.Text = "Rebuild WMC Database";
            this.ebtnRebuild.UseVisualStyleBackColor = true;
            this.ebtnRebuild.Click += new System.EventHandler(this.btnRebuild_Click);
            // 
            // ebtnSetup
            // 
            this.ebtnSetup.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ebtnSetup.Location = new System.Drawing.Point(6, 19);
            this.ebtnSetup.Name = "ebtnSetup";
            this.ebtnSetup.Size = new System.Drawing.Size(93, 23);
            this.ebtnSetup.TabIndex = 13;
            this.ebtnSetup.Text = "Client Setup";
            this.ebtnSetup.UseVisualStyleBackColor = true;
            this.ebtnSetup.Click += new System.EventHandler(this.btnSetup_Click);
            // 
            // btnTask
            // 
            this.btnTask.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnTask.Location = new System.Drawing.Point(326, 19);
            this.btnTask.Name = "btnTask";
            this.btnTask.Size = new System.Drawing.Size(80, 23);
            this.btnTask.TabIndex = 10;
            this.btnTask.Text = "Create";
            this.btnTask.UseVisualStyleBackColor = true;
            this.btnTask.Click += new System.EventHandler(this.btnTask_Click);
            // 
            // clientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 171);
            this.Name = "clientForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EPG123 Client Guide Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.clientForm_FormClosing);
            this.Load += new System.EventHandler(this.clientForm_Load);
            this.Shown += new System.EventHandler(this.clientForm_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mergedChannelToolStrip.ResumeLayout(false);
            this.mergedChannelToolStrip.PerformLayout();
            this.toolStripContainer2.ContentPanel.ResumeLayout(false);
            this.toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer2.TopToolStripPanel.PerformLayout();
            this.toolStripContainer2.ResumeLayout(false);
            this.toolStripContainer2.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.lineupChannelToolStrip.ResumeLayout(false);
            this.lineupChannelToolStrip.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.grpClientConfig.ResumeLayout(false);
            this.grpScheduledTask.ResumeLayout(false);
            this.grpScheduledTask.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView mergedChannelListView;
        private System.Windows.Forms.ColumnHeader columnCallSign;
        private System.Windows.Forms.ColumnHeader columnNumber;
        private System.Windows.Forms.ColumnHeader columnServiceName;
        private System.Windows.Forms.ListView lineupChannelListView;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer2;
        private System.Windows.Forms.ToolStrip mergedChannelToolStrip;
        private System.Windows.Forms.ToolStrip lineupChannelToolStrip;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem subscribeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unsubscribeMenuItem;
        private System.Windows.Forms.ToolStripLabel lblLineupCombobox;
        private System.Windows.Forms.ToolStripComboBox cmbObjectStoreLineups;
        private System.Windows.Forms.ColumnHeader columnSubscribedLineup;
        private System.Windows.Forms.ColumnHeader columnScannedSources;
        private System.Windows.Forms.ToolStripLabel lblMatchBy;
        private System.Windows.Forms.ToolStripButton btnAutoNumber;
        private System.Windows.Forms.ToolStripButton btnAutoCallsign;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnDeleteChannel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem renameMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox grpScheduledTask;
        private System.Windows.Forms.CheckBox cbTaskWake;
        private System.Windows.Forms.Label lblSchedStatus;
        private System.Windows.Forms.MaskedTextBox tbSchedTime;
        private System.Windows.Forms.Label lblUpdateTime;
        private System.Windows.Forms.ToolStripButton btnRefreshLineups;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.TextBox tbTaskInfo;
        private ElevatedButton btnTask;
        private System.Windows.Forms.RadioButton rdoClientMode;
        private System.Windows.Forms.RadioButton rdoFullMode;
        private System.Windows.Forms.CheckBox cbAutomatch;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox grpClientConfig;
        private ElevatedButton ebtnRebuild;
        private ElevatedButton ebtnSetup;
        private System.Windows.Forms.ToolStripStatusLabel lblToolStripStatus;
        private System.Windows.Forms.ToolStripButton btnChannelDisplay;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnAddChannels;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel lblMergedChannelListView;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripComboBox cmbSources;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private ElevatedButton ebtnRestore;
        private System.Windows.Forms.ToolStripButton btnLablesDisplay;
        private System.Windows.Forms.ToolStripMenuItem renumberMenuItem;
        private System.Windows.Forms.TextBox lvEditTextBox;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.Button btnTweakWmc;
        private System.Windows.Forms.Label lblDatabaseUtilities;
        private System.Windows.Forms.Button btnTransferTool;
        private System.Windows.Forms.ColumnHeader columnTuningInfo;
        private System.Windows.Forms.ToolStripButton btnStoreExplorer;
        private System.Windows.Forms.ToolStripButton btnExportMxf;
        private System.Windows.Forms.ToolStripButton btnDeleteLineup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.Button btnViewLog;
        private System.Windows.Forms.ToolStripProgressBar lvItemsProgressBar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem clipboardMenuItem;
        private System.Windows.Forms.ColumnHeader columnGuideEndTime;
        private System.Windows.Forms.ToolStripButton btnUndelete;
        private System.Windows.Forms.ToolStripMenuItem clearListingsMenuItem;
    }
}