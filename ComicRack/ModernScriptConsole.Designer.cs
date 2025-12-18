namespace cYo.Projects.ComicRack.Viewer
{
    partial class ModernScriptConsole
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

        private void InitializeComponent()
        {
            this.lstLogs = new System.Windows.Forms.ListView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btReload = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.btExport = new System.Windows.Forms.Button();
            this.chkAutoScroll = new System.Windows.Forms.CheckBox();
            this.chkEnableTrace = new System.Windows.Forms.CheckBox();
            this.cmbSourceFilter = new System.Windows.Forms.ComboBox();
            this.cmbLevelFilter = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStripLogs = new System.Windows.Forms.ContextMenuStrip();
            this.miCopySelected = new System.Windows.Forms.ToolStripMenuItem();
            this.miExportAll = new System.Windows.Forms.ToolStripMenuItem();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            //
            // contextMenuStripLogs
            //
            this.contextMenuStripLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miCopySelected,
                this.miExportAll});
            this.contextMenuStripLogs.Name = "contextMenuStripLogs";
            this.contextMenuStripLogs.Size = new System.Drawing.Size(180, 48);
            //
            // miCopySelected
            //
            this.miCopySelected.Name = "miCopySelected";
            this.miCopySelected.Size = new System.Drawing.Size(180, 22);
            this.miCopySelected.Text = "Copy Selected";
            this.miCopySelected.Click += new System.EventHandler(this.miCopySelected_Click);
            //
            // miExportAll
            //
            this.miExportAll.Name = "miExportAll";
            this.miExportAll.Size = new System.Drawing.Size(180, 22);
            this.miExportAll.Text = "Export Report...";
            this.miExportAll.Click += new System.EventHandler(this.btExport_Click);
            //
            // lstLogs
            //
            this.lstLogs.ContextMenuStrip = this.contextMenuStripLogs;
            this.lstLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLogs.FullRowSelect = true;
            this.lstLogs.HideSelection = false;
            this.lstLogs.Location = new System.Drawing.Point(0, 95);
            this.lstLogs.Name = "lstLogs";
            this.lstLogs.Size = new System.Drawing.Size(800, 410);
            this.lstLogs.TabIndex = 0;
            this.lstLogs.UseCompatibleStateImageBehavior = false;
            this.lstLogs.View = System.Windows.Forms.View.Details;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.txtSearch);
            this.panelTop.Controls.Add(this.cmbLevelFilter);
            this.panelTop.Controls.Add(this.cmbSourceFilter);
            this.panelTop.Controls.Add(this.chkEnableTrace);
            this.panelTop.Controls.Add(this.chkAutoScroll);
            this.panelTop.Controls.Add(this.btExport);
            this.panelTop.Controls.Add(this.btClear);
            this.panelTop.Controls.Add(this.btReload);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(800, 95);
            this.panelTop.TabIndex = 1;
            // 
            // btReload
            //
            this.btReload.Location = new System.Drawing.Point(12, 10);
            this.btReload.Name = "btReload";
            this.btReload.Size = new System.Drawing.Size(100, 23);
            this.btReload.TabIndex = 0;
            this.btReload.Text = "Reload Plugins";
            this.btReload.UseVisualStyleBackColor = true;
            this.btReload.Click += new System.EventHandler(this.btReload_Click);
            // 
            // btExport
            //
            this.btExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btExport.Location = new System.Drawing.Point(612, 10);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(95, 23);
            this.btExport.TabIndex = 7;
            this.btExport.Text = "Export Report";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // btClear
            //
            this.btClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btClear.Location = new System.Drawing.Point(713, 10);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(75, 23);
            this.btClear.TabIndex = 1;
            this.btClear.Text = "Clear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            //
            // chkAutoScroll
            //
            this.chkAutoScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAutoScroll.AutoSize = true;
            this.chkAutoScroll.Checked = true;
            this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoScroll.Location = new System.Drawing.Point(130, 13);
            this.chkAutoScroll.Name = "chkAutoScroll";
            this.chkAutoScroll.Size = new System.Drawing.Size(77, 17);
            this.chkAutoScroll.TabIndex = 2;
            this.chkAutoScroll.Text = "Auto-Scroll";
            this.chkAutoScroll.UseVisualStyleBackColor = true;
            //
            // chkEnableTrace
            //
            this.chkEnableTrace.AutoSize = true;
            this.chkEnableTrace.Checked = false;
            this.chkEnableTrace.Location = new System.Drawing.Point(250, 13);
            this.chkEnableTrace.Name = "chkEnableTrace";
            this.chkEnableTrace.Size = new System.Drawing.Size(130, 17);
            this.chkEnableTrace.TabIndex = 6;
            this.chkEnableTrace.Text = "Enable Python Trace";
            this.chkEnableTrace.UseVisualStyleBackColor = true;
            //
            // cmbSourceFilter
            //
            this.cmbSourceFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSourceFilter.FormattingEnabled = true;
            this.cmbSourceFilter.Items.AddRange(new object[] { "All Sources" });
            this.cmbSourceFilter.Location = new System.Drawing.Point(250, 43);
            this.cmbSourceFilter.Name = "cmbSourceFilter";
            this.cmbSourceFilter.Size = new System.Drawing.Size(121, 21);
            this.cmbSourceFilter.SelectedIndex = 0;
            this.cmbSourceFilter.TabIndex = 3;
            this.cmbSourceFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // cmbLevelFilter
            //
            this.cmbLevelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevelFilter.FormattingEnabled = true;
            this.cmbLevelFilter.Location = new System.Drawing.Point(12, 43);
            this.cmbLevelFilter.Name = "cmbLevelFilter";
            this.cmbLevelFilter.Size = new System.Drawing.Size(106, 21);
            this.cmbLevelFilter.TabIndex = 4;
            this.cmbLevelFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // txtSearch
            //
            this.txtSearch.Location = new System.Drawing.Point(65, 66);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(150, 20);
            this.txtSearch.TabIndex = 5;
            this.txtSearch.TextChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Search";
            // 
            // ModernScriptConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lstLogs);
            this.Controls.Add(this.panelTop);
            this.Name = "ModernScriptConsole";
            this.Text = "Modern Script Console";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.ListView lstLogs;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.Button btReload;
        private System.Windows.Forms.CheckBox chkAutoScroll;
        private System.Windows.Forms.CheckBox chkEnableTrace;
        private System.Windows.Forms.ComboBox cmbLevelFilter;
        private System.Windows.Forms.ComboBox cmbSourceFilter;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLogs;
        private System.Windows.Forms.ToolStripMenuItem miCopySelected;
        private System.Windows.Forms.ToolStripMenuItem miExportAll;
    }
}
