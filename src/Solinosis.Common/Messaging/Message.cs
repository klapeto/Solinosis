using System;

namespace Solinosis.Common.Messaging
{
	[Serializable]
	public class Message
	{
		public Guid Id { get; set; }
		
		public string ContractName { get; set; }
		public MessageType Type { get; set; }
		public object Payload { get; set; }


		public static Message CreateRequest<T>(string methodName, object[] arguments)
		{
			return new Message
			{
				Id = Guid.NewGuid(),
				Type = MessageType.Request,
				ContractName = typeof(T).FullName,
				Payload = new Request
				{
					MethodName = methodName,
					Arguments =arguments
				}
			};
		}
	}
}