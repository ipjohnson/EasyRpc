using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class UnauthorizedMethodException : DynamicMethodException
    {
        public UnauthorizedMethodException(MethodInfo method, object[] arguments) : base(method, arguments, "Unauthorized access to this method")
        {

        }
    }
}
