using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;

namespace Solinosis.Common
{
	public class CallContext: ICallContext
	{
		public ClientInfo CallerInfo { get; internal set; }
	}
}