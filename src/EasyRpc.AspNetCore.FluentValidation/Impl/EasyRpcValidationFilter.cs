using System;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.Messages;
using FluentValidation;
using FluentValidation.Results;

namespace EasyRpc.AspNetCore.FluentValidation.Impl
{
    public class EasyRpcValidationFilter<T> : ICallExecuteFilter
    {
        private readonly IValidator<T>[] _validators;
        private readonly int _parameterIndex;
        private readonly string _parameterName;
        private readonly bool _required;

        public EasyRpcValidationFilter(IValidator<T>[] validators, int parameterIndex, string parameterName, bool required)
        {
            _validators = validators;
            _parameterIndex = parameterIndex;
            _parameterName = parameterName;
            _required = required;
        }

        public void BeforeExecute(ICallExecutionContext context)
        {
            var objectValue = context.Parameters[_parameterIndex];

            if (objectValue == null)
            {
                if (_required)
                {
                    SetErrorResponse(context, $"{_parameterName} is required");
                }

                return;
            }

            var validationErrors = ImmutableLinkedList<ValidationFailure>.Empty;

            var value = (T)objectValue;

            for (var i = 0; i < _validators.Length; i++)
            {
                var result = _validators[i].Validate(value);

                if (!result.IsValid)
                {
                    validationErrors = validationErrors.AddRange(result.Errors);
                }
            }

            if (validationErrors == ImmutableLinkedList<ValidationFailure>.Empty) return;

            var errorMessage = $"Validation Errors {_parameterName}" + Environment.NewLine;

            var failed = false;

            foreach (var failure in validationErrors)
            {
                if (failure.Severity == Severity.Error)
                {
                    failed = true;
                }

                errorMessage += failure.ErrorMessage;
            }

            if (failed)
            {
                SetErrorResponse(context, errorMessage);
            }
        }
        
        public void AfterExecute(ICallExecutionContext context)
        {

        }

        private static void SetErrorResponse(ICallExecutionContext context, string errorMessage)
        {
            context.ContinueCall = false;

            if (context.ResponseMessage is ErrorResponseMessage currentError)
            {
                errorMessage = currentError.Error + errorMessage;
            }

            context.ResponseMessage = new ErrorResponseMessage(JsonRpcErrorCode.InvalidRequest, errorMessage, 
                context.RequestMessage.Version, context.RequestMessage.Id);
        }
    }
}
