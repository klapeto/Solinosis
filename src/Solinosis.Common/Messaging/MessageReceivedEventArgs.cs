using System;

namespace Solinosis.Common.Messaging
{
	public class MessageReceivedEventArgs: EventArgs
	{
		public MessageReceivedEventArgs(Message message)
		{
			Message = message;
		}

		public Message Message { get; }
	}
}