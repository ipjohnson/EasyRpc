using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.DynamicClient.Exceptions
{
    public class InvalidRequestException : DynamicMethodException
    {
        public InvalidRequestException(string methodName, string message) : base(methodName, message)
        {

        }
    }
}
