using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.Sample.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EasyRpc.AspNetCore.Sample.Service
{
    public class IntMathService : IIntMathService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Subract(int a, int b)
        {
            return a - b;
        }

        public int Multiply([FromServices]IMultiplyService multiplyService, int a, int b)
        {
            return multiplyService.Multiply(a, b);
        }
    }
}
