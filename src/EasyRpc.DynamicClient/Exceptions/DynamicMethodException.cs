using System;
using System.Reflection;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class DynamicMethodException : Exception
    {
        public DynamicMethodException(MethodInfo method, object[] arguments, string message) : base(message)
        {
            Method = method;
            Arguments = arguments;
        }

        public object[] Arguments { get; }

        public MethodInfo Method { get; }
    }
}
