using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
			var returnType = invocation.Method.ReturnType;

			// For this example, we'll just assume that we're dealing with
			// a `returnType` of `Task<TResult>`. In practice, you'd have
			// to have logic for non-`Task`, `Task`, `Task<TResult>`, and
			// any other awaitable types that you care about:
			Debug.Assert(typeof(Task).IsAssignableFrom(returnType) && returnType.IsGenericType);

			// Instantiate a `TaskCompletionSource<TResult>`, whose `Task`
			// we will return to the calling code, so that we can control
			// the result:
			var tcsType = typeof(TaskCompletionSource<>)
				.MakeGenericType(returnType.GetGenericArguments()[0]);
			var tcs = Activator.CreateInstance(tcsType);
			invocation.ReturnValue = tcsType.GetProperty("Task").GetValue(tcs, null);

			// Because we're not in an `async` method, we cannot use `await`
			// and have the compiler generate a continuation for the code
			// following it. Let's therefore set up the continuation manually:
			InterceptAsync(invocation).ContinueWith(_ =>
			{
				// This sets the result of the task that we have previously
				// returned to the calling code, based on `invocation.ReturnValue`
				// which has been set in the (by now completed) `InterceptAsync`
				// method (see below):
				tcsType.GetMethod("SetResult").Invoke(tcs, new object[] { invocation.ReturnValue });
			});

		}
		
		private async Task InterceptAsync(IInvocation invocation)
		{
			// In this method, we now have the comfort of `await`:
			// ... and we can still set the final return value! Note that
			// the return type of this method is now `Task`, not `Task<TResult>`,
			// so we can no longer `return` a value. Instead, we use the
			// "stale" `invocation` to hold the real return value for us.
			// It will get processed in the continuation (above) when this
			// async method completes:
			invocation.ReturnValue = await  Task.Run(() => _methodDispatcher.Call<T>(invocation.Method.Name, invocation.Arguments));
		}
	}
}