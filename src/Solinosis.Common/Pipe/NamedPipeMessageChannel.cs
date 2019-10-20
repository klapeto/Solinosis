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
	public abstract class NamedPipeMessageChannel<T> : IMessageChannel where T : PipeStream
	{
		private readonly object _locker = new object();
		private bool _connected;
		private T _pipe;
		private readonly ILogger<NamedPipeMessageChannel<T>> _logger;
		private readonly IFormatter _formatter;
		private bool _continue = true;
		private Thread _incomingMessagesThread;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		protected NamedPipeMessageChannel(ILogger<NamedPipeMessageChannel<T>> logger, IFormatter formatter)
		{
			_logger = logger;
			_formatter = formatter;
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Broadcast(Message message)
		{
			Connect();
			_formatter.Serialize(_pipe, message);
		}

		public Task BroadcastAsync(Message message, CancellationToken cancellationToken)
		{
			return Task.Run(() => Broadcast(message), cancellationToken);
		}

		public void Connect()
		{
			lock (_locker)
			{
				if (_connected) return;
				Disconnect();
				_pipe = CreateNewPipe();
				_continue = true;
				_cancellationTokenSource = new CancellationTokenSource();
				EnsureConnected(_pipe, _cancellationTokenSource.Token);
				_incomingMessagesThread = new Thread(ProcessIncomingMessages)
				{
					Name = $"[{nameof(NamedPipeMessageChannel<T>)}] Incoming Messages",
					IsBackground = true
				};
				_incomingMessagesThread.Start();
				_connected = true;
			}
		}

		public Task ConnectAsync(CancellationToken cancellationToken)
		{
			return Task.Run(Connect, cancellationToken);
		}

		public void Disconnect()
		{
			lock (_locker)
			{
				if (!_connected) return;
				_continue = false;
				_cancellationTokenSource.Cancel();
				if (_incomingMessagesThread?.IsAlive ?? false) _incomingMessagesThread.Join();
				EnsureDisconnected(_pipe);
				_pipe.Dispose();
				_cancellationTokenSource.Dispose();
				_connected = false;
			}
		}

		public Task DisconnectAsync(CancellationToken cancellationToken)
		{
			return Task.Run(Disconnect, cancellationToken);
		}

		private void ProcessIncomingMessages()
		{
			while (_continue)
				try
				{
					EnsureConnected(_pipe, _cancellationTokenSource.Token);
					var value = (Message) _formatter.Deserialize(_pipe);
					if (value != null) MessageReceived?.Invoke(this, new MessageReceivedEventArgs(value));
				}
				catch (Exception e)
				{
					_logger?.LogError(e, "Error occured on processing incoming messages");
				}
		}

		protected abstract void EnsureConnected(T pipe, CancellationToken cancellationToken);
		protected abstract void EnsureDisconnected(T pipe);

		protected abstract T CreateNewPipe();

		public void Dispose()
		{
			Disconnect();
			_pipe?.Dispose();
			_cancellationTokenSource?.Dispose();
		}
	}
}