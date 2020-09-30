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
            return Handle(HttpMethods.Get, path, method, false);
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
            return Handle(HttpMethods.Get, path, method, false);
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
            return Handle(HttpMethods.Get, path, method, false);
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
            return Handle(HttpMethods.Get, path, method, false);
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
            return Handle(HttpMethods.Get, path, method, false);
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
            return Handle(HttpMethods.Get, path, method, false);
        }
        /// <summary>
        /// Expose GET method with execution parameter
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="path"></param>
        /// <param name="getMethod"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Get<TResult>(string path, Func<RequestExecutionContext, TResult> getMethod)
        {
            return Handle(HttpMethods.Get, path, getMethod, false);
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
            return Handle(HttpMethods.Post, path, method, false);
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
        /// <param name="getMethod"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Post<TResult>(string path, Func<RequestExecutionContext, TResult> getMethod)
        {
            return Handle(HttpMethods.Post, path, getMethod);
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
            return Handle(HttpMethods.Patch, path, method, false);
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
        /// <param name="getMethod"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Patch<TResult>(string path, Func<RequestExecutionContext, TResult> getMethod)
        {
            return Handle(HttpMethods.Patch, path, getMethod);
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
            return Handle(HttpMethods.Put, path, method, false);
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
        /// <param name="getMethod"></param>
        /// <returns></returns>
        IExposureDelegateConfiguration Put<TResult>(string path, Func<RequestExecutionContext, TResult> getMethod)
        {
            return Handle(HttpMethods.Put, path, getMethod);
        }
        
        #endregion

        #region DELETE

        

        #endregion

        #region Handle

        IExposureDelegateConfiguration
            Handle<TResult>(string method, string path,Func<TResult> func, bool hasBody = true);

        IExposureDelegateConfiguration
            Handle<TArg, TResult>(string method, string path,
                Func<TArg, TResult> func, bool hasBody = true);

        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TResult>(string method, string path,
                Func<TArg1, TArg2, TResult> expression, bool hasBody = true);

        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TArg3, TResult>(string method, string path,
                Func<TArg1, TArg2, TArg3, TResult> expression, bool hasBody = true);

        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TArg3, TArg4, TResult>(string method, string path,
                Func<TArg1, TArg2, TArg3, TArg4, TResult> expression, bool hasBody = true);

        IExposureDelegateConfiguration
            Handle<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string method, string path,
                Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> expression, bool hasBody = true);


        #endregion

    }
}
