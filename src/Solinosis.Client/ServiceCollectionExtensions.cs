using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Client.Interfaces;
using Solinosis.Common;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;
using Solinosis.Common.Pipe;

namespace Solinosis.Client
{
	public static class ServiceCollectionExtensions
	{
		public static INamedPipeClientBuilder AddNamedPipeClient(this IServiceCollection serviceCollection,
			string pipeName, Action<NamedPipeConfiguration> configureOptions = null)
		{
			var builder = new NamedPipeClientBuilder(serviceCollection);
			var options = new NamedPipeConfiguration(pipeName);
			configureOptions?.Invoke(options);
			serviceCollection.AddLogging();
			serviceCollection.AddSingleton(options);
			serviceCollection.AddSingleton<IMessageChannel, ClientMessageChannel>();
			serviceCollection.AddSingleton<NamedPipeClient>();
			serviceCollection.AddTransient<IFormatter, BinaryFormatter>();
			serviceCollection.AddScoped<ICallContext, CallContext>();
			serviceCollection.AddTransient<IMethodDispatcher, MethodDispatcher>();
			serviceCollection.AddTransient<IProxyGenerator, ProxyGenerator>();
			serviceCollection.AddTransient(typeof(ContractProxyGenerator<>));
			
			serviceCollection.AddSingleton<IMessageHandler, MessageHandler>();
			return builder;
		}
	}
}