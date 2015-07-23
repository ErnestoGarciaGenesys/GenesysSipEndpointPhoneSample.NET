using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Logging;
using Genesyslab.Platform.Logging.Configuration;
using Genesyslab.Sip.Endpoint;
using Genesyslab.Sip.Endpoint.Configuration;
using Genesyslab.Sip.Endpoint.Provider;

namespace SIP_Phone_Sample
{
	public partial class MainForm : Form
	{
		IEndpoint endpoint;
		IExtendedService endpointExtendedService;
		ILogger logger;
		
		public MainForm()
		{
			InitializeComponent();
		}
				
		void MainFormLoad(object sender, EventArgs eventArgs)
		{
			//var rootLogger = CreateLogger();
			//logger = rootLogger.CreateChildLogger(typeof(MainForm).Name);
			//var settingsXml = XDocument.Load("SipEndpoint.config").Root;
			//var settingsArray = GetSettingsToCreateEndpoint(settingsXml);
			
			//if (settingsArray == null)
			//{
			//    endpoint = EndpointFactory.CreateSipEndpoint(rootLogger);
			//}
			//else
			//{
			//    endpoint = EndpointFactory.CreateSipEndpoint(rootLogger, settingsArray);
			//}

			endpoint = SipEndpointConfig.CreateEndpoint();
            
			endpoint.StatusChanged += MakeGuiInvoked<EndpointEventArgs>(OnEndpointStatusChanged);
            endpoint.SessionManager.SessionStatusChanged += MakeGuiInvoked<SessionEventArgs>(OnSessionStatusChanged);
            endpoint.ConnectionManager.ConnectionStatusChanged += MakeGuiInvoked<ConnectionEventArgs>(OnConnectionStatusChanged);
            //endpoint.ApplyConfiguration(settingsXml);
            
            endpointExtendedService = endpoint.Resolve("IExtendedService") as IExtendedService;
            if (endpointExtendedService != null)
            	AddVolumeSliders();
            
            RefreshStatus();
            
            endpoint.BeginActivate();
		}
		
		EventHandler<E> MakeGuiInvoked<E>(EventHandler<E> handler) where E: EventArgs
		{
			return (s, e) => { BeginInvoke(handler, s, e); };
		}
		
		abstract class EndpointVolumeSlider : VolumeSlider
		{
			protected readonly IExtendedService ext;
			public EndpointVolumeSlider(IExtendedService ext)
			{
				this.ext = ext;
			}
		}
		
		class SpeakerVolumeSlider : EndpointVolumeSlider
		{
			public SpeakerVolumeSlider(IExtendedService ext) : base(ext) {}
			protected override int GetVolume() { return ext.GetSpeakerVolume(); }
			protected override void SetVolume(int v) { ext.SetSpeakerVolume(v); }
			protected override bool GetMuted() { return ext.IsSpeakerMuted(); }
			protected override void SetMuted(bool m) { ext.SpeakerMute(m); }
		}
		
		class MicVolumeSlider : EndpointVolumeSlider
		{
			public MicVolumeSlider(IExtendedService ext) : base(ext) {}
			protected override int GetVolume() { return ext.GetMicVolume(); }
			protected override void SetVolume(int v) { ext.SetMicVolume(v); }
			protected override bool GetMuted() { return ext.IsMicMuted(); }
			protected override void SetMuted(bool m) { ext.MicMute(m); }
		}
		
		void AddVolumeSliders()
		{
			contextMenu.RenderMode = ToolStripRenderMode.Professional;
			var renderer = (ToolStripProfessionalRenderer)contextMenu.Renderer;
			var backColor = renderer.ColorTable.ToolStripDropDownBackground;
			
			var speakerVolume = new SpeakerVolumeSlider(endpointExtendedService);
			speakerVolume.Text = "Speaker";
			speakerVolume.BackColor = backColor;
			contextMenu.Items.Insert(0, new ToolStripControlHost(speakerVolume));
			
			var microphoneVolume = new MicVolumeSlider(endpointExtendedService);
			microphoneVolume.Text = "Microphone";
			microphoneVolume.BackColor = backColor;
			contextMenu.Items.Insert(1, new ToolStripControlHost(microphoneVolume));
			
			contextMenu.Items.Insert(2, new ToolStripSeparator());

			contextMenu.Opening += (s, e) =>
			{
				speakerVolume.RefreshAudioState();
				microphoneVolume.RefreshAudioState();
			};
		}
		
		void MainFormFormClosed(object sender, FormClosedEventArgs e)
		{
			var d = (IDisposable)endpoint;
			d.Dispose();
		}
		
		void OnConnectionStatusChanged(object sender, ConnectionEventArgs e)
        {
			logger.DebugFormat("ConnectionStatusChanged: connection id = {0}, URI = {1}, status = {2}",
			                   e.Connection.Id, e.Connection.Server, e.Status);

			if (e.Properties.ContainsKey("Error"))
			{
				var error = e.Properties["Error"].ToString();
				
				// Alternatives for checking whether an error happenned:
				//  a. checking connection properties for an error message distinct from "No Error" 
				//  b. checking whether the connection status is Failed
				// Maybe both are effectively equivalent, but b. seems more general, so it is used here.
				if (error != "No Error")
				{
					logger.Error("ConnectionStatusChanged: error = " + error);
					notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
					notifyIcon.BalloonTipTitle = "SIP Phone";
					notifyIcon.BalloonTipText = string.Format("Connection {0}: {1}", e.Status, error);
					notifyIcon.ShowBalloonTip(3000);
				}
			}
        }

        void OnEndpointStatusChanged(object sender, EndpointEventArgs e)
        {
			logger.DebugFormat("EndpointStatusChanged: status = {0}", endpoint.Status);
			RefreshStatus();
        }

        void OnSessionStatusChanged(object sender, SessionEventArgs e)
        {
			logger.DebugFormat("SessionStatusChanged: session id = {0}, status = {1}",
        	                   e.Session.Id, e.Session.Status);
			RefreshStatus();
        }
        
        void RefreshStatus()
        {
        	SummarizedStatus aggregated;        	
        	if (endpoint.Status != EndpointStatus.Active)
        	{
        		aggregated = SummarizedStatus.Inactive;
        	}
        	else
        	{
        		aggregated = SummarizedStatus.NoSessions;        		
				foreach (var session in endpoint.SessionManager.Sessions)
				{
					var relevant = ToSummarizedSessionStatus(session.Status);
					if (aggregated < relevant)
						aggregated = relevant;
				}
        	}
        	
			notifyIcon.Icon = ToIcon(aggregated);
			notifyIcon.Text = "Phone: " + ToDisplayText(aggregated);
        }
		
        enum SummarizedStatus
        {
        	// Order is important, in the case of having several sessions,
        	// it reflects importance of status of each session
        	Inactive,
        	NoSessions,
        	Connected,
        	Alerting
        }
        
        SummarizedStatus ToSummarizedSessionStatus(SessionStatus sessionStatus)
        {
			switch (sessionStatus)
			{
				case SessionStatus.Unknown:
				case SessionStatus.Disconnected:
				case SessionStatus.InProgress:
					return SummarizedStatus.NoSessions;
					
				case SessionStatus.Connected:
					return SummarizedStatus.Connected;
					
				case SessionStatus.Alerting:
					return SummarizedStatus.Alerting;
				
				default:
					return SummarizedStatus.NoSessions;
			}
        }
        
        string ToDisplayText(SummarizedStatus status)
        {
			switch (status)
			{
				case SummarizedStatus.Inactive:
					return "Inactive";
					
				case SummarizedStatus.NoSessions:
					return "No calls";
					
				case SummarizedStatus.Connected:
					return "In a call";
					
				case SummarizedStatus.Alerting:
					return "Ringing";
				
				default:
					return "Unknown state";
			}
        }
        
        
        Icon ToIcon(SummarizedStatus status)
        {
			switch (status)
			{
				case SummarizedStatus.Inactive:
					return Icons.phone_gray;
					
				case SummarizedStatus.NoSessions:
					return Icons.phone;
					
				case SummarizedStatus.Connected:
					return Icons.arrows_meet;
					
				case SummarizedStatus.Alerting:
					return Icons.bell;
				
				default:
					return Icons.phone_gray;
			}
        }
		
		void ExitMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}

		// This method contents have been copied from the SIP Endpoint QuickStartWindowsForms sample application.
        ILogger CreateLogger()
		{
			var logConf = new LogConfiguration();
            logConf.Verbose = VerboseLevel.All;
            logConf.Targets.Trace.IsEnabled = true;
            logConf.Targets.Trace.Verbose = VerboseLevel.All;
            FileConfiguration fileConf = new FileConfiguration(true, VerboseLevel.All, "logs" + Path.DirectorySeparatorChar + "SipEndpoint.log");
            fileConf.MessageHeaderFormat = MessageHeaderFormat.Medium;
            logConf.Targets.Files.Add(fileConf);
            return LoggerFactory.CreateRootLogger("SipEndpoint", logConf);
		}
		
        // This method has been copied from the SIP Endpoint QuickStartWindowsForms sample application.
		string[,] GetSettingsToCreateEndpoint(XElement conf)
        {
            string[,] settings = new string[1000, 2];
            int i = 0;
            int c = 0;

            XElement settingsConf = EndpointConfiguration.GetContainer(conf, "Cp");
            if (!settingsConf.IsEmpty)
            {

                IEnumerable<XElement> settingElements = settingsConf.Descendants("{http://www.counterpath.com/cps}setting");

                foreach (XElement s in settingElements)
                {
                    bool isAccountSetting = s.Parent.Parent.Attribute(XName.Get("name")).Value.CompareTo("proxies") == 0;

                    String name;
                    String accountName;
                    int accountId;

                    if (isAccountSetting)
                    {
                        name = s.Attribute(XName.Get("name")).Value;
                        accountName = s.Parent.Attribute(XName.Get("name")).Value;
                        accountName = accountName.Substring(5);
                        accountId = Convert.ToInt32(accountName);
                    }
                    else
                    {
                        name = s.Parent.Parent.Attribute(XName.Get("name")).Value + ":" + s.Parent.Attribute(XName.Get("name")).Value + ":" + s.Attribute(XName.Get("name")).Value;
                    }

                    String value = s.Attribute(XName.Get("value")).Value;

                    if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(value))
                    {
                        // Populate array with initial settings that should be applied to the CP SDK just before phone is started 

                        if (isAccountSetting)
                        {
                            // Find and add Account settings to the initial settings array
                        }
                        else
                        {
                            // Find and add Phone settings to the initial settings array
                            if (name.Equals("codecs:g729:vad_enabled"))
                            {
                                settings[i, 0] = name;
                                settings[i, 1] = value;
                                i++;
                                c++;
                            }
                        }
                    }
                }
            }

            if (c > 0)
            {
                string[,] t = new string[c, 2];

                for (i = 0; i < c; i++)
                {
                    t[i, 0] = settings[i, 0];
                    t[i, 1] = settings[i, 1];
                }

                return t;
            }
            else
            {
                return null;
            }
        }
	}
}
