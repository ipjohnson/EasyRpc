using EasyRpc.AspNetCore.FluentValidation.Impl;

namespace EasyRpc.AspNetCore.FluentValidation
{
    public static class ApiConfigurationExtensions
    {
        public static IApiConfiguration UseFluentValidation(this IApiConfiguration configuration)
        {
            var filterProvider = new ValidationFilterProvider(configuration.AppServices);

            return configuration.ApplyFilter(filterProvider.GetFilters);
        }
    }
}
