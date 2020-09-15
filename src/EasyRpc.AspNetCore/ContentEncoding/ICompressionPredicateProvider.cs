using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.ContentEncoding
{
    public interface ICompressionPredicateProvider
    {
        Action<RequestExecutionContext> ProvideCompressionPredicate(IEndPointMethodConfigurationReadOnly configuration);
    }
}
