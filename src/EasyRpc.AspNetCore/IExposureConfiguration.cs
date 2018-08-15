using System;
using System.Reflection;

namespace EasyRpc.AspNetCore
{
    public interface IExposureConfiguration
    {
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
    }

    public interface IExposureConfiguration<T>
    {
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
    }
}
