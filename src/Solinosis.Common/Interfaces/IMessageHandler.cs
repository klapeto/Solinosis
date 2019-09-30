using Solinosis.Common.Messaging;

namespace Solinosis.Common.Interfaces
{
	public interface IMessageHandler
	{
		Response SendMessage(Message message);
		void SendMessageAndForget(Message message);
	}
}