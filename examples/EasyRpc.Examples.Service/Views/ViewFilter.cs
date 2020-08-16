using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace EasyRpc.Examples.Service.Views
{
    public class ViewFilter : IAsyncRequestExecutionFilter
    {
        private readonly string _viewName;
        private readonly bool _isMainPage;
        private readonly ViewExecutor _executor;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly string _contentType;

        public ViewFilter(string viewName, 
            bool isMainPage, 
            ViewExecutor executor,
            IModelMetadataProvider modelMetadataProvider, 
            ITempDataProvider tempDataProvider, 
            string contentType)
        {
            _viewName = viewName;
            _isMainPage = isMainPage;
            _executor = executor;
            _modelMetadataProvider = modelMetadataProvider;
            _tempDataProvider = tempDataProvider;
            _contentType = contentType;
        }

        /// <inheritdoc />
        public Task BeforeExecute(RequestExecutionContext context)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AfterExecute(RequestExecutionContext context)
        {
            var viewActionResult = new ViewActionResult(_executor, _modelMetadataProvider, _tempDataProvider, _viewName, _isMainPage, _contentType, context.Result, context.HttpStatusCode);

            // swap the result and it will be executed later in the pipeline if all filters succeed
            context.Result = viewActionResult;

            return Task.CompletedTask;
        }
    }
}
