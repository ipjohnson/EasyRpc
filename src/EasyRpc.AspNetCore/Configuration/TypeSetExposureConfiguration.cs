using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;

namespace EasyRpc.AspNetCore.Configuration
{
    public class TypeSetExposureConfiguration : ITypeSetExposureConfiguration, IConfigurationInformationProvider
    {
        protected IEnumerable<Type> Types;
        protected ImmutableLinkedList<Func<Type, bool>> WhereFilters = ImmutableLinkedList<Func<Type, bool>>.Empty;
        protected ImmutableLinkedList<Func<Type, IEnumerable<string>>> AsFuncs =
            ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;
        protected ImmutableLinkedList<Func<Type, IEnumerable<IEndPointMethodAuthorization>>> AuthorizeFuncs =
            ImmutableLinkedList<Func<Type, IEnumerable<IEndPointMethodAuthorization>>>.Empty;

        public TypeSetExposureConfiguration(IEnumerable<Type> types)
        {
            Types = types;
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration As(Func<Type, IEnumerable<string>> nameFunc)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Authorize(string role = null, string policy = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Authorize(Func<Type, IEnumerable<IEndPointMethodAuthorization>> authorizationFunc)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Interfaces(Func<Type, bool> filter = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Methods(Func<MethodInfo, bool> methodFilter)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration OnlyAttributed()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ITypeSetExposureConfiguration Where(Func<Type, bool> filter)
        {
            return this;
        }

        /// <inheritdoc />
        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            foreach (var type in Types)
            {

            }
        }
    }
}
