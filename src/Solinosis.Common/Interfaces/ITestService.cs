using System.Threading.Tasks;

namespace Solinosis.Common.Interfaces
{
	public interface ITestService
	{
		Task<string> TestCall(string arg);
	}
}