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
        private readonly MethodInfo _methodInfo;
        private readonly INamedParameterToArrayDelegateProvider _namedDelegateProvider;
        private readonly IOrderedParameterToArrayDelegateProvider _orderedDelegateProvider;
        private readonly IArrayMethodInvokerBuilder _invokerBuilder;

        public ExposedMethodCache(MethodInfo methodInfo,
                                  string methodName,
                                  IMethodAuthorization[] authorizations,
                                  Func<HttpContext, IEnumerable<ICallFilter>>[] filters,
                                  INamedParameterToArrayDelegateProvider namedDelegateProvider,
                                  IOrderedParameterToArrayDelegateProvider orderedDelegateProvider,
                                  IArrayMethodInvokerBuilder invokerBuilder)
        {
            _methodInfo = methodInfo;
            Authorizations = authorizations;
            Filters = filters;
            _namedDelegateProvider = namedDelegateProvider;
            _orderedDelegateProvider = orderedDelegateProvider;
            _invokerBuilder = invokerBuilder;
            MethodName = methodName;
            InstanceType = methodInfo.DeclaringType;
        }

        public Type InstanceType { get; }

        public string MethodName { get; }

        public IMethodAuthorization[] Authorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public OrderedParametersToArray OrderedParameterToArrayDelegate =>
            _orderedParametersToArray ??
            (_orderedParametersToArray = _orderedDelegateProvider.CreateDelegate(_methodInfo));

        public NamedParametersToArray NamedParametersToArrayDelegate =>
            _namedParametersToArray ??
            (_namedParametersToArray = _namedDelegateProvider.CreateDelegate(_methodInfo));

        public InvokeMethodWithArray InvokeMethod =>
            _invokeMethod ??
            (_invokeMethod = _invokerBuilder.CreateMethodInvoker(_methodInfo));
    }
}
