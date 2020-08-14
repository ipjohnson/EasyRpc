using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EasyRpc.AspNetCore.Configuration
{
    public class EnvironmentConfiguration : IEnvironmentConfiguration
    {
        private readonly IConfigurationManager _configurationManager;

        public EnvironmentConfiguration(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public IEnvironmentConfiguration Action<T>(Action<T> valueAction)
        {
            _configurationManager.RegisterConfigurationMethod(valueAction);

            return this;
        }
    }
}
