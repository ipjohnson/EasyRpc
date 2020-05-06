using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Errors
{
    /// <summary>
    /// Error handling service
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handle unauthorized request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleUnauthorized(RequestExecutionContext context);

        /// <summary>
        /// Handle exception from request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task HandleException(RequestExecutionContext context, Exception e);

        /// <summary>
        /// Handle unknown content type error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        ValueTask<T> HandleDeserializeUnknownContentType<T>(RequestExecutionContext context);

        /// <summary>
        /// Handle error where no content serializer can be found
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleSerializerUnknownContentType(RequestExecutionContext context);
    }
}
