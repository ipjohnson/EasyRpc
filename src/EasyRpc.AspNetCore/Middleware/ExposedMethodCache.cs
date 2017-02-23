using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodCache
    {
        Type InstanceType { get; }

        string MethodName { get; }

        IMethodAuthorization[] Authorizations { get; }

        Func<HttpContext,IEnumerable<ICallFilter>>[] Filters { get; }

        InvokeMethodByNamedParameters NamedParametersExecution { get; }

        InvokeMethodOrderedParameters OrderedParametersExecution { get; }
    }

    public class ExposedMethodCache : IExposedMethodCache
    {
        private InvokeMethodOrderedParameters _invokeMethodOrdered;
        private InvokeMethodByNamedParameters _invokeMethodByNamed;
        private readonly MethodInfo _methodInfo;
        private readonly IOrderedParameterMethodInvokeBuilder _orderedBuilder;
        private readonly INamedParameterMethodInvokerBuilder _namedBuilder;
        
        public ExposedMethodCache(MethodInfo methodInfo, string methodName, IOrderedParameterMethodInvokeBuilder orderedBuilder, INamedParameterMethodInvokerBuilder namedBuilder, IMethodAuthorization[] authorizations, Func<HttpContext, IEnumerable<ICallFilter>>[] filters)
        {
            _methodInfo = methodInfo;
            _orderedBuilder = orderedBuilder;
            _namedBuilder = namedBuilder;
            Authorizations = authorizations;
            Filters = filters;
            MethodName = methodName;
            InstanceType = methodInfo.DeclaringType;
        }

        public Type InstanceType { get; }

        public string MethodName { get; }

        public IMethodAuthorization[] Authorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public InvokeMethodByNamedParameters NamedParametersExecution =>
            _invokeMethodByNamed ??
            (_invokeMethodByNamed = _namedBuilder.BuildInvokeMethodByNamedParameters(_methodInfo));

        public InvokeMethodOrderedParameters OrderedParametersExecution =>
            _invokeMethodOrdered ?? 
            (_invokeMethodOrdered = _orderedBuilder.BuildInvokeMethodOrderedParameters(_methodInfo));
    }
}
