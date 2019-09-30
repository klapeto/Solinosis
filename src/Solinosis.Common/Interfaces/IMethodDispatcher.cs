namespace Solinosis.Common.Interfaces
{
	public interface IMethodDispatcher
	{
		object Call<T>(string methodName, object[] arguments);
	}
}