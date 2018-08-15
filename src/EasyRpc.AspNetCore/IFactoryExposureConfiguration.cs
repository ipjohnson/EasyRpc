using System;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Factory exposure
    /// </summary>
    public interface IFactoryExposureConfiguration
    {
        /// <summary>
        /// Define the methods that will be exposed.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        IFactoryExposureConfiguration Methods(Action<IFactoryMethodConfiguration> method);
    }

    /// <summary>
    /// Factory Methods
    /// </summary>
    public interface IFactoryMethodConfiguration
    {
        /// <summary>
        /// Export a Func under a specific name
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="methodName">name of method</param>
        /// <param name="func">function to export</param>
        /// <returns></returns>
        IFactoryMethodConfiguration Func<TResult>(string methodName, Func<TResult> func);

        /// <summary>
        /// Export action under a specific name
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IFactoryMethodConfiguration Action(string methodName, Action method);

        /// <summary>
        /// Export function under specific name
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        IFactoryMethodConfiguration Func<TIn, TResult>(string methodName, Func<TIn, TResult> method);

        IFactoryMethodConfiguration Action<TIn>(string methodName, Action<TIn> method);

        IFactoryMethodConfiguration Func<TIn1, TIn2, TResult>(string methodName, Func<TIn1, TIn2, TResult> method);

        IFactoryMethodConfiguration Action<TIn1, TIn2>(string methodName, Action<TIn1, TIn2> method);

        IFactoryMethodConfiguration Func<TIn1, TIn2, TIn3, TResult>(string methodName, Func<TIn1, TIn2, TIn3, TResult> method);

        IFactoryMethodConfiguration Action<TIn1, TIn2, TIn3>(string methodName, Action<TIn1, TIn2, TIn3> method);

        IFactoryMethodConfiguration Func<TIn1, TIn2, TIn3,TIn4, TResult>(string methodName, Func<TIn1, TIn2, TIn3,TIn4, TResult> method);

        IFactoryMethodConfiguration Action<TIn1, TIn2, TIn3, TIn4>(string methodName, Action<TIn1, TIn2, TIn3, TIn4> method);
    }
}
