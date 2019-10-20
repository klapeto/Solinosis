using System;
using Solinosis.Common.Messaging;

namespace Solinosis.Common.Pipe
{
	public class NamedPipeConfiguration
	{
		public NamedPipeConfiguration(string pipeName)
		{
			if (string.IsNullOrWhiteSpace(pipeName)) throw new ArgumentException("Pipe Name cannot be null or whitespace", nameof(pipeName));
			PipeName = pipeName;
			ClientInfo = new ClientInfo
			{
				Id = Guid.NewGuid()
			};
		}

		public string PipeName { get; }
		public int MaxServerInstances { get; set; } = 5;
		public int InBufferSize { get; set; }
		public int OutBufferSize { get; set; }
		
		public ClientInfo ClientInfo { get; }
	}
}