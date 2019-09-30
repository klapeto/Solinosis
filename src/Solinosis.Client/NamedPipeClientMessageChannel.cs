using System.IO.Pipes;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Pipe;

namespace Solinosis.Client
{
	public class NamedPipeClientMessageChannel : NamedPipeMessageChannel
	{
		public NamedPipeClientMessageChannel(ILogger<NamedPipeMessageChannel> logger, IFormatter formatter,
			NamedPipeConfiguration configuration)
			: base(logger, formatter,
				new NamedPipeClientStream(".", configuration.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
		{
		}

		protected override void EnsureConnected()
		{
			if (Pipe.IsConnected) return;
			((NamedPipeClientStream)Pipe).Connect();
		}
	}
}