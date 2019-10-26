using System;
using Solinosis.Common.Messaging;

namespace Solinosis.Server
{
	public class ClientConnectedEventArgs: EventArgs
	{
		public ClientConnectedEventArgs(ClientInfo clientInfo)
		{
			ClientInfo = clientInfo;
		}

		public ClientInfo ClientInfo { get; }
	}
}