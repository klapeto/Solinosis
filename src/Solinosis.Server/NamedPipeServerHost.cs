using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;
using Solinosis.Server.Interfaces;

namespace Solinosis.Server
{
	public class NamedPipeServerHost: INamedPipeServerHost
	{
		private readonly IServiceProvider _serviceProvider;

		public NamedPipeServerHost(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public T GetCallbackProxy<T>() where T : class
		{
			return _serviceProvider.GetRequiredService<T>();
		}

		public void Start()
		{
			var messageHandler = _serviceProvider.GetRequiredService<IMessageHandler>();
		}

		public void Stop()
		{
			throw new System.NotImplementedException();
		}
	}
}