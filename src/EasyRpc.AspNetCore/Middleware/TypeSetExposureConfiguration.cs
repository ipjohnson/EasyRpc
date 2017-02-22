using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public class TypeSetExposureConfiguration : ITypeSetExposureConfiguration, IExposedMethodInformationProvider
    {
        private IEnumerable<Type> _types;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="types"></param>
        public TypeSetExposureConfiguration(IEnumerable<Type> types)
        {
            _types = types;
        }

        /// <summary>
        /// Function for picking name
        /// </summary>
        /// <param name="nameFunc"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration As(Func<Type, string> nameFunc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize exposures
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Authorize(string role = null, string policy = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Use func for providing authorization
        /// </summary>
        /// <param name="authorizationFunc"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Authorize(Func<Type, IEnumerable<IMethodAuthorization>> authorizationFunc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Expose interfaces from type set
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Interfaces(Func<Type, bool> filter = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Expose types, this is the default
        /// </summary>
        /// <param name="filter">filter out types to be exported</param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Types(Func<Type, bool> filter = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Expose types that match filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ITypeSetExposureConfiguration Where(Func<Type, bool> filter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExposedMethodInformation> GetExposedMethods()
        {
            yield break;
        }
    }
}
