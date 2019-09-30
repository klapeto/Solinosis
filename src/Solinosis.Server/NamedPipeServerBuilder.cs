using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common;
using Solinosis.Server.Interfaces;

namespace Solinosis.Server
{
	public class NamedPipeServerBuilder: INamedPipeServerBuilder
	{
		private readonly IServiceCollection _serviceCollection;
		
		public NamedPipeServerBuilder(IServiceCollection serviceCollection)
		{
			_serviceCollection = serviceCollection;
		}

		public INamedPipeServerBuilder AddService<TInterface, TImplementation>() where TInterface : class where TImplementation : class, TInterface
		{
			_serviceCollection.AddScoped<TInterface, TImplementation>();
			return this;
		}

		public INamedPipeServerBuilder AddService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> factory) where TInterface : class where TImplementation : class, TInterface
		{
			_serviceCollection.AddScoped<TInterface, TImplementation>(factory);
			return this;
		}

		public INamedPipeServerBuilder AddService<TInterface>(Func<IServiceProvider, TInterface> factory) where TInterface : class
		{
			_serviceCollection.AddScoped(factory);
			return this;
		}
		
		
		public INamedPipeServerBuilder AddCallback<TInterface>() where TInterface : class
		{
			_serviceCollection.AddScoped(provider => provider.GetRequiredService<ContractProxyGenerator<TInterface>>().Create());
			return this;
		}
		
	}
}