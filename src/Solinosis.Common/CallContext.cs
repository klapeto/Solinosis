using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;

namespace Solinosis.Common
{
	public class CallContext: ICallContext
	{
		private readonly IServiceProvider _serviceProvider;

		public CallContext(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public ClientInfo CallerInfo { get; internal set; }
		public ClientInfo HandlerInfo { get; }
		public T GetCallbackChannel<T>() => _serviceProvider.GetRequiredService<T>();
	}
}