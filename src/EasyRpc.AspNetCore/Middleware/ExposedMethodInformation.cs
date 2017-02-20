using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public class ExposedMethodInformation
    {
        public ExposedMethodInformation(Type type, IEnumerable<string> names, string methodName, MethodInfo method, IMethodAuthorization[] methodAuthorizations)
        {
            Type = type;
            Names = names;
            MethodName = methodName;
            Method = method;
            MethodAuthorizations = methodAuthorizations;
        }

        public Type Type { get; }

        public IEnumerable<string> Names { get; }

        public string MethodName { get; }

        public MethodInfo Method { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }
    }
}
