using System;

namespace Solinosis.Client.Interfaces
{
	public interface INamedPipeClientBuilder
	{
		INamedPipeClientBuilder AddService<TInterface>() where TInterface : class;

		INamedPipeClientBuilder AddCallbackService<TInterface, TImplementation>() where TInterface : class
			where TImplementation : class, TInterface;
		
		INamedPipeClientBuilder AddCallbackService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> factory) where TInterface : class
			where TImplementation : class, TInterface;

		INamedPipeClientBuilder AddCallbackService<TInterface>(Func<IServiceProvider, TInterface> factory) where TInterface : class;
	}
}