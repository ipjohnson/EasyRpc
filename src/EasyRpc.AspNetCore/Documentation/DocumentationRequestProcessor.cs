using System;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        private readonly IWebAssetProvider _assetProvider;
        private readonly ILogger<IDocumentationRequestProcessor> _logger;

        public DocumentationRequestProcessor(IWebAssetProvider assetProvider, ILogger<IDocumentationRequestProcessor> logger)
        {
            _assetProvider = assetProvider;
            _logger = logger;
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
