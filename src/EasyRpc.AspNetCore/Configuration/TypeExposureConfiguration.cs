using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.Utilities;

namespace EasyRpc.AspNetCore.Configuration
{
    public class TypeExposureConfiguration : IExposureConfiguration, IConfigurationInformationProvider
    {
        private ImmutableLinkedList<IEndPointMethodAuthorization> _authorizations = ImmutableLinkedList<IEndPointMethodAuthorization>.Empty;
        private readonly ICurrentApiInformation _currentApiInformation;
        private readonly Type _exposeType;
        private string _name;
        private GenericFilterGroup<MethodInfo> _methodFilterGroup;
        private string _obsoleteMessage;

        public TypeExposureConfiguration(ICurrentApiInformation currentApiInformation, Type exposeType)
        {
            
            _currentApiInformation = currentApiInformation;
            _exposeType = exposeType;
            _methodFilterGroup = new GenericFilterGroup<MethodInfo>(FilterObjectMethods);
        }

        public IExposureConfiguration As(string name)
        {
            _name = name;

            return this;
        }

        public IExposureConfiguration Authorize(string role = null, string policy = null)
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

            _authorizations = _authorizations.Add(authorization);

            return this;
        }

        public IExposureConfiguration Methods(Func<MethodInfo, bool> methods)
        {
            _methodFilterGroup.Add(methods);

            return this;
        }

        public IExposureConfiguration Obsolete(string message)
        {
            _obsoleteMessage = message;

            return this;
        }

        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            List<IEndPointMethodAuthorization> authorizations = null;

            if (_authorizations != ImmutableLinkedList<IEndPointMethodAuthorization>.Empty)
            {
                authorizations = _authorizations.ToList();
            }

            service.ExposeType(_currentApiInformation, _exposeType, _name, authorizations, _methodFilterGroup, _obsoleteMessage);
        }

        private bool FilterObjectMethods(MethodInfo method)
        {
            return method.DeclaringType != typeof(object);
        }
    }

    public class TypeExposureConfiguration<T> : IExposureConfiguration<T>, IConfigurationInformationProvider
    {
        private ImmutableLinkedList<IEndPointMethodAuthorization> _authorizations = ImmutableLinkedList<IEndPointMethodAuthorization>.Empty;
        private readonly ICurrentApiInformation _currentApiInformation;
        private readonly Type _exposeType;
        private string _name;
        private GenericFilterGroup<MethodInfo> _methodFilterGroup;
        private string _obsoleteMessage;

        public TypeExposureConfiguration(ICurrentApiInformation currentApiInformation)
        {
            _currentApiInformation = currentApiInformation;

            _methodFilterGroup = new GenericFilterGroup<MethodInfo>(FilterObjectMethods);
        }

        public IExposureConfiguration<T> As(string name)
        {
            _name = name;

            return this;
        }

        public IExposureConfiguration<T> Authorize(string role = null, string policy = null)
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

            _authorizations = _authorizations.Add(authorization);

            return this;
        }

        public IExposureConfiguration<T> Methods(Func<MethodInfo, bool> methods)
        {
            _methodFilterGroup.Add(methods);

            return this;
        }

        public IExposureConfiguration<T> Obsolete(string message)
        {
            _obsoleteMessage = message;

            return this;
        }

        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            List<IEndPointMethodAuthorization> authorizations = null;

            if (_authorizations != ImmutableLinkedList<IEndPointMethodAuthorization>.Empty)
            {
                authorizations = _authorizations.ToList();
            }

            var name = _name;

            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }

            service.ExposeType(_currentApiInformation, typeof(T), name, authorizations, _methodFilterGroup, _obsoleteMessage);
        }

        private bool FilterObjectMethods(MethodInfo method)
        {
            return method.DeclaringType != typeof(object);
        }
    }
}
