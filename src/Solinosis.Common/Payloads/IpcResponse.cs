using System;

namespace Solinosis.Common.Payloads
{
	[Serializable]
	public class IpcResponse: Response
	{
		public bool IsError { get; set; }
		public object Payload { get; set; }
	}
}