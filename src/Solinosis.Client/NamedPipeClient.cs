using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Client.Interfaces;
using Solinosis.Common.Interfaces;

namespace Solinosis.Client
{
	public class NamedPipeClient: INamedPipeClient
	{
		private readonly IServiceProvider _serviceProvider;

		public NamedPipeClient(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}


		public void Connect()
		{
			var svr = _serviceProvider.GetRequiredService<IMessageHandler>();
		}

		public void Disconnect()
		{
			throw new NotImplementedException();
		}

		public T GetServiceProxy<T>() where T : class
		{
			return (T)_serviceProvider.GetService(typeof(T));
		}
	}
}