using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Messaging;

namespace Solinosis.Server
{
	public class ConnectedClient
	{
		private readonly IServiceProvider _serviceProvider;

		public ClientInfo Info { get; }

		public T GetCallbackProxy<T>()
		{
			return _serviceProvider.GetRequiredService<T>();
		}

		public ConnectedClient(ClientInfo info, IServiceProvider serviceProvider)
		{
			Info = info;
			_serviceProvider = serviceProvider;
		}
	}
}