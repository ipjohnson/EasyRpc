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
        /// <param name="context">http context for the request</param>
        /// <param name="requestDelegate"></param>
        /// <returns></returns>
        Task HandleServerOptionsRequest(HttpContext context, RequestDelegate requestDelegate);

        /// <summary>
        /// Handle an option request for a specific path
        /// </summary>
        /// <param name="context">http context for the request</param>
        /// <param name="getMethod">get method can be null</param>
        /// <param name="postMethod">post method can be null</param>
        /// <param name="others">other methods can be null</param>
        /// <returns></returns>
        Task HandlePathOptionRequest(HttpContext context,
            IEndPointMethodHandler getMethod,
            IEndPointMethodHandler postMethod,
            IReadOnlyList<IEndPointMethodHandler> others);
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
        public Task HandleServerOptionsRequest(HttpContext context, RequestDelegate next)
        {
            if (!_optionsConfig.Enabled)
            {
                return next(context);
            }

            var headers = context.Response.Headers;

            if (_optionsConfig.Headers.Count > 0)
            {
                var requestContext = new RequestExecutionContext(context, _emptyHandler, 204);

                foreach (var optionsConfigHeader in _optionsConfig.Headers)
                {
                    optionsConfigHeader.ApplyHeader(requestContext, headers);
                }
            }

            headers.TryAdd(_optionsConfig.AllowHeader, _methods);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task HandlePathOptionRequest(HttpContext context,
            IEndPointMethodHandler getMethod,
            IEndPointMethodHandler postMethod,
            IReadOnlyList<IEndPointMethodHandler> others)
        {
            IEndPointMethodHandler defaultHandler = null;
            StringBuilder response = new StringBuilder();

            if (getMethod != null)
            {
                response.Append(getMethod.HttpMethod);

                defaultHandler = getMethod;
            }

            if (postMethod != null)
            {
                if (response.Length > 0)
                {
                    response.Append(", ");
                }

                response.Append(postMethod.HttpMethod);

                defaultHandler ??= postMethod;
            }

            if (others != null)
            {
                foreach (var methodHandler in others)
                {
                    if (response.Length > 0)
                    {
                        response.Append(", ");
                    }

                    response.Append(methodHandler.HttpMethod);

                    defaultHandler ??= methodHandler;
                }
            }

            var headers = context.Response.Headers;

            if (_optionsConfig.Headers.Count > 0)
            {
                var requestContext = new RequestExecutionContext(context, defaultHandler, 204);

                foreach (var optionsConfigHeader in _optionsConfig.Headers)
                {
                    optionsConfigHeader.ApplyHeader(requestContext, headers);
                }
            }

            headers.TryAdd(_optionsConfig.AllowHeader, response.ToString());

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
