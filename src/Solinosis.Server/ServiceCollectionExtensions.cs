using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;
using Solinosis.Common.Pipe;
using Solinosis.Server.Interfaces;

namespace Solinosis.Server
{
	public static class ServiceCollectionExtensions
	{
		public static INamedPipeServerBuilder AddNamedPipeServer(this IServiceCollection serviceCollection,
			string pipeName, Action<NamedPipeConfiguration> configureOptions = null)
		{
			var builder = new NamedPipeServerBuilder(serviceCollection);
			var options = new NamedPipeConfiguration(pipeName);
			configureOptions?.Invoke(options);
			serviceCollection.AddLogging();
			serviceCollection.AddSingleton(options);
			serviceCollection.AddSingleton<IMessageChannel, NamedPipeServerMessageChannel>();
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