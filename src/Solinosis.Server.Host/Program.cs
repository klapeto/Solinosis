using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;

namespace Solinosis.Server.Host
{
	internal class Program
	{
		public class TestService: ITestService
		{
			private readonly ICallContext _callContext;

			public TestService(ICallContext callContext)
			{
				_callContext = callContext;
			}

			public Task<string> TestCall(string arg)
			{
				return Task.Run(async () =>
				{
					Console.WriteLine($"TestCall called from {_callContext.CallerInfo.Name}: {arg}");
					var str = await _callContext.GetCallbackChannel<ITestCallbackService>().TestCallbackAsync("Hello!");
					Console.WriteLine($"Callback from {_callContext.CallerInfo.Name} returned: {str}");
					return $"TestCall received: {arg}";
				});
			}
		}
		
		private static void Main(string[] args)
		{
			var servicesCollection = new ServiceCollection();
			servicesCollection.AddNamedPipeServer("KKlapeto")
				.AddService<ITestService, TestService>()
				.AddCallback<ITestCallbackService>();

			var serverHost = new NamedPipeServerHost(servicesCollection.BuildServiceProvider());
			serverHost.Start();
			Console.Read();
		}
	}
}