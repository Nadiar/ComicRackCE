namespace cYo.Projects.ComicRack.Viewer
{
    partial class AdvancedScriptConsole
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstLogs = new System.Windows.Forms.ListView();
            this.contextMenuStripLogs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCopySelected = new System.Windows.Forms.ToolStripMenuItem();
            this.miExportAll = new System.Windows.Forms.ToolStripMenuItem();

            this.btReload = new System.Windows.Forms.Button();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.btExport = new System.Windows.Forms.Button();
            this.chkAutoScroll = new System.Windows.Forms.CheckBox();
            this.chkEnableTrace = new System.Windows.Forms.CheckBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.cmbSourceFilter = new System.Windows.Forms.ComboBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.cmbLevelFilter = new System.Windows.Forms.ComboBox();
            this.lblLevel = new System.Windows.Forms.Label();
            this.btReload = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.contextMenuStripLogs.SuspendLayout();
            this.pnlControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripLogs
            // 
            this.contextMenuStripLogs.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCopySelected,
            this.miExportAll});
            this.contextMenuStripLogs.Name = "contextMenuStripLogs";
            this.contextMenuStripLogs.Size = new System.Drawing.Size(180, 48);
            // 
            // miCopySelected
            // 
            this.miCopySelected.Name = "miCopySelected";
            this.miCopySelected.Size = new System.Drawing.Size(179, 22);
            this.miCopySelected.Text = "Copy Selected";
            this.miCopySelected.Click += new System.EventHandler(this.miCopySelected_Click);
            // 
            // miExportAll
            // 
            this.miExportAll.Name = "miExportAll";
            this.miExportAll.Size = new System.Drawing.Size(179, 22);
            this.miExportAll.Text = "Export Report...";
            this.miExportAll.Click += new System.EventHandler(this.btExport_Click);
            // 
            // lstLogs
            // 
            this.lstLogs.ContextMenuStrip = this.contextMenuStripLogs;
            this.lstLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLogs.FullRowSelect = true;
            this.lstLogs.GridLines = true;
            this.lstLogs.HideSelection = false;
            this.lstLogs.Location = new System.Drawing.Point(0, 80);
            this.lstLogs.Name = "lstLogs";
            this.lstLogs.Size = new System.Drawing.Size(800, 370);
            this.lstLogs.TabIndex = 1;
            this.lstLogs.UseCompatibleStateImageBehavior = false;
            this.lstLogs.View = System.Windows.Forms.View.Details;
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.btExport);
            this.pnlControls.Controls.Add(this.chkAutoScroll);
            this.pnlControls.Controls.Add(this.chkEnableTrace);
            this.pnlControls.Controls.Add(this.txtSearch);
            this.pnlControls.Controls.Add(this.lblSearch);
            this.pnlControls.Controls.Add(this.cmbSourceFilter);
            this.pnlControls.Controls.Add(this.lblSource);
            this.pnlControls.Controls.Add(this.cmbLevelFilter);
            this.pnlControls.Controls.Add(this.lblLevel);
            this.pnlControls.Controls.Add(this.btReload);
            this.pnlControls.Controls.Add(this.btClear);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(800, 80);
            this.pnlControls.TabIndex = 0;
            // 
            // btExport
            // 
            this.btExport.Location = new System.Drawing.Point(430, 10);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(80, 32);
            this.btExport.TabIndex = 10;
            this.btExport.Text = "Export";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // chkAutoScroll
            // 
            this.chkAutoScroll.AutoSize = true;
            this.chkAutoScroll.Checked = true;
            this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoScroll.Location = new System.Drawing.Point(650, 48);
            this.chkAutoScroll.Name = "chkAutoScroll";
            this.chkAutoScroll.Size = new System.Drawing.Size(90, 24);
            this.chkAutoScroll.TabIndex = 9;
            this.chkAutoScroll.Text = "Auto Scroll";
            this.chkAutoScroll.UseVisualStyleBackColor = true;
            // 
            // chkEnableTrace
            // 
            this.chkEnableTrace.AutoSize = true;
            this.chkEnableTrace.Location = new System.Drawing.Point(650, 15);
            this.chkEnableTrace.Name = "chkEnableTrace";
            this.chkEnableTrace.Size = new System.Drawing.Size(125, 24);
            this.chkEnableTrace.TabIndex = 8;
            this.chkEnableTrace.Text = "Enable Trace";
            this.chkEnableTrace.UseVisualStyleBackColor = true;
            this.chkEnableTrace.CheckedChanged += new System.EventHandler(this.chkEnableTrace_CheckedChanged);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(360, 45);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(200, 27);
            this.txtSearch.TabIndex = 7;
            this.txtSearch.TextChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(300, 48);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(56, 20);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Search:";
            // 
            // cmbSourceFilter
            // 
            this.cmbSourceFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSourceFilter.FormattingEnabled = true;
            this.cmbSourceFilter.Items.AddRange(new object[] {
            "All"});
            this.cmbSourceFilter.Location = new System.Drawing.Point(70, 45);
            this.cmbSourceFilter.Name = "cmbSourceFilter";
            this.cmbSourceFilter.Size = new System.Drawing.Size(200, 28);
            this.cmbSourceFilter.TabIndex = 5;
            this.cmbSourceFilter.SelectedIndex = 0;
            this.cmbSourceFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(12, 48);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(57, 20);
            this.lblSource.TabIndex = 4;
            this.lblSource.Text = "Source:";
            // 
            // cmbLevelFilter
            // 
            this.cmbLevelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevelFilter.FormattingEnabled = true;
            this.cmbLevelFilter.Location = new System.Drawing.Point(70, 12);
            this.cmbLevelFilter.Name = "cmbLevelFilter";
            this.cmbLevelFilter.Size = new System.Drawing.Size(120, 28);
            this.cmbLevelFilter.TabIndex = 3;
            this.cmbLevelFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Location = new System.Drawing.Point(12, 15);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(46, 20);
            this.lblLevel.TabIndex = 2;
            this.lblLevel.Text = "Level:";
            // 
            // btReload
            // 
            this.btReload.Location = new System.Drawing.Point(210, 10);
            this.btReload.Name = "btReload";
            this.btReload.Size = new System.Drawing.Size(120, 32);
            this.btReload.TabIndex = 1;
            this.btReload.Text = "Reload Plugins";
            this.btReload.UseVisualStyleBackColor = true;
            this.btReload.Click += new System.EventHandler(this.btReload_Click);
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(340, 10);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(80, 32);
            this.btClear.TabIndex = 0;
            this.btClear.Text = "Clear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // AdvancedScriptConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lstLogs);
            this.Controls.Add(this.pnlControls);
            this.Name = "AdvancedScriptConsole";
            this.Text = "Script Console";
            this.contextMenuStripLogs.ResumeLayout(false);
            this.pnlControls.ResumeLayout(false);
            this.pnlControls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstLogs;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.Button btReload;
        private System.Windows.Forms.CheckBox chkAutoScroll;
        private System.Windows.Forms.CheckBox chkEnableTrace;
        private System.Windows.Forms.ComboBox cmbLevelFilter;
        private System.Windows.Forms.ComboBox cmbSourceFilter;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLogs;
        private System.Windows.Forms.ToolStripMenuItem miCopySelected;
        private System.Windows.Forms.ToolStripMenuItem miExportAll;
    }
}
