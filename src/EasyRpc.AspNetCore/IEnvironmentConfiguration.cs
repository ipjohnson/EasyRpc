using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Interface for configuring rpc environment
    /// </summary>
    public interface IEnvironmentConfiguration
    {
        /// <summary>
        /// Configuration action to be applied to the rpc environment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueAction"></param>
        /// <returns></returns>
        IEnvironmentConfiguration Action<T>(Action<T> valueAction);
    }
}
