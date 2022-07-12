using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.Config;
using Elastic.Apm.Extensions.Hosting;
using Elastic.Apm.Extensions.Hosting.Config;
using Elastic.Apm.Logging;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;

namespace Elastic.Apm.Maui
{
	public static class ElasticApmMauiExtension
	{
		/// <summary>
		/// Enables the Elastic APM .NET Agent
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <returns>The <paramref name="builder"/>.</returns>
		public static MauiAppBuilder UseElasticApm(this MauiAppBuilder builder)
		{
			var services = builder.Services;

			services.AddSingleton<IApmLogger, NetCoreLogger>();
			
			services.AddSingleton<IConfigurationReader>(sp =>
				new MicrosoftExtensionsConfig(builder.Configuration, sp.GetService<IApmLogger>(), "TODO"));


			services.AddSingleton<IMauiInitializeService, ElasticApmInitializer>();
			services.AddSingleton<MauiEventsBinder>();
			return builder;
		}
	}
}
