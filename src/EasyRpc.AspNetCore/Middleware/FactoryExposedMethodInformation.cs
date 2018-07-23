using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class FactoryExposedMethodInformation : IExposedMethodInformation
    {
        public FactoryExposedMethodInformation(Type type, IEnumerable<string> routeNames, string methodName, MethodInfo methodInfo, IMethodAuthorization[] methodAuthorizations, Func<HttpContext, IEnumerable<ICallFilter>>[] filters, InvokeMethodWithArray invokeMethod)
        {
            Type = type;
            RouteNames = routeNames;
            MethodName = methodName;
            MethodInfo = methodInfo;
            MethodAuthorizations = methodAuthorizations;
            Filters = filters;
            InvokeMethod = invokeMethod;
        }

        public Type Type { get; }

        public Func<HttpContext, IServiceProvider, object> InstanceProvider => (context, service) => null;

        public IEnumerable<string> RouteNames { get; }

        public string MethodName { get; }

        public MethodInfo MethodInfo { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public InvokeMethodWithArray InvokeMethod { get; }
    }
}
