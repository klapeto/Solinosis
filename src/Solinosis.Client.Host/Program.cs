using System;
using System.IO.Pipes;
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
		}

		private static void Main(string[] args)
		{
			var client = new NamedPipeClientStream(".", "KKlapeto",PipeDirection.InOut);
			client.Connect();
			client.Close();
			client.Connect();
			
//			var servicesCollection = new ServiceCollection();
//			servicesCollection.AddNamedPipeClient("KKlapeto", configuration =>
//					{
//						configuration.ClientInfo.Name = "Client #1";
//					})
//				.AddService<ITestService>()
//				.AddCallbackService<ITestCallbackService, Callback>();
//
//			var client = new NamedPipeClient(servicesCollection.BuildServiceProvider());
//			client.Connect();
//			var res = client.GetServiceProxy<ITestService>().TestCall("Hahaha");
			Console.Read();
		}
	}
}