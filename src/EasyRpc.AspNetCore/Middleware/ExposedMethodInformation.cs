using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public class ExposedMethodInformation
    {
        public ExposedMethodInformation(Type type, IEnumerable<string> routeNames,string methodName, MethodInfo method, IMethodAuthorization[] methodAuthorizations)
        {
            Type = type;
            RouteNames = routeNames;
            MethodName = methodName;
            Method = method;
            MethodAuthorizations = methodAuthorizations;
        }

        public Type Type { get; }

        public IEnumerable<string> RouteNames { get; }

        public string MethodName { get; }

        public MethodInfo Method { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }
    }
}
