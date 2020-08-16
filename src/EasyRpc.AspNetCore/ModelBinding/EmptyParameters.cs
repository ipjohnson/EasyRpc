using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.ModelBinding
{
    /// <inheritdoc />
    public class EmptyParameters : IRequestParameters
    {
        private static readonly IReadOnlyList<IRpcParameterInfo> _parameterInfos = Array.Empty<IRpcParameterInfo>();

        /// <summary>
        /// Instance of empty parameters
        /// </summary>
        public static readonly EmptyParameters Instance = new EmptyParameters();

        /// <inheritdoc />
        public bool TryGetParameter(string parameterName, out object parameterValue)
        {
            parameterValue = null;

            return false;
        }

        /// <inheritdoc />
        public bool TrySetParameter(string parameterName, object parameterValue)
        {
            return false;
        }

        /// <inheritdoc />
        public IReadOnlyList<IRpcParameterInfo> ParameterInfos => _parameterInfos;

        /// <inheritdoc />
        public object this[int index]
        {
            get => throw new IndexOutOfRangeException();
            set => throw new IndexOutOfRangeException();
        }

        /// <inheritdoc />
        public int ParameterCount => 0;

        /// <inheritdoc />
        public IRequestParameters Clone()
        {
            return this;
        }
    }
}
