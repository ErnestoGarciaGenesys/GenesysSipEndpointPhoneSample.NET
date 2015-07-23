using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SIP_Phone_Sample
{
	public abstract partial class VolumeSlider : UserControl
	{
		public VolumeSlider()
		{
			InitializeComponent();
			unmutedCheckbox.CheckedChanged += new EventHandler(OnMutedChanged);
			volumeTrackBar.ValueChanged += new EventHandler(OnVolumeChanged);
		}

		protected abstract bool GetMuted();
		
		protected abstract void SetMuted(bool muted);
		
		protected abstract int GetVolume();
		
		protected abstract void SetVolume(int volume);

		void OnMutedChanged(object sender, EventArgs e)
		{
			SetMuted(!unmutedCheckbox.Checked);
		}
		
		void OnVolumeChanged(object sender, EventArgs e)
		{
			SetVolume(volumeTrackBar.Value);
		}

		public void RefreshAudioState()
		{
			unmutedCheckbox.Checked = !GetMuted();
			volumeTrackBar.Value = GetVolume();
		}
		
		public override string Text
		{
			get { return unmutedCheckbox.Text; }
			set { unmutedCheckbox.Text = value; }
		}
		
		public override Color BackColor 
		{
			get { return base.BackColor; }
			set
			{
				base.BackColor = value;
				volumeTrackBar.BackColor = value;
			}
		}
	}
}
