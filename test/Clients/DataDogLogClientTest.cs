using PipServices3.Commons.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.DataDog.Clients
{
	[Collection("Sequential")]
	public class DataDogLogClientTest : IDisposable
	{
		private DataDogLogClient _client;

		public DataDogLogClientTest()
		{
			var apiKey = Environment.GetEnvironmentVariable("DATADOG_API_KEY") ?? "3eb3355caf628d4689a72084425177ac";

			_client = new DataDogLogClient();

			var config = ConfigParams.FromTuples(
				"source", "test",
				"credential.access_key", apiKey
			);
			_client.Configure(config);

			_client.OpenAsync(null).Wait();
		}

		public void Dispose()
		{
			_client.CloseAsync(null).Wait();
		}

		[Fact]
		public async Task SendLogsAsync()
		{
			var messages = new List<DataDogLogMessage>
			{
				new DataDogLogMessage
				{
					Time = DateTime.UtcNow,
					Service = "TestService",
					Host = "TestHost",
					Status = DataDogStatus.Debug,
					Message = "Test trace message"
				},
				new DataDogLogMessage
				{
					Time = DateTime.UtcNow,
					Service = "TestService",
					Host = "TestHost",
					Status = DataDogStatus.Info,
					Message = "Test info message"
				},
				new DataDogLogMessage
				{
					Time = DateTime.UtcNow,
					Service = "TestService",
					Host = "TestHost",
					Status = DataDogStatus.Error,
					Message = "Test error message",
					ErrorKind = "Exception",
					ErrorStack = "Stack trace..."
				},
				new DataDogLogMessage
				{
					Time = DateTime.UtcNow,
					Service = "TestService",
					Host = "TestHost",
					Status = DataDogStatus.Emergency,
					Message = "Test fatal message",
					ErrorKind = "Exception",
					ErrorStack = "Stack trace..."
				},
			};

			await _client.SendLogsAsync(null, messages);
		}
	}
}
