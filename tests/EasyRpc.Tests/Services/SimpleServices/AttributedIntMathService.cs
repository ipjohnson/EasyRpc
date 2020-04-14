using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;

namespace EasyRpc.Tests.Services.SimpleServices
{
    public class AttributedIntMathService
    {
        [GetMethod("/RandomNumber")]
        public GenericResult<int> RandomNumber()
        {
            var random = new Random();
            var result = random.Next();

            if (result == 0)
            {
                result++;
            }

            return new GenericResult<int> { Result =  result};
        }

        [GetMethod("/RandomNumberAsync")]
        public async ValueTask<GenericResult<int>> RandomNumberAsync()
        {
            var random = new Random();
            var result = random.Next();

            if (result == 0)
            {
                result++;
            }

            return new GenericResult<int> { Result = 8 };
        }

        [PostMethod("/AddGenericResult")]
        public GenericResult<int> AddGenericResult(int a, int b)
        {
            return new GenericResult<int> { Result = a + b };
        }
    }
}
