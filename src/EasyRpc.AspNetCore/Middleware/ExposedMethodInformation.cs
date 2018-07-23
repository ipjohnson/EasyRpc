using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodInformation
    {
        Type Type { get; }

        Func<HttpContext, IServiceProvider, object> InstanceProvider { get; }

        IEnumerable<string> RouteNames { get; }

        string MethodName { get; }

        MethodInfo MethodInfo { get; }

        IMethodAuthorization[] MethodAuthorizations { get; }

        Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        InvokeMethodWithArray InvokeMethod { get; }
    }

    public class ExposedMethodInformation : IExposedMethodInformation
    {
        private readonly IArrayMethodInvokerBuilder _invokeMethodBuilder;
        private readonly bool _allowCompression;

        public ExposedMethodInformation(Type type,
            IEnumerable<string> routeNames,
            string methodName,
            MethodInfo method,
            IMethodAuthorization[] methodAuthorizations,
            Func<HttpContext, IEnumerable<ICallFilter>>[] filters,
            IInstanceActivator instanceActivator,
            IArrayMethodInvokerBuilder invokeMethodBuilder,
            bool allowCompression)
        {
            _invokeMethodBuilder = invokeMethodBuilder;
            _allowCompression = allowCompression;
            Type = type;
            RouteNames = routeNames;
            MethodName = methodName;
            MethodInfo = method;
            MethodAuthorizations = methodAuthorizations;
            Filters = filters;
            InstanceProvider = (context, provider) => instanceActivator.ActivateInstance(context, provider, Type);
        }

        public Type Type { get; }

        public Func<HttpContext, IServiceProvider, object> InstanceProvider { get; }

        public IEnumerable<string> RouteNames { get; }

        public string MethodName { get; }

        public MethodInfo MethodInfo { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public InvokeMethodWithArray InvokeMethod =>
            _invokeMethodBuilder.CreateMethodInvoker(MethodInfo, _allowCompression);
    }
}
