using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Response;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.Examples.Service.Views
{
    public class ReturnViewAttribute : Attribute, IRequestFilterAttribute, IRawContentAttribute
    {
        public string ViewName { get; set; }

        public bool IsPartial { get; set; } = false;

        /// <inheritdoc />
        public IEnumerable<Func<RequestExecutionContext, IRequestFilter>> ProvideFilters(ICurrentApiInformation currentApi,
            IEndPointMethodConfigurationReadOnly configurationReadOnly)
        {
            var viewEngine = currentApi.ServiceProvider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

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
            if (string.IsNullOrEmpty(ViewName))
            {
                var path = configurationReadOnly.RouteInformation.RouteBasePath;

                path = path.TrimEnd('/');

                return "~/Views" + path + ".cshtml";
            }

            return ViewName;
        }

        /// <inheritdoc />
        public string ContentType { get; }

        /// <inheritdoc />
        public string ContentEncoding { get; }
    }
}
