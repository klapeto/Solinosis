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

namespace Solinosis.Client
{
	public class NamedPipeClient : IDisposable
	{
		private readonly ILogger<NamedPipeClient> _logger;
		private NamedPipeClientStream _pipe;
		private readonly NamedPipeConfiguration _configuration;
		private readonly IFormatter _formatter;
		private readonly object _locker = new object();

		public NamedPipeClient(NamedPipeConfiguration configuration, IFormatter formatter, ILogger<NamedPipeClient> logger)
		{
			_configuration = configuration;
			_formatter = formatter;
			_logger = logger;
		}

		public void SendMessage(Message message)
		{
			lock (_locker)
			{
				_formatter.Serialize(_pipe, message);
			}
		}

		public void Disconnect()
		{
			Regenerate();
		}
		
		public async Task<Message> GetNextMessage(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
				try
				{
					await ConnectAsync(cancellationToken);
					return (Message) _formatter.Deserialize(_pipe);
				}
				catch (Exception e)
				{
					_logger.LogWarning(e, "IO Exception when waiting for client");
					Regenerate();
					await ConnectAsync(cancellationToken);
				}

			throw new OperationCanceledException();
		}

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
			_pipe = new NamedPipeClientStream(".", _configuration.PipeName, PipeDirection.InOut,
				PipeOptions.Asynchronous);
		}

		public async Task ConnectAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
				try
				{
					if (_pipe.IsConnected) return;
					await _pipe.ConnectAsync(cancellationToken);
					_formatter.Serialize(_pipe, Message.CreateNegotiation(_configuration.ClientInfo));
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
						_formatter.Serialize(_pipe, Message.CreateNegotiation(_configuration.ClientInfo));
						return;
					}
				}
				catch (Exception e)
				{
					_logger.LogWarning(e, "IO Exception when waiting for client");
					Regenerate();
				}
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
		}
	}
}