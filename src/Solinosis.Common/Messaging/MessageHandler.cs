using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Payloads;
using Solinosis.Common.Pipe;

namespace Solinosis.Common.Messaging
{
	public class MessageHandler : IMessageHandler
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IMessageChannel _messageChannel;
		private readonly NamedPipeConfiguration _configuration;

		private readonly Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

		public MessageHandler(IServiceProvider serviceProvider, IMessageChannel messageChannel,
			NamedPipeConfiguration configuration)
		{
			_serviceProvider = serviceProvider;
			_messageChannel = messageChannel;
			_configuration = configuration;

			messageChannel.MessageReceived += MessageChannel_MessageReceived;
		}

		private void MessageChannel_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			Task.Run(() => HandleMessageReceived(e.Message));
		}

		private void HandleMessageReceived(Message message)
		{
			switch (message.Type)
			{
				case MessageType.Request:
					HandleRequest(message);
					break;
				case MessageType.Response:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleRequest(Message message)
		{
			var payload = (IpcRequest) message.Payload;
			var type = GetType(payload.ContractName);

			using (var scope = _serviceProvider.CreateScope())
			{
				try
				{
					if (scope.ServiceProvider.GetRequiredService<ICallContext>() is CallContext context)
					{
						context.CallerInfo = message.SenderInfo;
					}
						
					var instance = scope.ServiceProvider.GetRequiredService(type);
					var request = (IpcRequest) message.Payload;
					var response = Call(type, request.MethodName, request.Arguments, instance);

					_messageChannel.Broadcast(Message.CreateResponse(message.Id, _configuration.ClientInfo,
						message.SenderInfo, response));
				}
				catch (Exception e)
				{
					_messageChannel.Broadcast(Message.CreateError(_configuration.ClientInfo, 
						message.SenderInfo, message.Id, e, "Error occured handling request"));
				}
			}
		}

		private Type GetType(string name)
		{
			if (_cachedTypes.TryGetValue(name, out var type)) return type;
			type = Type.GetType(name);
			_cachedTypes.Add(name, type);
			return type;
		}

		private static object Call(Type type, string name, object[] arguments, object instance)
		{
			var method = type.GetMethod(name);
			if (method == null) throw new MissingMethodException(name);

			if (method.ReturnType == typeof(Task) || method.ReturnType.BaseType == typeof(Task))
			{
				if (method.ReturnType.IsGenericType)
					return method.ReturnType.GetProperty(nameof(Task<object>.Result))
						.GetValue(method.Invoke(instance, arguments));

				// if Oneway, just call and return
				((Task) method.Invoke(instance, arguments)).Wait();
				return null;
			}

			return method.Invoke(instance, arguments);
		}

		public IpcResponse SendMessage(Message message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			IpcResponse ipcResponse = null;
			using (var notifier = new ManualResetEventSlim(false))
			{
				var eventHandler = new EventHandler<MessageReceivedEventArgs>(
					delegate(object sender, MessageReceivedEventArgs args)
					{
						if (args.Message.Type != MessageType.Response) return;
						var resp = (IpcResponse) args.Message.Payload;
						if (resp.RequestId != message.Id) return;
						ipcResponse = resp;
						notifier.Set();
					});
				try
				{
					message.SenderInfo = _configuration.ClientInfo;
					_messageChannel.MessageReceived += eventHandler;
					_messageChannel.Broadcast(message);
					notifier.Wait();
				}
				finally
				{
					_messageChannel.MessageReceived -= eventHandler;
				}
			}

			return ipcResponse;
		}

		public void SendMessageAndForget(Message message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			_messageChannel.Broadcast(message);
		}
	}
}