using System;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using SimpleFixture.NSubstitute;

namespace EasyRpc.Tests.Documentation
{
    [SubFixtureInitialize]
    public class BaseDocumentationTests
    {
        protected class MiddlewareContext
        {
            public Func<RequestDelegate, RequestDelegate> ExecuteDelegate { get; set; }
        }

        protected MiddlewareContext MiddlewareContextInstance;

        protected void Configure(IApplicationBuilder app, string route, Action<IApiConfiguration> api, RpcServiceConfiguration configuration = null)
        {
            Func<RequestDelegate, RequestDelegate> executeDelegate = null;
            var options = Options.Create(configuration ?? new RpcServiceConfiguration());
            var fromService = new FromServicesManager(options);

            app.ApplicationServices.GetService(typeof(IRpcMessageProcessor))
                .Returns(new RpcMessageProcessor(options,
                    new ContentEncodingProvider(new IContentEncoder[0]),
                    new ContentSerializerProvider(new IContentSerializer[]
                    {
                        new DefaultJsonContentSerializer(new ParameterArrayDeserializerBuilder(fromService), 
                            new NamedParameterDeserializerBuilder(fromService), JsonSerializer.CreateDefault())
                    }),
                    new ExposeMethodInformationCacheManager(), 
                    new InstanceActivator()
                ));

            app.Use(Arg.Do<Func<RequestDelegate, RequestDelegate>>(func => executeDelegate = func));

            app.UseJsonRpc(route, api);

            MiddlewareContextInstance = new MiddlewareContext { ExecuteDelegate = executeDelegate };
        }

        protected virtual string MakeHttpCall(HttpContext context, string route, bool compress = false)
        {
            return "";
        }

    }
}
