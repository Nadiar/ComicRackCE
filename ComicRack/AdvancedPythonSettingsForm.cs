using System;
using System.Windows.Forms;
using cYo.Common.Windows.Forms;
using cYo.Projects.ComicRack.Plugins;

namespace cYo.Projects.ComicRack.Viewer
{
    public partial class AdvancedPythonSettingsForm : FormEx
    {
        public AdvancedPythonSettingsForm()
        {
            InitializeComponent();
            
            // Load initial values from PythonCommand static properties
            var settings = PythonCommand.Settings;
            if (settings != null)
            {
                chkDebugMode.Checked = settings.PythonDebug;
                chkEnableFrames.Checked = settings.PythonEnableFrames;
                chkEnableFullFrames.Checked = settings.PythonEnableFullFrames;

                // Wire up events
                chkDebugMode.CheckedChanged += (s, e) => settings.PythonDebug = chkDebugMode.Checked;
                chkEnableFrames.CheckedChanged += (s, e) => settings.PythonEnableFrames = chkEnableFrames.Checked;
                chkEnableFullFrames.CheckedChanged += (s, e) => settings.PythonEnableFullFrames = chkEnableFullFrames.Checked;
            }
            else
            {
                // Fallback or disable controls if settings not loaded yet
                this.Enabled = false;
            }

            // Add specialized tooltips or warnings if needed?
            // For now, straightforward binding is sufficient.
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
