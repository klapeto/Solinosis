namespace Solinosis.Server.Interfaces
{
	public interface INamedPipeServerHost
	{
		T GetCallbackProxy<T>() where T : class;
		void Start();
		void Stop();
	}
}