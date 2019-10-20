using System;
using System.Threading;
using System.Threading.Tasks;
using Solinosis.Common.Messaging;

namespace Solinosis.Common.Interfaces
{
	public interface IMessageChannel: IDisposable
	{
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		void Broadcast(Message message);
		Task BroadcastAsync(Message message, CancellationToken cancellationToken);

		void Connect();
		Task ConnectAsync(CancellationToken cancellationToken);
		void Disconnect();
		Task DisconnectAsync(CancellationToken cancellationToken);
	}
}