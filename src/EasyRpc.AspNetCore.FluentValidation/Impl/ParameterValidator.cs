using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.FluentValidation.Impl
{
    public interface IParameterValidator
    {
        Task Execute(RequestExecutionContext context);
    }

    public class ParameterValidator<T> : IParameterValidator
    {
        private int _position;
        private IErrorWrappingService _errorWrappingService;

        public ParameterValidator(IErrorWrappingService errorWrappingService, int position)
        {
            _errorWrappingService = errorWrappingService;
            _position = position;
        }

        public async Task Execute(RequestExecutionContext context)
        {
            var validators = context.HttpContext.RequestServices.GetServices<IValidator<T>>();

            var value = (T)context.Parameters[_position];

            foreach (var validator in validators)
            {
                var validationContext = new ValidationContext<T>(value);

                ValidationResult result;

                if (ShouldValidateAsynchronously(validator, validationContext))
                {
                    result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
                }
                else
                {
                    result = validator.Validate(validationContext);
                }

                if (!result.IsValid)
                {
                    SetErrorMessage(context, result);
                }
            }
        }

        private void SetErrorMessage(RequestExecutionContext requestExecutionContext, ValidationResult result)
        {
            requestExecutionContext.Result ??= _errorWrappingService.GetErrorWrapper();

            if (requestExecutionContext.Result is IErrorWrapper errorWrapper)
            {
                errorWrapper.Message += result.ToString();
                requestExecutionContext.HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity;
            }
        }

        private bool ShouldValidateAsynchronously(IValidator<T> validator, ValidationContext<T> validationContext)
        {
            if (validator is IEnumerable<IValidationRule> rules)
            {
                foreach (var rule in rules)
                {
                    if (rule.Validators.Any(x => x.ShouldValidateAsynchronously(validationContext)))
                        return true;
                }
            }

            return false;
        }
    }
}
