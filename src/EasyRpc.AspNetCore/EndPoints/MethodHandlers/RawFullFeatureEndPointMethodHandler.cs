using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    public class RawFullFeatureEndPointMethodHandler : BaseContentEndPointMethodHandler
    {
        private readonly string _contentType;

        public RawFullFeatureEndPointMethodHandler(EndPointMethodConfiguration configuration, string contentType, BaseEndPointServices services) : base(configuration, services)
        {
            _contentType = contentType;
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
                    await Services.RawContentWriter.WriteRawContent(requestContext, _contentType, null);
                }
            }
            catch (Exception e)
            {
                await Services.ErrorHandler.DefaultErrorHandlerError(requestContext, e);
            }
        }
    }
}
