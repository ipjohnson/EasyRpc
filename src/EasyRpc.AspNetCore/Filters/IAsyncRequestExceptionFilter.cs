using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Filters
{
    public interface IAsyncRequestExceptionFilter : IRequestFilter
    {
        /// <summary>
        /// Handle exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        Task HandleException(RequestExecutionContext context, Exception exception);
    }
}
