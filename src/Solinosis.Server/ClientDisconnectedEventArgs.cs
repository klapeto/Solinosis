using System;
using Solinosis.Common.Messaging;

namespace Solinosis.Server
{
	public class ClientDisconnectedEventArgs: EventArgs
	{
		public ClientDisconnectedEventArgs(ClientInfo clientInfo)
		{
			ClientInfo = clientInfo;
		}

		public ClientInfo ClientInfo { get; }
	}
}