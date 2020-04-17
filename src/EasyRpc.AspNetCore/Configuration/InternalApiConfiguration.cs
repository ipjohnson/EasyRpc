using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Configuration
{
    public partial class InternalApiConfiguration : IInternalApiConfiguration
    {
        private ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>> _authorizations =
            ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>>.Empty;
        private ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>> _filters =
            ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>>>.Empty;
        private readonly ImmutableLinkedList<Func<MethodInfo, bool>> _methodFilters = ImmutableLinkedList<Func<MethodInfo, bool>>.Empty;
        private ImmutableLinkedList<Func<Type, IEnumerable<string>>> _prefixes = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;

        private readonly IConfigurationManager _configurationMethodRepository;

        private ICurrentApiInformation _currentApiInformation;
        private ExposeDefaultMethod _defaultMethod = ExposeDefaultMethod.PostOnly;

        private readonly IApplicationConfigurationService _applicationConfigurationService;
        private readonly IServiceProvider _applicationServiceProvider;
        
        public InternalApiConfiguration(IServiceProvider applicationServiceProvider)
        {
            _applicationConfigurationService = applicationServiceProvider.GetRequiredService<IApplicationConfigurationService>();
            Configure = applicationServiceProvider.GetRequiredService<IEnvironmentConfiguration>();
            _applicationServiceProvider = applicationServiceProvider;
            _configurationMethodRepository =
                applicationServiceProvider.GetRequiredService<IConfigurationManager>();
        }

        public IApiConfiguration Authorize(string role = null, string policy = null)
        {
            throw new NotImplementedException();
        }

        public IApiConfiguration Authorize(Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>> authorizations)
        {
            _authorizations = _authorizations.Add(authorizations);

            return this;
        }

        public IApiConfiguration ClearAuthorize()
        {
            _authorizations = ImmutableLinkedList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>>.Empty;

            ClearCurrentApi();

            return this;
        }

        public IEnvironmentConfiguration Configure { get; }


        public IApiConfiguration Prefix(string prefix)
        {
            var prefixArray = new [] {prefix};

            return Prefix(type => prefixArray);
        }

        public IApiConfiguration Prefix(Func<Type, IEnumerable<string>> prefixFunc)
        {
            _prefixes = _prefixes.Add(prefixFunc);

            return this;
        }

        public IApiConfiguration ClearPrefixes()
        {
            _prefixes = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;

            ClearCurrentApi();

            return this;
        }

        public IExposureConfiguration Expose(Type type)
        {
            var config = new TypeExposureConfiguration(GetCurrentApiInformation(), type);

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureConfiguration<T> Expose<T>()
        {
            var config = new TypeExposureConfiguration<T>(GetCurrentApiInformation());

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public ITypeSetExposureConfiguration Expose(IEnumerable<Type> types)
        {
            throw new NotImplementedException();
        }

        public IApiConfiguration ApplyFilter<T>(Func<MethodInfo, bool> where = null, bool shared = false) where T : IRequestFilter
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

        public IApiConfiguration ApplyFilter(Func<IEndPointMethodConfigurationReadOnly, Func<RequestExecutionContext, IRequestFilter>> filterFunc)
        {
            _filters = _filters.Add(filterFunc);

            return this;
        }
        
        public IApiConfiguration MethodFilter(Func<MethodInfo, bool> methodFilter)
        {
            throw new NotImplementedException();
        }

        public IApiConfiguration ClearMethodFilters()
        {
            throw new NotImplementedException();
        }

        public IServiceProvider AppServices => _applicationServiceProvider;
        
        public IApiConfiguration DefaultHttpMethod(ExposeDefaultMethod defaultMethod)
        {
            _defaultMethod = defaultMethod;
            return this;
        }

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
                _applicationServiceProvider, 
                _configurationMethodRepository);

            return _currentApiInformation;
        }

        public IReadOnlyList<IEndPointMethodHandler> GetEndPointHandlers()
        {
            return _applicationConfigurationService.ProvideEndPointHandlers();
        }

        protected void ClearCurrentApi()
        {
            _currentApiInformation = null;
        }
    }
}
