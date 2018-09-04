using System.Threading.Tasks;

namespace EasyRpc.Tests.Classes
{
    public interface IIntMathService
    {
        int Add(int a, int b);

        Task<int> AsyncAdd(int a, int b);
    }

    public class IntMathService : IIntMathService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public async Task<int> AsyncAdd(int a, int b)
        {
            return a + b;
        }
    }
}
