using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IApiConfigurationProvider : IExposedMethodInformationProvider
    {
        
    }

    public class ApiConfigurationProvider : IApiConfigurationProvider, IApiConfiguration
    {
        private readonly List<IExposedMethodInformationProvider> _providers =
            new List<IExposedMethodInformationProvider>();

        /// <summary>
        /// Expose type for RPC
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IExposureConfiguration Expose(Type type)
        {
            var config = new ExposureConfiguration(type);

            _providers.Add(config);

            return config;
        }

        /// <summary>
        /// Expose type for RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IExposureConfiguration Expose<T>()
        {
            var config = new ExposureConfiguration(typeof(T));

            _providers.Add(config);

            return config;
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            foreach (var provider in _providers)
            {
                foreach (var exposedMethod in provider.GetExposedMethods())
                {
                    yield return exposedMethod;
                }
            }
        }
    }
}
