using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.ContentEncoding;
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
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration BasePath(this IEnvironmentConfiguration configuration, string path)
        {
            return configuration.Action((BasePathOptions option) => option.Path = path);
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
        /// Disable documentation for api
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration DisableDocumentation(this IEnvironmentConfiguration configuration)
        {
            return configuration.Documentation(c => c.Enabled = false);
        }

        /// <summary>
        /// Enable response compression
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration EnableCompression(this IEnvironmentConfiguration configuration)
        {
            return configuration.Action<ContentEncodingConfiguration>(config =>
                config.CompressionEnabled = true);
        }

        /// <summary>
        /// Configures default exposures
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public static IEnvironmentConfiguration Exposures(this IEnvironmentConfiguration configuration,
            Action<ExposeConfigurations> configurationAction)
        {
            return configuration.Action(configurationAction);
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
