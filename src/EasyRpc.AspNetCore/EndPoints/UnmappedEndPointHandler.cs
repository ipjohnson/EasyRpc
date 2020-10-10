using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Documentation;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// Handles unmapped requests
    /// </summary>
    public interface IUnmappedEndPointHandler
    {
        /// <summary>
        /// Handle unmapped request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        Task HandleUnmatched(HttpContext httpContext, RequestDelegate next);

        /// <summary>
        /// Handle unmapped request that matches path but hot http method
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="next"></param>
        /// <param name="handlers"></param>
        /// <returns></returns>
        Task HandleMatchedButNoMethod(HttpContext httpContext, RequestDelegate next, IEndPointMethodHandler[] handlers);

        /// <summary>
        /// Configure handler
        /// </summary>
        /// <param name="apiInformation"></param>
        /// <param name="endPointMethodHandlersList"></param>
        void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);
    }

    public class UnmappedEndPointHandler : IUnmappedEndPointHandler, IApiConfigurationCompleteAware
    {
        private readonly IDefaultHttpMethodHandler[] _methodHandlers;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="methodHandlers"></param>
        public UnmappedEndPointHandler(IEnumerable<IDefaultHttpMethodHandler> methodHandlers)
        {
            
            _methodHandlers = methodHandlers.ToArray();
        }

        /// <inheritdoc />
        public Task HandleUnmatched(HttpContext httpContext, RequestDelegate next)
        {
            foreach (var handler in _methodHandlers)
            {
                if (handler.CanHandle(httpContext, false, null))
                {
                    return handler.HandleUnmatched(httpContext, next);
                }
            }

            return next(httpContext);
        }

        /// <inheritdoc />
        public Task HandleMatchedButNoMethod(HttpContext httpContext, RequestDelegate next, IEndPointMethodHandler[] handlers)
        {
            foreach (var handler in _methodHandlers)
            {
                if (handler.CanHandle(httpContext, true, handlers))
                {
                    return handler.HandleMatchedPath(httpContext, next, handlers);
                }
            }

            return next(httpContext);
        }

        /// <inheritdoc />
        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            
        }

        /// <inheritdoc />
        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            foreach (var handler in _methodHandlers)
            {
                if (handler is IApiConfigurationCompleteAware aware)
                {
                    aware.ApiConfigurationComplete(serviceScope);
                }
            }
        }
    }
}
