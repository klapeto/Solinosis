
using Castle.DynamicProxy;
using Solinosis.Common.Interfaces;

namespace Solinosis.Common
{
	public class ContractProxyGenerator<T> where T: class
	{
		private readonly IProxyGenerator _proxyGenerator;
		private readonly IMethodDispatcher _methodDispatcher;

		public ContractProxyGenerator(IProxyGenerator proxyGenerator, IMethodDispatcher methodDispatcher)
		{
			_proxyGenerator = proxyGenerator;
			_methodDispatcher = methodDispatcher;
		}

		public T Create() => _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(new ContractInterceptor<T>(_methodDispatcher));
	}
}