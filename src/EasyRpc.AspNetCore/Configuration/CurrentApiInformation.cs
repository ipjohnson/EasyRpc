using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;

namespace EasyRpc.AspNetCore.Configuration
{
    public class CurrentApiInformation : ICurrentApiInformation
    {
        public CurrentApiInformation(
            ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>> authorizations, 
            ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>> filters, 
            ImmutableLinkedList<Func<Type, IEnumerable<string>>> prefixes, 
            ImmutableLinkedList<Func<MethodInfo, bool>> methodFilters,
            bool supportResponseCompression, 
            ExposeDefaultMethod defaultMethod, 
            ServiceActivationMethod serviceActivationMethod, 
            IServiceProvider serviceProvider, 
            IConfigurationManager configurationMethods, 
            ImmutableLinkedList<IResponseHeader> headers)

        {
            Authorizations = authorizations;
            Filters = filters;
            Prefixes = prefixes;
            MethodFilters = methodFilters;
            SupportResponseCompression = supportResponseCompression;
            DefaultMethod = defaultMethod;
            ServiceActivationMethod = serviceActivationMethod;
            ServiceProvider = serviceProvider;
            ConfigurationMethods = configurationMethods;
            Headers = headers;
        }

        public string BasePath { get; }

        public ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>> Authorizations { get; }

        public ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>> Filters { get; }

        public ImmutableLinkedList<Func<Type, IEnumerable<string>>> Prefixes { get; }

        public ImmutableLinkedList<Func<MethodInfo, bool>> MethodFilters { get; }

        public ImmutableLinkedList<IResponseHeader> Headers { get; }

        public bool SupportResponseCompression { get; }

        public ExposeDefaultMethod DefaultMethod { get; }

        public ServiceActivationMethod ServiceActivationMethod { get; }

        public IServiceProvider ServiceProvider { get; }

        public IConfigurationManager ConfigurationMethods { get; }
    }
}
