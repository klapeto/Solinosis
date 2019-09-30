using System;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;

namespace Solinosis.Client.Host
{
    static class Program
    {

        public class Callback : ITestCallbackService
        {
            public string TestCallback(string arg)
            {
                Console.WriteLine($"Callback: {arg}");
                return $"Callback: {arg}";
            }
        }

        private static void Main(string[] args)
        {
            var servicesCollection = new ServiceCollection();
            servicesCollection.AddNamedPipeClient("KKlapeto")
                .AddService<ITestService>()
                .AddCallbackService<ITestCallbackService>(provider => new Callback());

            var client = new NamedPipeClient(servicesCollection.BuildServiceProvider());
            client.Connect();
            var res = client.GetServiceProxy<ITestService>().TestCall("Hahaha");
            Console.Read();
        }
    }
}