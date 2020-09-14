using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Filters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.FluentValidation.Impl
{
    public class EasyRpcValidationFilter : IAsyncRequestExecutionFilter
    {
        private readonly IParameterValidator[] _validators;

        public EasyRpcValidationFilter(IParameterValidator[] validators)
        {
            _validators = validators;
        }

        public async Task BeforeExecute(RequestExecutionContext context)
        {
            foreach (var validator in _validators)
            {
                await validator.Execute(context);
            }
        }

        public Task AfterExecute(RequestExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
