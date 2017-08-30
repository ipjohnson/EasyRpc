using EasyRpc.AspNetCore.DataAnnotations.Impl;

namespace EasyRpc.AspNetCore.DataAnnotations
{
    public static class ApiConfigurationExtensions
    {
        public static IApiConfiguration UseDataAnnotations(this IApiConfiguration configuration)
        {
            var provider = new DataAnnotationFilterProvider();

            configuration.ApplyFilter(provider.ProvideFilters);

            return configuration;
        }
    }
}
