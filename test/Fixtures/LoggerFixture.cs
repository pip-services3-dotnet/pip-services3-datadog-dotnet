using PipServices3.Components.Log;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.DataDog.Fixtures
{
	public class LoggerFixture
	{
		private CachedLogger _logger;

		public LoggerFixture(CachedLogger logger)
		{
			_logger = logger;
		}

		public void TestLogLevel()
		{
			Assert.True(_logger.Level >= LogLevel.None);
			Assert.True(_logger.Level <= LogLevel.Trace);
		}

		public async Task TestSimpleLoggingAsync()
		{
			_logger.Level = LogLevel.Trace;
			_logger.Fatal("987", null, "Fatal error message");
			_logger.Error("987", null, "Error message");
			_logger.Warn("987", "Warning message");
			_logger.Info("987", "Information message");
			_logger.Debug("987", "Debug message");
			_logger.Trace("987", "Trace message");
			_logger.Dump();

			await Task.Delay(1000);
		}

		public async Task TestErrorLoggingAsync()
		{
			try
			{
				// Raise an exception
				throw new ApplicationException();
			}
			catch (Exception ex)
			{
				_logger.Fatal("123", ex, "Fatal error");
				_logger.Error("123", ex, "Recoverable error");

				Assert.NotNull(ex);
			}

			_logger.Dump();

			await Task.Delay(1000);
		}
	}
}
