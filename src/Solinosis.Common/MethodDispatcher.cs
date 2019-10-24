using System;
using Solinosis.Common.Interfaces;
using Solinosis.Common.Messaging;
using Solinosis.Common.Payloads;

namespace Solinosis.Common
{
	public class MethodDispatcher: IMethodDispatcher
	{
		private readonly IMessageHandler _messageHandler;
		private readonly ICallContext _callContext;

		public MethodDispatcher(IMessageHandler messageHandler, ICallContext callContext)
		{
			_messageHandler = messageHandler;
			_callContext = callContext;
		}

		public object Call<T>(string methodName, object[] arguments)
		{
			var response = _messageHandler.SendMessage(Message.CreateRequest<T>(_callContext.HandlerInfo, _callContext.CallerInfo, methodName, arguments));
			if (!response.IsError) return response.Payload;
			if (response.Payload is Error error)
			{
				throw error.Exception;
			}

			throw new Exception("Unknown error occured on the other side of the pipe");
		}
	}
}