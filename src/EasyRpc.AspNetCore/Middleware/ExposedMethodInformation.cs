using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class ExposedMethodInformation
    {
        public ExposedMethodInformation(Type type, IEnumerable<string> routeNames,string methodName, MethodInfo method, IMethodAuthorization[] methodAuthorizations, Func<HttpContext, IEnumerable<ICallFilter>>[] filters)
        {
            Type = type;
            RouteNames = routeNames;
            MethodName = methodName;
            MethodInfo = method;
            MethodAuthorizations = methodAuthorizations;
            Filters = filters;
        }

        public Type Type { get; }

        public IEnumerable<string> RouteNames { get; }

        public string MethodName { get; }

        public MethodInfo MethodInfo { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }
    }
}
