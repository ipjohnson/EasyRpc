using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    public class RawNoFeaturesEndPointMethodHandler : BaseContentEndPointMethodHandler
    {
        private readonly string _contentType;

        public RawNoFeaturesEndPointMethodHandler(EndPointMethodConfiguration configuration, string contentType, BaseEndPointServices services) : base(configuration, services)
        {
            _contentType = contentType;
        }

        public override async Task HandleRequest(HttpContext context)
        {
            var requestContext = new RequestExecutionContext(context, this, Configuration.SuccessStatusCode);

            try
            {
                if (BindParametersDelegate == null)
                {
                    SetupMethod();
                }

                requestContext.ServiceInstance = Configuration.ActivationFunc(requestContext);

                await BindParametersDelegate(requestContext);

                if (requestContext.ContinueRequest)
                {
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
