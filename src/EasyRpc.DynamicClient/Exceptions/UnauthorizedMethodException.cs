using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class UnauthorizedMethodException : DynamicMethodException
    {
        public UnauthorizedMethodException(string method) : base(method, "Unauthorized access to this method")
        {

        }
    }
}
