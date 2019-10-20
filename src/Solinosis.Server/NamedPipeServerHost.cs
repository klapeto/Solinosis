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
			_serviceProvider.GetRequiredService<IMessageChannel>().Connect();
			_serviceProvider.GetRequiredService<IMessageHandler>();
		}

		public void Stop()
		{
			_serviceProvider.GetRequiredService<IMessageChannel>().Disconnect();
		}
	}
}