﻿using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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

            app.ApplicationServices.GetService(typeof(IJsonRpcMessageProcessor))
                .Returns(new JsonRpcMessageProcessor(Options.Create(configuration ?? new RpcServiceConfiguration()),
                    new JsonSerializerProvider(),
                    new NamedParameterToArrayDelegateProvider(),
                    new OrderedParameterToArrayDelegateProvider(),
                    new ArrayMethodInvokerBuilder(),
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