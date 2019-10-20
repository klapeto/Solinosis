using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Pipe;

namespace Solinosis.Client
{
	public class NamedPipeClientMessageChannel : NamedPipeMessageChannel<NamedPipeClientStream>
	{
		private readonly NamedPipeConfiguration _configuration;

		public NamedPipeClientMessageChannel(ILogger<NamedPipeMessageChannel<NamedPipeClientStream>> logger,
			IFormatter formatter,
			NamedPipeConfiguration configuration)
			: base(logger, formatter)
		{
			_configuration = configuration;
		}
		
		protected override void EnsureConnected(NamedPipeClientStream pipe, CancellationToken cancellationToken)
		{
			if (pipe.IsConnected) return;
			pipe.ConnectAsync(cancellationToken).GetAwaiter().GetResult();
		}

		protected override void EnsureDisconnected(NamedPipeClientStream pipe)
		{
			if (!pipe.IsConnected) return;
			pipe.Flush();
			pipe.Dispose();
		}

		protected override NamedPipeClientStream CreateNewPipe()
		{
			return new NamedPipeClientStream(".", _configuration.PipeName, PipeDirection.InOut,
				PipeOptions.Asynchronous);
		}
	}
}