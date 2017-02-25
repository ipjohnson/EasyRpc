using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class MethodNotFoundException : DynamicMethodException
    {
        public MethodNotFoundException(MethodInfo method, object[] arguments) : base(method, arguments, $"Could not find method for {method.DeclaringType?.Name}.{method.Name}")
        {

        }
    }
}
