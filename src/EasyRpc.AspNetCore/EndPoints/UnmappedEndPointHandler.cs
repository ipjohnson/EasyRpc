using System;
using System.Collections.Generic;
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

    /// <inheritdoc />
    public class UnmappedEndPointHandler : IUnmappedEndPointHandler
    {
        private readonly IOptionsEndPointHandler _optionsEndPointHandler;
        private readonly IDocumentationService _documentationService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="documentationService"></param>
        /// <param name="optionsEndPointHandler"></param>
        public UnmappedEndPointHandler(IDocumentationService documentationService, 
            IOptionsEndPointHandler optionsEndPointHandler)
        {
            _documentationService = documentationService;
            _optionsEndPointHandler = optionsEndPointHandler;
        }

        /// <inheritdoc />
        public Task HandleUnmatched(HttpContext httpContext, RequestDelegate next)
        {
            if (httpContext.Request.Method == HttpMethods.Options &&
                httpContext.Request.Path == "/*")
            {
                return _optionsEndPointHandler.HandleServerOptionsRequest(httpContext, next);
            }

            return _documentationService.Execute(httpContext, next);
        }

        /// <inheritdoc />
        public Task HandleMatchedButNoMethod(HttpContext httpContext, RequestDelegate next, IEndPointMethodHandler[] handlers)
        {
            if (httpContext.Request.Method == HttpMethods.Options)
            {
                return _optionsEndPointHandler.HandlePathOptionRequest(httpContext, next, handlers);
            }

            return _documentationService.Execute(httpContext, next);
        }

        /// <inheritdoc />
        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            _documentationService.Configure(apiInformation, endPointMethodHandlersList);
        }
    }
}
