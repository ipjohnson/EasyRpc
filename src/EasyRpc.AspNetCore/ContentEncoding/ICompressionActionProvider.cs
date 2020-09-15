using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.ContentEncoding
{
    public interface ICompressionActionProvider
    {
        Action<RequestExecutionContext> ProvideCompressionAction(IEndPointMethodConfigurationReadOnly configuration);
    }
}
