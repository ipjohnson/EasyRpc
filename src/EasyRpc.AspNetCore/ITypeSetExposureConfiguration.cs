using System;
using System.Collections.Generic;
using System.Reflection;
using EasyRpc.AspNetCore.Middleware;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// configuration for set of types
    /// </summary>
    public interface ITypeSetExposureConfiguration
    {
        /// <summary>
        /// Function for picking name
        /// </summary>
        /// <param name="nameFunc"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration As(Func<Type, IEnumerable<string>> nameFunc);

        /// <summary>
        /// Authorize exposures
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Authorize(string role = null, string policy = null);

        /// <summary>
        /// Use func for providing authorization
        /// </summary>
        /// <param name="authorizationFunc"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Authorize(Func<Type, IEnumerable<IMethodAuthorization>> authorizationFunc);

        /// <summary>
        /// Expose interfaces from type set
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Interfaces(Func<Type, bool> filter = null);

        /// <summary>
        /// Filter methods
        /// </summary>
        /// <param name="methodFilter"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Methods(Func<MethodInfo, bool> methodFilter);

        /// <summary>
        /// Expose types, this is the default
        /// </summary>
        /// <param name="filter">filter out types to be exported</param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Types(Func<Type, bool> filter = null);

        /// <summary>
        /// Expose types that match filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        ITypeSetExposureConfiguration Where(Func<Type, bool> filter);
    }
}
