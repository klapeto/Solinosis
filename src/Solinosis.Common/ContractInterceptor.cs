using Castle.DynamicProxy;
using Solinosis.Common.Interfaces;

namespace Solinosis.Common
{
	internal class ContractInterceptor<T>: IInterceptor
	{
		private readonly IMethodDispatcher _methodDispatcher;

		public ContractInterceptor(IMethodDispatcher methodDispatcher)
		{
			_methodDispatcher = methodDispatcher;
		}

		public void Intercept(IInvocation invocation)
		{
			invocation.ReturnValue = _methodDispatcher.Call<T>(invocation.Method.Name, invocation.Arguments);
		}
	}
}