using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface IHttpMethodConfiguration
    {
        /// <summary>
        /// Expose GET method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="getMethod"></param>
        /// <returns></returns>
        IRpcApi Get<TResult>(string path, Func<RequestExecutionContext, TResult> getMethod);

        /// <summary>
        /// Expose GET Method with request and response parameters
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="getMethod"></param>
        /// <returns></returns>
        IRpcApi Get<TResult>(string path, Func<HttpRequest, HttpResponse, TResult> getMethod);
    }
}
