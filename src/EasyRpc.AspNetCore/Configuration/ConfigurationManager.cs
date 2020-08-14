using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IConfigurationManager
    {
        void CreationMethod<T>(Func<T> defaultRegistration);

        void RegisterConfigurationMethod<T>(Action<T> configurationMethod);

        T GetConfiguration<T>();
    }


    public class ConfigurationManager : IConfigurationManager
    {
        private readonly ConcurrentDictionary<Type, object> _configurationObjectCache = new ConcurrentDictionary<Type, object>();
        private readonly Dictionary<Type, Delegate> _creationMethods = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, Delegate> _configurationMethods = new Dictionary<Type, Delegate>();

        /// <inheritdoc cref="IConfigurationManager"/>
        public void CreationMethod<T>(Func<T> defaultRegistration)
        {
            _creationMethods[typeof(T)] = defaultRegistration;
        }

        /// <inheritdoc cref="IConfigurationManager"/>
        public void RegisterConfigurationMethod<T>(Action<T> configurationMethod)
        {
            _configurationMethods[typeof(T)] = configurationMethod;
        }

        /// <inheritdoc cref="IConfigurationManager"/>
        public T GetConfiguration<T>()
        {
            return (T)_configurationObjectCache.GetOrAdd(typeof(T), type => CreateConfigurationObject<T>());
        }

        private object CreateConfigurationObject<T>()
        {
            T newT;

            if (_creationMethods.TryGetValue(typeof(T), out var delegateValue))
            {
                newT = ((Func<T>) delegateValue)();
            }
            else
            {
                newT = Activator.CreateInstance<T>();
            }

            if(_configurationMethods.TryGetValue(typeof(T), out var configDelegate))
            {
                ((Action<T>) configDelegate)(newT);
            }

            return newT;
        }
    }
}
