using System.IO.Pipes;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Solinosis.Common.Pipe;

namespace Solinosis.Server
{
	public class NamedPipeServerMessageChannel : NamedPipeMessageChannel 
	{
		private readonly object _connectionLocker = new object();
		
		public NamedPipeServerMessageChannel(ILogger<NamedPipeMessageChannel> logger, IFormatter formatter,
			NamedPipeConfiguration configuration)
			: base(logger, formatter,
				new NamedPipeServerStream(configuration.PipeName, PipeDirection.InOut, configuration.MaxServerInstances,
					PipeTransmissionMode.Byte, PipeOptions.Asynchronous, configuration.InBufferSize,
					configuration.OutBufferSize))
		{
			
		}

		protected override void EnsureConnected()
		{
			lock (_connectionLocker)
			{
				if (Pipe.IsConnected) return;
				((NamedPipeServerStream)Pipe).WaitForConnection();
			}
		}
	}
}