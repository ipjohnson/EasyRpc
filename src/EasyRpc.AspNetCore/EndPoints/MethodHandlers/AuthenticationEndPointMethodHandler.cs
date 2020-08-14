﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    /// <summary>
    /// End point with authentication requirements 
    /// </summary>
    public class AuthenticationEndPointMethodHandler : BaseContentEndPointMethodHandler
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public AuthenticationEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services) : base(configuration, services)
        {
        }

        /// <inheritdoc />
        public override async Task HandleRequest(HttpContext httpContext)
        {
            var requestContext = new RequestExecutionContext(httpContext, this, Configuration.SuccessStatusCode);

            try
            {
                if (BindParametersDelegate == null)
                {
                    SetupMethod();
                }

                if (Configuration.Authorizations != null)
                {
                    if (!await Services.AuthorizationService.AuthorizeRequest(requestContext, Configuration.Authorizations))
                    {
                        await Services.ErrorHandler.HandleUnauthorized(requestContext);

                        return;
                    }
                }

                await BindParametersDelegate(requestContext);

                if (requestContext.ContinueRequest)
                {
                    requestContext.ServiceInstance = Configuration.ActivationFunc(requestContext);

                    await InvokeMethodDelegate(requestContext);
                }

                if (!requestContext.ResponseHasStarted &&
                    requestContext.ContinueRequest)
                {
                    await ResponseDelegate(requestContext);
                }
            }
            catch (Exception e)
            {
                await Services.ErrorHandler.HandleException(requestContext, e);
            }
        }
    }
}
