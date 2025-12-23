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

        private void InitializeComponent()
        {
            this.lstLogs = new System.Windows.Forms.ListView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.btReload = new System.Windows.Forms.Button();
            this.btSaveSettings = new System.Windows.Forms.Button();
            this.btAdvanced = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.btExport = new System.Windows.Forms.Button();
            this.chkAutoScroll = new System.Windows.Forms.CheckBox();
            this.chkEnableTrace = new System.Windows.Forms.CheckBox();
            this.cmbSourceFilter = new System.Windows.Forms.ComboBox();
            this.cmbLevelFilter = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkAutosave = new System.Windows.Forms.CheckBox();
            this.txtAutosavePath = new System.Windows.Forms.TextBox();
            this.btnAutosavePath = new System.Windows.Forms.Button();
            this.btSaveSettings = new System.Windows.Forms.Button();
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
            // 
            // panelTop
            // 
            this.panelTop.AutoSize = true;
            this.panelTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelTop.Controls.Add(this.pnlFilters);
            this.panelTop.Controls.Add(this.pnlControls);
            this.panelTop.Controls.Add(this.flpButtons);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 105);
            this.panelTop.TabIndex = 1;

            //
            // flpButtons
            //
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpButtons.Padding = new System.Windows.Forms.Padding(5);
            // Add buttons in Order: Reload, Settings, Export, Advanced, Clear
            this.flpButtons.Controls.Add(this.btReload);
            this.flpButtons.Controls.Add(this.btSaveSettings);
            this.flpButtons.Controls.Add(this.btExport);
            this.flpButtons.Controls.Add(this.btAdvanced);
            this.flpButtons.Controls.Add(this.btClear);
            //
            // pnlControls
            //
            this.pnlControls.AutoSize = true;
            this.pnlControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControls.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.pnlControls.Controls.Add(this.chkEnableTrace);
            this.pnlControls.Controls.Add(this.chkAutoScroll);
            this.pnlControls.Controls.Add(this.chkAutosave);
            this.pnlControls.Controls.Add(this.txtAutosavePath);
            this.pnlControls.Controls.Add(this.btnAutosavePath);
            //
            // pnlFilters
            //
            this.pnlFilters.AutoSize = true;
            this.pnlFilters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.pnlFilters.Controls.Add(this.cmbLevelFilter);
            this.pnlFilters.Controls.Add(this.cmbSourceFilter);
            this.pnlFilters.Controls.Add(this.label1);
            this.pnlFilters.Controls.Add(this.txtSearch);

            // ... (control properties remain same) ...

            // 
            // AdvancedScriptConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);

            // 
            // btReload
            // 
            this.btReload.Name = "btReload";
            this.btReload.Size = new System.Drawing.Size(100, 23);
            this.btReload.TabIndex = 0;
            this.btReload.Margin = new System.Windows.Forms.Padding(5);
            this.btReload.Text = "Reload Plugins";
            this.btReload.UseVisualStyleBackColor = true;
            this.btReload.Click += new System.EventHandler(this.btReload_Click);
            // 
            // btSaveSettings
            // 
            this.btSaveSettings.Name = "btSaveSettings";
            this.btSaveSettings.Size = new System.Drawing.Size(95, 23);
            this.btSaveSettings.TabIndex = 1;
            this.btSaveSettings.Margin = new System.Windows.Forms.Padding(5);
            this.btSaveSettings.Text = "Save Settings";
            this.btSaveSettings.UseVisualStyleBackColor = true;
            this.btSaveSettings.Click += new System.EventHandler(this.btSaveSettings_Click);
            // 
            // btExport
            // 
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(95, 23);
            this.btExport.TabIndex = 2;
            this.btExport.Margin = new System.Windows.Forms.Padding(5);
            this.btExport.Text = "Export Report";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // btAdvanced
            // 
            this.btAdvanced.Name = "btAdvanced";
            this.btAdvanced.Size = new System.Drawing.Size(95, 23);
            this.btAdvanced.TabIndex = 3;
            this.btAdvanced.Margin = new System.Windows.Forms.Padding(5);
            this.btAdvanced.Text = "Advanced...";
            this.btAdvanced.UseVisualStyleBackColor = true;
            this.btAdvanced.Click += new System.EventHandler(this.btAdvanced_Click);
            // 
            // btClear
            // 
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(75, 23);
            this.btClear.TabIndex = 4;
            this.btClear.Margin = new System.Windows.Forms.Padding(5);
            this.btClear.Text = "Clear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);

            // 
            // chkEnableTrace
            // 
            this.chkEnableTrace.AutoSize = true;
            this.chkEnableTrace.Location = new System.Drawing.Point(12, 8);
            this.chkEnableTrace.Name = "chkEnableTrace";
            this.chkEnableTrace.Size = new System.Drawing.Size(130, 17);
            this.chkEnableTrace.TabIndex = 5;
            this.chkEnableTrace.Text = "Enable Python Trace";
            this.chkEnableTrace.UseVisualStyleBackColor = true;
            this.chkEnableTrace.CheckedChanged += new System.EventHandler(this.chkEnableTrace_CheckedChanged);
            // 
            // chkAutoScroll
            // 
            this.chkAutoScroll.AutoSize = true;
            this.chkAutoScroll.Checked = true;
            this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoScroll.Location = new System.Drawing.Point(165, 8);
            this.chkAutoScroll.Name = "chkAutoScroll";
            this.chkAutoScroll.Size = new System.Drawing.Size(77, 17);
            this.chkAutoScroll.TabIndex = 6;
            this.chkAutoScroll.Text = "Auto-Scroll";
            this.chkAutoScroll.UseVisualStyleBackColor = true;
            // 
            // chkAutosave
            // 
            this.chkAutosave.AutoSize = true;
            this.chkAutosave.Location = new System.Drawing.Point(270, 8);
            this.chkAutosave.Name = "chkAutosave";
            this.chkAutosave.Size = new System.Drawing.Size(71, 17);
            this.chkAutosave.TabIndex = 7;
            this.chkAutosave.Text = "Autosave";
            this.chkAutosave.UseVisualStyleBackColor = true;
            // 
            // txtAutosavePath
            // 
            this.txtAutosavePath.Location = new System.Drawing.Point(360, 6);
            this.txtAutosavePath.Name = "txtAutosavePath";
            this.txtAutosavePath.Size = new System.Drawing.Size(120, 20);
            this.txtAutosavePath.TabIndex = 8;
            this.txtAutosavePath.Text = "Trace.log";
            // 
            // btnAutosavePath
            // 
            this.btnAutosavePath.Location = new System.Drawing.Point(490, 5);
            this.btnAutosavePath.Name = "btnAutosavePath";
            this.btnAutosavePath.Size = new System.Drawing.Size(25, 23);
            this.btnAutosavePath.TabIndex = 9;
            this.btnAutosavePath.Text = "...";
            this.btnAutosavePath.UseVisualStyleBackColor = true;
            this.btnAutosavePath.Click += new System.EventHandler(this.btnAutosavePath_Click);

            // 
            // cmbLevelFilter
            // 
            this.cmbLevelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevelFilter.FormattingEnabled = true;
            this.cmbLevelFilter.Location = new System.Drawing.Point(12, 6);
            this.cmbLevelFilter.Name = "cmbLevelFilter";
            this.cmbLevelFilter.Size = new System.Drawing.Size(106, 21);
            this.cmbLevelFilter.TabIndex = 10;
            this.cmbLevelFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // cmbSourceFilter
            // 
            this.cmbSourceFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSourceFilter.FormattingEnabled = true;
            this.cmbSourceFilter.Items.AddRange(new object[] { "All Sources" });
            this.cmbSourceFilter.Location = new System.Drawing.Point(135, 6);
            this.cmbSourceFilter.Name = "cmbSourceFilter";
            this.cmbSourceFilter.Size = new System.Drawing.Size(121, 21);
            this.cmbSourceFilter.SelectedIndex = 0;
            this.cmbSourceFilter.TabIndex = 11;
            this.cmbSourceFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_Changed);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(280, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Search";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(340, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.TabIndex = 13;
            this.txtSearch.TextChanged += new System.EventHandler(this.cmbFilter_Changed);

            this.Controls.Add(this.lstLogs);
            this.Controls.Add(this.panelTop);

            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.pnlControls.ResumeLayout(false);
            this.pnlControls.PerformLayout();
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.ListView lstLogs;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Panel pnlFilters;
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
        private System.Windows.Forms.CheckBox chkAutosave;
        private System.Windows.Forms.TextBox txtAutosavePath;
        private System.Windows.Forms.Button btnAutosavePath;
        private System.Windows.Forms.Button btSaveSettings;
        private System.Windows.Forms.Button btAdvanced;
        private System.Windows.Forms.ToolStripMenuItem miCopySelected;
        private System.Windows.Forms.ToolStripMenuItem miExportAll;
    }
}
