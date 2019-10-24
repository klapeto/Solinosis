using System.Threading.Tasks;

namespace Solinosis.Common.Interfaces
{
	public interface ITestCallbackService
	{
		string TestCallback(string arg);
		Task<string> TestCallbackAsync(string arg);
	}
}