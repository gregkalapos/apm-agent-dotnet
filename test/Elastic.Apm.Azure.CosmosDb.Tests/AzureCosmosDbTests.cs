using System;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Logging;
using Elastic.Apm.Tests.Utilities;
using Elastic.Apm.Tests.Utilities.Azure;
using Elastic.Apm.Tests.Utilities.XUnit;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Abstractions;

namespace Elastic.Apm.Azure.CosmosDb.Tests
{
	[Collection("AzureCosmosDb")]
	public class AzureCosmosDbTests
	{
		private readonly AzureCosmosDbTestEnvironment _environment;
		private readonly ITestOutputHelper _output;
		private readonly MockPayloadSender _sender;
		private readonly ApmAgent _agent;
		private readonly CosmosClient _client;

		public AzureCosmosDbTests(AzureCosmosDbTestEnvironment environment, ITestOutputHelper output)
		{
			_environment = environment;
			_output = output;

			var logger = new XUnitLogger(LogLevel.Trace, output);
			_sender = new MockPayloadSender(logger);
			_agent = new ApmAgent(new TestAgentComponents(logger: logger, payloadSender: _sender));
			_agent.Subscribe(new HttpDiagnosticsSubscriber());
			_client = new CosmosClient(_environment.Endpoint, _environment.PrimaryMasterKey);
		}

		[AzureCredentialsFact]
		public async Task Capture_Span_When_Create_Database()
		{
			var databaseName = Guid.NewGuid().ToString();
			await _agent.Tracer.CaptureTransaction("Create CosmosDb Database", ApiConstants.TypeDb, async () =>
			{
				var response = await _client.CreateDatabaseAsync(databaseName);
			});

			AssertSpan("Create database");
		}

		private void AssertSpan(string action)
		{
			if (!_sender.WaitForSpans())
				throw new Exception("No span received in timeout");

			_sender.Spans.Should().HaveCount(1);
			var span = _sender.FirstSpan;

			span.Name.Should().Be($"Cosmos DB {action}");
			span.Type.Should().Be(ApiConstants.TypeDb);
			span.Subtype.Should().Be(ApiConstants.SubTypeCosmosDb);
		}
	}
}
