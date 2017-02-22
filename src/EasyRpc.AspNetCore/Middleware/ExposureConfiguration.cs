using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public class ExposureConfiguration : IExposureConfiguration, IExposedMethodInformationProvider
    {
        private readonly Type _type;
        private readonly ICurrentApiInformation _apiInformation;
        private readonly List<string> _names = new List<string>();
        private readonly List<IMethodAuthorization> _authorizations = new List<IMethodAuthorization>();

        public ExposureConfiguration(Type type, ICurrentApiInformation apiInformation)
        {
            _type = type;
            _apiInformation = apiInformation;
        }

        public IExposureConfiguration As(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _names.Add(name);
            }

            return this;
        }

        public IExposureConfiguration Authorize(string role = null, string policy = null)
        {
            if (role != null)
            {
                _authorizations.Add(new UserRoleAuthorization(role));
            }
            else if (policy != null)
            {
                _authorizations.Add(new UserPolicyAuthorization(policy));
            }
            else
            {
                _authorizations.Add(new UserAuthenticatedAuthorization());
            }

            return this;
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            if (_names.Count == 0)
            {
                foreach (var routeName in _apiInformation.NamingConventions.RouteNameGenerator(_type))
                {
                    _names.Add(routeName);
                }
            }

            var finalNames = _names;

            if (_apiInformation.Prefixes.Count > 0)
            {
                var newNames = new List<string>();

                foreach (var prefixes in _apiInformation.Prefixes)
                {
                    foreach (var prefix in prefixes(_type))
                    {
                        foreach (var name in _names)
                        {
                            newNames.Add(prefix + name);
                        }
                    }
                }

                finalNames = newNames;
            }

            foreach (var method in _type.GetRuntimeMethods())
            {
                if (method.IsStatic || !method.IsPublic)
                {
                    continue;
                }

                yield return new ExposedMethodInformation(_type, finalNames, _apiInformation.NamingConventions.MethodNameGenerator(method), method, _authorizations.ToArray());
            }
        }
    }

    public class ExposureConfiguration<T> : IExposureConfiguration<T>, IExposedMethodInformationProvider
    {
        private readonly List<string> _names = new List<string>();
        private readonly ICurrentApiInformation _apiInformation;
        private readonly List<IMethodAuthorization> _authorizations = new List<IMethodAuthorization>();

        public ExposureConfiguration(ICurrentApiInformation apiInformation)
        {
            _apiInformation = apiInformation;
        }

        public IExposureConfiguration<T> As(string name)
        {
            _names.Add(name);

            return this;
        }

        public IExposureConfiguration<T> Authorize(string role = null, string policy = null)
        {
            if (role != null)
            {
                _authorizations.Add(new UserRoleAuthorization(role));
            }
            else if (policy != null)
            {
                _authorizations.Add(new UserPolicyAuthorization(policy));
            }
            else
            {
                _authorizations.Add(new UserAuthenticatedAuthorization());
            }

            return this;
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            var type = typeof(T);

            if (_names.Count == 0)
            {
                foreach (var routeName in _apiInformation.NamingConventions.RouteNameGenerator(type))
                {
                    _names.Add(routeName);
                }
            }

            var finalNames = _names;

            if (_apiInformation.Prefixes.Count > 0)
            {
                var newNames = new List<string>();

                foreach (var prefixes in _apiInformation.Prefixes)
                {
                    foreach (var prefix in prefixes(type))
                    {
                        foreach (var name in _names)
                        {
                            newNames.Add(prefix + name);
                        }
                    }
                }

                finalNames = newNames;
            }

            foreach (var method in type.GetRuntimeMethods())
            {
                if (method.IsStatic || !method.IsPublic)
                {
                    continue;
                }

                yield return new ExposedMethodInformation(type, finalNames, _apiInformation.NamingConventions.MethodNameGenerator(method), method, _authorizations.ToArray());
            }
        }
    }
}
