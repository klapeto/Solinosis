using System;
using System.Collections.Generic;

namespace Solinosis.Server.Interfaces
{
	public interface IConnectedClientsState
	{
		IReadOnlyDictionary<Guid, ConnectedClient> ConnectedClients { get; }
	}
}