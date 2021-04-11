using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Interface for configuring http methods directly
    /// </summary>
    public interface IHttpMethodConfiguration
    {
        #region GET

        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TResult>(string path, Func<TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TArg1, TResult>(string path, Func<TArg1, TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TArg1, TArg2, TResult>(string path,
            Func<TArg1, TArg2, TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TArg1, TArg2, TArg3, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TArg1, TArg2, TArg3, TArg4, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Register simple GET method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Expose GET method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TResult>(string path, Func<RequestExecutionContext, TResult> method)
        {
            return Handle(HttpMethods.Get, path, method);
        }

        /// <summary>
        /// Expose string as a GET method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get(string path, string stringValue)
        {
            var stringValueBytes = Encoding.UTF8.GetBytes(stringValue);

            return Handle(HttpMethods.Get, path, () => stringValueBytes).Raw("text/plain");
        }

        #endregion

        #region POST

        /// <summary>
        /// Register simple POST method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TResult>(string path, Func<TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        /// <summary>
        /// Register simple POST method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TArg1, TResult>(string path, Func<TArg1, TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        /// <summary>
        /// Register simple POST method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TArg1, TArg2, TResult>(string path,
            Func<TArg1, TArg2, TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        /// <summary>
        /// Register simple POST method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TArg1, TArg2, TArg3, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        /// <summary>
        /// Register simple POST method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TArg1, TArg2, TArg3, TArg4, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        /// <summary>
        /// Register simple POST method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        /// <summary>
        /// Expose POST async method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TResult>(string path, Func<RequestExecutionContext, TResult> method)
        {
            return Handle(HttpMethods.Post, path, method);
        }

        #endregion

        #region PATCH

        /// <summary>
        /// Register simple PATCH method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TResult>(string path, Func<TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        /// <summary>
        /// Register simple PATCH method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TArg1, TResult>(string path, Func<TArg1, TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        /// <summary>
        /// Register simple PATCH method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TArg1, TArg2, TResult>(string path,
            Func<TArg1, TArg2, TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        /// <summary>
        /// Register simple PATCH method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TArg1, TArg2, TArg3, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        /// <summary>
        /// Register simple PATCH method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TArg1, TArg2, TArg3, TArg4, TResult>(string path,
            Func<TArg1, TArg2, TArg3,TArg4, TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        /// <summary>
        /// Register simple PATCH method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        /// <summary>
        /// Expose PATCH method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TResult>(string path, Func<RequestExecutionContext, TResult> method)
        {
            return Handle(HttpMethods.Patch, path, method);
        }

        #endregion

        #region PUT

        /// <summary>
        /// Register simple PUT method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TResult>(string path, Func<TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        /// <summary>
        /// Register simple PUT method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TArg1, TResult>(string path, Func<TArg1, TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        /// <summary>
        /// Register simple PUT method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TArg1, TArg2, TResult>(string path,
            Func<TArg1, TArg2, TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        /// <summary>
        /// Register simple PUT method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TArg1, TArg2, TArg3, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        /// <summary>
        /// Register simple PUT method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TArg1, TArg2, TArg3, TArg4, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        /// <summary>
        /// Register simple PUT method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        /// <summary>
        /// Expose PUT method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TResult>(string path, Func<RequestExecutionContext, TResult> method)
        {
            return Handle(HttpMethods.Put, path, method);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Register simple DELETE method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Delete<TArg1, TResult>(string path, Func<TArg1, TResult> method)
        {
            return Handle(HttpMethods.Delete, path, method);
        }

        /// <summary>
        /// Register simple DELETE method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Delete<TArg1, TArg2, TResult>(string path,
            Func<TArg1, TArg2, TResult> method)
        {
            return Handle(HttpMethods.Delete, path, method);
        }

        /// <summary>
        /// Register simple DELETE method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Delete<TArg1, TArg2, TArg3, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TResult> method)
        {
            return Handle(HttpMethods.Delete, path, method);
        }

        /// <summary>
        /// Register simple DELETE method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Delete<TArg1, TArg2, TArg3, TArg4, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TResult> method)
        {
            return Handle(HttpMethods.Delete, path, method, false);
        }

        /// <summary>
        /// Register simple DELETE method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Delete<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string path,
            Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> method)
        {
            return Handle(HttpMethods.Delete, path, method);
        }

        /// <summary>
        /// Expose DELETE method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Delete<TResult>(string path, Func<RequestExecutionContext, TResult> method)
        {
            return Handle(HttpMethods.Delete, path, method);
        }

        #endregion

        #region Handle

        /// <summary>
        /// Handle custom HTTP method
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">path</param>
        /// <param name="method">method to execute</param>
        /// <param name="hasRequestBody">does the request have a body</param>
        /// <returns></returns>
        IExposureDelegateConfiguration Handle<TResult>(string httpMethod, string path, Func<TResult> method, bool? hasRequestBody = null);

        /// <summary>
        /// Handle custom HTTP method
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">path</param>
        /// <param name="method">method to execute</param>
        /// <param name="hasRequestBody">does the request have a body</param>
        /// <returns></returns>
        IExposureDelegateConfiguration
            Handle<TArg, TResult>(string httpMethod, string path,
                Func<TArg, TResult> method, bool? hasRequestBody = null);

        /// <summary>
        /// Handle custom HTTP method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">path</param>
        /// <param name="method">method to execute</param>
        /// <param name="hasRequestBody">does the request have a body</param>
        /// <returns></returns>
        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TResult>(string httpMethod, string path,
                Func<TArg1, TArg2, TResult> method, bool? hasRequestBody = null);

        /// <summary>
        /// Handle custom HTTP method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">path</param>
        /// <param name="method">method to execute</param>
        /// <param name="hasRequestBody">does the request have a body</param>
        /// <returns></returns>
        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TArg3, TResult>(string httpMethod, string path,
                Func<TArg1, TArg2, TArg3, TResult> method, bool? hasRequestBody = null);

        /// <summary>
        /// Handle custom HTTP method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">path</param>
        /// <param name="method">method to execute</param>
        /// <param name="hasRequestBody">does the request have a body</param>
        /// <returns></returns>
        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TArg3, TArg4, TResult>(string httpMethod, string path,
                Func<TArg1, TArg2, TArg3, TArg4, TResult> method, bool? hasRequestBody = null);

        /// <summary>
        /// Handle custom HTTP method
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">path</param>
        /// <param name="method">method to execute</param>
        /// <param name="hasRequestBody">does the request have a body</param>
        /// <returns></returns>
        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string httpMethod, string path,
                Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> method, bool? hasRequestBody = null);

        #endregion

    }
}
