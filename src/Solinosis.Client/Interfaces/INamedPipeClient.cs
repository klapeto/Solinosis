namespace Solinosis.Client.Interfaces
{
	public interface INamedPipeClient
	{
		void Connect();

		void Disconnect();

		T GetServiceProxy<T>() where T : class;
	}
}