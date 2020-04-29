using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// configuration object for exposure expression
    /// </summary>
    public interface IExposureExpressionConfiguration
    {
        /// <summary>
        /// Require authorization for method
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration Authorize(string role = null, string policy = null);

        /// <summary>
        /// Return raw content as a specific content type
        /// </summary>
        /// <param name="contentType">Content-Type header value (i.e. text/plain, etc.)</param>
        /// <returns></returns>
        IExposureExpressionConfiguration Raw(string contentType);
        
        /// <summary>
        /// Adds response header
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IExposureExpressionConfiguration Header(string header, string value);
    }
}
