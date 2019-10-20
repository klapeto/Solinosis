using System;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;

namespace Solinosis.Common
{
	public class MethodDispatcher: IMethodDispatcher
	{
		private readonly IMessageHandler _messageHandler;

		public MethodDispatcher(IMessageHandler messageHandler)
		{
			_messageHandler = messageHandler;
		}

		public object Call<T>(string methodName, object[] arguments)
		{
			var response = _messageHandler.SendMessage(Message.CreateRequest<T>(methodName, arguments));
			if (!response.IsError) return response.Payload;
			if (response.Payload is ErrorPayload error)
			{
				throw error.Exception;
			}

			throw new Exception("Unknown error occured on the other side of the pipe");
		}
	}
}