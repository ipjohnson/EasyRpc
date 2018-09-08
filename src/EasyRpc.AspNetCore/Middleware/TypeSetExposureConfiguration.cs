using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyRpc.AspNetCore.Attributes;
using EasyRpc.AspNetCore.Data;
using Microsoft.AspNetCore.Authorization;

namespace EasyRpc.AspNetCore.Middleware
{
    public class TypeSetExposureConfiguration : ITypeSetExposureConfiguration, IExposedMethodInformationProvider
    {
        private readonly IEnumerable<Type> _types;
        private readonly ICurrentApiInformation _apiInformation;
        private readonly IInstanceActivator _activator;
        private readonly IArrayMethodInvokerBuilder _invokerBuilder;
        private Func<Type, IEnumerable<string>> _names;
        private ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>> _authorizations = ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>>.Empty;
        private Func<Type, bool> _typeFilter;
        private Func<Type, bool> _interfacesFilter;
        private Func<MethodInfo, bool> _methodFilter;
        private Func<Type, bool> _where;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="types"></param>
        /// <param name="apiInformation"></param>
        /// <param name="activator"></param>
        public TypeSetExposureConfiguration(IEnumerable<Type> types, ICurrentApiInformation apiInformation, IInstanceActivator activator, IArrayMethodInvokerBuilder invokerBuilder)
        {
            _types = types;
            _apiInformation = apiInformation;
            _activator = activator;
            _invokerBuilder = invokerBuilder;
        }

        /// <summary>
        /// Function for picking name
        /// </summary>
        /// <param name="nameFunc"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration As(Func<Type, IEnumerable<string>> nameFunc)
        {
            _names = nameFunc;

            return this;
        }

        /// <summary>
        /// Authorize exposures
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Authorize(string role = null, string policy = null)
        {
            IMethodAuthorization authorize = null;

            if (role != null)
            {
                authorize = new UserRoleAuthorization(role);
            }
            else if (policy != null)
            {
                authorize = new UserPolicyAuthorization(policy);
            }
            else
            {
                authorize = new UserAuthenticatedAuthorization();
            }

            _authorizations = _authorizations.Add(t => new[] { authorize });

            return this;
        }

        /// <summary>
        /// Use func for providing authorization
        /// </summary>
        /// <param name="authorizationFunc"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Authorize(Func<Type, IEnumerable<IMethodAuthorization>> authorizationFunc)
        {
            if (authorizationFunc == null) throw new ArgumentNullException(nameof(authorizationFunc));

            _authorizations = _authorizations.Add(authorizationFunc);

            return this;
        }

        /// <summary>
        /// Expose interfaces from type set
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Interfaces(Func<Type, bool> filter = null)
        {
            _interfacesFilter = filter ?? (t => true);

            return this;
        }

        /// <summary>
        /// Filter methods
        /// </summary>
        /// <param name="methodFilter"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Methods(Func<MethodInfo, bool> methodFilter)
        {
            _methodFilter = methodFilter;

            return this;
        }

        /// <summary>
        /// Expose types, this is the default
        /// </summary>
        /// <param name="filter">filter out types to be exported</param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Types(Func<Type, bool> filter = null)
        {
            _typeFilter = filter ?? (t => true);

            return this;
        }

        /// <summary>
        /// Expose types that match filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Where(Func<Type, bool> filter)
        {
            _where = filter;

            return this;
        }

        /// <summary>
        /// Get exposed methods for set
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IExposedMethodInformation> GetExposedMethods()
        {
            if (_interfacesFilter == null && _typeFilter == null)
            {
                _typeFilter = t => true;
            }

            foreach (var type in _types)
            {
                if (_where != null && !_where(type))
                {
                    continue;
                }

                bool expose = false;

                if (type.GetTypeInfo().IsInterface)
                {
                    if (_interfacesFilter != null)
                    {
                        expose = _interfacesFilter(type);
                    }
                }
                else if (_typeFilter != null)
                {
                    expose = _typeFilter(type);
                }

                if (expose)
                {
                    List<IMethodAuthorization> authorizations = new List<IMethodAuthorization>();

                    foreach (var authorizationFunc in _authorizations)
                    {
                        authorizations.AddRange(authorizationFunc(type));
                    }

                    var nameList = new List<string>();
                    
                    if (nameList.Count > 0)
                    {
                        _names = nameType => nameList;
                    }

                    foreach (var exposedMethodInformation in BaseExposureConfiguration.GetExposedMethods(type, _apiInformation,
                            _names ?? _apiInformation.NamingConventions.RouteNameGenerator, authorizations, _methodFilter,_activator, _invokerBuilder, null))
                    {
                        yield return exposedMethodInformation;
                    }
                }
            }
        }
    }
}
