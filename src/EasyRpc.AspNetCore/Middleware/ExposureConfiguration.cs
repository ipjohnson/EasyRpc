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
        private readonly List<string> _names = new List<string>();
        private readonly List<IMethodAuthorization> _authorizations = new List<IMethodAuthorization>();

        public ExposureConfiguration(Type type)
        {
            _type = type;
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
                _names.Add(_type.Name);
            }

            foreach (var method in _type.GetRuntimeMethods())
            {
                if (method.IsStatic || !method.IsPublic)
                {
                    continue;
                }

                yield return new ExposedMethodInformation(_type, _names, method.Name, method, _authorizations.ToArray());
            }
        }
    }

    public class ExposureConfiguration<T> : IExposureConfiguration<T>, IExposedMethodInformationProvider
    {
        private readonly List<string> _names = new List<string>();
        private readonly List<IMethodAuthorization> _authorizations = new List<IMethodAuthorization>();

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
                _names.Add(type.Name);
            }

            foreach (var method in type.GetRuntimeMethods())
            {
                if (method.IsStatic || !method.IsPublic)
                {
                    continue;
                }

                yield return new ExposedMethodInformation(type, _names, method.Name, method, _authorizations.ToArray());
            }
        }
    }
}
