using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Api configuration interface
    /// </summary>
    public partial interface IRpcApi
    {
        /// <summary>
        /// Set default Authorize
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        IRpcApi Authorize(string role = null, string policy = null);

        /// <summary>
        /// Apply authorize to types 
        /// </summary>
        /// <param name="authorizations"></param>
        /// <returns></returns>
        IRpcApi Authorize(Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>> authorizations);
        
        /// <summary>
        /// Clear authorize flags
        /// </summary>
        /// <returns></returns>
        IRpcApi ClearAuthorize();

        /// <summary>
        /// Configuration options that apply for all end points
        /// </summary>
        IEnvironmentConfiguration Environment { get; }

        /// <summary>
        /// Apply prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        IRpcApi Prefix(string prefix);

        /// <summary>
        /// Prefix function returns list of prefixes based on type
        /// </summary>
        /// <param name="prefixFunc"></param>
        /// <returns></returns>
        IRpcApi Prefix(Func<Type, IEnumerable<string>> prefixFunc);

        /// <summary>
        /// Clear prefixes
        /// </summary>
        /// <returns></returns>
        IRpcApi ClearPrefixes();

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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IRpcApi ExposeModule<T>()
        {
            return ExposeModules(new[] {typeof(T)});
        }

        /// <summary>
        /// Expose modules, if types are null expose entry assembly
        /// </summary>
        /// <returns></returns>
        IRpcApi ExposeModules(IEnumerable<Type> types = null);

        /// <summary>
        /// Expose individual HTTP method
        /// </summary>
        IHttpMethodConfiguration Method { get; }

        /// <summary>
        /// Add header to all responses
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IRpcApi Header(string header, string value);

        /// <summary>
        /// Clear all global headers
        /// </summary>
        /// <returns></returns>
        IRpcApi ClearHeaders();

        /// <summary>
        /// Apply call filter
        /// </summary>
        /// <returns></returns>
        IRpcApi ApplyFilter<T>(Func<MethodInfo, bool> where = null, bool shared = false) where T : IRequestFilter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterFunc"></param>
        /// <returns></returns>
        IRpcApi ApplyFilter(
            Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>> filterFunc);
        
        /// <summary>
        /// Add method filter
        /// </summary>
        /// <param name="methodFilter"></param>
        /// <returns></returns>
        IRpcApi MethodFilter(Func<MethodInfo, bool> methodFilter);

        /// <summary>
        /// Clear method filters
        /// </summary>
        /// <returns></returns>
        IRpcApi ClearMethodFilters();

        /// <summary>
        /// Application services
        /// </summary>
        IServiceProvider AppServices { get; }
        
        /// <summary>
        /// Set the default http method to be used when exposing method
        /// </summary>
        /// <param name="defaultMethod"></param>
        /// <returns></returns>
        IRpcApi DefaultHttpMethod(ExposeDefaultMethod defaultMethod);

        /// <summary>
        /// Clone the current api configuration
        /// </summary>
        /// <returns></returns>
        IRpcApi Clone();
    }

}
