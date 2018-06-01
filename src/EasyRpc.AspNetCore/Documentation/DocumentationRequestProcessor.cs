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

        Task ProcessRequest(HttpContext context, Func<Task> next);
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

        public Task ProcessRequest(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path.Value.StartsWith(_configuration.Route))
            {
                var result = _assetProvider.ProcessRequest(context).Result;

                if (result)
                {
                    return Task.CompletedTask;
                }
            }

            return next();
        }
    }
}
