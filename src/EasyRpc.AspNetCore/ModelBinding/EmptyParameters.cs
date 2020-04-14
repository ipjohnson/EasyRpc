using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public class EmptyParameters : IRequestParameters
    {
        private static readonly IReadOnlyList<IRpcParameterInfo> _parameterInfos = Array.Empty<IRpcParameterInfo>();

        public bool TryGetParameter(string parameterName, out object parameterValue)
        {
            parameterValue = null;

            return false;
        }

        public bool TrySetParameter(string parameterName, object parameterValue)
        {
            return false;
        }

        public IReadOnlyList<IRpcParameterInfo> ParameterInfos => _parameterInfos;

        public object this[int index]
        {
            get => throw new IndexOutOfRangeException();
            set => throw new IndexOutOfRangeException();
        }

        public int ParameterCount => 0;

        public IRequestParameters Clone()
        {
            return this;
        }
    }
}
