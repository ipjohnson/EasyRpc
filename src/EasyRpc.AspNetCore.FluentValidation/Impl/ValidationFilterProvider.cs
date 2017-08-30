using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.FluentValidation.Impl
{
    public class ValidationFilterProvider
    {
        private readonly IServiceProvider _services;
        private readonly MethodInfo _getValidatorFilterMethod;

        public ValidationFilterProvider(IServiceProvider services)
        {
            _services = services;
            _getValidatorFilterMethod = GetType().GetTypeInfo().GetMethod("GetValidatorFilter", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public Func<HttpContext, IEnumerable<ICallFilter>> GetFilters(MethodInfo method)
        {
            List<ICallFilter> callFilters = new List<ICallFilter>();

            int index = 0;
            foreach (var parameter in method.GetParameters())
            {
                if (!parameter.ParameterType.GetTypeInfo().IsValueType)
                {
                    var closedMethod = _getValidatorFilterMethod.MakeGenericMethod(parameter.ParameterType);

                    var filter = (ICallFilter)closedMethod.Invoke(this, new object[] { index, parameter.Name });

                    if (filter != null)
                    {
                        callFilters.Add(filter);
                    }
                }

                index++;
            }

            if (callFilters.Count > 0)
            {
                return context => callFilters;
            }

            return null;
        }


        private ICallFilter GetValidatorFilter<T>(int index, string parameterName)
        {
            try
            {
                var validators = _services.GetServices<IValidator<T>>().ToArray();

                if (validators?.Length > 0)
                {
                    return new EasyRpcValidationFilter<T>(validators, index, parameterName, true);
                }
            }
            catch (Exception e)
            {

            }

            return null;
        }
    }
}
