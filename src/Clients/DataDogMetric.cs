using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PipServices3.DataDog.Clients
{
	[DataContract]
	public class DataDogMetric
	{
		[DataMember(Name = "metric")]
		public string Metric { get; set; }
		[DataMember(Name = "service")]
		public string Service { get; set; }
		[DataMember(Name = "host")]
		public string Host { get; set; }
		[DataMember(Name = "tags")]
		public Dictionary<string, string> Tags { get; set; }
		[DataMember(Name = "type")]
		public string Type { get; set; }
		[DataMember(Name = "interval")]
		public int? Interval { get; set; }
		[DataMember(Name = "points")]
		public List<DataDogMetricPoint> Points { get; set; }

	}
}
