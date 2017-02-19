using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Configure api 
    /// </summary>
    public interface IApiConfiguration
    {
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
        IExposureConfiguration Expose<T>();
    }
}
