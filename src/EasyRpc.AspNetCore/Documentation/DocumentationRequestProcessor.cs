using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IDocumentationRequestProcessor
    {
        void Configure(EndPointConfiguration configuration);

        Task ProcessRequest(HttpContext context);
    }
    
    public class DocumentationRequestProcessor : IDocumentationRequestProcessor
    {
        private EndPointConfiguration _configuration;
        private IWebAssetProvider _assetProvider;

        public DocumentationRequestProcessor(IWebAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        public void Configure(EndPointConfiguration configuration)
        {
            _configuration = configuration;
            _assetProvider.Configure(configuration);
        }

        public Task ProcessRequest(HttpContext context)
        {
            if (context.Request.Path.Value.StartsWith(_configuration.Route))
            {
                return _assetProvider.ProcessRequest(context);
            }

            return Task.CompletedTask;
        }
    }
}
