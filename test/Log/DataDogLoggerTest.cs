using PipServices3.Commons.Config;
using PipServices3.DataDog.Log;
using PipServices3.DataDog.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.DataDog.Log
{
	[Collection("Sequential")]
	public class DataDogLoggerTest : IDisposable
	{
		private DataDogLogger _logger;
		private LoggerFixture _fixture;

		public DataDogLoggerTest()
		{
			var apiKey = Environment.GetEnvironmentVariable("DATADOG_API_KEY") ?? "3eb3355caf628d4689a72084425177ac";

			_logger = new DataDogLogger();
			_fixture = new LoggerFixture(_logger);

			var config = ConfigParams.FromTuples(
				"source", "test",
				"credential.access_key", apiKey
			);
			_logger.Configure(config);

			_logger.OpenAsync(null).Wait();
		}

		public void Dispose()
		{
			_logger.CloseAsync(null).Wait();
		}

		[Fact]
		public void LogLevelAsync()
		{
			_fixture.TestLogLevel();
		}

		[Fact]
		public async Task SimpleLoggingAsync()
		{
			await _fixture.TestSimpleLoggingAsync();
		}

		[Fact]
		public async Task ErrorLoggingAsync()
		{
			await _fixture.TestErrorLoggingAsync();
		}
	}
}
