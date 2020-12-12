using PipServices3.Commons.Config;
using PipServices3.Commons.Random;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.DataDog.Clients
{
	[Collection("Sequential")]
	public class DataDogMetricClientTest : IDisposable
	{
		private DataDogMetricsClient _client;

		public DataDogMetricClientTest()
		{
			var apiKey = Environment.GetEnvironmentVariable("DATADOG_API_KEY") ?? "3eb3355caf628d4689a72084425177ac";

			_client = new DataDogMetricsClient();

			var config = ConfigParams.FromTuples(
				"source", "test",
				"credential.access_key", apiKey
			);
			_client.Configure(config);

			_client.OpenAsync(null).Wait();
		}

		public void Dispose()
		{
			_client.CloseAsync(null);
		}

		[Fact]
		public async Task SendMetricsAsync()
		{
			var metrics = new List<DataDogMetric>
			{
				new DataDogMetric
				{
					Metric = "test.metric.1",
					Service = "TestService",
					Host = "TestHost",
					Type = DataDogMetricType.Gauge,
					Points = new List<DataDogMetricPoint>
					{
						new DataDogMetricPoint
						{
							Time = DateTime.UtcNow,
							Value = RandomDouble.NextDouble(0, 100)
						}
					}
				},
				new DataDogMetric
				{
					Metric = "test.metric.2",
					Service = "TestService",
					Host = "TestHost",
					Type = DataDogMetricType.Rate,
					Interval = 100,
					Points = new List<DataDogMetricPoint>
					{
						new DataDogMetricPoint
						{
							Time = DateTime.UtcNow,
							Value = RandomDouble.NextDouble(0, 100)
						}
					}
				},
				new DataDogMetric
				{
					Metric = "test.metric.3",
					Service = "TestService",
					Host = "TestHost",
					Type = DataDogMetricType.Count,
					Interval = 100,
					Points = new List<DataDogMetricPoint>
					{
						new DataDogMetricPoint
						{
							Time = DateTime.UtcNow,
							Value = RandomDouble.NextDouble(0, 100)
						}
					}
				}
			};

			await _client.SendMetricsAsync(null, metrics);
		}
	}
}
