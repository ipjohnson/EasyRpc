using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.Utilities;

namespace EasyRpc.AspNetCore.Configuration
{
    /// <summary>
    /// Exposes a set of types
    /// </summary>
    public class TypeSetExposureConfiguration : ITypeSetExposureConfiguration, IConfigurationInformationProvider
    {
        private readonly ICurrentApiInformation _currentApiInformation;
        protected IEnumerable<Type> Types;
        protected bool OnlyAttributedFlag = false;
        protected ImmutableLinkedList<Func<Type, IEnumerable<string>>> AsFuncs =
            ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;
        protected ImmutableLinkedList<Func<Type, IEnumerable<IEndPointMethodAuthorization>>> AuthorizeFuncs =
            ImmutableLinkedList<Func<Type, IEnumerable<IEndPointMethodAuthorization>>>.Empty;
        protected GenericFilterGroup<Type> WhereFilters;
        protected GenericFilterGroup<MethodInfo> MethodFilterGroup;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="currentApiInformation"></param>
        /// <param name="types"></param>
        public TypeSetExposureConfiguration(ICurrentApiInformation currentApiInformation, IEnumerable<Type> types)
        {
            Types = types;
            _currentApiInformation = currentApiInformation;

            MethodFilterGroup = new GenericFilterGroup<MethodInfo>(FilterObjectMethods);
            WhereFilters = new GenericFilterGroup<Type>(FilterOutTypes);
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration As(Func<Type, IEnumerable<string>> nameFunc)
        {
            AsFuncs = AsFuncs.Add(nameFunc);

            return this;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Authorize(string role = null, string policy = null)
        {
            IEndPointMethodAuthorization authorization = null;

            if (!string.IsNullOrEmpty(policy))
            {
                authorization = new UserHasPolicy(policy);
            }
            else if (!string.IsNullOrEmpty(role))
            {
                authorization = new UserHasRole(role);
            }
            else
            {
                authorization = new UserIsAuthorized();
            }

            return Authorize(type => new[] { authorization });
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Authorize(Func<Type, IEnumerable<IEndPointMethodAuthorization>> authorizationFunc)
        {
            AuthorizeFuncs = AuthorizeFuncs.Add(authorizationFunc);

            return this;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Interfaces(Func<Type, bool> filter = null)
        {
            if (filter == null)
            {
                filter = type => type.GetTypeInfo().ImplementedInterfaces.Any();
            }

            WhereFilters.Add(filter);

            bool methodFilter(MethodInfo method)
            {
                // TODO: this needs to be tweaked to only export methods that are interfaces
                // this is just a close approximation
                return method.IsPublic && method.IsVirtual && filter(method.DeclaringType);
            }

            MethodFilterGroup.Add(methodFilter);

            return this;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Methods(Func<MethodInfo, bool> methodFilter)
        {
            MethodFilterGroup.Add(methodFilter);

            return this;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration OnlyAttributed()
        {
            OnlyAttributedFlag = true;

            return this;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Where(Func<Type, bool> filter)
        {
            WhereFilters.Add(filter);

            return this;
        }

        /// <inheritdoc />
        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            if (OnlyAttributedFlag)
            {
                MethodFilterGroup.Add(m => m.GetCustomAttributes().Any(a => a is IPathAttribute));
                WhereFilters.Add(type => type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Any(member => member.GetCustomAttributes().Any(attr => attr is IPathAttribute)));
            }

            foreach (var type in Types)
            {
                if (!WhereFilters.Execute(type))
                {
                    continue;
                }

                var authorizationList = AuthorizeFuncs.SelectMany(func => func(type)).ToList();

                if (AsFuncs != ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty)
                {
                    foreach (var asFunc in AsFuncs)
                    {
                        foreach (var nameString in asFunc(type))
                        {
                            service.ExposeType(_currentApiInformation, type, null, nameString, authorizationList,
                                MethodFilterGroup, null);
                        }
                    }
                }
                else
                {
                    service.ExposeType(_currentApiInformation, type, null, "", authorizationList,
                        MethodFilterGroup, null);

                }
            }
        }

        private bool FilterObjectMethods(MethodInfo method)
        {
            return !method.IsSpecialName &&
                   method.DeclaringType != typeof(object);
        }

        private bool FilterOutTypes(Type type)
        {
            return !type.IsAbstract && 
                   type.IsClass && 
                   type.IsPublic;
        }
    }
}
