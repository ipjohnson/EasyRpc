using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Headers
{
    public class ResponseHeaderAttribute : Attribute
    {
        public ResponseHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }
    }
}
