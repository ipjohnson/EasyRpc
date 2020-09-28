using System;
using System.Collections.Generic;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.FluentValidation.Impl;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.FluentValidation
{
    /// <summary>
    /// Api configuration extension class
    /// </summary>
    public static class ApiConfigurationExtensions
    {
        /// <summary>
        /// Apply filter that locates validator for models
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IRpcApi ApplyFluentValidation(this IRpcApi configuration)
        {
            var filterProvider = new ValidationFilterProvider(
                configuration.AppServices.GetRequiredService< IErrorWrappingService>());

            return configuration.ApplyFilter(filterProvider.GetFilters);
        }
    }
}
