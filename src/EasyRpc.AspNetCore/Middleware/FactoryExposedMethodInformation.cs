using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class FactoryExposedMethodInformation : IExposedMethodInformation
    {
        public FactoryExposedMethodInformation(Type type, IEnumerable<string> routeNames, string methodName, MethodInfo methodInfo, IMethodAuthorization[] methodAuthorizations, Func<HttpContext, IEnumerable<ICallFilter>>[] filters, InvokeMethodWithArray invokeMethod, IEnumerable<IExposedMethodParameter> parameters)
        {
            Type = type;
            RouteNames = routeNames;
            MethodName = methodName;
            MethodInfo = methodInfo;
            MethodAuthorizations = methodAuthorizations;
            Filters = filters;
            InvokeMethod = invokeMethod;
            Parameters = CalculateParameterInfo(methodInfo, parameters);
        }

        private IEnumerable<IExposedMethodParameter> CalculateParameterInfo(MethodInfo methodInfo, IEnumerable<IExposedMethodParameter> parameters)
        {
            var methodParameters = methodInfo.GetParameters();
            var calculatedParameters = new IExposedMethodParameter[methodParameters.Length];

            if (parameters != null)
            {
                foreach (var exposedMethodParameter in parameters)
                {
                    calculatedParameters[exposedMethodParameter.Position] = new ExposedMethodParameter
                    {
                        Name = exposedMethodParameter.Name,
                        Position = exposedMethodParameter.Position,
                        HasDefaultValue = exposedMethodParameter.HasDefaultValue,
                        DefaultValue = exposedMethodParameter.HasDefaultValue ? exposedMethodParameter.DefaultValue : null,
                        ParameterType = exposedMethodParameter.ParameterType,
                        Attributes = exposedMethodParameter.Attributes?.ToArray() ?? Array.Empty<Attribute>()
                    };
                }
            }

            for (var i = 0; i < calculatedParameters.Length; i++)
            {
                if (calculatedParameters[i] == null)
                {
                    var parameter = methodParameters[i];
                    calculatedParameters[i] = new ExposedMethodParameter
                    {
                        Name = parameter.Name,
                        Position = parameter.Position,
                        ParameterType = parameter.ParameterType,
                        DefaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : null,
                        Attributes = parameter.GetCustomAttributes<Attribute>()
                    };
                }
                else if (calculatedParameters[i].ParameterType == null)
                {
                    ((ExposedMethodParameter)calculatedParameters[i]).ParameterType = methodParameters[i].ParameterType;
                }
            }

            return calculatedParameters;
        }

        public Type Type { get; }

        public Func<HttpContext, IServiceProvider, object> InstanceProvider => (context, service) => null;

        public IEnumerable<string> RouteNames { get; }

        public string MethodName { get; }

        public MethodInfo MethodInfo { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public InvokeMethodWithArray InvokeMethod { get; }

        public string DocumentationMethodName => throw new NotImplementedException();

        public string DocumentationTypeName => throw new NotImplementedException();

        public IEnumerable<IExposedMethodParameter> Parameters { get; }
    }
}
