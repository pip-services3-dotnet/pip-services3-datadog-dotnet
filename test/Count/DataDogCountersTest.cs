using PipServices3.Commons.Config;
using PipServices3.DataDog.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.DataDog.Count
{
	[Collection("Sequential")]
	public class DataDogCountersTest : IDisposable
	{
		private DataDogCounters _counters;
		private CountersFixture _fixture;

		public DataDogCountersTest()
		{
			var apiKey = Environment.GetEnvironmentVariable("DATADOG_API_KEY") ?? "3eb3355caf628d4689a72084425177ac";

			_counters = new DataDogCounters();
			_fixture = new CountersFixture(_counters);

			var config = ConfigParams.FromTuples(
				"source", "test",
				"credential.access_key", apiKey
			);
			_counters.Configure(config);

			_counters.OpenAsync(null).Wait();
		}

		public void Dispose()
		{
			_counters.CloseAsync(null).Wait();
		}

		[Fact]
		public async Task SimpleCountersAsync()
		{
			await _fixture.TestSimpleCountersAsync();
		}

		[Fact]
		public async Task MeasureElapsedTimeAsync()
		{
			await _fixture.TestMeasureElapsedTimeAsync();
		}
	}
}
