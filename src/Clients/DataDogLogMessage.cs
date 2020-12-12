using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PipServices3.DataDog.Clients
{
	[DataContract]
	public class DataDogLogMessage
	{
		[DataMember(Name = "time")]
		public DateTime? Time { get; set; }
		[DataMember(Name = "tags")]
		public Dictionary<string, string> Tags { get; set; }
		[DataMember(Name = "status")]
		public string Status { get; set; }
		[DataMember(Name = "source")]
		public string Source { get; set; }
		[DataMember(Name = "service")]
		public string Service { get; set; }
		[DataMember(Name = "host")]
		public string Host { get; set; }
		[DataMember(Name = "message")]
		public string Message { get; set; }
		[DataMember(Name = "logger_name")]
		public string LoggerName { get; set; }
		[DataMember(Name = "thread_name")]
		public string ThreadName { get; set; }
		[DataMember(Name = "error_message")]
		public string ErrorMessage { get; set; }
		[DataMember(Name = "error_kind")]
		public string ErrorKind { get; set; }
		[DataMember(Name = "error_stack")]
		public string ErrorStack { get; set; }
	}
}
