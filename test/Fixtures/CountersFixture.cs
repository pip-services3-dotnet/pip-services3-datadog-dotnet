using PipServices3.Components.Count;
using System.Threading.Tasks;
using Xunit;

namespace PipServices3.DataDog.Fixtures
{
	public class CountersFixture
	{
		private CachedCounters _counters;

		public CountersFixture(CachedCounters counters)
		{
			_counters = counters;
		}

		public async Task TestSimpleCountersAsync()
		{
            _counters.Last("Test.LastValue", 123);
            _counters.Last("Test.LastValue", 123456);

            var counter = _counters.Get("Test.LastValue", CounterType.LastValue);
            Assert.NotNull(counter);
            Assert.NotNull(counter.Last);
            Assert.Equal(123456.0, counter.Last.Value, 3);

            _counters.IncrementOne("Test.Increment");
            _counters.Increment("Test.Increment", 3);

            counter = _counters.Get("Test.Increment", CounterType.Increment);
            Assert.NotNull(counter);
            Assert.Equal(4, counter.Count);

            _counters.TimestampNow("Test.Timestamp");
            _counters.TimestampNow("Test.Timestamp");

            counter = _counters.Get("Test.Timestamp", CounterType.Timestamp);
            Assert.NotNull(counter);
            Assert.NotNull(counter.Time);

            _counters.Stats("Test.Statistics", 1);
            _counters.Stats("Test.Statistics", 2);
            _counters.Stats("Test.Statistics", 3);

            counter = _counters.Get("Test.Statistics", CounterType.Statistics);
            Assert.NotNull(counter);
            Assert.NotNull(counter.Average);
            Assert.Equal(2, counter.Average.Value, 3);

            _counters.Dump();

            await Task.Delay(1000);
        }

        public async Task TestMeasureElapsedTimeAsync()
		{
            var timer = _counters.BeginTiming("Test.Elapsed");

            await Task.Delay(100);

            timer.EndTiming();

            var counter = _counters.Get("Test.Elapsed", CounterType.Interval);
            Assert.True(counter.Last > 50);
            Assert.True(counter.Last < 5000);

            _counters.Dump();
        }

	}
}
