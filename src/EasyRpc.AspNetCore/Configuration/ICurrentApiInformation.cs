using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface ICurrentApiInformation
    {
        /// <summary>
        /// Authorizations to apply
        /// </summary>
        ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>> Authorizations { get; }

        /// <summary>
        /// Filters to apply
        /// </summary>
        ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>> Filters { get; }

        /// <summary>
        /// Prefixes to apply
        /// </summary>
        ImmutableLinkedList<Func<Type, IEnumerable<string>>> Prefixes { get; }

        /// <summary>
        /// List of method filters
        /// </summary>
        ImmutableLinkedList<Func<MethodInfo, bool>> MethodFilters { get; }

        bool SupportResponseCompression { get; }

        ExposeDefaultMethod DefaultMethod { get; }
        
        /// <summary>
        /// Default way to activate services
        /// </summary>
        ServiceActivationMethod ServiceActivationMethod { get; }

        IServiceProvider ServiceProvider { get; }

        IConfigurationManager ConfigurationMethods { get; }
    }
}
