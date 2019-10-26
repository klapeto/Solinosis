using System;
using Solinosis.Common.Payloads;

namespace Solinosis.Common.Messaging
{
	[Serializable]
	public class Message
	{
		public Guid Id { get; set; }
		public MessageType Type { get; set; }
		public object Payload { get; set; }
		public ClientInfo ReceiverInfo { get; set; }
		public ClientInfo SenderInfo { get; set; }
		
		public static Message CreateError(ClientInfo sender, ClientInfo receiver,Guid requestId, Exception exception, string message)
		{
			return new Message
			{
				Id = Guid.NewGuid(),
				Type = MessageType.Response,
				SenderInfo = sender,
				ReceiverInfo = receiver,
				Payload = new Error
				{
					RequestId = requestId,
					Exception = exception,
					Message = message
				}
			};
		}
		
		public static Message CreateNegotiation(ClientInfo sender)
		{
			return new Message
			{
				Id = Guid.NewGuid(),
				Type = MessageType.Negotiation,
				SenderInfo = sender
			};
		}
		
		public static Message CreateRequest<T>(ClientInfo sender, ClientInfo receiver, string methodName, object[] arguments)
		{
			return new Message
			{
				Id = Guid.NewGuid(),
				Type = MessageType.Request,
				SenderInfo = sender,
				ReceiverInfo = receiver,
				Payload = new IpcRequest
				{
					ContractName = typeof(T).FullName,
					MethodName = methodName,
					Arguments = arguments
				}
			};
		}
		
		public static Message CreateResponse(Guid requestId, ClientInfo sender, ClientInfo receiver, object returnValue)
		{
			return new Message
			{
				Id = Guid.NewGuid(),
				Type = MessageType.Response,
				SenderInfo = sender,
				ReceiverInfo = receiver,
				Payload = new IpcResponse
				{
					Payload = returnValue,
					IsError = false,
					RequestId = requestId
				}
			};
		}
	}
}