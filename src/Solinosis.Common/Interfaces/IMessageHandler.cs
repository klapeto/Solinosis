using Solinosis.Common.Messaging;
using Solinosis.Common.Payloads;

namespace Solinosis.Common.Interfaces
{
	public interface IMessageHandler
	{
		IpcResponse SendMessage(Message message);
		void SendMessageAndForget(Message message);
	}
}