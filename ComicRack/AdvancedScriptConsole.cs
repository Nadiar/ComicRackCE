using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using cYo.Projects.ComicRack.Plugins;

using cYo.Common.Windows.Forms;

namespace cYo.Projects.ComicRack.Viewer
{
    public partial class AdvancedScriptConsole : FormEx
    {
        private ConcurrentQueue<LogEntry> _pendingLogs = new ConcurrentQueue<LogEntry>();
        private Timer _uiTimer;
        private HashSet<string> _sources = new HashSet<string>();

        public AdvancedScriptConsole()
        {
            try
            {
                InitializeComponent();
                this.Icon = Properties.Resources.ComicRackAppSmall;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize component: {ex.Message}\n\n{ex.StackTrace}", "Error in InitializeComponent");
                throw;
            }

            // Set up ListView columns
            lstLogs.Columns.Add("Time", 80);
            lstLogs.Columns.Add("Level", 70);
            lstLogs.Columns.Add("Source", 100);
            lstLogs.Columns.Add("Message", 600);
            
            // Set up Level Filter
            cmbLevelFilter.Items.AddRange(Enum.GetNames(typeof(LogLevel)));
            // Default to Info if available, otherwise Trace (0)
            int infoIndex = cmbLevelFilter.Items.IndexOf("Info");
            cmbLevelFilter.SelectedIndex = infoIndex >= 0 ? infoIndex : 0;

            // Set up UI refresh timer
            _uiTimer = new Timer { Interval = 200 };
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();

            LogManager.LogAdded += OnLogAdded;
            
            // Load existing logs
            foreach (var log in LogManager.GetLogs())
            {
                OnLogAdded(log);
            }

            lstLogs.KeyDown += LstLogs_KeyDown;
            
            // Initialize trace checkbox from global setting (loaded from INI via PythonCommand)
            if (this.chkEnableTrace != null)
                this.chkEnableTrace.Checked = PythonRuntimeManager.EnablePythonTracing;
            
            // Initialize Autosave settings (UI Only - Backend handles writes)
            if (PythonCommand.Settings != null)
            {
                if (chkAutosave != null) chkAutosave.Checked = PythonCommand.Settings.PythonAutosaveEnabled;
                
                string rawPath = PythonCommand.Settings.PythonAutosavePath;
                if (!string.IsNullOrEmpty(rawPath))
                {
                    if (txtAutosavePath != null) txtAutosavePath.Text = rawPath;
                }
                
                LogManager.Info("System", $"Console Initialized. Autosave: {chkAutosave?.Checked}, Path: {txtAutosavePath?.Text}");
            }
            else
            {
                LogManager.Warning("System", "Console Initialized but PythonCommand.Settings was NULL.");
            }

            // Register cleanup when form closes
            this.FormClosing += AdvancedScriptConsole_FormClosing;
        }

        private void AdvancedScriptConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the UI timer to prevent it from running after form closes
            if (_uiTimer != null)
            {
                _uiTimer.Stop();
                _uiTimer.Dispose();
                _uiTimer = null;
            }

            // Unsubscribe from LogManager events
            LogManager.LogAdded -= OnLogAdded;
        }

        private void OnLogAdded(LogEntry entry)
        {
            _pendingLogs.Enqueue(entry);
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (_pendingLogs.IsEmpty) return;

            lstLogs.BeginUpdate();
            while (_pendingLogs.TryDequeue(out var entry))
            {
                if (!ShouldShow(entry)) continue;

            var item = new ListViewItem(entry.Timestamp.ToString("HH:mm:ss"));
            item.SubItems.Add(entry.Level.ToString());
            item.SubItems.Add(entry.Source);
            item.SubItems.Add(entry.Message);

            // Basic coloring
            switch (entry.Level)
            {
                case LogLevel.Error: item.ForeColor = Color.Red; break;
                case LogLevel.Warning: item.ForeColor = Color.DarkOrange; break;
                case LogLevel.Debug: item.ForeColor = Color.Gray; break;
                case LogLevel.Trace: item.ForeColor = Color.LightGray; break;
            }

            lstLogs.Items.Add(item);
            
            if (!_sources.Contains(entry.Source))
            {
                _sources.Add(entry.Source);
                cmbSourceFilter.Items.Add(entry.Source);
            }
        } // End While

        // Scroll to bottom if requested (optional)
        if (chkAutoScroll.Checked && lstLogs.Items.Count > 0)
        {
            lstLogs.EnsureVisible(lstLogs.Items.Count - 1);
        }

        lstLogs.EndUpdate();
    }

        private bool ShouldShow(LogEntry entry)
        {
            // Level Fix
            if (cmbLevelFilter.SelectedIndex >= 0)
            {
                var minLevel = (LogLevel)Enum.Parse(typeof(LogLevel), cmbLevelFilter.SelectedItem.ToString());
                if (entry.Level < minLevel) return false;
            }

            // Source Filter
            if (cmbSourceFilter.SelectedIndex > 0) // 0 is "All"
            {
                if (entry.Source != cmbSourceFilter.SelectedItem.ToString()) return false;
            }

            // Search text
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                if (entry.Message.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) < 0 &&
                    entry.Source.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            return true;
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            lstLogs.Items.Clear();
        }

        private void btReload_Click(object sender, EventArgs e)
        {
            if (chkEnableTrace.Checked)
            {
                LogManager.Info("System", "Requesting plugin reload... (Python Trace ENABLED)");
                PythonRuntimeManager.EnablePythonTracing = true;
            }
            else
            {
                LogManager.Info("System", "Requesting plugin reload...");
                PythonRuntimeManager.EnablePythonTracing = false;
            }

            ScriptUtility.Reload();
            // LogManager.Warning("System", "Plugin Reload not supported in legacy backport.");
            
            // Note: We might want to keep tracing enabled for subsequent calls until unchecked?
            // For safety and lower noise, maybe we disable it after one reload cycle if that's the intent.
            // But usually "Enable Trace" means "Keep tracing". Let's assume persistent for this session.
            // Requirement said "Pass trace mode to reload/plugin execution". 
            // If we just set the static property, it stays set.
        }

        private void cmbFilter_Changed(object sender, EventArgs e)
        {
            // Trigger a re-load of all logs if we had a persistent buffer, 
            // but for now just clearing and waiting for new ones is simpler for MVP.
            // In a full implementation, we'd clear lstLogs and re-populate from LogManager.GetLogs()
            RefreshLogs();
        }

        private void chkEnableTrace_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableTrace.Checked)
            {
                LogManager.Info("System", "Python Trace Enabled by User");
                PythonRuntimeManager.EnablePythonTracing = true;
            }
            else
            {
                LogManager.Info("System", "Python Trace Disabled by User");
                PythonRuntimeManager.EnablePythonTracing = false;
            }
        }

        private void RefreshLogs()
        {
            lstLogs.Items.Clear();
            foreach (var log in LogManager.GetLogs())
            {
                if (ShouldShow(log)) OnLogAdded(log);
            }
        }

        private void LstLogs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedLogs();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAllVisibleItems();
                e.Handled = true;
            }
        }

        private void CopySelectedLogs()
        {
            if (lstLogs.SelectedItems.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            foreach (ListViewItem item in lstLogs.SelectedItems)
            {
                // Format: [Time] [Level] [Source] Message
                sb.AppendLine($"[{item.Text}] [{item.SubItems[1].Text}] [{item.SubItems[2].Text}] {item.SubItems[3].Text}");
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void SelectAllVisibleItems()
        {
            lstLogs.BeginUpdate();
            foreach (ListViewItem item in lstLogs.Items)
            {
                item.Selected = true;
            }
            lstLogs.EndUpdate();
        }

        private void miCopySelected_Click(object sender, EventArgs e)
        {
            CopySelectedLogs();
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Log Files (*.log)|*.log|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.FileName = $"trace_report_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                sfd.DefaultExt = "log";
                sfd.Title = "Export Trace Report";
                
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportTraceToFile(sfd.FileName);
                }
            }
        }

        private void ExportTraceToFile(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine("Python Script Execution Trace Report");
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Trace Enabled: {PythonRuntimeManager.EnablePythonTracing}");
                    writer.WriteLine();
                    
                    // Write filter information
                    writer.WriteLine("Filters Applied:");
                    writer.WriteLine($"  Level Filter: {(cmbLevelFilter.SelectedIndex >= 0 ? cmbLevelFilter.SelectedItem.ToString() : "None")}");
                    writer.WriteLine($"  Source Filter: {(cmbSourceFilter.SelectedIndex > 0 ? cmbSourceFilter.SelectedItem.ToString() : "All")}");
                    writer.WriteLine($"  Search Text: {(string.IsNullOrEmpty(txtSearch.Text) ? "None" : txtSearch.Text)}");
                    writer.WriteLine();
                    
                    // Count entries by level
                    var levelCounts = new Dictionary<LogLevel, int>();
                    foreach (ListViewItem item in lstLogs.Items)
                    {
                        if (Enum.TryParse<LogLevel>(item.SubItems[1].Text, out var level))
                        {
                            if (!levelCounts.ContainsKey(level)) levelCounts[level] = 0;
                            levelCounts[level]++;
                        }
                    }
                    
                    writer.WriteLine("Summary:");
                    writer.WriteLine($"  Total Entries: {lstLogs.Items.Count}");
                    foreach (var kvp in levelCounts.OrderBy(k => k.Key))
                    {
                        writer.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                    writer.WriteLine();
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine();
                    
                    // Write log entries
                    foreach (ListViewItem item in lstLogs.Items)
                    {
                        writer.WriteLine($"[{item.Text}] [{item.SubItems[1].Text}] [{item.SubItems[2].Text}] {item.SubItems[3].Text}");
                    }
                }
                
                LogManager.Info("System", $"Trace report exported: {filePath}");
                MessageBox.Show($"Trace report exported successfully to:\n{filePath}\n\nEntries: {lstLogs.Items.Count}", 
                               "Export Complete", 
                               MessageBoxButtons.OK, 
                               MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogManager.Error("System", $"Failed to export trace report: {ex.Message}");
                MessageBox.Show($"Failed to export trace report:\n{ex.Message}", 
                               "Export Failed", 
                               MessageBoxButtons.OK, 
                               MessageBoxIcon.Error);
            }
        }

        private Rectangle safeBounds;

        public Rectangle SafeBounds
        {
            get => safeBounds;
            set
            {
                base.StartPosition = FormStartPosition.Manual;
                base.Bounds = value;
                safeBounds = value;
            }
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            UpdateSafeBounds();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            UpdateSafeBounds();
        }

        private void UpdateSafeBounds()
        {
            if (base.IsHandleCreated && base.WindowState == FormWindowState.Normal && base.FormBorderStyle != FormBorderStyle.None)
            {
                safeBounds = base.Bounds;
            }
        }

        public void AutoExportLog(string logPath)
        {
            try
            {
                var logs = LogManager.GetLogs().ToList();
                using (var writer = new StreamWriter(logPath, false, Encoding.UTF8))
                {
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine("Automatic Python Trace Export");
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Trace Enabled: {PythonRuntimeManager.EnablePythonTracing}");
                    
                    try {
                        writer.WriteLine("Library Config Paths:");
                        writer.WriteLine($"  AppData: {Program.Paths.ApplicationDataPath}");
                        writer.WriteLine($"  System Scripts: {Program.Paths.ScriptPath}");
                        writer.WriteLine($"  User Scripts: {Program.Paths.ScriptPathSecondary}");
                        writer.WriteLine($"  Database: {Program.Paths.DatabasePath}");
                    } catch (Exception ex) {
                        writer.WriteLine($"  Error reading paths: {ex.Message}");
                    }

                    writer.WriteLine();

                    foreach (var entry in logs)
                    {
                         writer.WriteLine($"[{entry.Timestamp:HH:mm:ss}] [{entry.Level}] [{entry.Source}] {entry.Message}");
                    }
                }
                LogManager.Info("System", $"Trace automatically exported to: {logPath}");
            }
            catch (Exception ex)
            {
                // Last ditch effort to log to a file if LogManager fails or we can't write
                 try { File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trace_export_fail.txt"), ex.ToString()); } catch { }
            }
        }
        private void btAdvanced_Click(object sender, EventArgs e)
        {
            using (var form = new AdvancedPythonSettingsForm())
            {
                form.ShowDialog(this);
                // Settings are updated in static properties automatically by the form.
                // User can choose to Save Settings afterwards.
            }
        }
        
        private void btnAutosavePath_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Log Files (*.log)|*.log|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.FileName = txtAutosavePath.Text;
                sfd.Title = "Select Autosave File";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    txtAutosavePath.Text = sfd.FileName;
                }
            }
        }

        private void btSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // Update settings object from UI
                if (PythonCommand.Settings != null)
                {
                    PythonCommand.Settings.PythonAutosaveEnabled = chkAutosave.Checked;
                    PythonCommand.Settings.PythonAutosavePath = txtAutosavePath.Text;
                }

                PythonCommand.SaveSettings();
                MessageBox.Show("Trace settings saved to ComicRack.ini", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
