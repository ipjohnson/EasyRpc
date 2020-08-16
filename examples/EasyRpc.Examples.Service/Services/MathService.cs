using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.Examples.Service.Views;
using EasyRpc.Examples.SharedDefinitions;

namespace EasyRpc.Examples.Service.Services
{
    public class MathService : IMathService
    {
        /// <inheritdoc />
        public async Task<int> Add(int a, int b)
        {
            return a + b;
        }
        
        public async Task<List<int>> All(TestStruct? blah)
        {
            return new List<int> { 1, 2, 3, 4 };
        }

        [GetMethod]
        [ReturnView]
        public async Task<List<ResultClass>> AddView()
        {
            return new List<ResultClass> { new ResultClass { Result = 5 }, new ResultClass { Result = 10 } };
        }
    }

    public struct TestStruct
    {
        public string Blah { get; set; }
    }

    public class ResultClass
    {
        public int Result { get; set; }
    }
}
