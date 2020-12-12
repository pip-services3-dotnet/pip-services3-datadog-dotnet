using PipServices3.Commons.Config;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Data;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Components.Auth;
using PipServices3.Rpc.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PipServices3.DataDog.Clients
{
	public class DataDogMetricsClient: RestClient
	{
		private ConfigParams _defaultConfig = ConfigParams.FromTuples(
			"connection.protocol", "https",
			"connection.host", "api.datadoghq.com",
			"connection.port", 443,
			"credential.internal_network", "true"
		);

		private CredentialResolver _credentialResolver = new CredentialResolver();

		public DataDogMetricsClient(ConfigParams config = null)
			: base()
		{
			if (config != null) Configure(config);
			_baseRoute = "api/v1";
		}

		public override void Configure(ConfigParams config)
		{
			config = _defaultConfig.Override(config);
			base.Configure(config);
			_credentialResolver.Configure(config);
		}

		public override void SetReferences(IReferences references)
		{
			base.SetReferences(references);
			_credentialResolver.SetReferences(references);
		}

		public override async Task OpenAsync(string correlationId)
		{
			var credential = await _credentialResolver.LookupAsync(correlationId);

			if (credential == null || credential.AccessKey == null)
			{
				throw new ConfigException(correlationId, "NO_ACCESS_KEY", "Missing access key in credentials");
			}

			await base.OpenAsync(correlationId);

			_client.DefaultRequestHeaders.Add("DD-API-KEY", credential.AccessKey);
		}

		public async Task SendMetricsAsync(string correlationId, IEnumerable<DataDogMetric> metrics)
		{
			var data = ConvertMetrics(metrics);

			// Commented instrumentation because otherwise it will never stop sending logs...
			//let timing = this.instrument(correlationId, "datadog.send_metrics");
			try
			{
				await ExecuteAsync<object>(correlationId, HttpMethod.Post, "series", data);
			}
			catch (Exception ex)
			{
				//timing.endTiming();
				InstrumentError(correlationId, "datadog.send_metrics", ex, true);
			}
		}

		private dynamic ConvertMetrics(IEnumerable<DataDogMetric> metrics)
		{
			return new
			{
				series = metrics.Select(m => ConvertMetric(m)).ToArray()
			};
		}

		private Dictionary<string, object> ConvertMetric(DataDogMetric metric)
		{
			var tags = metric.Tags;
			if (metric.Service != null)
			{
				tags = tags ?? new Dictionary<string, string>();
				tags["service"] = metric.Service;
			}

			var result = new Dictionary<string, object>
			{
				{ "metric", metric.Metric },
				{ "type", metric.Type ?? "gauge" },
				{ "points", ConvertPoints(metric.Points) }
			};

			if (tags != null)
				result["tags"] = ConvertTags(tags);
			if (metric.Host != null)
				result["host"] = metric.Host;
			if (metric.Interval != null)
				result["interval"] = metric.Interval.Value;

			return result;
		}

		private List<string[]> ConvertPoints(List<DataDogMetricPoint> points)
		{
			return points.Select(p =>
			{
				var time = (p.Time ?? DateTime.UtcNow).Subtract(DateTime.MinValue).TotalSeconds;
				return new[]
				{
					StringConverter.ToString(time),
					StringConverter.ToString(p.Value)
				};
			}).ToList();
		}

		private string ConvertTags(IDictionary<string, string> tags)
		{
			if (tags == null) return null;
			return string.Join(",", tags.Select(x => string.Format($"{x.Key}:{x.Value}")));
        }
	}
}
