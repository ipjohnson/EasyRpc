using System;
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
    public class DefaultErrorHandler : IErrorHandler, IApiConfigurationCompleteAware
    {
        protected IErrorWrappingService ErrorWrappingService;
        protected IContentSerializationService ContentSerializationService;

        public DefaultErrorHandler(IErrorWrappingService errorWrappingService)
        {
            ErrorWrappingService = errorWrappingService;
        }

        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            ContentSerializationService = serviceScope.GetRequiredService<IContentSerializationService>();
        }

        public virtual Task HandleUnauthorized(RequestExecutionContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated ?? false)
            {
                return Return403(context);
            }

            return Return401(context);
        }

        public virtual Task DefaultErrorHandlerError(RequestExecutionContext context, Exception e)
        {
            context.HttpStatusCode = (int)HttpStatusCode.InternalServerError;

            context.Result = ErrorWrappingService.WrapError(context, e);

            return ContentSerializationService.SerializeToResponse(context);
        }

        public virtual ValueTask<T> HandleDeserializeUnknownContentType<T>(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;

            return default;
        }

        public virtual Task HandleSerializerUnknownContentType(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;

            return default;
        }

        protected virtual Task Return403(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            return Task.CompletedTask;
        }

        protected virtual Task Return401(RequestExecutionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.CompletedTask;
        }
    }
}
