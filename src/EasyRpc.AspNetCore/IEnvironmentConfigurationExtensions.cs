using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using EasyRpc.AspNetCore.Routing;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Static C# extensions for the configuration object
    /// </summary>
    public static class IEnvironmentConfigurationExtensions
    {
        /// <summary>
        /// Configure api base path
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration BasePath(this IEnvironmentConfiguration configuration, Action<BasePathOptions> options)
        {
            return configuration.Action(options);
        }

        /// <summary>
        /// Configure open api documentation
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration Documentation(this IEnvironmentConfiguration configuration, Action<DocumentationOptions> options)
        {
            return configuration.Action(options);
        }

        /// <summary>
        /// Configure System.Text.Json serializer
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration SystemTextJson(this IEnvironmentConfiguration configuration, Action<JsonSerializerOptions> options)
        {
            return configuration.Action(options);
        }

        /// <summary>
        /// Use Asp.Net routing instead of internal routing. 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration UseAspNetRouting(this IEnvironmentConfiguration configuration)
        {
            return configuration.Action<RoutingConfiguration>(routingConfiguration => routingConfiguration.UseAspNetCoreRouting = true);
        }
    }
}
