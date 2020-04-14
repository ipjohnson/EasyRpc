using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    public class NoFeaturesEndPointMethodHandler : BaseContentEndPointMethodHandler
    {
        public NoFeaturesEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services) : base(configuration, services)
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
