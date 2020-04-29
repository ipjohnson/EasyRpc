using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Filters
{
    /// <summary>
    /// Attribute methods or classes with instance filter
    /// </summary>
    public class InstanceFilterAttribute : Attribute, IRequestFilterAttribute
    {
        private readonly Type _filterType;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filterType"></param>
        public InstanceFilterAttribute(Type filterType)
        {
            _filterType = filterType;
        }

        /// <summary>
        /// Is this filter shared
        /// </summary>
        public bool Shared { get; set; } = false;

        /// <inheritdoc />
        public IEnumerable<Func<RequestExecutionContext, IRequestFilter>> ProvideFilters(ICurrentApiInformation currentApi, IEndPointMethodConfigurationReadOnly configurationReadOnly)
        {
            if (Shared)
            {
                var instance =
                    (IRequestFilter) ActivatorUtilities.CreateInstance(currentApi.ServiceProvider, _filterType);

                yield return context => instance;
            }
            else
            {
                yield return context =>
                    (IRequestFilter)ActivatorUtilities.CreateInstance(context.HttpContext.RequestServices,
                        _filterType);
            }
        }
    }
}
