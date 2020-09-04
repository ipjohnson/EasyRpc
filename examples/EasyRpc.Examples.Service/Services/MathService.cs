using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore.Views;
using EasyRpc.Examples.SharedDefinitions;

namespace EasyRpc.Examples.Service.Services
{
    /// <summary>
    /// Everything math related!
    /// </summary>
    public class MathService : IMathService
    {
        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="a">A parameter</param>
        /// <param name="b">B parameter</param>
        /// <returns></returns>
        public  Task<int> Add(int a, int b)
        {
            //await Task.Delay(1);

            return Task.FromResult(a + b);
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

    /// <summary>
    /// Result class
    /// </summary>
    public class ResultClass
    {
        /// <summary>
        /// Result value
        /// </summary>
        public int Result { get; set; }
    }
}
