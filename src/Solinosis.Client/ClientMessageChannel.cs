using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;

namespace Solinosis.Client
{
	public class ClientMessageChannel : IMessageChannel
	{
		private readonly ILogger<ClientMessageChannel> _logger;
		private Thread _incomingMessagesThread;
		private CancellationTokenSource _cancellationTokenSource;
		private readonly NamedPipeClient _client;

		public ClientMessageChannel(NamedPipeClient client, ILogger<ClientMessageChannel> logger)
		{
			_client = client;
			_logger = logger;
		}

		public void Dispose()
		{
			_cancellationTokenSource?.Cancel();
			_incomingMessagesThread?.Join();
			_cancellationTokenSource?.Dispose();
			_client.Disconnect();
			_client.Dispose();
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Broadcast(Message message)
		{
			_client.SendMessage(message);
		}

		public void Connect()
		{
			Disconnect();
			_cancellationTokenSource = new CancellationTokenSource();
			_client.ConnectAsync(_cancellationTokenSource.Token).GetAwaiter().GetResult();
			_incomingMessagesThread = new Thread(ProcessIncomingMessages)
			{
				Name = $"[NPipe]",
				IsBackground = true
			};
			_incomingMessagesThread.Start();
		}

		private void ProcessIncomingMessages()
		{
			while (!_cancellationTokenSource.IsCancellationRequested)
				try
				{
					var message = _client.GetNextMessage(_cancellationTokenSource.Token).GetAwaiter().GetResult();
					if (message != null) MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Error occured when processing pipe server");
				}
		}

		public void Disconnect()
		{
			_cancellationTokenSource?.Cancel();
			_incomingMessagesThread?.Join();
			_cancellationTokenSource?.Dispose();
			_client.Disconnect();
			_incomingMessagesThread = null;
			_cancellationTokenSource = null;
		}
	}
}