using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.Examples.SharedDefinitions;

namespace EasyRpc.Examples.Service.Services
{
    public class MathService : IMathService
    {
        /// <inheritdoc />
        public async Task<IntResult> Add(int a, int b)
        {
            return new IntResult { result = a + b };
        }
    }
}
