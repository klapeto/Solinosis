using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;
using Solinosis.Common.Pipe;
using Solinosis.Server.Interfaces;

namespace Solinosis.Server
{
	public class ServerMessageChannel : IMessageChannel, IConnectedClientsState
	{
		private readonly ILogger<ServerMessageChannel> _logger;
		private readonly List<NamedPipeServer> _pipeServers = new List<NamedPipeServer>();
		private readonly List<Thread> _serverThreads = new List<Thread>();
		private readonly NamedPipeConfiguration _configuration;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private readonly ConcurrentDictionary<Guid, ConnectedClient> _connectedClients =
			new ConcurrentDictionary<Guid, ConnectedClient>();

		private readonly IConnectedClientFactory _connectedClientFactory;

		public ServerMessageChannel(IFormatter formatter, NamedPipeConfiguration configuration,
			ILoggerFactory loggerFactory, IConnectedClientFactory connectedClientFactory)
		{
			_configuration = configuration;
			_connectedClientFactory = connectedClientFactory;
			_logger = loggerFactory.CreateLogger<ServerMessageChannel>();
			for (var i = 0; i < configuration.MaxServerInstances; i++)
			{
				var svr = new NamedPipeServer(formatter, configuration,
					loggerFactory.CreateLogger<NamedPipeServer>());
				svr.ClientConnected += PipeServer_ClientConnected;
				svr.ClientDisconnected += PipeServer_ClientDisconnected;
				_pipeServers.Add(svr);
			}
		}

		private void PipeServer_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
		{
			if (!_connectedClients.TryRemove(e.ClientInfo.Id, out _))
				_logger.LogWarning("A client was disconnected when it was not registered");
		}

		private void PipeServer_ClientConnected(object sender, ClientConnectedEventArgs e)
		{
			var client = _connectedClientFactory.Create(e.ClientInfo);
			if (_connectedClients.TryRemove(e.ClientInfo.Id, out _))
				_logger.LogWarning("A client was reconnected when it was not unregistered");
			if (!_connectedClients.TryAdd(e.ClientInfo.Id, client))
				_logger.LogWarning("A client could not be added to dictionary");
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
					_logger.LogDebug($"{nameof(ProcessPipe)} operation canceled exception thrown");
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

		public IReadOnlyDictionary<Guid, ConnectedClient> ConnectedClients => _connectedClients;
	}
}