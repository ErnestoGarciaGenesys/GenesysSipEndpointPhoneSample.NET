
namespace SIP_Phone_Sample
{
	partial class VolumeSlider
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.volumeTrackBar = new System.Windows.Forms.TrackBar();
			this.unmutedCheckbox = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.volumeTrackBar)).BeginInit();
			this.SuspendLayout();
			// 
			// volumeTrackBar
			// 
			this.volumeTrackBar.Dock = System.Windows.Forms.DockStyle.Right;
			this.volumeTrackBar.LargeChange = 10;
			this.volumeTrackBar.Location = new System.Drawing.Point(86, 0);
			this.volumeTrackBar.Maximum = 255;
			this.volumeTrackBar.Name = "volumeTrackBar";
			this.volumeTrackBar.Size = new System.Drawing.Size(104, 26);
			this.volumeTrackBar.TabIndex = 0;
			this.volumeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			// 
			// unmutedCheckbox
			// 
			this.unmutedCheckbox.Checked = true;
			this.unmutedCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.unmutedCheckbox.Dock = System.Windows.Forms.DockStyle.Left;
			this.unmutedCheckbox.Location = new System.Drawing.Point(0, 0);
			this.unmutedCheckbox.Name = "unmutedCheckbox";
			this.unmutedCheckbox.Size = new System.Drawing.Size(88, 26);
			this.unmutedCheckbox.TabIndex = 1;
			this.unmutedCheckbox.Text = "[Text]";
			this.unmutedCheckbox.UseVisualStyleBackColor = true;
			// 
			// VolumeSlider
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.unmutedCheckbox);
			this.Controls.Add(this.volumeTrackBar);
			this.MinimumSize = new System.Drawing.Size(190, 26);
			this.Name = "VolumeSlider";
			this.Size = new System.Drawing.Size(190, 26);
			((System.ComponentModel.ISupportInitialize)(this.volumeTrackBar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.CheckBox unmutedCheckbox;
		private System.Windows.Forms.TrackBar volumeTrackBar;
	}
}
