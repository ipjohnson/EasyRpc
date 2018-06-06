using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodCache
    {
        Type InstanceType { get; }

        string MethodName { get; }

        MethodInfo Method { get; }

        IMethodAuthorization[] Authorizations { get; }

        Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        OrderedParametersToArray OrderedParameterToArrayDelegate { get; }

        NamedParametersToArray NamedParametersToArrayDelegate { get; }

        InvokeMethodWithArray InvokeMethod { get; }
    }

    public class ExposedMethodCache : IExposedMethodCache
    {
        private NamedParametersToArray _namedParametersToArray;
        private OrderedParametersToArray _orderedParametersToArray;
        private InvokeMethodWithArray _invokeMethod;
        private readonly INamedParameterToArrayDelegateProvider _namedDelegateProvider;
        private readonly IOrderedParameterToArrayDelegateProvider _orderedDelegateProvider;
        private readonly IArrayMethodInvokerBuilder _invokerBuilder;
        private readonly bool _allowCompression;

        public ExposedMethodCache(MethodInfo methodInfo,
                                  string methodName,
                                  IMethodAuthorization[] authorizations,
                                  Func<HttpContext, IEnumerable<ICallFilter>>[] filters,
                                  INamedParameterToArrayDelegateProvider namedDelegateProvider,
                                  IOrderedParameterToArrayDelegateProvider orderedDelegateProvider,
                                  IArrayMethodInvokerBuilder invokerBuilder,
                                  bool allowCompression)
        {
            Method = methodInfo;
            Authorizations = authorizations;
            Filters = filters;
            _namedDelegateProvider = namedDelegateProvider;
            _orderedDelegateProvider = orderedDelegateProvider;
            _invokerBuilder = invokerBuilder;
            _allowCompression = allowCompression;
            MethodName = methodName;
            InstanceType = methodInfo.DeclaringType;
        }

        public Type InstanceType { get; }

        public string MethodName { get; }

        public MethodInfo Method { get; }

        public IMethodAuthorization[] Authorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public OrderedParametersToArray OrderedParameterToArrayDelegate =>
            _orderedParametersToArray ??
            (_orderedParametersToArray = _orderedDelegateProvider.CreateDelegate(Method));

        public NamedParametersToArray NamedParametersToArrayDelegate =>
            _namedParametersToArray ??
            (_namedParametersToArray = _namedDelegateProvider.CreateDelegate(Method));

        public InvokeMethodWithArray InvokeMethod =>
            _invokeMethod ??
            (_invokeMethod = _invokerBuilder.CreateMethodInvoker(Method, _allowCompression));
    }
}
