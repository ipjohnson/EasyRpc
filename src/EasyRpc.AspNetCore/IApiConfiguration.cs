using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
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
    public partial interface IApiConfiguration
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
        IApiConfiguration Authorize(Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>> authorizations);

        /// <summary>
        /// Clear authorize flags
        /// </summary>
        /// <returns></returns>
        IApiConfiguration ClearAuthorize();

        /// <summary>
        /// Configuration options that apply for all end points
        /// </summary>
        IEnvironmentConfiguration Configure { get; }
        
        /// <summary>
        /// Apply prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        IApiConfiguration Prefix(string prefix);

        /// <summary>
        /// Prefix function returns list of prefixes based on type
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
        IApiConfiguration ApplyFilter<T>(Func<MethodInfo, bool> where = null, bool shared = false) where T : IRequestFilter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterFunc"></param>
        /// <returns></returns>
        IApiConfiguration ApplyFilter(
            Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>> filterFunc);
        
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
        /// Application services
        /// </summary>
        IServiceProvider AppServices { get; }
        
        /// <summary>
        /// Set the default http method to be used when exposing method
        /// </summary>
        /// <param name="defaultMethod"></param>
        /// <returns></returns>
        IApiConfiguration DefaultHttpMethod(ExposeDefaultMethod defaultMethod);
    }

    /// <summary>
    /// Http method to use when exposing methods
    /// </summary>
    public enum ExposeDefaultMethod
    {
        /// <summary>
        /// Always use post when no method configured through Attribute or Fluent
        /// </summary>
        PostOnly,

        /// <summary>
        /// Post when there are parameters, Get when no parameters
        /// </summary>
        PostAndGet,

        /// <summary>
        /// Post when more than int parameters, Get when no parameters and int
        /// </summary>
        PostAndGetInt,

        /// <summary>
        /// Get Only methods
        /// </summary>
        GetOnly
    }
}
