using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public interface IUrlParameterBinder
    {
        void BindUrlParameter(RequestExecutionContext context,
            EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext,
            ref int currentIndex, in ReadOnlySpan<char> pathSpan);
    }
}
