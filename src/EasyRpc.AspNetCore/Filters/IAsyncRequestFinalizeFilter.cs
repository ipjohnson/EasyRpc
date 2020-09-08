using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Filters
{
    /// <summary>
    /// Request filter that will be called after the response has been written
    /// </summary>
    public interface IAsyncRequestFinalizeFilter : IBaseRequestFinalizerFilter
    {
        /// <summary>
        /// Finalize request, called after response has been written
        /// </summary>
        /// <param name="context"></param>
        Task Finalize(RequestExecutionContext context);
    }
}
