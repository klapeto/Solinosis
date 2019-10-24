using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;
using Solinosis.Common.Pipe;

namespace Solinosis.Server
{
	public class ServerMessageChannel : IMessageChannel
	{
		private readonly ILogger<ServerMessageChannel> _logger;
		private readonly List<NamedPipeServer> _pipeServers = new List<NamedPipeServer>();
		private readonly List<Thread> _serverThreads = new List<Thread>();
		private readonly NamedPipeConfiguration _configuration;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public ServerMessageChannel(IFormatter formatter, NamedPipeConfiguration configuration,
			ILoggerFactory loggerFactory)
		{
			_configuration = configuration;
			_logger = loggerFactory.CreateLogger<ServerMessageChannel>();
			for (var i = 0; i < configuration.MaxServerInstances; i++)
				_pipeServers.Add(new NamedPipeServer(formatter, configuration,
					loggerFactory.CreateLogger<NamedPipeServer>()));
		}

		private void ProcessPipe(NamedPipeServer server)
		{
			while (!_cancellationTokenSource.IsCancellationRequested)
				try
				{
					var message = server.GetNextMessage(_cancellationTokenSource.Token).GetAwaiter().GetResult();
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

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			foreach (var namedPipeServer in _pipeServers) namedPipeServer.Dispose();
			foreach (var serverThread in _serverThreads) serverThread.Join();
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public void Broadcast(Message message)
		{
			var pipe = _pipeServers.First(server => server.ConnectedClient?.Id == message.ReceiverInfo?.Id);
			if (pipe != null)
				pipe.SendMessage(message);
			else
				throw new Exception("Target client is not connected");
		}

		public void Connect()
		{
			var c = 0;
			foreach (var namedPipeServer in _pipeServers)
			{
				var th = new Thread(() => ProcessPipe(namedPipeServer))
				{
					Name = $"[NPipe: '{_configuration.PipeName}' #{c++}]",
					IsBackground = true
				};
				_serverThreads.Add(th);
				th.Start();
			}
		}

		public void Disconnect()
		{
			_cancellationTokenSource.Cancel();
			foreach (var serverThread in _serverThreads) serverThread.Join();
			foreach (var namedPipeServer in _pipeServers) namedPipeServer.Disconnect();
		}
	}
}