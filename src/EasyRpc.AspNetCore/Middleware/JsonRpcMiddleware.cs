using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Middleware
{
    public class JsonRpcMiddleware
    {
        private readonly string _route;
        private readonly IJsonRpcMessageProcessor _messageProcessor;

        public JsonRpcMiddleware(IApplicationBuilder app, string route, Action<IApiConfiguration> configuration)
        {
            _route = PrepareRoute(route);

            _messageProcessor = app.ApplicationServices.GetService<IJsonRpcMessageProcessor>();

            if (_messageProcessor == null)
            {
                throw new Exception("Please call services.AddJsonRpc()");
            }

            var provider = new ApiConfigurationProvider();

            configuration(provider);

            _messageProcessor.Configure(provider, _route);
        }

        private string PrepareRoute(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return "/";
            }

            if (route[0] != '/')
            {
                route = "/" + route;
            }

            if (route[route.Length - 1] != '/')
            {
                route += "/";
            }

            return route;
        }

        public static void AttachMiddleware(IApplicationBuilder app, string route,
            Action<IApiConfiguration> configuration)
        {
            var middleware = new JsonRpcMiddleware(app, route, configuration);

            app.Use(middleware.Execute);
        }

        private Task Execute(HttpContext context, Func<Task> next)
        {
            var path = context.Request.Path;

            if (context.Request.ContentType != null &&
                context.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var pathString = path.Value;

                if (string.IsNullOrEmpty(pathString))
                {
                    pathString = "/";
                }
                else if (pathString[pathString.Length - 1] != '/')
                {
                    pathString += '/';
                }

                if (string.Compare(pathString, 0, _route, 0, _route.Length, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return _messageProcessor.ProcessRequest(context);
                }
            }

            return next();
        }
    }
}
