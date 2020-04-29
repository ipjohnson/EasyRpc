using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    public class NoParamsEndPointMethodHandler : BaseContentEndPointMethodHandler
    {
        public NoParamsEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services) : base(configuration, services)
        {

        }

        public override async Task HandleRequest(HttpContext context)
        {
            var requestContext = new RequestExecutionContext(context, this, Configuration.SuccessStatusCode);

            try
            {
                if (InvokeMethodDelegate == null)
                {
                    SetupMethod();
                }

                requestContext.ServiceInstance = Configuration.ActivationFunc(requestContext);
                
                if (requestContext.ContinueRequest)
                {
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
                await Services.ErrorHandler.DefaultErrorHandlerError(requestContext, e);
            }
        }
    }
}
