using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore.DataAnnotations.Impl
{
    public class InstanceDataAnnotationFilter : ICallExecuteFilter
    {
        private readonly int _index;

        public InstanceDataAnnotationFilter(int index)
        {
            _index = index;
        }

        public void BeforeExecute(ICallExecutionContext context)
        {
            var instance = context.Parameters[_index];
            var validationContext = new ValidationContext(instance, context.Context.RequestServices, null);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(instance, validationContext, validationResults, true))
            {
                var errorMessage = validationResults.Aggregate("", (current, result) =>
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        current += result.ErrorMessage + Environment.NewLine;
                    }

                    return current;
                });

                if (context.ResponseMessage is ErrorResponseMessage currentResponse)
                {
                    errorMessage += currentResponse.Error;
                }

                context.ContinueCall = false;
                context.ResponseMessage = new ErrorResponseMessage(context.RequestMessage.Version, context.RequestMessage.Id, JsonRpcErrorCode.InvalidRequest, errorMessage);
            }
        }

        public void AfterExecute(ICallExecutionContext context)
        {

        }
    }
}
