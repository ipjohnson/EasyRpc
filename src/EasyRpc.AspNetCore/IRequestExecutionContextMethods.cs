using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Set of methods to register http context based handlers
    /// </summary>
    public interface IRequestExecutionContextMethods
    {
        /// <summary>
        /// Expose Get method without parameter binding
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TResult>(string path, Func<RequestExecutionContext, TResult> handler);

        /// <summary>
        /// Expose Get method with no parameter binding and request/response
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TResult>(string path, Func<HttpRequest, HttpResponse, TResult> handler)
        {
            return Get(path, context => handler(context.HttpContext.Request, context.HttpContext.Response));
        }
    }
}
