using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.Examples.SharedDefinitions
{
    public class IntResult
    {
        public int result { get; set; }
    }

    public interface IMathService
    {
        Task<int> Add(int a, int b);
    }
}
