using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Sample.Service
{
    public class StringService
    {
        public string Hello(string name)
        {
            return "Hello " + name;
        }
    }
}
