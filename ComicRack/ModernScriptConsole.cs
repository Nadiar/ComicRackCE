using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using cYo.Projects.ComicRack.Plugins;
using ComicRack.Plugins;
using cYo.Common.Windows.Forms;

namespace cYo.Projects.ComicRack.Viewer
{
    public partial class ModernScriptConsole : FormEx
    {
        private ConcurrentQueue<LogEntry> _pendingLogs = new ConcurrentQueue<LogEntry>();
        private Timer _uiTimer;
        private HashSet<string> _sources = new HashSet<string>();

        public ModernScriptConsole()
        {
            InitializeComponent();
            
            // Set up ListView columns
            lstLogs.Columns.Add("Time", 80);
            lstLogs.Columns.Add("Level", 70);
            lstLogs.Columns.Add("Source", 100);
            lstLogs.Columns.Add("Message", 600);
            
            // Set up Level Filter
            cmbLevelFilter.Items.AddRange(Enum.GetNames(typeof(LogLevel)));
            cmbLevelFilter.SelectedIndex = 2; // Info by default

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
            }

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
                if (!entry.Message.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) &&
                    !entry.Source.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase))
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
    }
}
