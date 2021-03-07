using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Response
{
    /// <summary>
    /// Used to define type that will be returned from a method if it can't be discerned from the signature
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReturnsTypeAttribute : Attribute
    {
        public ReturnsTypeAttribute(Type returnType)
        {
            ReturnType = returnType;
        }

        public Type ReturnType { get; }
    }
}
