using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Newtonsoft
{
    public static class NewtonsoftExtensions
    {
        /// <summary>
        /// Configure Newtonsoft json serializer
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration NewtonsoftJson(this IEnvironmentConfiguration configuration,
            Action<JsonSerializer> configurationAction)
        {
            return configuration.Action(configurationAction);
        }
    }
}
