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
			return response.Payload;
		}
	}
}