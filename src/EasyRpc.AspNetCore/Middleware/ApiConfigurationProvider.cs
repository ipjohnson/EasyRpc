using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Middleware
{

    public interface IApiConfigurationProvider : IExposedMethodInformationProvider
    {
        ICurrentApiInformation GetCurrentApiInformation();
    }

    public class ApiConfigurationProvider : IApiConfigurationProvider, IApiConfiguration
    {
        private readonly List<IExposedMethodInformationProvider> _providers =
            new List<IExposedMethodInformationProvider>();

        private ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>> _authorizations =
            ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>>.Empty;

        private ImmutableLinkedList<Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>>> _filters =
            ImmutableLinkedList<Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>>>.Empty;

        private ImmutableLinkedList<Func<MethodInfo, bool>> _methodFilters = ImmutableLinkedList<Func<MethodInfo, bool>>.Empty;

        private ImmutableLinkedList<Func<Type, IEnumerable<string>>> _prefixes = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;

        /// <summary>
        /// Set default Authorize
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public IApiConfiguration Authorize(string role = null, string policy = null)
        {
            IMethodAuthorization authorization = null;

            if (!string.IsNullOrEmpty(role))
            {
                authorization = new UserRoleAuthorization(role);
            }
            else if (!string.IsNullOrEmpty(policy))
            {
                authorization = new UserPolicyAuthorization(policy);
            }
            else
            {
                authorization = new UserAuthenticatedAuthorization();
            }
            var array = new[] { authorization };

            _authorizations = _authorizations.Add(t => array);

            return this;
        }

        /// <summary>
        /// Apply authorize to types
        /// </summary>
        /// <param name="authorizations"></param>
        /// <returns></returns>
        public IApiConfiguration Authorize(Func<Type, IEnumerable<IMethodAuthorization>> authorizations)
        {
            if (authorizations == null) throw new ArgumentNullException(nameof(authorizations));

            _authorizations = _authorizations.Add(authorizations);

            return this;
        }

        /// <summary>
        /// Clear authorize flags
        /// </summary>
        /// <returns></returns>
        public IApiConfiguration ClearAuthorize()
        {
            _authorizations = ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>>.Empty;

            return this;
        }

        /// <summary>
        /// Apply prefix 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IApiConfiguration Prefix(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            var array = new[] { prefix };

            _prefixes = _prefixes.Add(t => array);

            return this;
        }

        /// <summary>
        /// List of 
        /// </summary>
        /// <param name="prefixFunc"></param>
        /// <returns></returns>
        public IApiConfiguration Prefix(Func<Type, IEnumerable<string>> prefixFunc)
        {
            if (prefixFunc == null) throw new ArgumentNullException(nameof(prefixFunc));

            _prefixes = _prefixes.Add(prefixFunc);

            return this;
        }

        /// <summary>
        /// Clear prefixes
        /// </summary>
        /// <returns></returns>
        public IApiConfiguration ClearPrefixes()
        {
            _prefixes = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;

            return this;
        }

        /// <summary>
        /// Expose type for RPC
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IExposureConfiguration Expose(Type type)
        {
            var config = new ExposureConfiguration(type, GetCurrentApiInformation());

            _providers.Add(config);

            return config;
        }

        /// <summary>
        /// Expose type for RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IExposureConfiguration<T> Expose<T>()
        {
            var config = new ExposureConfiguration<T>(GetCurrentApiInformation());

            _providers.Add(config);

            return config;
        }

        /// <summary>
        /// Expose a set of types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Expose(IEnumerable<Type> types)
        {
            var typeSetConfiguration = new TypeSetExposureConfiguration(types, GetCurrentApiInformation());

            _providers.Add(typeSetConfiguration);

            return typeSetConfiguration;
        }

        /// <summary>
        /// Apply call filter
        /// </summary>
        /// <returns></returns>
        public IApiConfiguration ApplyFilter<T>(Func<Type,bool> where = null) where T : ICallFilter
        {
            _filters = _filters.Add(t =>
            {
                if (where?.Invoke(t) ?? true)
                {
                    return CreateFilter<T>;
                }

                return null;
            });

            return this;
        }

        private static IEnumerable<ICallFilter> CreateFilter<T>(HttpContext context) where T : ICallFilter
        {
            yield return ActivatorUtilities.GetServiceOrCreateInstance<T>(context.RequestServices);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterFunc"></param>
        /// <returns></returns>
        public IApiConfiguration ApplyFilter(Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>> filterFunc)
        {
            if (filterFunc == null) throw new ArgumentNullException(nameof(filterFunc));

            _filters = _filters.Add(filterFunc);

            return this;
        }

        /// <summary>
        /// Naming conventions for api
        /// </summary>
        public NamingConventions NamingConventions { get; } = new NamingConventions();

        /// <summary>
        /// Add method filter
        /// </summary>
        /// <param name="methodFilter"></param>
        /// <returns></returns>
        public IApiConfiguration MethodFilter(Func<MethodInfo, bool> methodFilter)
        {
            if (methodFilter == null) throw new ArgumentNullException(nameof(methodFilter));
            
            _methodFilters = _methodFilters.Add(methodFilter);

            return this;
        }

        /// <summary>
        /// Clear method filters
        /// </summary>
        /// <returns></returns>
        public IApiConfiguration ClearMethodFilters()
        {
            _methodFilters = ImmutableLinkedList<Func<MethodInfo, bool>>.Empty;

            return this;
        }

        /// <summary>
        /// Add exposures to 
        /// </summary>
        /// <param name="provider"></param>
        public IApiConfiguration AddExposures(IExposedMethodInformationProvider provider)
        {
            if (provider != null)
            {
                _providers.Add(provider);
            }

            return this;
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            foreach (var provider in _providers)
            {
                foreach (var exposedMethod in provider.GetExposedMethods())
                {
                    yield return exposedMethod;
                }
            }
        }

        private NamingConventions _currentNamingConventions;

        public ICurrentApiInformation GetCurrentApiInformation()
        {
            //need to make a copy of naming convention so that if it changes current api information stays the same
            if (_currentNamingConventions == null)
            {
                _currentNamingConventions = new NamingConventions { RouteNameGenerator = NamingConventions.RouteNameGenerator, MethodNameGenerator = NamingConventions.MethodNameGenerator };
            }
            else if (_currentNamingConventions.RouteNameGenerator != NamingConventions.RouteNameGenerator ||
                     _currentNamingConventions.MethodNameGenerator != NamingConventions.MethodNameGenerator)
            {
                _currentNamingConventions = new NamingConventions { RouteNameGenerator = NamingConventions.RouteNameGenerator, MethodNameGenerator = NamingConventions.MethodNameGenerator };
            }

            return new CurrentApiInformation(_authorizations, _filters, _prefixes, _currentNamingConventions, _methodFilters);
        }
    }
}
