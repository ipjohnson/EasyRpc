using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Routing
{
    /// <summary>
    /// Internal routing handler 
    /// </summary>
    public interface IInternalRoutingHandler
    {
        /// <summary>
        /// Attaches routing handler to asp.net pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="apiConfig"></param>
        /// <param name="scopedProvider"></param>
        void Attach(IApplicationBuilder builder, IInternalApiConfiguration apiConfig, IServiceProvider scopedProvider);
    }

    /// <inheritdoc />
    public class InternalRoutingHandler : IInternalRoutingHandler
    {
        protected RequestDelegate _requestDelegate;
        protected Func<string, IEndPointHandler> _endPointHandlers;
        protected IUnmappedEndPointHandler _unmappedEndPointHandler;
        
        /// <inheritdoc />
        public virtual void Attach(IApplicationBuilder builder, IInternalApiConfiguration apiConfig, IServiceProvider scopedProvider)
        {
            _unmappedEndPointHandler = scopedProvider.GetService<IUnmappedEndPointHandler>();

            var endPoints = CreateEndPointDictionary(apiConfig, _unmappedEndPointHandler);

            var routeBuilder = scopedProvider.GetService<IEndPointRouteBuilder>();

            _endPointHandlers = routeBuilder.BuildRouteFunc(endPoints);

            builder.Use(requestDelegate =>
            {
                _requestDelegate = requestDelegate;

                return Execute;
            });
        }
        
        protected virtual Task Execute(HttpContext context)
        {
            var endpointHandler = _endPointHandlers(context.Request.Path);

            if (endpointHandler != null)
            {
                return endpointHandler.HandleRequest(context, _requestDelegate);
            }

            return _unmappedEndPointHandler.Execute(context, _requestDelegate);
        }

        protected virtual IDictionary<string, IEndPointHandler> CreateEndPointDictionary(
            IInternalApiConfiguration apiConfig, IUnmappedEndPointHandler unmappedEndPointHandler)
        {
            var endPointMethodHandlersList = apiConfig.GetEndPointHandlers();

            _unmappedEndPointHandler.Configure(apiConfig, endPointMethodHandlersList);

            var endPointMethodHandlersDictionary = new Dictionary<string,Dictionary<string,IEndPointMethodHandler>>();

            foreach (var handler in endPointMethodHandlersList)
            {
                if (!endPointMethodHandlersDictionary.TryGetValue(handler.RouteInformation.RouteBasePath,
                    out var methodDictionary))
                {
                    methodDictionary = new Dictionary<string, IEndPointMethodHandler>();

                    endPointMethodHandlersDictionary[handler.RouteInformation.RouteBasePath] = methodDictionary;
                }

                methodDictionary[handler.HttpMethod] = handler;
            }

            var endPointHandlerDictionary = new Dictionary<string,IEndPointHandler>();

            foreach (var kvp in endPointMethodHandlersDictionary)
            {
                var valuesList = kvp.Value.Values.ToList();

                var anyParameters = valuesList.Any(h => h.RouteInformation.Tokens.Count > 0);

                endPointHandlerDictionary[kvp.Key] = new HttpMethodEndPointHandler(kvp.Key, anyParameters, valuesList, _unmappedEndPointHandler);
            }

            return endPointHandlerDictionary;
        }

    }
}
