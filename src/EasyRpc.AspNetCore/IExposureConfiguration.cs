using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Exposure configuration
    /// </summary>
    public interface IExposureConfiguration
    {
        /// <summary>
        /// Activation func
        /// </summary>
        /// <param name="activationFunc"></param>
        /// <returns></returns>
        IExposureConfiguration Activation(Func<RequestExecutionContext, object> activationFunc);

        /// <summary>
        /// Expose type as a particular name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IExposureConfiguration As(string name);

        /// <summary>
        /// Assign authorization to particular exposed type
        /// </summary>
        /// <param name="role">optional role</param>
        /// <param name="policy">optional policy</param>
        /// <returns></returns>
        IExposureConfiguration Authorize(string role = null, string policy = null);

        /// <summary>
        /// Filter out methods
        /// </summary>
        /// <param name="methods"></param>
        /// <returns></returns>
        IExposureConfiguration Methods(Func<MethodInfo, bool> methods);

        /// <summary>
        /// Mark services as obsolete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IExposureConfiguration Obsolete(string message);
    }

    /// <summary>
    /// Typed Exposure configuration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExposureConfiguration<T>
    {
        /// <summary>
        /// Provide mechanism for activating type
        /// </summary>
        /// <param name="activationFunc"></param>
        /// <returns></returns>
        IExposureConfiguration<T> Activation(Func<RequestExecutionContext, T> activationFunc);

        /// <summary>
        /// Expose type as a specific name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IExposureConfiguration<T> As(string name);

        /// <summary>
        /// Add authorization rule to export
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        IExposureConfiguration<T> Authorize(string role = null, string policy = null);

        /// <summary>
        /// Export only particular methods
        /// </summary>
        /// <param name="methods"></param>
        /// <returns></returns>
        IExposureConfiguration<T> Methods(Func<MethodInfo, bool> methods);

        /// <summary>
        /// Mark service as obsolete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IExposureConfiguration<T> Obsolete(string message);
    }
}
