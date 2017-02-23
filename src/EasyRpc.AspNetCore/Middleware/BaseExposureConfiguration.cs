using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public abstract class BaseExposureConfiguration
    {
        protected readonly ICurrentApiInformation ApiInformation;
        protected readonly List<string> Names = new List<string>();
        protected readonly List<IMethodAuthorization> Authorizations = new List<IMethodAuthorization>();

        protected BaseExposureConfiguration(ICurrentApiInformation apiInformation)
        {
            ApiInformation = apiInformation;
        }

        protected IEnumerable<ExposedMethodInformation> GetExposedMethods(Type type)
        {
            if (Names.Count == 0)
            {
                foreach (var routeName in ApiInformation.NamingConventions.RouteNameGenerator(type))
                {
                    Names.Add(routeName);
                }
            }

            var finalNames = Names;

            if (ApiInformation.Prefixes.Count > 0)
            {
                var newNames = new List<string>();

                foreach (var prefixes in ApiInformation.Prefixes)
                {
                    foreach (var prefix in prefixes(type))
                    {
                        foreach (var name in Names)
                        {
                            newNames.Add(prefix + name);
                        }
                    }
                }

                finalNames = newNames;
            }

            foreach (var authorization in ApiInformation.Authorizations)
            {
                foreach (var methodAuthorization in authorization(type))
                {
                    Authorizations.Add(methodAuthorization);
                }
            }
            
            var filters = new List<Func<HttpContext,IEnumerable<ICallFilter>>>();

            foreach (var func in ApiInformation.Filters)
            {
                var filter = func(type);

                if (filter != null)
                {
                    filters.Add(filter);
                }
            }

            foreach (var method in type.GetRuntimeMethods())
            {
                if (method.IsStatic || !method.IsPublic)
                {
                    continue;
                }

                yield return new ExposedMethodInformation(type, finalNames, ApiInformation.NamingConventions.MethodNameGenerator(method), method, Authorizations.ToArray(), filters.ToArray());
            }
        }
    }
}
