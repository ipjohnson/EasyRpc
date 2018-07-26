using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodParameter
    {
        string Name { get; }

        Type ParameterType { get; }

        int Position { get; }

        bool HasDefaultValue { get; }

        object DefaultValue { get; }

        IEnumerable<Attribute> Attributes { get; }
    }

    public class ExposedMethodParameter : IExposedMethodParameter
    {
        public string Name { get; set; }

        public Type ParameterType { get; set; }

        public int Position { get; set; }

        public bool HasDefaultValue { get; set; }

        public object DefaultValue { get; set; }

        public IEnumerable<Attribute> Attributes { get; set; }
    }
}
