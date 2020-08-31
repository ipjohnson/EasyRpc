using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace EasyRpc.AspNetCore.Views
{

    /// <summary>
    /// Action result for views
    /// </summary>
    public class ViewActionResult : IActionResult
    {
        private readonly string _viewName;
        private readonly bool _isMainPage;
        private readonly ViewExecutor _executor;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly string _contentType;
        private readonly object _result;
        private readonly int _httpResult;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="modelMetadataProvider"></param>
        /// <param name="tempDataProvider"></param>
        /// <param name="viewName"></param>
        /// <param name="isMainPage"></param>
        /// <param name="contentType"></param>
        /// <param name="result"></param>
        /// <param name="httpResult"></param>
        public ViewActionResult(ViewExecutor executor,
            IModelMetadataProvider modelMetadataProvider,
            ITempDataProvider tempDataProvider,
            string viewName,
            bool isMainPage,
            string contentType,
            object result,
            int httpResult)
        {
            _executor = executor;
            _modelMetadataProvider = modelMetadataProvider;
            _tempDataProvider = tempDataProvider;
            _viewName = viewName;
            _isMainPage = isMainPage;
            _contentType = contentType;
            _result = result;
            _httpResult = httpResult;
        }

        /// <inheritdoc />
        public Task ExecuteResultAsync(ActionContext context)
        {
            var viewEngine = context.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            if (viewEngine == null)
            {
                throw new Exception("Could not find ICompositeViewEngine");
            }

            var viewEngineResult = viewEngine.GetView(null, _viewName, _isMainPage);
            
            return _executor.ExecuteAsync(context,
                viewEngineResult.View,
                new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                {
                    Model = _result
                },
                new TempDataDictionary(context.HttpContext, _tempDataProvider),
                _contentType,
                _httpResult);
        }
    }
}
