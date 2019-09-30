using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Client.Interfaces;
using Solinosis.Common;

namespace Solinosis.Client
{
	public class NamedPipeClientBuilder: INamedPipeClientBuilder
	{
		private readonly IServiceCollection _serviceCollection;

		public NamedPipeClientBuilder(IServiceCollection serviceCollection)
		{
			_serviceCollection = serviceCollection;
		}
		
		public INamedPipeClientBuilder AddService<TInterface>() where TInterface : class
		{
			_serviceCollection.AddScoped(provider => provider.GetRequiredService<ContractProxyGenerator<TInterface>>().Create());
			return this;
		}

		public INamedPipeClientBuilder AddCallbackService<TInterface, TImplementation>() where TInterface : class where TImplementation : class, TInterface
		{
			_serviceCollection.AddScoped<TInterface, TImplementation>();
			return this;
		}

		public INamedPipeClientBuilder AddCallbackService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> factory) where TInterface : class where TImplementation : class, TInterface
		{
			_serviceCollection.AddScoped<TInterface, TImplementation>(factory);
			return this;
		}

		public INamedPipeClientBuilder AddCallbackService<TInterface>(Func<IServiceProvider, TInterface> factory) where TInterface : class
		{
			_serviceCollection.AddScoped(factory);
			return this;
		}
	}
}