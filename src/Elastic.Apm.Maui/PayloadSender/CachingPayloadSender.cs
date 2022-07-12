using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Elastic.Apm.BackendComm;
using Elastic.Apm.Config;
using Elastic.Apm.Helpers;
using Elastic.Apm.Logging;
using Elastic.Apm.Model;
using Elastic.Apm.Report;
using Elastic.Apm.Report.Serialization;
using Elastic.Apm.ServerInfo;

namespace Elastic.Apm.Maui.PayloadSender
{
	internal class CachingPayloadSender : BackendCommComponentBase, IPayloadSender
	{
		private readonly PayloadItemSerializer _payloadItemSerializer;
		private readonly IApmLogger _logger;
		private string _cachedMetadataJsonLine;
		private readonly Metadata _metadata;
		private TaskCompletionSource _taskCompletionSource;
		private readonly object _lock = new object();
		private readonly Uri _intakeV2EventsAbsoluteUrl;

		private readonly List<string> _serializedEvents;

		private static readonly UTF8Encoding Utf8Encoding;

		static CachingPayloadSender() => Utf8Encoding = new UTF8Encoding(false);


		public CachingPayloadSender(IApmLogger logger,
			IConfiguration configuration,
			Service service,
			Api.System system,
			//IApmServerInfo apmServerInfo,
			string dbgName = null,
			bool isEnabled = true,
			IEnvironmentVariables environmentVariables = null,
			Action<bool, IApmServerInfo> serverInfoCallback = null) : base(isEnabled, logger,
				nameof(CachingPayloadSender), service, configuration)
		{
			_logger = logger;
			_payloadItemSerializer = new PayloadItemSerializer();
			_serializedEvents = new List<string>();

			_metadata = new Metadata { Service = service, System = system };
			_intakeV2EventsAbsoluteUrl = BackendCommUtils.ApmServerEndpoints.BuildIntakeV2EventsAbsoluteUrl(configuration.ServerUrl);

			_taskCompletionSource = new TaskCompletionSource();
			StartWorkLoop();
		}

		public void QueueError(IError error) => throw new NotImplementedException();
		public void QueueMetrics(IMetricSet metrics) => throw new NotImplementedException();
		public void QueueSpan(ISpan span) => throw new NotImplementedException();
		public void QueueTransaction(ITransaction transaction) => Serialize(transaction, "transaction");

		private void Serialize(object item, string eventType)
		{
			var writer = new StringWriter();
			writer.Write("{\"");
			writer.Write(eventType);
			writer.Write("\":");
			var traceLogger = _logger?.IfLevel(LogLevel.Trace);
			if (traceLogger.HasValue)
			{
				var serialized = _payloadItemSerializer.Serialize(item);
				writer.Write(serialized);
				traceLogger.Value.Log("Serialized item to send: {ItemToSend} as {SerializedItem}", item, serialized);
			}
			else
				_payloadItemSerializer.Serialize(item, writer);

			writer.Write("}\n");

			var serializedEvent = writer.ToString();

			lock (_lock)
			{
				_serializedEvents.Add(serializedEvent);
				_taskCompletionSource.TrySetResult();
			}
		}

		protected override async Task WorkLoopIteration()
		{
			string item = null;
			await _taskCompletionSource.Task;
			lock (_lock)
			{
				_taskCompletionSource = new TaskCompletionSource();
				item = _serializedEvents[0];
				_serializedEvents.RemoveAt(0);
			}

			_cachedMetadataJsonLine ??= SerializeMetadata();


			//TODO make static
			var mediaTypeHeaderValue = new MediaTypeHeaderValue("application/x-ndjson")
			{
				CharSet = Utf8Encoding.WebName
			};

			using (var content = new StringContent(_cachedMetadataJsonLine + "\n" + item))
			{
				content.Headers.ContentType = mediaTypeHeaderValue;

				var response = await HttpClient.PostAsync(_intakeV2EventsAbsoluteUrl, content, CancellationTokenSource.Token);

			}

			string SerializeMetadata()
			{
				var stringBuilder = new StringBuilder();

				stringBuilder.Append("{\"metadata\":");
				stringBuilder.Append(_payloadItemSerializer.Serialize(_metadata));
				stringBuilder.Append("}\n");

				return stringBuilder.ToString();
			}

		}
	}
}
