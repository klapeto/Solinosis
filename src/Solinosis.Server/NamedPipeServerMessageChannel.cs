using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Pipe;

namespace Solinosis.Server
{
	public class NamedPipeServerMessageChannel : NamedPipeMessageChannel<NamedPipeServerStream>
	{
		private readonly NamedPipeConfiguration _configuration;

		public NamedPipeServerMessageChannel(ILogger<NamedPipeMessageChannel<NamedPipeServerStream>> logger,
			IFormatter formatter,
			NamedPipeConfiguration configuration)
			: base(logger, formatter)
		{
			_configuration = configuration;
		}

		protected override void EnsureConnected(NamedPipeServerStream pipe, CancellationToken cancellationToken)
		{
			if (pipe.IsConnected) return;
			pipe.WaitForConnectionAsync(cancellationToken).GetAwaiter().GetResult();
		}

		protected override void EnsureDisconnected(NamedPipeServerStream pipe)
		{
			if (!pipe.IsConnected) return;
			pipe.Disconnect();
			pipe.Dispose();
		}

		protected override NamedPipeServerStream CreateNewPipe()
		{
			return new NamedPipeServerStream(_configuration.PipeName, PipeDirection.InOut,
				_configuration.MaxServerInstances,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous, _configuration.InBufferSize,
				_configuration.OutBufferSize);
		}
	}
}