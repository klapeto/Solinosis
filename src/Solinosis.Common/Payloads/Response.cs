using System;

namespace Solinosis.Common.Payloads
{
	[Serializable]
	public class Response
	{
		public Guid RequestId { get; set; }
	}
}