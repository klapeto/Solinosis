using System;

namespace Solinosis.Common.Messaging
{
	[Serializable]
	public class Response
	{
		public Guid RequestId { get; set; }
		public bool IsError { get; set; }
		public object Payload { get; set; }
	}
}