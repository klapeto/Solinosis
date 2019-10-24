using Solinosis.Common.Messaging;

namespace Solinosis.Common.Interfaces
{
	public interface ICallContext
	{
		ClientInfo CallerInfo { get; }
		ClientInfo HandlerInfo { get; }

		T GetCallbackChannel<T>();
	}
}