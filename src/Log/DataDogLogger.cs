using PipServices3.Commons.Config;
using PipServices3.Commons.Refer;
using PipServices3.Commons.Run;
using PipServices3.Components.Info;
using PipServices3.Components.Log;
using PipServices3.DataDog.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PipServices3.DataDog.Log
{
	/// <summary>
	/// Logger that dumps execution logs to DataDog service.
	/// 
	/// DataDog is a popular monitoring SaaS service. It collects logs, metrics, events
	/// from infrastructure and applications and analyze them in a single place.
	/// 
	/// ### Configuration parameters ###
	/// 
	/// - level:             maximum log level to capture
	/// - source:            source (context) name
	/// - connection:
	///     - discovery_key:         (optional) a key to retrieve the connection from [[https://pip-services3-node.github.io/pip-services3-components-node/interfaces/connect.idiscovery.html IDiscovery]]
	///     - protocol:              (optional) connection protocol: http or https (default: https)
	///     - host:                  (optional) host name or IP address (default: http-intake.logs.datadoghq.com)
	///     - port:                  (optional) port number (default: 443)
	///     - uri:                   (optional) resource URI or connection string with all parameters in it
	/// - credential:
	///     - access_key:      DataDog client api key
	/// - options:
	///     - interval:        interval in milliseconds to save log messages (default: 10 seconds)
	///     - max_cache_size:  maximum number of messages stored in this cache (default: 100)
	///     - reconnect:       reconnect timeout in milliseconds (default: 60 sec)
	///     - timeout:         invocation timeout in milliseconds (default: 30 sec)
	///     - max_retries:     maximum number of retries (default: 3)
	/// 
	/// ### References ###
	/// 
	/// - <code>\*:context-info:\*:\*:1.0</code>      (optional) [[https://pip-services3-node.github.io/pip-services3-components-node/classes/info.contextinfo.html ContextInfo]] to detect the context id and specify counters source
	/// - <code>\*:discovery:\*:\*:1.0</code>         (optional) [[https://pip-services3-node.github.io/pip-services3-components-node/interfaces/connect.idiscovery.html IDiscovery]] services to resolve connection
	/// </summary>
	/// <example>
	/// <code>
	/// let logger = new DataDogLogger();
	/// logger.Configure(ConfigParams.FromTuples(
	///     "credential.access_key", "827349874395872349875493"
	/// ));
	/// 
	/// await logger.OpenAsync("123", (err) => {
	///     ...
	/// });
	/// 
	/// logger.Error("123", ex, "Error occured: {0}", ex.message);
	/// logger.Debug("123", "Everything is OK.");
	/// </code>
	/// </example>
	public class DataDogLogger: CachedLogger, IReferenceable, IOpenable
	{
		private DataDogLogClient _client = new DataDogLogClient();
		private FixedRateTimer _timer;
		private string _instance = Dns.GetHostName();

		/// <summary>
		/// Creates a new instance of the logger.
		/// </summary>
		public DataDogLogger()
			: base()
		{ 
		}

		/// <summary>
		/// Configures component by passing configuration parameters.
		/// </summary>
		/// <param name="config">configuration parameters to be set.</param>
		public override void Configure(ConfigParams config)
		{
			base.Configure(config);

			_client.Configure(config);
			_instance = config.GetAsStringWithDefault("instance", _instance);
		}

		/// <summary>
		/// Sets references to dependent components.
		/// </summary>
		/// <param name="references">references to locate the component dependencies.</param>
		public override void SetReferences(IReferences references)
		{
			base.SetReferences(references);

			_client.SetReferences(references);

			var contextInfo = references.GetOneOptional<ContextInfo>(
				new Descriptor("pip-services", "context-info", "default", "*", "1.0"));
			if (contextInfo != null && _source == null)
				_source = contextInfo.Name;
			if (contextInfo != null && _instance == null)
				_instance = contextInfo.ContextId;
		}

		/// <summary>
		/// Checks if the component is opened.
		/// </summary>
		/// <returns>true if the component has been opened and false otherwise.</returns>
		public bool IsOpen()
		{
			return _timer != null;
		}

		/// <summary>
		/// Opens the component.
		/// </summary>
		/// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
		public async Task OpenAsync(string correlationId)
		{
			if (IsOpen())
			{
				return;
			}

			await _client.OpenAsync(correlationId);

			_timer = new FixedRateTimer(() => Dump(), _interval, 0);
			_timer.Start();
		}

		/// <summary>
		/// Closes component and frees used resources.
		/// </summary>
		/// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
		public async Task CloseAsync(string correlationId)
		{
			Save(_cache);

			_cache = new List<LogMessage>();

			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
			}

			await _client.CloseAsync(correlationId);
		}

		private DataDogLogMessage ConvertMessage(LogMessage message)
		{
			var result = new DataDogLogMessage
			{
				Time = message.Time,
				Tags = new Dictionary<string, string>
				{
					{ "correlation_id", message.CorrelationId }
				},
				Host = _instance,
				Service = message.Source ?? _source,
				Status = message.Level,
				Message = message.Message
			};

			if (message.Error != null)
			{
				result.ErrorKind = message.Error.Type;
				result.ErrorMessage = message.Error.Message;
				result.ErrorStack = message.Error.StackTrace;
			}

			return result;
		}

		/// <summary>
		/// Saves log messages from the cache.
		/// </summary>
		/// <param name="messages">a list with log messages</param>
		protected override void Save(List<LogMessage> messages)
		{
			if (!IsOpen() || messages.Count == 0)
			{
				return;
			}

			var data = messages.Select(m => ConvertMessage(m)).ToList();

			_client.SendLogsAsync("datadog-logger", data).Wait();
		}
	}
}
