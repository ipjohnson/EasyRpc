using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Errors
{
    public interface IErrorHandler
    {
        Task HandleUnauthorized(RequestExecutionContext context);

        Task DefaultErrorHandlerError(RequestExecutionContext context, Exception e);

        ValueTask<T> HandleDeserializeUnknownContentType<T>(RequestExecutionContext context);

        Task HandleSerializerUnknownContentType(RequestExecutionContext context);
    }
}
