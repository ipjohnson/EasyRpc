using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyRpc.AspNetCore.Middleware
{
    public class ExposureConfiguration : BaseExposureConfiguration, IExposureConfiguration, IExposedMethodInformationProvider
    {
        private readonly Type _type;
        private Func<MethodInfo, bool> _methods;
        private string _obsoleteMessage;

        public ExposureConfiguration(Type type, ICurrentApiInformation apiInformation, IInstanceActivator activator, IArrayMethodInvokerBuilder invokerBuilder) : base(apiInformation, activator, invokerBuilder)
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

        public IExposureConfiguration Methods(Func<MethodInfo, bool> methods)
        {
            _methods = methods;

            return this;
        }

        public IEnumerable<IExposedMethodInformation> GetExposedMethods()
        {
            return GetExposedMethods(_type,_methods, _obsoleteMessage);
        }

        public IExposureConfiguration Obsolete(string message)
        {
            _obsoleteMessage = message;

            return this;
        }
    }

    public class ExposureConfiguration<T> : BaseExposureConfiguration, IExposureConfiguration<T>, IExposedMethodInformationProvider
    {
        private Func<MethodInfo, bool> _methods;
        private string _obsoleteMessage;

        public ExposureConfiguration(ICurrentApiInformation apiInformation, IInstanceActivator activator, IArrayMethodInvokerBuilder invokerBuilder) : base(apiInformation, activator, invokerBuilder)
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

        public IExposureConfiguration<T> Methods(Func<MethodInfo, bool> methods)
        {
            _methods = methods;

            return this;
        }

        /// <summary>
        /// Mark service as obsolete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IExposureConfiguration<T> Obsolete(string message)
        {
            _obsoleteMessage = message;

            return this;
        }

        public IEnumerable<IExposedMethodInformation> GetExposedMethods()
        {
            return GetExposedMethods(typeof(T), _methods, _obsoleteMessage);
        }
    }
}
