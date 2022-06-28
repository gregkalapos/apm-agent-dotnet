using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.DiagnosticSource;

namespace Elastic.Apm.Maui
{
	internal class ElasticApmInitializer : IMauiInitializeService
	{
		public void Initialize(IServiceProvider services)
		{
			Agent.Setup(new AgentComponents());
			Agent.Subscribe(new HttpDiagnosticsSubscriber());
			// Bind MAUI events
			var binder = services.GetRequiredService<MauiEventsBinder>();
			binder.BindEvents();

		}
	}
}
