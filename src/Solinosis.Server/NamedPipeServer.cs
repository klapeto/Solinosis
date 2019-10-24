using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Exceptions;
using Solinosis.Common.Messaging;
using Solinosis.Common.Pipe;

namespace Solinosis.Server
{
	public class NamedPipeServer : IDisposable
	{
		public event EventHandler<ClientConnectedEventArgs> ClientConnected;
		public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
		private NamedPipeServerStream _pipe;
		public ClientInfo ConnectedClient { get; private set; }

		private readonly IFormatter _formatter;
		private readonly NamedPipeConfiguration _configuration;
		private readonly ILogger<NamedPipeServer> _logger;

		private object _writeLocker = new object();

		private void Regenerate()
		{
			try
			{
				_pipe?.Flush();
			}
			catch
			{
				// ignored
			}

			_pipe?.Dispose();
			ConnectedClient = null;
			_pipe = new NamedPipeServerStream(_configuration.PipeName, PipeDirection.InOut,
				_configuration.MaxServerInstances,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous, _configuration.InBufferSize,
				_configuration.OutBufferSize);
		}

		public void SendMessage(Message message)
		{
			lock (_writeLocker)
			{
				_formatter.Serialize(_pipe, message);
			}
		}

		public async Task<Message> GetNextMessage(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
				try
				{
					return (Message) _formatter.Deserialize(_pipe);
				}
				catch (Exception e)
				{
					_logger.LogWarning(e, "IO Exception when waiting for client");
					if (ConnectedClient != null)
						ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(ConnectedClient));
					Regenerate();
					await WaitForClientAsync(cancellationToken);
				}

			throw new OperationCanceledException();
		}

		public void Disconnect()
		{
			Regenerate();
		}

		private async Task WaitForClientAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
				try
				{
					await _pipe.WaitForConnectionAsync(cancellationToken);
					var negotiationMessage = (Message) _formatter.Deserialize(_pipe);
					if (negotiationMessage.Type != MessageType.Negotiation)
					{
						_formatter.Serialize(_pipe,
							Message.CreateError(
								_configuration.ClientInfo,
								negotiationMessage.SenderInfo,
								negotiationMessage.Id,
								new NegotiationException(
									$"Expected a negotiation message but got: {Enum.GetName(typeof(MessageType), negotiationMessage.Type)}"),
								null));
						Regenerate();
					}
					else
					{
						ConnectedClient = negotiationMessage.SenderInfo;
						_formatter.Serialize(_pipe, Message.CreateNegotiation(_configuration.ClientInfo));
						ClientConnected?.Invoke(this, new ClientConnectedEventArgs(ConnectedClient));
						return;
					}
				}
				catch (IOException e)
				{
					_logger.LogWarning(e, "IO Exception when waiting for client");
					Regenerate();
				}
		}

		public NamedPipeServer(IFormatter formatter, NamedPipeConfiguration configuration,
			ILogger<NamedPipeServer> logger)
		{
			_formatter = formatter;
			_configuration = configuration;
			_logger = logger;
			Regenerate();
		}

		public void Dispose()
		{
			try
			{
				_pipe?.Flush();
			}
			catch
			{
				// ignored
			}

			_pipe?.Dispose();
			ConnectedClient = null;
		}
	}
}