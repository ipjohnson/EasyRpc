﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Documentation;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// Handles unmapped requests
    /// </summary>
    public interface IUnmappedEndPointHandler
    {
        /// <summary>
        /// Handle unmapped request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="next"></param>
        /// <param name="matchPathNotMethod"></param>
        /// <returns></returns>
        Task Execute(HttpContext httpContext, RequestDelegate next, bool matchPathNotMethod = false);

        /// <summary>
        /// Configure handler
        /// </summary>
        /// <param name="apiInformation"></param>
        /// <param name="endPointMethodHandlersList"></param>
        void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);
    }

    /// <inheritdoc />
    public class UnmappedEndPointHandler : IUnmappedEndPointHandler
    {
        private readonly IDocumentationService _documentationService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="documentationService"></param>
        public UnmappedEndPointHandler(IDocumentationService documentationService)
        {
            _documentationService = documentationService;
        }

        /// <inheritdoc />
        public Task Execute(HttpContext httpContext, RequestDelegate next, bool matchPathNotMethod = false)
        {
            return _documentationService.Execute(httpContext, next);
        }

        /// <inheritdoc />
        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            _documentationService.Configure(apiInformation, endPointMethodHandlersList);
        }
    }
}