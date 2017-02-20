using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Sample.Service
{
    public interface IMultiplyService
    {
        int Multiply(int a, int b);
    }

    public class MultiplyService : IMultiplyService
    {
        public int Multiply(int a, int b)
        {
            return a * b;
        }
    }
}
