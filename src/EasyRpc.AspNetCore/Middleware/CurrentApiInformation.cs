using System;
using System.Collections.Generic;
using System.Reflection;
using EasyRpc.AspNetCore.Data;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    /// <summary>
    /// Current information about api
    /// </summary>
    public interface ICurrentApiInformation
    {
        /// <summary>
        /// Authorizations to apply
        /// </summary>
        ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>> Authorizations { get; }

        /// <summary>
        /// Filters to apply
        /// </summary>
        ImmutableLinkedList<Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>>> Filters { get; }

        /// <summary>
        /// Prefixes to apply
        /// </summary>
        ImmutableLinkedList<Func<Type, IEnumerable<string>>> Prefixes { get; }

        /// <summary>
        /// List of method filters
        /// </summary>
        ImmutableLinkedList<Func<MethodInfo, bool>> MethodFilters { get; }

        /// <summary>
        /// Current naming conventions
        /// </summary>
        NamingConventions NamingConventions { get; }
    }

    public class CurrentApiInformation : ICurrentApiInformation
    {
        public CurrentApiInformation(ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>> authorizations, ImmutableLinkedList<Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>>> filters, ImmutableLinkedList<Func<Type, IEnumerable<string>>> prefixes, NamingConventions namingConventions, ImmutableLinkedList<Func<MethodInfo, bool>> methodFilters)
        {
            Authorizations = authorizations;
            Filters = filters;
            Prefixes = prefixes;
            NamingConventions = namingConventions;
            MethodFilters = methodFilters;
        }

        /// <summary>
        /// Authorizations to apply
        /// </summary>
        public ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>> Authorizations { get; }

        /// <summary>
        /// Filters to apply
        /// </summary>
        public ImmutableLinkedList<Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>>> Filters { get; }

        /// <summary>
        /// Prefixes to apply
        /// </summary>
        public ImmutableLinkedList<Func<Type, IEnumerable<string>>> Prefixes { get; }

        /// <summary>
        /// List of method filters
        /// </summary>
        public ImmutableLinkedList<Func<MethodInfo, bool>> MethodFilters { get; }

        /// <summary>
        /// Current naming conventions
        /// </summary>
        public NamingConventions NamingConventions { get; }
    }
}
