using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Data;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class CurrentApiInformation : ICurrentApiInformation
    {
        public CurrentApiInformation(ImmutableLinkedList<Func<Type, IEnumerable<IMethodAuthorization>>> authorizations, ImmutableLinkedList<Func<Type, Func<HttpContext, IEnumerable<ICallFilter>>>> filters, ImmutableLinkedList<Func<Type, IEnumerable<string>>> prefixes, NamingConventions namingConventions)
        {
            Authorizations = authorizations;
            Filters = filters;
            Prefixes = prefixes;
            NamingConventions = namingConventions;
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
        /// Current naming conventions
        /// </summary>
        public NamingConventions NamingConventions { get; }
    }
}
