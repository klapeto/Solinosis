using System;
using System.Threading.Tasks;
using Solinosis.Common.Interfaces;

namespace Solinosis.Common.Messaging
{
	public class InProcessMessageChannel: IMessageChannel
	{
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Broadcast(Message message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
		}

		public Task BroadcastAsync(Message message)
		{
			return Task.Run(() => Broadcast(message));
		}
	}
}