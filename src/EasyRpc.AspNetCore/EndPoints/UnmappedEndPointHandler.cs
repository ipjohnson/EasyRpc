using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Documentation;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    public interface IUnmappedEndPointHandler
    {
        Task Execute(HttpContext httpContext, RequestDelegate next, bool matchPathNotMethod = false);

        void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);
    }

    public class UnmappedEndPointHandler : IUnmappedEndPointHandler
    {
        private readonly IDocumentationService _documentationService;

        public UnmappedEndPointHandler(IDocumentationService documentationService)
        {
            _documentationService = documentationService;
        }

        public Task Execute(HttpContext httpContext, RequestDelegate next, bool matchPathNotMethod = false)
        {
            return _documentationService.Execute(httpContext, next);
        }

        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            _documentationService.Configure(apiInformation, endPointMethodHandlersList);
        }
    }
}
