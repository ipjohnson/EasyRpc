using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodCache
    {
        Type InstanceType { get; }

        Func<HttpContext, IServiceProvider, object> CreateInstance { get; }

        string MethodName { get; }

        MethodInfo Method { get; }

        IMethodAuthorization[] Authorizations { get; }

        Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        InvokeMethodWithArray InvokeMethod { get; }
    }

    public class ExposedMethodCache : IExposedMethodCache
    {
        private InvokeMethodWithArray _invokeMethod;
        private readonly IInstanceActivator _instanceActivator;

        public ExposedMethodCache(MethodInfo methodInfo,
                                  string methodName,
                                  IMethodAuthorization[] authorizations,
                                  Func<HttpContext, IEnumerable<ICallFilter>>[] filters,
                                  Func<HttpContext, IServiceProvider, object> activator, 
                                  InvokeMethodWithArray invokeMethod)
        {
            Method = methodInfo;
            Authorizations = authorizations;
            Filters = filters;
            MethodName = methodName;
            InstanceType = methodInfo.DeclaringType;
            CreateInstance = activator;
            InvokeMethod = invokeMethod;
        }

        public Type InstanceType { get; }

        public string MethodName { get; }

        public MethodInfo Method { get; }

        public IMethodAuthorization[] Authorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public InvokeMethodWithArray InvokeMethod { get; }

        public Func<HttpContext, IServiceProvider, object> CreateInstance { get; }
    }
}
