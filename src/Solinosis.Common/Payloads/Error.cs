using System;

namespace Solinosis.Common.Payloads
{
	[Serializable]
	public class Error: Response
	{
		public string Message { get; set; }
		public Exception Exception { get; set; }
	}
}