using System;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;

namespace Solinosis.Common.Pipe
{
	public abstract class NamedPipeMessageChannel : IMessageChannel
	{
		protected PipeStream Pipe { get; }
		private readonly ILogger<NamedPipeMessageChannel> _logger;
		private readonly IFormatter _formatter;
		private bool _continue = true;
		private readonly Thread _incomingMessagesThread;

		protected NamedPipeMessageChannel(ILogger<NamedPipeMessageChannel> logger, IFormatter formatter,
			PipeStream pipe)
		{
			_logger = logger;
			_formatter = formatter;
			Pipe = pipe;
			_incomingMessagesThread = new Thread(ProcessIncomingMessages)
			{
				Name = $"[{nameof(NamedPipeMessageChannel)}] Incoming Messages",
				IsBackground = true
			};
			_incomingMessagesThread.Start();
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;


		public void Broadcast(Message message)
		{
			EnsureConnected();
			_formatter.Serialize(Pipe, message);
		}

		public Task BroadcastAsync(Message message)
		{
			return Task.Run(() => Broadcast(message));
		}

		private void ProcessIncomingMessages()
		{
			while (_continue)
				try
				{
					EnsureConnected();
					var value = (Message) _formatter.Deserialize(Pipe);
					if (value != null) MessageReceived?.Invoke(this, new MessageReceivedEventArgs(value));
				}
				catch (Exception e)
				{
					_logger?.LogError(e, "Error occured on processing incoming messages");
					continue;
				}
				finally
				{
					Thread.Sleep(10);
				}
		}

		protected abstract void EnsureConnected();

		public void Dispose()
		{
			_continue = false;

			Pipe.Dispose();
			if (_incomingMessagesThread.Join(2000))
				_logger?.LogWarning("Failed to end incoming messages thread within the expected time");
		}
	}
}