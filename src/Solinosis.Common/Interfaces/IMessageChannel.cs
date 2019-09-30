using System;
using System.Threading.Tasks;
using Solinosis.Common.Messaging;

namespace Solinosis.Common.Interfaces
{
	public interface IMessageChannel
	{
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		void Broadcast(Message message);
		Task BroadcastAsync(Message message);
	}
}