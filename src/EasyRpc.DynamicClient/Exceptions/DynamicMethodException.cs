using System;
using System.Reflection;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class DynamicMethodException : Exception
    {
        public DynamicMethodException(string methodName, string message) : base(message)
        {
            Method = methodName;
        }

        public string Method { get; }
    }
}
