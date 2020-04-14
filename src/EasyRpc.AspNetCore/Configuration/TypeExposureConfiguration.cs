using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public class TypeExposureConfiguration : IExposureConfiguration, IConfigurationInformationProvider
    {
        private readonly ICurrentApiInformation _currentApiInformation;
        private readonly Type _exposeType;
        private string _name;

        public TypeExposureConfiguration(ICurrentApiInformation currentApiInformation, Type exposeType)
        {
            
            _currentApiInformation = currentApiInformation;
            _exposeType = exposeType;
        }

        public IExposureConfiguration As(string name)
        {
            _name = name;

            return this;
        }

        public IExposureConfiguration Authorize(string role = null, string policy = null)
        {
            throw new NotImplementedException();
        }

        public IExposureConfiguration Methods(Func<MethodInfo, bool> methods)
        {
            throw new NotImplementedException();
        }

        public IExposureConfiguration Obsolete(string message)
        {
            throw new NotImplementedException();
        }

        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeType(_currentApiInformation, _exposeType, _name, null, null, null);
        }
    }

    public class TypeExposureConfiguration<T> : IExposureConfiguration<T>, IConfigurationInformationProvider
    {
        private readonly ICurrentApiInformation _currentApiInformation;
        private string _name;

        public TypeExposureConfiguration(ICurrentApiInformation currentApiInformation)
        {
            _currentApiInformation = currentApiInformation;
        }

        public IExposureConfiguration<T> As(string name)
        {
            _name = name;

            return this;
        }

        public IExposureConfiguration<T> Authorize(string role = null, string policy = null)
        {
            throw new NotImplementedException();
        }

        public IExposureConfiguration<T> Methods(Func<MethodInfo, bool> methods)
        {
            throw new NotImplementedException();
        }

        public IExposureConfiguration<T> Obsolete(string message)
        {
            throw new NotImplementedException();
        }

        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeType(_currentApiInformation, typeof(T), _name, null, null, null);
        }
    }
}
