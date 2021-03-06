﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace EasyRpc.AspNetCore.Serializers
{
    /// <summary>
    /// Content serialization service
    /// </summary>
    public class ContentSerializationService : IContentSerializationService, IApiConfigurationCompleteAware
    {
        private readonly IContentSerializer _serializer = null;
        private readonly IContentSerializer _defaultSerializer = null;
        private readonly List<IContentSerializer> _contentSerializers;
        private readonly ICustomActionResultExecutor _actionResultExecutor;
        private readonly IErrorHandler _errorHandler;
        private List<string> _supportedContentTypes;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionResultExecutor"></param>
        /// <param name="errorHandler"></param>
        /// <param name="contentSerializers"></param>
        public ContentSerializationService(ICustomActionResultExecutor actionResultExecutor, IErrorHandler errorHandler, IEnumerable<IContentSerializer> contentSerializers)
        {
            _actionResultExecutor = actionResultExecutor;
            _errorHandler = errorHandler;

            _contentSerializers = contentSerializers.ToList();
            _contentSerializers.Reverse();

            if (_contentSerializers.Count == 1)
            {
                _serializer = _contentSerializers[0];
            }
            else
            {
                _defaultSerializer = _contentSerializers.FirstOrDefault(s => s.IsDefault);
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> SupportedContentTypes =>
            _supportedContentTypes ??= GenerateSupportedContentTypesList();

        /// <inheritdoc />
        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            foreach (var contentSerializer in _contentSerializers)
            {
                if (contentSerializer is IApiConfigurationCompleteAware awareComponent)
                {
                    awareComponent.ApiConfigurationComplete(serviceScope);
                }
            }
        }

        /// <inheritdoc />
        public Task SerializeToResponse(RequestExecutionContext context)
        {
            if (context.Result is IActionResult actionResult)
            {
                return _actionResultExecutor.Execute(context, actionResult);
            }

            if (context.Result != null &&
                context.ContentSerializer != null)
            {
                return context.ContentSerializer.SerializeToResponse(context);
            }

            if (context.Result == null)
            {
                return NullResult(context);
            }

            if (_serializer != null && 
                _serializer.IsDefault)
            {
                return _serializer.SerializeToResponse(context);
            }

            return NegotiateSerialization(context);
        }

        /// <inheritdoc />
        public ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;

            if (_serializer != null)
            {
                if (_serializer.CanDeserialize(context, contentType))
                {
                    context.ContentSerializer = _serializer;

                    return _serializer.DeserializeFromRequest<T>(context);
                }

                return _errorHandler.HandleDeserializeUnknownContentType<T>(context);
            }

            return NegotiateDeserialize<T>(context, contentType);
        }

        /// <summary>
        /// Deserializes content using Content-Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected virtual ValueTask<T> NegotiateDeserialize<T>(RequestExecutionContext context, string contentType)
        {
            foreach (var contentSerializer in _contentSerializers)
            {
                if (contentSerializer.CanDeserialize(context, contentType))
                {
                    context.ContentSerializer = contentSerializer;

                    return contentSerializer.DeserializeFromRequest<T>(context);
                }
            }

            return _errorHandler.HandleDeserializeUnknownContentType<T>(context);
        }

        /// <summary>
        /// Negotiate serialization
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Task NegotiateSerialization(RequestExecutionContext context)
        {
            context.HttpContext.Request.Headers.TryGetValue("Accept", out var accepts);

            foreach (var serializer in _contentSerializers)
            {
                if (serializer.CanSerialize(context, accepts))
                {
                    return serializer.SerializeToResponse(context);
                }
            }

            if (_defaultSerializer != null)
            {
                return _defaultSerializer.SerializeToResponse(context);
            }

            return _errorHandler.HandleSerializerUnknownContentType(context);
        }

        /// <summary>
        /// Return a null result value based
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Task NullResult(RequestExecutionContext context)
        {
            var method = context.HttpContext.Request.Method;

            if (method == HttpMethods.Post)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Generates a list of content types from the content serializer
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> GenerateSupportedContentTypesList()
        {
            if (_serializer != null)
            {
                return _serializer.SupportedContentTypes.ToList();
            }

            var returnList = new List<string>();

            // we want to put the default in first
            if (_defaultSerializer != null)
            {
                returnList.AddRange(_defaultSerializer.SupportedContentTypes);
            }

            foreach (var contentSerializer in _contentSerializers)
            {
                if (contentSerializer != _defaultSerializer)
                {
                    returnList.AddRange(contentSerializer.SupportedContentTypes);
                }
            }

            return returnList;
        }
    }
}
