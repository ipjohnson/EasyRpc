using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Configuration
{
    /// <summary>
    /// Internal implementation for api configuration
    /// </summary>
    public partial class InternalApiConfiguration : IInternalApiConfiguration
    {
        private ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>> _authorizations =
            ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>>.Empty;
        private ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>> _filters =
            ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>>.Empty;
        private ImmutableLinkedList<Func<MethodInfo, bool>> _methodFilters = ImmutableLinkedList<Func<MethodInfo, bool>>.Empty;
        private ImmutableLinkedList<Func<Type, IEnumerable<string>>> _prefixes = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;
        private ImmutableLinkedList<IResponseHeader> _responseHeaders = ImmutableLinkedList<IResponseHeader>.Empty;

        private readonly IConfigurationManager _configurationMethodRepository;

        private ICurrentApiInformation _currentApiInformation;
        private ExposeDefaultMethod _defaultMethod = ExposeDefaultMethod.PostOnly;


        private readonly IAuthorizationImplementationProvider _authorizationImplementationProvider;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="applicationServiceProvider"></param>
        /// <param name="authorizationImplementationProvider"></param>
        public InternalApiConfiguration(IServiceProvider applicationServiceProvider, 
            IAuthorizationImplementationProvider authorizationImplementationProvider)
        {
            ApplicationConfigurationService = applicationServiceProvider.GetRequiredService<IApplicationConfigurationService>();
            Configure = applicationServiceProvider.GetRequiredService<IEnvironmentConfiguration>();
            AppServices = applicationServiceProvider;
            _authorizationImplementationProvider = authorizationImplementationProvider;
            _configurationMethodRepository =
                applicationServiceProvider.GetRequiredService<IConfigurationManager>();
        }

        /// <inheritdoc />
        public IRpcApi Authorize(string role = null, string policy = null)
        {
            IEndPointMethodAuthorization authorization;

            if (!string.IsNullOrEmpty(role))
            {
                authorization = _authorizationImplementationProvider.UserHasRole(role);
            }
            else if (!string.IsNullOrEmpty(policy))
            {
                authorization = _authorizationImplementationProvider.UserHasPolicy(policy);
            }
            else
            {
                authorization = _authorizationImplementationProvider.Authorized();
            }

            return Authorize(endPoint => new[] {authorization});
        }

        /// <inheritdoc />
        public IRpcApi Authorize(Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>> authorizations)
        {
            _authorizations = _authorizations.Add(authorizations);

            return this;
        }

        /// <inheritdoc />
        public IRpcApi ClearAuthorize()
        {
            _authorizations = ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>>.Empty;

            ClearCurrentApi();

            return this;
        }

        /// <inheritdoc />
        public IEnvironmentConfiguration Configure { get; }

        /// <inheritdoc />
        public IRpcApi Prefix(string prefix)
        {
            var prefixArray = new [] {prefix};

            return Prefix(type => prefixArray);
        }

        /// <inheritdoc />
        public IRpcApi Prefix(Func<Type, IEnumerable<string>> prefixFunc)
        {
            _prefixes = _prefixes.Add(prefixFunc);

            return this;
        }

        /// <inheritdoc />
        public IRpcApi ClearPrefixes()
        {
            _prefixes = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;

            ClearCurrentApi();

            return this;
        }

        /// <inheritdoc />
        public IExposureConfiguration Expose(Type type)
        {
            var config = new TypeExposureConfiguration(GetCurrentApiInformation(), type);

            ApplicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        /// <inheritdoc />
        public IExposureConfiguration<T> Expose<T>()
        {
            var config = new TypeExposureConfiguration<T>(GetCurrentApiInformation());

            ApplicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Expose(IEnumerable<Type> types)
        {
            var config = new TypeSetExposureConfiguration(GetCurrentApiInformation(), types);

            ApplicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IRpcApi ExposeModules(IEnumerable<Type> types = null)
        {
            types ??= Assembly.GetEntryAssembly()?.ExportedTypes;

            if (types != null)
            {
                ExposeModuleTypes(types);
            }

            return this;
        }
        
        /// <inheritdoc />
        public IRpcApi Header(string header, string value)
        {
            _responseHeaders = _responseHeaders.Add(new ResponseHeader.ResponseHeader(header, value));

            return this;
        }

        /// <inheritdoc />
        public IRpcApi ClearHeaders()
        {
            _responseHeaders = ImmutableLinkedList<IResponseHeader>.Empty;
            
            return this;
        }

        /// <inheritdoc />
        public IRpcApi ApplyFilter<T>(Func<MethodInfo, bool> where = null, bool shared = false) where T : IRequestFilter
        {
            if (where == null)
            {
                where = methodInfo => true;
            }

            return ApplyFilter(methodInfo =>
                {
                    if (where(methodInfo.InvokeInformation.Signature))
                    {
                        return context => ActivatorUtilities.CreateInstance<T>(context.HttpContext.RequestServices);
                    }

                    return null;
                });
        }

        /// <inheritdoc />
        public IRpcApi ApplyFilter(Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>> filterFunc)
        {
            _filters = _filters.Add(filterFunc);

            return this;
        }

        /// <inheritdoc />
        public IRpcApi MethodFilter(Func<MethodInfo, bool> methodFilter)
        {
            _methodFilters = _methodFilters.Add(methodFilter);

            return this;
        }

        /// <inheritdoc />
        public IRpcApi ClearMethodFilters()
        {
            _methodFilters = ImmutableLinkedList<Func<MethodInfo, bool>>.Empty;

            return this;
        }

        /// <inheritdoc />
        public IServiceProvider AppServices { get; }

        /// <inheritdoc />
        public IRpcApi DefaultHttpMethod(ExposeDefaultMethod defaultMethod)
        {
            _defaultMethod = defaultMethod;

            return this;
        }

        /// <inheritdoc />
        public IRpcApi Clone()
        {
            return new InternalApiConfiguration(AppServices, _authorizationImplementationProvider)
            {
                _authorizations = _authorizations,
                _defaultMethod = _defaultMethod,
                _filters = _filters,
                _methodFilters = _methodFilters,
                _responseHeaders = _responseHeaders,
                _prefixes = _prefixes,
                _currentApiInformation = _currentApiInformation
            };
        }

        /// <inheritdoc />
        public IApplicationConfigurationService ApplicationConfigurationService { get; }

        /// <inheritdoc />
        public ICurrentApiInformation GetCurrentApiInformation()
        {
            if (_currentApiInformation != null)
            {
                return _currentApiInformation;
            }

            _currentApiInformation = new CurrentApiInformation(
                _authorizations, 
                _filters, 
                _prefixes, 
                _methodFilters, 
                false, 
                _defaultMethod, 
                ServiceActivationMethod.ActivationUtility, 
                AppServices, 
                _configurationMethodRepository,
                _responseHeaders);

            return _currentApiInformation;
        }

        /// <inheritdoc />
        public IReadOnlyList<IEndPointMethodHandler> GetEndPointHandlers()
        {
            return ApplicationConfigurationService.ProvideEndPointHandlers();
        }

        /// <summary>
        /// Gets a static copy of the current api information
        /// </summary>
        protected void ClearCurrentApi()
        {
            _currentApiInformation = null;
        }

        private void ExposeModuleTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                if (type.GetTypeInfo().GetInterface(nameof(IRpcModule)) != null)
                {
                    var rpcModule = ActivatorUtilities.CreateInstance(AppServices, type) as IRpcModule;

                    rpcModule?.Configure(Clone());
                }
            }
        }
    }
}
