using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using EasyRpc.Abstractions.Response;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Views
{
    /// <summary>
    /// Methods marked with this will return a view instead of serialized data
    /// </summary>
    public class ReturnViewAttribute : Attribute, IRequestFilterAttribute, IRawContentAttribute
    {
        /// <summary>
        /// Is view a partial
        /// </summary>
        public bool IsPartial { get; set; } = true;

        /// <summary>
        /// View name to return
        /// </summary>
        public string ViewName { get; set; }
        
        /// <inheritdoc />
        public string ContentType { get; set; } = "text/html";

        /// <inheritdoc />
        public string ContentEncoding { get; set; }

        /// <inheritdoc />
        public IEnumerable<Func<RequestExecutionContext, IRequestFilter>> ProvideFilters(ICurrentApiInformation currentApi,
            IEndPointMethodConfigurationReadOnly configurationReadOnly)
        {
            var viewName = GetViewName(currentApi, configurationReadOnly);

            var executor = ActivatorUtilities.CreateInstance<ViewExecutor>(currentApi.ServiceProvider);

            var modelMetadataProvider = currentApi.ServiceProvider.GetService(typeof(IModelMetadataProvider)) as IModelMetadataProvider;

            var tempDataProvider =
                currentApi.ServiceProvider.GetService(typeof(ITempDataProvider)) as ITempDataProvider;


            var filter = new ViewFilter(viewName, !IsPartial, executor, modelMetadataProvider, tempDataProvider, ContentType);

            return new Func<RequestExecutionContext, IRequestFilter>[] { context => filter };
        }

        private string GetViewName(ICurrentApiInformation currentApi,
            IEndPointMethodConfigurationReadOnly configurationReadOnly)
        {
            if (currentApi.ServiceProvider.GetService(typeof(IViewNameGenerator)) is IViewNameGenerator nameGenerator)
            {
                return nameGenerator.GenerateName(currentApi, configurationReadOnly);
            }

            if (string.IsNullOrEmpty(ViewName))
            {
                var path = configurationReadOnly.RouteInformation.RouteBasePath;

                path = path.TrimEnd('/');

                return "~/Views" + path + ".cshtml";
            }

            return ViewName;
        }
    }
}
