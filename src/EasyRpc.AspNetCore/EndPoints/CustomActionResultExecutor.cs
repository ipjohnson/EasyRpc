using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace EasyRpc.AspNetCore.EndPoints
{
    public interface ICustomActionResultExecutor
    {
        Task Execute(RequestExecutionContext context, IActionResult actionResult);
    }

    public class CustomActionResultExecutor : ICustomActionResultExecutor
    {
        public Task Execute(RequestExecutionContext context, IActionResult actionResult)
        {
            return actionResult.ExecuteResultAsync(GenerateActionContext(context, actionResult));
        }

        protected virtual ActionContext GenerateActionContext(RequestExecutionContext context, IActionResult actionResult)
        {
            var routeData = GenerateRouteData(context, actionResult);
            var actionDescription = GenerateActionDescriptor(context, actionResult);

            return new ActionContext(context.HttpContext, routeData, actionDescription);
        }


        protected virtual ActionDescriptor GenerateActionDescriptor(RequestExecutionContext context, IActionResult actionResult)
        {
            return new ActionDescriptor();
        }

        protected virtual RouteData GenerateRouteData(RequestExecutionContext context, IActionResult actionResult)
        {
            if (context.Parameters.ParameterCount == 0)
            {
                return new RouteData();
            }

            var routeDictionary = new RouteValueDictionary();
            var parameters = context.Parameters;

            foreach (var parameterInfo in parameters.ParameterInfos)
            {
                routeDictionary.TryAdd(parameterInfo.Name, parameters[parameterInfo.Position]);
            }

            return new RouteData(routeDictionary);
        }
    }
}
