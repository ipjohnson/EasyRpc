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

                yield return new ExposedMethodInformation(_type, _names, method.Name, method);
            }
        }
    }
}
