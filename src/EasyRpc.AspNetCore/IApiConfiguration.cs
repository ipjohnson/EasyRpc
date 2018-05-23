using System;
using System.Collections.Generic;
using System.Reflection;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Configure api 
    /// </summary>
    public interface IApiConfiguration
    {
        /// <summary>
        /// Set default Authorize
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        IApiConfiguration Authorize(string role = null, string policy = null);

        /// <summary>
        /// Apply authorize to types
        /// </summary>
        /// <param name="authorizations"></param>
        /// <returns></returns>
        IApiConfiguration Authorize(Func<Type, IEnumerable<IMethodAuthorization>> authorizations);

        /// <summary>
        /// Clear authorize flags
        /// </summary>
        /// <returns></returns>
        IApiConfiguration ClearAuthorize();

        /// <summary>
        /// Apply prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        IApiConfiguration Prefix(string prefix);

        /// <summary>
        /// List of 
        /// </summary>
        /// <param name="prefixFunc"></param>
        /// <returns></returns>
        IApiConfiguration Prefix(Func<Type, IEnumerable<string>> prefixFunc);
        
        /// <summary>
        /// Clear prefixes
        /// </summary>
        /// <returns></returns>
        IApiConfiguration ClearPrefixes();

        /// <summary>
        /// Expose type for RPC
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IExposureConfiguration Expose(Type type);

        /// <summary>
        /// Expose type for RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IExposureConfiguration<T> Expose<T>();

        /// <summary>
        /// Expose a set of types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Expose(IEnumerable<Type> types);

        /// <summary>
        /// Apply call filter
        /// </summary>
        /// <returns></returns>
        IApiConfiguration ApplyFilter<T>(Func<MethodInfo,bool> where = null) where T : ICallFilter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterFunc"></param>
        /// <returns></returns>
        IApiConfiguration ApplyFilter(Func<MethodInfo, Func<HttpContext, IEnumerable<ICallFilter>>> filterFunc);

        /// <summary>
        /// Naming conventions for api
        /// </summary>
        NamingConventions NamingConventions { get; }

        /// <summary>
        /// Add method filter
        /// </summary>
        /// <param name="methodFilter"></param>
        /// <returns></returns>
        IApiConfiguration MethodFilter(Func<MethodInfo, bool> methodFilter);

        /// <summary>
        /// Clear method filters
        /// </summary>
        /// <returns></returns>
        IApiConfiguration ClearMethodFilters();

        /// <summary>
        /// Add exposures to 
        /// </summary>
        /// <param name="provider"></param>
        IApiConfiguration AddExposures(IExposedMethodInformationProvider provider);

        /// <summary>
        /// Application services
        /// </summary>
        IServiceProvider AppServices { get; }

        /// <summary>
        /// By default documentation is on, this turns it off for this configuration
        /// </summary>
        void DisableDocumentation();
    }
}
