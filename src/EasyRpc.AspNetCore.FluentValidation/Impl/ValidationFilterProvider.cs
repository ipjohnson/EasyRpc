using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore.FluentValidation.Impl
{
    public class ValidationFilterProvider
    {
        private IErrorWrappingService _errorWrappingService;

        public ValidationFilterProvider(IErrorWrappingService errorWrappingService)
        {
            _errorWrappingService = errorWrappingService;
        }

        public Func<RequestExecutionContext, IRequestFilter> GetFilters(IEndPointMethodConfigurationReadOnly method)
        {
            var parameterValidators = new List<IParameterValidator>();

            foreach (var parameter in method.Parameters)
            {
                if (!parameter.ParamType.IsValueType)
                {
                    var validatorType = typeof(ParameterValidator<>).MakeGenericType(parameter.ParamType);

                    var validator = 
                        (IParameterValidator)Activator.CreateInstance(validatorType,  _errorWrappingService, parameter.Position);

                    parameterValidators.Add(validator);
                }
            }

            if (parameterValidators.Count > 0)
            {
                var filter = new EasyRpcValidationFilter(parameterValidators.ToArray());

                return context => filter;
            }

            return null;
        }
    }
}
