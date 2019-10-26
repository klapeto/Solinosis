using System;
using System.Runtime.Serialization;

namespace Solinosis.Common.Exceptions
{
	[Serializable]
	public class NegotiationException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public NegotiationException()
		{
		}

		public NegotiationException(string message) : base(message)
		{
		}

		public NegotiationException(string message, Exception inner) : base(message, inner)
		{
		}

		protected NegotiationException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}