using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IFromServicesManager
    {
        bool ParameterIsFromServices(ParameterInfo parameter);
    }

    public class FromServicesManager : IFromServicesManager
    {
        private IOptions<RpcServiceConfiguration> _configuration;

        public FromServicesManager(IOptions<RpcServiceConfiguration> configuration)
        {
            _configuration = configuration;
        }

        public bool ParameterIsFromServices(ParameterInfo parameter)
        {
            if (parameter.GetCustomAttributes()
                .Any(a => string.Compare(a.GetType().Name, "FromServicesAttribute", StringComparison.CurrentCultureIgnoreCase) == 0))
            {
                return true;
            }

            if (_configuration.Value.AutoResolveInterfaces &&
                parameter.ParameterType.GetTypeInfo().IsInterface)
            {
                var parameterType = parameter.ParameterType;

                if (parameterType.IsConstructedGenericType)
                {
                    var typeDef = parameterType.GetGenericTypeDefinition();

                    if (typeDef == typeof(IEnumerable<>) ||
                        typeDef == typeof(ICollection<>) ||
                        typeDef == typeof(IReadOnlyCollection<>) ||
                        typeDef == typeof(IList<>))
                    {
                        return parameterType.GenericTypeArguments[0].GetTypeInfo().IsInterface;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
