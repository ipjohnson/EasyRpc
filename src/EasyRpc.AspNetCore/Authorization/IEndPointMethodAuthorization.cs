﻿using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Authorization
{
    /// <summary>
    /// Interface representing an end point authorization
    /// </summary>
    public interface IEndPointMethodAuthorization
    {
        /// <summary>
        /// authorize request async
        /// </summary>
        /// <param name="context">request</param>
        /// <returns>true is authorized</returns>
        Task<bool> AsyncAuthorize(RequestExecutionContext context);
    }
}
