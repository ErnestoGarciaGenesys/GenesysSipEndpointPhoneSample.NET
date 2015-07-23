using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Genesyslab.Platform.Logging.Configuration;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Logging;

using SipEndpoint = Genesyslab.Sip.Endpoint;
using SipProvider = Genesyslab.Sip.Endpoint.Provider.IEndpointProvider;
using Genesyslab.Sip.Endpoint.Configuration;
using System.IO;

namespace SIP_Phone_Sample
{
	// Takes care of the endpoint configuration.
	// Excerpts copied from the SIP Endpoint 8.1.2 QuickStartWinFormVS9 sample.
	class SipEndpointConfig
	{
		#region Copied from QuickStartWinFormVS9

		string loggerType = string.Empty;
		string logFile = "SipEndpoint.log";
		bool enableLogging = false;
		int logLevel = 0;

		NullLogger nullLogger;

		SipEndpoint.IEndpoint endpoint;
		ILogger Logger;
		ILogger endpointLogger;
		SipProvider provider;
		
		#endregion

		public static SipEndpoint.IEndpoint CreateEndpoint()
		{
			var instance = new SipEndpointConfig();
			instance.RunUpEndpoint();
			return instance.endpoint;
		}

		#region Copied from QuickStartWinFormVS9, except for excerpts marked with OMITTED comments

		void RunUpEndpoint()
		{
			// OMITTED: No empty try-catch
			//try
			//{
				//this.Logger.Debug("1 RunUpEndpoint()");

				// Get configuration settings
				XDocument confDoc = XDocument.Load("SipEndpoint.config");
				this.SetAppLevelConfiguration(confDoc.Root);

				/*  Matching levels between:
				 *  -----------------------------|--------------------------------
				 *   Platform SDK logging levels | SIP Endpoint SDK logging levels
				 *  -----------------------------|--------------------------------
				 *    All = 0                    |       NA
				 *    Debug = 1                  |       4
				 *    Trace = 2                  |       3
				 *    Interaction = 3            |       2
				 *    Standard = 4               |       1
				 *    Alarm = 5                  |       0
				 *    None = 6                   |       NA
				 *  -----------------------------|--------------------------------
				 * 
				 *  SIP Endpoint SDK logging levels
				 *  ===============================
				 *   Fatal = 0
				 *   Error = 1
				 *   Warning = 2
				 *   Info = 3
				 *   Debug = 4
				 */

				if (this.enableLogging)
				{
					// Setup logger
					LogConfiguration logConf = new LogConfiguration();
					logConf.Verbose = VerboseLevel.All;
					logConf.Targets.Trace.IsEnabled = true;

					switch (this.logLevel)
					{
						case 0:
							logConf.Targets.Trace.Verbose = VerboseLevel.Alarm;
							break;
						case 1:
							logConf.Targets.Trace.Verbose = VerboseLevel.Standard;
							break;
						case 2:
							logConf.Targets.Trace.Verbose = VerboseLevel.Interaction;
							break;
						case 3:
							logConf.Targets.Trace.Verbose = VerboseLevel.Trace;
							break;
						case 4:
							logConf.Targets.Trace.Verbose = VerboseLevel.Debug;
							break;
					}

					FileConfiguration fileConf = new FileConfiguration(true, logConf.Targets.Trace.Verbose, "logs" + Path.DirectorySeparatorChar + this.logFile);
					fileConf.MessageHeaderFormat = MessageHeaderFormat.Medium;

					logConf.Targets.Files.Add(fileConf);

					this.endpointLogger = LoggerFactory.CreateRootLogger("SipEndpoint", logConf);
					this.Logger = this.endpointLogger.CreateChildLogger("QuickStart");

					this.endpoint = SipEndpoint.EndpointFactory.CreateSipEndpoint(this.endpointLogger);
				}
				else
				{
					// Setup NULL logger
					this.nullLogger = new NullLogger();
					this.Logger = this.nullLogger;
					this.endpoint = SipEndpoint.EndpointFactory.CreateSipEndpoint();
				}

				// OMITTED: Endpoint configuration

				this.provider = this.endpoint.Resolve("IEndpointPovider") as SipProvider;

				// Setup Endpoint Logger
				if (this.enableLogging)
					this.provider.SetEndpointLogger(this.endpointLogger);
				else
					this.provider.SetEndpointLogger(this.nullLogger);

				this.endpoint.ApplyConfiguration(confDoc.Root);

				// OMITTED:
				//this.endpoint.BeginActivate();

				// At this point the actual configuration can be updated and
				// Then the Endpoint can be started
				// The next two lines of the code are unnecessary if configuration
				// is not going to be changed

				// Get updated configuration settings
				//confDoc = XDocument.Load("SipEndpoint.config");
				//this.endpoint.ApplyConfiguration(confDoc.Root);

				// The actual SIP Endpoint Start
				// OMITTED:
				//this.endpoint.Start();
				//this.Logger.Debug("this.endpoint.Start()");
			
			// OMITTED: No empty try-catch
			//}
			//catch (Exception exception)
			//{
			//	//this.Logger.Debug("2 RunUpEndpoint()" + exception.Message);
			//	//System.Windows.Forms.MessageBox.Show(exception.Message);
			//}
		}

	    void SetAppLevelConfiguration(XElement conf)
        {
			// OMITTED: No empty try-catch
			//try
            //{
                XElement settingsConf = EndpointConfiguration.GetContainer(conf, "Genesys");

                if (settingsConf != null)
                {
                    IEnumerable<XElement> settingElements = settingsConf.Descendants("{http://www.genesyslab.com/sip}setting");

                    foreach (XElement s in settingElements)
                    {
                        bool isSystemDiagnosticsSetting =
                            (s.Parent.Parent.Attribute(XName.Get("name")).Value + ":" + s.Parent.Attribute(XName.Get("name")).Value) == "system:diagnostics";

                        string name = s.Attribute(XName.Get("name")).Value;
                        string value = s.Attribute(XName.Get("value")).Value;

                        if (isSystemDiagnosticsSetting)
                        {
                            if (name.Equals("logger_type"))
                                this.loggerType = value;
                            else if (name.Equals("log_file"))
                                this.logFile = value;
                            else if (name.Equals("enable_logging"))
                            {
                                if (this.enableLogging = Convert.ToInt32(value) == 1)
                                    this.enableLogging = true;
                            }
                            else if (name.Equals("log_level"))
                                this.logLevel = Convert.ToInt32(value);
                        }
                    }
                }
            //}
            //catch (Exception exception)
            //{
            //    //this.Logger.Debug("2 SetAppLevelConfiguration(XElement conf)" + exception.Message);
            //    //System.Windows.Forms.MessageBox.Show(exception.Message);
            //}
		}

		#endregion

	}

}
