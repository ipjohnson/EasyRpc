using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Features;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Serialize extensions
    /// </summary>
   public static class SerializeExtensions
    {
        /// <summary>
        /// Deserialize from request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<T> Deserialize<T>(this RequestExecutionContext context)
        {
            return context.Handler.Services.SerializationService.DeserializeFromRequest<T>(context);
        }

        /// <summary>
        /// Serialize object to response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseObject"></param>
        /// <returns></returns>
        public static Task Serialize(this RequestExecutionContext context, object responseObject)
        {
            context.Result = responseObject;

            return context.Handler.Services.SerializationService.SerializeToResponse(context);
        }

        /// <summary>
        /// Deserialize from request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ValueTask<T> Deserialize<T>(this HttpRequest request)
        {
            var requestExecutionContextFeature = GetRequestExecutionContext(request.HttpContext);

            return Deserialize<T>(requestExecutionContextFeature);
        }

        /// <summary>
        /// Serialize object to response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseObject"></param>
        /// <returns></returns>
        public static Task Serialize(this HttpResponse response, object responseObject)
        {
            var requestExecutionContextFeature = GetRequestExecutionContext(response.HttpContext);

            return Serialize(requestExecutionContextFeature, responseObject);
        }
        
        private static RequestExecutionContext GetRequestExecutionContext(HttpContext context)
        {
            var requestExecutionContextFeature = context.Features.Get<IRequestExecutionContextFeature>();

            if (requestExecutionContextFeature == null)
            {
                throw new Exception("RequestExecutionContext feature not found");
            }

            if (requestExecutionContextFeature.Context == null)
            {
                throw new Exception("RequestExecutionContext context is null");
            }

            return requestExecutionContextFeature.Context;
        }

    }
}
