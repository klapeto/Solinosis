using System;

namespace Solinosis.Common.Payloads
{
	[Serializable]
	public class IpcRequest
	{
		public string ContractName { get; set; }
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }
	}
}