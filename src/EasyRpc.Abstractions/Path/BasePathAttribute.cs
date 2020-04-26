using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Interface for describing a base path attribute
    /// </summary>
    public interface IBasePathAttribute
    {
        /// <summary>
        /// Base path
        /// </summary>
        string BasePath { get; }
    }

    /// <inheritdoc cref="IBasePathAttribute"/>
    public class BasePathAttribute : Attribute, IBasePathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="basePath"></param>
        public BasePathAttribute(string basePath)
        {
            BasePath = basePath;
        }

        /// <inheritdoc />
        public string BasePath { get; }
    }
}
