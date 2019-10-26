using Solinosis.Common.Messaging;

namespace Solinosis.Server.Interfaces
{
	public interface IConnectedClientFactory
	{
		ConnectedClient Create(ClientInfo clientInfo);
	}
}