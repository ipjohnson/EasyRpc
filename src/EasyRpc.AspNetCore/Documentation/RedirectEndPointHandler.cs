using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// class for redirecting to a specific path
    /// </summary>
    public class RedirectEndPointHandler : IEndPointMethodHandler
    {
        private readonly string _redirectPath;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="redirectPath"></param>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public RedirectEndPointHandler(string redirectPath, IEndPointMethodConfigurationReadOnly configuration, EndPointServices services)
        {
            _redirectPath = redirectPath;
            Configuration = configuration;
            Services = services;
        }

        /// <inheritdoc />
        public IEndPointMethodConfigurationReadOnly Configuration { get; }

        /// <inheritdoc />
        public EndPointServices Services { get; }

        /// <inheritdoc />
        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        /// <inheritdoc />
        public string HttpMethod => HttpMethods.Get;

        /// <inheritdoc />
        public Task HandleRequest(HttpContext context)
        {
            context.Response.Headers["Location"] = _redirectPath;
            context.Response.StatusCode = (int)HttpStatusCode.Redirect;

            return Task.CompletedTask;

        }
    }
}
