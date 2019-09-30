using System;

namespace Solinosis.Server.Interfaces
{
	public interface INamedPipeServerBuilder
	{
		INamedPipeServerBuilder AddService<TInterface, TImplementation>()
			where TInterface : class where TImplementation : class, TInterface;

		INamedPipeServerBuilder AddService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> factory)
			where TInterface : class where TImplementation : class, TInterface;

		INamedPipeServerBuilder AddService<TInterface>(Func<IServiceProvider, TInterface> factory)
			where TInterface : class;

		INamedPipeServerBuilder AddCallback<TInterface>() where TInterface : class;
	}
}