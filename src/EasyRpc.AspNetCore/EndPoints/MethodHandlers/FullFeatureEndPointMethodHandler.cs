using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    public class FullFeatureEndPointMethodHandler : BaseContentEndPointMethodHandler
    {
        public FullFeatureEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services) : base(configuration,services)
        {
        }


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
                    await Services.SerializationService.SerializeToResponse(requestContext);
                }
            }
            catch (Exception e)
            {
                await Services.ErrorHandler.DefaultErrorHandlerError(requestContext, e);
            }
        }
    }
}
