using System;

namespace Solinosis.Common.Messaging
{
	[Serializable]
	public class Request
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }
	}
}