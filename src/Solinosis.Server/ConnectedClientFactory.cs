using System;
using Solinosis.Common.Messaging;
using Solinosis.Server.Interfaces;

namespace Solinosis.Server
{
	public class ConnectedClientFactory: IConnectedClientFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public ConnectedClientFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public ConnectedClient Create(ClientInfo clientInfo)
		{
			return new ConnectedClient(clientInfo, _serviceProvider);
		}
	}
}