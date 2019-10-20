using System;

namespace Solinosis.Common.Messaging
{
	[Serializable]
	public class ErrorPayload
	{
		public string Message { get; set; }
		public Exception Exception { get; set; }
	}
}