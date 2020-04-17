using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// base interface for result wrapper
    /// </summary>
    public interface IResultWrapper
    {

    }

    /// <summary>
    /// Interface for wrapping result value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResultWrapper<T> : IResultWrapper
    {
        /// <summary>
        /// Result for call
        /// </summary>
        T Result { get; set; }
    }
}
