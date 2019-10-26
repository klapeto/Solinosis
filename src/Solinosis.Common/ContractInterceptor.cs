using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Solinosis.Common.Interfaces;

namespace Solinosis.Common
{
	internal class ContractInterceptor<T> : IInterceptor
	{
		private readonly IMethodDispatcher _methodDispatcher;

		public ContractInterceptor(IMethodDispatcher methodDispatcher)
		{
			_methodDispatcher = methodDispatcher;
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void Intercept(IInvocation invocation)
		{
			var returnType = invocation.Method.ReturnType;

			if (typeof(Task).IsAssignableFrom(returnType))
			{
				if (returnType.IsGenericType)
				{
					var taskCompletionSourceType = typeof(TaskCompletionSource<>)
						.MakeGenericType(returnType.GetGenericArguments()[0]);

					var taskCompletionSource = Activator.CreateInstance(taskCompletionSourceType);

					invocation.ReturnValue = taskCompletionSourceType
						.GetProperty(nameof(TaskCompletionSource<object>.Task))
						.GetValue(taskCompletionSource);

					InterceptAsync(invocation).ContinueWith(task => taskCompletionSourceType
						.GetMethod(nameof(TaskCompletionSource<object>.SetResult))
						.Invoke(taskCompletionSource, new[] {invocation.ReturnValue}));
				}
				else
				{
					invocation.ReturnValue = Task.Run(() =>
						_methodDispatcher.Call<T>(invocation.Method.Name, invocation.Arguments));
				}
			}
			else
			{
				invocation.ReturnValue = _methodDispatcher.Call<T>(invocation.Method.Name, invocation.Arguments);
			}

			// Async logic here created with the help from here https://github.com/castleproject/Core/blob/master/docs/dynamicproxy-async-interception.md
		}

		private async Task InterceptAsync(IInvocation invocation)
		{
			invocation.ReturnValue =
				await Task.Run(() => _methodDispatcher.Call<T>(invocation.Method.Name, invocation.Arguments));
		}
	}
}