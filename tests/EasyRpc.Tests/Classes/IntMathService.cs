using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRPC.AspNetCore.Tests.Classes
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
