using System;
using System.Collections.Generic;
using System.Linq;
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

        Type InstanceType { get; }

        IMethodAuthorization[] MethodAuthorizations { get; }

        Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        InvokeMethodWithArray InvokeMethod { get; }

        IEnumerable<IExposedMethodParameter> Parameters { get; }

        string ObsoleteMessage { get; }

        object GetSerializerData(int serializerId);

        void SetSerializerData(int serializerId, object serializerData);
    }

    public class ExposedMethodInformation : BaseExposedMethodInformation, IExposedMethodInformation
    {
        private readonly IArrayMethodInvokerBuilder _invokeMethodBuilder;
        private readonly bool _allowCompression;
        private InvokeMethodWithArray _invokeMethod;

        public ExposedMethodInformation(Type type,
            IEnumerable<string> routeNames,
            string methodName,
            MethodInfo method,
            IMethodAuthorization[] methodAuthorizations,
            Func<HttpContext, IEnumerable<ICallFilter>>[] filters,
            IInstanceActivator instanceActivator,
            IArrayMethodInvokerBuilder invokeMethodBuilder,
            bool allowCompression,
            string obsoleteMessage)
        {
            _invokeMethodBuilder = invokeMethodBuilder;
            _allowCompression = allowCompression;
            Type = type;
            RouteNames = routeNames;
            MethodName = methodName;
            MethodInfo = method;
            MethodAuthorizations = methodAuthorizations;
            Filters = filters;
            ObsoleteMessage = obsoleteMessage;
            InstanceType = MethodInfo.DeclaringType;
            InstanceProvider = (context, provider) => instanceActivator.ActivateInstance(context, provider, Type);
            Parameters = GenerateParameters();
        }

        public Type Type { get; }

        public Func<HttpContext, IServiceProvider, object> InstanceProvider { get; }

        public IEnumerable<string> RouteNames { get; }

        public string MethodName { get; }

        public MethodInfo MethodInfo { get; }

        public Type InstanceType { get; }

        public IMethodAuthorization[] MethodAuthorizations { get; }

        public Func<HttpContext, IEnumerable<ICallFilter>>[] Filters { get; }

        public string ObsoleteMessage { get; }

        public InvokeMethodWithArray InvokeMethod => _invokeMethod ??
            (_invokeMethod = _invokeMethodBuilder.CreateMethodInvoker(MethodInfo, _allowCompression));
        
        public IEnumerable<IExposedMethodParameter> Parameters { get; }

        private IExposedMethodParameter[] GenerateParameters()
        {
            var parameters = MethodInfo.GetParameters().Select(p =>
                (IExposedMethodParameter)new ExposedMethodParameter
                {
                    Name = p.Name,
                    Position = p.Position,
                    HasDefaultValue = p.HasDefaultValue,
                    DefaultValue = p.HasDefaultValue ? p.DefaultValue : null,
                    ParameterType = p.ParameterType,
                    Attributes = p.GetCustomAttributes<Attribute>(),
                    ParameterInfo = p
                }).ToArray();

            return parameters.Length > 0 ? parameters : Array.Empty<IExposedMethodParameter>();
        }
    }
}
