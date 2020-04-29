using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration;

namespace EasyRpc.AspNetCore.Filters
{
    /// <summary>
    /// Interface for attributes that produce a filter
    /// </summary>
    public interface IRequestFilterAttribute
    {
        /// <summary>
        /// Provide a list of filters
        /// </summary>
        /// <returns></returns>
        IEnumerable<Func<RequestExecutionContext,IRequestFilter>> ProvideFilters(ICurrentApiInformation currentApi, IEndPointMethodConfigurationReadOnly configurationReadOnly);
    }
}
