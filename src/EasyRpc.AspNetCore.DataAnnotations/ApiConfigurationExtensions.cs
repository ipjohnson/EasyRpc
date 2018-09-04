using EasyRpc.AspNetCore.DataAnnotations.Impl;

namespace EasyRpc.AspNetCore.DataAnnotations
{
    /// <summary>
    /// Static class for C# extentions
    /// </summary>
    public static class ApiConfigurationExtensions
    {
        /// <summary>
        /// Use data annotations for validation
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApiConfiguration UseDataAnnotations(this IApiConfiguration configuration)
        {
            var provider = new DataAnnotationFilterProvider();

            configuration.ApplyFilter(provider.ProvideFilters);

            return configuration;
        }
    }
}
