using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Helpers;
using Elastic.Apm.Logging;
using Elastic.Apm.Maui.PayloadSender;
using Elastic.Apm.Report;
using Elastic.Apm.ServerInfo;
using Microsoft.Extensions.Logging;

namespace Elastic.Apm.Maui
{
	internal class ElasticApmInitializer : IMauiInitializeService
	{
		public void Initialize(IServiceProvider services)
		{
			//Environment.SetEnvironmentVariable("ELASTIC_APM_ENABLEOPENTELEMETRYBRIDGE", "true");
			//var components = new AgentComponents();

			var logger = services.GetService<IApmLogger>();
			var configReader = services.GetService<IConfigurationReader>();

			var systemInfoHelper = new SystemInfoHelper(logger);
			var system = systemInfoHelper.GetSystemInfo(configReader.HostName);

			var configurationStore = new ConfigurationStore(new ConfigurationSnapshotFromReader(configReader, "local"), logger);


			//var payloadSender = new CachingPayloadSender(logger, configurationStore.CurrentSnapshot,
			//	Service.GetDefaultService(configReader, logger),
			//	system);

			var payloadSender = new PayloadSenderV2(logger, configurationStore.CurrentSnapshot,
				Service.GetDefaultService(configReader, logger), system, new ApmServerInfo());

			Agent.Setup(new AgentComponents(payloadSender: payloadSender));

			Agent.Subscribe(new HttpDiagnosticsSubscriber());
			// Bind MAUI events
			var binder = services.GetRequiredService<MauiEventsBinder>();
			binder.BindEvents();
		}
	}
}
