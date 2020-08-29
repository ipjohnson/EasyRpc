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
        private readonly IEndPointMethodHandler _postHandler;
        private readonly IEndPointMethodHandler _getHandler;
        private readonly List<IEndPointMethodHandler> _other;
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

            _postHandler = handlers.FirstOrDefault(x => x.HttpMethod == HttpMethods.Post);

            if (_postHandler != null)
            {
                handlers.Remove(_postHandler);
            }

            _getHandler = handlers.FirstOrDefault(x => x.HttpMethod == HttpMethods.Get);

            if (_getHandler != null)
            {
                handlers.Remove(_getHandler);
            }

            if (handlers.Count > 0)
            {
                _other = handlers;
            }
        }
        
        /// <inheritdoc />
        public string Path { get; }

        /// <inheritdoc />
        public bool SupportsLongerPaths { get; }

        /// <inheritdoc />
        public Task HandleRequest(HttpContext context, RequestDelegate next)
        {
            if (_getHandler != null &&
                context.Request.Method == HttpMethods.Get)
            {
                return _getHandler.HandleRequest(context);
            }

            if (_postHandler != null && 
                context.Request.Method == HttpMethods.Post)
            {
                return _postHandler.HandleRequest(context);
            }
            
            return HandleOther(context, next);
        }

        private Task HandleOther(HttpContext context, RequestDelegate next)
        {
            if (_other != null)
            {
                foreach (var endPointMethodHandler in _other)
                {
                    if (endPointMethodHandler.HttpMethod == context.Request.Method)
                    {
                        return endPointMethodHandler.HandleRequest(context);
                    }
                }
            }

            return _unknownEndPointHandler.Execute(context, next, true);
        }
    }
}
