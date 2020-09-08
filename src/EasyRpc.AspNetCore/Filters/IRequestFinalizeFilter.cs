using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Filters
{
    /// <summary>
    /// Request filter that will be called after the response has been written
    /// </summary>
    public interface IRequestFinalizeFilter : IBaseRequestFinalizerFilter
    {
        /// <summary>
        /// Finalize request, called after response has been written
        /// </summary>
        /// <param name="context"></param>
        void Finalize(RequestExecutionContext context);
    }
}
