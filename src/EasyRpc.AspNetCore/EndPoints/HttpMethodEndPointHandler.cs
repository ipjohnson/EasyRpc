using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// Collection of end point method handlers
    /// </summary>
    public class HttpMethodEndPointHandler : IEndPointHandler
    {
        private readonly IEndPointMethodHandler[] _endPoints;
        private readonly IUnmappedEndPointHandler _unknownEndPointHandler;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="supportsLongerPaths"></param>
        /// <param name="handlers"></param>
        /// <param name="unknownEndPointHandler"></param>
        public HttpMethodEndPointHandler(string path, bool supportsLongerPaths, List<IEndPointMethodHandler> handlers, IUnmappedEndPointHandler unknownEndPointHandler)
        {
            Path = path;
            SupportsLongerPaths = supportsLongerPaths;
            _unknownEndPointHandler = unknownEndPointHandler;


            _endPoints = handlers.ToArray();
        }
        
        /// <inheritdoc />
        public string Path { get; }

        /// <inheritdoc />
        public bool SupportsLongerPaths { get; }

        /// <inheritdoc />
        public Task HandleRequest(HttpContext context, RequestDelegate next)
        {
            var endPoint = _endPoints[0];

            return endPoint.HttpMethod == context.Request.Method ? 
                endPoint.HandleRequest(context) : 
                HandleOther(context, next);
        }

        private Task HandleOther(HttpContext context, RequestDelegate next)
        {
            if (_endPoints.Length > 1)
            {
                var requestMethod = context.Request.Method;

                for (var i = 1; i < _endPoints.Length; i++)
                {
                    if (_endPoints[i].HttpMethod == requestMethod)
                    {
                        return _endPoints[i].HandleRequest(context);
                    }
                }
            }

            return _unknownEndPointHandler.HandleMatchedButNoMethod(context, next, _endPoints);
        }
    }
}
