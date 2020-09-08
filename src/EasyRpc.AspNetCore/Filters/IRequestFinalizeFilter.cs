using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Filters
{
    public interface IRequestFinalizeFilter : IBaseRequestFinalizerFilter
    {
        /// <summary>
        /// Finalize request, called after response has been written
        /// </summary>
        /// <param name="context"></param>
        void Finalize(RequestExecutionContext context);
    }
}
