using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Sample.Service
{
    public class IntMathService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Subract(int a, int b)
        {
            return a - b;
        }
    }
}
