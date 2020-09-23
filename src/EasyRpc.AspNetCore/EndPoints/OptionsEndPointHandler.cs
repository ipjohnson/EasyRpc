using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints.MethodHandlers;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// interface for handling unrequested 
    /// </summary>
    public interface IOptionsEndPointHandler
    {
        /// <summary>
        /// Is the options end point enabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Handle server option request (i.e. OPTIONS *)
        /// </summary>
        /// <param name="httpContext">http context for the request</param>
        /// <param name="requestDelegate"></param>
        /// <returns></returns>
        Task HandleServerOptionsRequest(HttpContext httpContext, RequestDelegate requestDelegate);

        /// <summary>
        /// Handle path based options request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="getMethod"></param>
        /// <param name="postMethod"></param>
        /// <returns></returns>
        Task HandlePathOptionRequest(HttpContext httpContext, RequestDelegate getMethod, IEndPointMethodHandler[] postMethod);
    }

    public class OptionsEndPointHandler : IOptionsEndPointHandler, IApiConfigurationCompleteAware
    {
        private readonly IConfigurationManager _configurationManager;
        private OptionsMethodConfiguration _optionsConfig;
        private IEndPointMethodHandler _emptyHandler;
        private string _methods;

        public OptionsEndPointHandler(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        /// <inheritdoc />
        public bool Enabled => _optionsConfig.Enabled;

        /// <inheritdoc />
        public Task HandleServerOptionsRequest(HttpContext httpContext, RequestDelegate next)
        {
            if (!_optionsConfig.Enabled)
            {
                return next(httpContext);
            }

            httpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;

            var headers = httpContext.Response.Headers;
            
            headers.TryAdd(_optionsConfig.AllowHeader, _methods);

            if (_optionsConfig.Headers.Count > 0)
            {
                var requestContext = new RequestExecutionContext(httpContext, _emptyHandler, 204);

                foreach (var optionsConfigHeader in _optionsConfig.Headers)
                {
                    optionsConfigHeader.ApplyHeader(requestContext, headers);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task HandlePathOptionRequest(HttpContext httpContext, RequestDelegate next, IEndPointMethodHandler[] methods)
        {
            if (!_optionsConfig.Enabled)
            {
                return next(httpContext);
            }

            var response = new StringBuilder ("OPTIONS");

            foreach (var endPointMethodHandler in methods)
            {
                response.Append(", ");

                response.Append(endPointMethodHandler.HttpMethod);
            }

            var headers = httpContext.Response.Headers;

            headers.TryAdd(_optionsConfig.AllowHeader, response.ToString());

            httpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;

            if (_optionsConfig.Headers.Count > 0)
            {
                var requestContext = new RequestExecutionContext(httpContext, methods[0], 204);

                foreach (var optionsConfigHeader in _optionsConfig.Headers)
                {
                    optionsConfigHeader.ApplyHeader(requestContext, headers);
                }
            }

            return Task.CompletedTask;
        }

        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            _emptyHandler =
                new NoParamsEndPointMethodHandler<object>(
                    new EndPointMethodConfiguration(
                        new RpcRouteInformation{ Method = "OPTIONS", HasBody = false, RouteBasePath = "*"}, 
                        context => new object(), 
                        new MethodInvokeInformation{ MethodInvokeDelegate = context => null}, 
                        typeof(object)), 
                    serviceScope.GetService(typeof(BaseEndPointServices)) as BaseEndPointServices);

            _optionsConfig = _configurationManager.GetConfiguration<OptionsMethodConfiguration>();

            foreach (var supportedMethod in _optionsConfig.SupportedMethods)
            {
                if (!string.IsNullOrEmpty(_methods))
                {
                    _methods += ", ";
                }

                _methods += supportedMethod;
            }
        }
    }
}
