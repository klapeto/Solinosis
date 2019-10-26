using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;

namespace Solinosis.Client.Host
{
	internal static class Program
	{
		public class Callback : ITestCallbackService
		{
			private ICallContext _callContext;

			public Callback(ICallContext callContext)
			{
				_callContext = callContext;
			}

			public string TestCallback(string arg)
			{
				Console.WriteLine($"Callback: {arg}");
				return $"Callback: {arg}";
			}

			public Task<string> TestCallbackAsync(string arg)
			{
				return Task.FromResult("Hello From Async");
			}
		}

		private static void Main(string[] args)
		{
			var servicesCollection = new ServiceCollection();
			servicesCollection.AddNamedPipeClient("KKlapeto", configuration =>
					{
						configuration.ClientInfo.Name = $"Client: {Guid.NewGuid()}";
					})
				.AddService<ITestService>()
				.AddCallbackService<ITestCallbackService, Callback>();

			var client = new NamedPipeClientHost(servicesCollection.BuildServiceProvider());
			client.Connect();
			var res = client.GetServiceProxy<ITestService>().TestCall("Hahaha");
			Console.Read();
		}
	}
}