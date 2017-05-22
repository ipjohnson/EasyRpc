using System;
using System.Collections.Generic;

namespace EasyRpc.AspNetCore.Middleware
{
    public class ExposureConfiguration : BaseExposureConfiguration, IExposureConfiguration, IExposedMethodInformationProvider
    {
        private readonly Type _type;

        public ExposureConfiguration(Type type, ICurrentApiInformation apiInformation) : base(apiInformation)
        {
            _type = type;
        }

        public IExposureConfiguration As(string name)
        {
            Names.Add(name);
            
            return this;
        }

        public IExposureConfiguration Authorize(string role = null, string policy = null)
        {
            if (role != null)
            {
                Authorizations.Add(new UserRoleAuthorization(role));
            }
            else if (policy != null)
            {
                Authorizations.Add(new UserPolicyAuthorization(policy));
            }
            else
            {
                Authorizations.Add(new UserAuthenticatedAuthorization());
            }

            return this;
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            return GetExposedMethods(_type);
        }
    }

    public class ExposureConfiguration<T> : BaseExposureConfiguration, IExposureConfiguration<T>, IExposedMethodInformationProvider
    {
        public ExposureConfiguration(ICurrentApiInformation apiInformation) : base(apiInformation)
        {
        }

        public IExposureConfiguration<T> As(string name)
        {
            Names.Add(name);

            return this;
        }

        public IExposureConfiguration<T> Authorize(string role = null, string policy = null)
        {
            if (role != null)
            {
                Authorizations.Add(new UserRoleAuthorization(role));
            }
            else if (policy != null)
            {
                Authorizations.Add(new UserPolicyAuthorization(policy));
            }
            else
            {
                Authorizations.Add(new UserAuthenticatedAuthorization());
            }

            return this;
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            return GetExposedMethods(typeof(T));
        }
    }
}
