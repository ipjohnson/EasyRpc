using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Errors;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class CustomErrorHandler : DefaultErrorHandler
    {
        private readonly Action<Exception> _handleError;

        /// <inheritdoc />
        public CustomErrorHandler(IErrorWrappingService errorWrappingService, Action<Exception> handleError) : base(errorWrappingService)
        {
            _handleError = handleError;
        }

        /// <inheritdoc />
        public override Task HandleException(RequestExecutionContext context, Exception exception)
        {
            _handleError(exception);

            return base.HandleException(context, exception);
        }
    }
}
