namespace cYo.Projects.ComicRack.Viewer
{
    partial class AdvancedPythonSettingsForm
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
            this.chkDebugMode = new System.Windows.Forms.CheckBox();
            this.chkEnableFrames = new System.Windows.Forms.CheckBox();
            this.chkEnableFullFrames = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelWarning = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkDebugMode
            // 
            this.chkDebugMode.AutoSize = true;
            this.chkDebugMode.Location = new System.Drawing.Point(15, 25);
            this.chkDebugMode.Name = "chkDebugMode";
            this.chkDebugMode.Size = new System.Drawing.Size(124, 17);
            this.chkDebugMode.TabIndex = 0;
            this.chkDebugMode.Text = "Enable Debug Mode";
            this.chkDebugMode.UseVisualStyleBackColor = true;
            // 
            // chkEnableFrames
            // 
            this.chkEnableFrames.AutoSize = true;
            this.chkEnableFrames.Location = new System.Drawing.Point(15, 50);
            this.chkEnableFrames.Name = "chkEnableFrames";
            this.chkEnableFrames.Size = new System.Drawing.Size(96, 17);
            this.chkEnableFrames.TabIndex = 1;
            this.chkEnableFrames.Text = "Enable Frames";
            this.chkEnableFrames.UseVisualStyleBackColor = true;
            // 
            // chkEnableFullFrames
            // 
            this.chkEnableFullFrames.AutoSize = true;
            this.chkEnableFullFrames.Location = new System.Drawing.Point(15, 75);
            this.chkEnableFullFrames.Name = "chkEnableFullFrames";
            this.chkEnableFullFrames.Size = new System.Drawing.Size(114, 17);
            this.chkEnableFullFrames.TabIndex = 2;
            this.chkEnableFullFrames.Text = "Enable Full Frames";
            this.chkEnableFullFrames.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(197, 137);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkDebugMode);
            this.groupBox1.Controls.Add(this.chkEnableFrames);
            this.groupBox1.Controls.Add(this.chkEnableFullFrames);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 110);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Engine Settings (Requires Restart)";
            // 
            // labelWarning
            // 
            this.labelWarning.AutoSize = true;
            this.labelWarning.ForeColor = System.Drawing.Color.DimGray;
            this.labelWarning.Location = new System.Drawing.Point(13, 137);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(161, 13);
            this.labelWarning.TabIndex = 5;
            this.labelWarning.Text = "Note: Changes apply on restart.";
            // 
            // AdvancedPythonSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 172);
            this.Controls.Add(this.labelWarning);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdvancedPythonSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Advanced Python Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.CheckBox chkDebugMode;
        private System.Windows.Forms.CheckBox chkEnableFrames;
        private System.Windows.Forms.CheckBox chkEnableFullFrames;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelWarning;
    }
}
