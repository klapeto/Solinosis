using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;

namespace Solinosis.Server.Host
{
	internal class Program
	{
		public class TestService: ITestService
		{
			private ICallContext _callContext;

			public TestService(ICallContext callContext)
			{
				_callContext = callContext;
			}

			public string TestCall(string arg)
			{
				Console.WriteLine($"TestCall called: {arg}");
				return $"TestCall received: {arg}";
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
			var reps = serverHost.GetCallbackProxy<ITestCallbackService>().TestCallback("Hello");
			Console.Read();
		}
	}
}