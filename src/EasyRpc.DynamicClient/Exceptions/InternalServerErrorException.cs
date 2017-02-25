using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class InternalServerErrorException : DynamicMethodException
    {
        public InternalServerErrorException(MethodInfo method, object[] arguments, string message) : base(method, arguments, message)
        {
        }
    }
}
