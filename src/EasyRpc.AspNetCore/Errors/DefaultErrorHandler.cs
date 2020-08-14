﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Errors
{
    /// <summary>
    /// Default error handler
    /// </summary>
    public class DefaultErrorHandler : IErrorHandler, IApiConfigurationCompleteAware
    {
        /// <summary>
        /// Error wrapping service
        /// </summary>
        protected IErrorWrappingService ErrorWrappingService;

        /// <summary>
        /// Content serialization service
        /// </summary>
        protected IContentSerializationService ContentSerializationService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="errorWrappingService"></param>
        public DefaultErrorHandler(IErrorWrappingService errorWrappingService)
        {
            ErrorWrappingService = errorWrappingService;
        }

        /// <inheritdoc />
        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            ContentSerializationService = serviceScope.GetRequiredService<IContentSerializationService>();
        }
        
        /// <inheritdoc />
        public virtual Task HandleUnauthorized(RequestExecutionContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated ?? false)
            {
                return Return403(context);
            }

            return Return401(context);
        }

        /// <inheritdoc />
        public virtual Task HandleException(RequestExecutionContext context, Exception e)
        {
            context.HttpStatusCode = (int)HttpStatusCode.InternalServerError;

            context.Result = ErrorWrappingService.WrapError(context, e);

            return ContentSerializationService.SerializeToResponse(context);
        }

        /// <inheritdoc />
        public virtual ValueTask<T> HandleDeserializeUnknownContentType<T>(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;

            return default;
        }
        
        /// <inheritdoc />
        public virtual Task HandleSerializerUnknownContentType(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;

            return default;
        }

        /// <summary>
        /// Return 403 error
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Task Return403(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Return 401 error
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Task Return401(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.CompletedTask;
        }
    }
}
