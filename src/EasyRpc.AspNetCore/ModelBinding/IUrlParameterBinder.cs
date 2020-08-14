using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.ModelBinding
{
    /// <summary>
    /// Interface for binding url/query string values to a request parameter object
    /// </summary>
    public interface IUrlParameterBinder
    {
        /// <summary>
        /// bind parameters
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="parameter"></param>
        /// <param name="parameterContext"></param>
        /// <param name="currentIndex"></param>
        /// <param name="pathSpan"></param>
        void BindUrlParameter(RequestExecutionContext context,
            EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext,
            ref int currentIndex, in ReadOnlySpan<char> pathSpan);
    }
}
