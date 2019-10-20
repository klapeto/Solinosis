using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Pipe;

namespace Solinosis.Common.Messaging
{
	public class MessageHandler: IMessageHandler
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IMessageChannel _messageChannel;
		private readonly NamedPipeConfiguration _configuration;
		
		private readonly Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

		public MessageHandler(IServiceProvider serviceProvider, IMessageChannel messageChannel, NamedPipeConfiguration configuration)
		{
			_serviceProvider = serviceProvider;
			_messageChannel = messageChannel;
			_configuration = configuration;

			messageChannel.MessageReceived += MessageChannel_MessageReceived;
		}

		private void MessageChannel_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			HandleMessageReceived(e.Message);
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
			var type = GetType(message.ContractName);

			using (var scope = _serviceProvider.CreateScope())
			{
				try
				{
					if (scope.ServiceProvider.GetRequiredService<ICallContext>() is CallContext context)
					{
						context.CallerInfo = message.ClientInfo;
					}
					var instance = scope.ServiceProvider.GetRequiredService(type);
					var request = (Request) message.Payload;
					var response = Call(type, request.MethodName, request.Arguments, instance);
			
					_messageChannel.Broadcast(new Message
					{
						Id = message.Id,
						ContractName = message.ContractName,
						Type = MessageType.Response,
						ClientInfo = _configuration.ClientInfo,
						Payload = new Response
						{
							Payload = response,
							RequestId = message.Id
						}
					});
				}
				catch (Exception e)
				{
					_messageChannel.Broadcast(new Message
					{
						Id = message.Id,
						ContractName = message.ContractName,
						Type = MessageType.Response,
						ClientInfo = _configuration.ClientInfo,
						Payload = new Response
						{
							Payload = new ErrorPayload
							{
								Exception = e,
								Message = e.ToString()
							},
							IsError = true,
							RequestId = message.Id
						}
					});
				}
			}
		}

		private Type GetType(string name)
		{
			if (_cachedTypes.TryGetValue(name, out var type))
			{
				return type;
			}
			type = Type.GetType(name);
			_cachedTypes.Add(name, type);
			return type;
		}
		
		private static object Call(Type type, string name, object[] arguments, object instance)
		{
			var method = type.GetMethod(name);
			if (method == null) throw new MissingMethodException(name);
			
			return method.Invoke(instance, arguments);
		}

		public Response SendMessage(Message message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			Response response = null;
			using (var notifier = new ManualResetEventSlim(false))
			{
				var eventHandler = new EventHandler<MessageReceivedEventArgs>(
					delegate(object sender, MessageReceivedEventArgs args)
					{
						if (args.Message.Type != MessageType.Response) return;
						var resp = (Response) args.Message.Payload;
						if (resp.RequestId != message.Id) return;
						response = resp;
						notifier.Set();
					});
				try
				{
					message.ClientInfo = _configuration.ClientInfo;
					_messageChannel.MessageReceived += eventHandler;
					_messageChannel.Broadcast(message);
					notifier.Wait();
				}
				finally
				{
					_messageChannel.MessageReceived -= eventHandler;
				}
			}

			return response;
		}

		public void SendMessageAndForget(Message message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			_messageChannel.Broadcast(message);
		}
	}
}