using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// Executes IActionResult instances
    /// </summary>
    public interface ICustomActionResultExecutor
    {
        /// <summary>
        /// Execute the IActionResult
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        Task Execute(RequestExecutionContext context, IActionResult actionResult);
    }

    /// <inheritdoc />
    public class CustomActionResultExecutor : ICustomActionResultExecutor
    {
        /// <inheritdoc />
        public Task Execute(RequestExecutionContext context, IActionResult actionResult)
        {
            return actionResult.ExecuteResultAsync(GenerateActionContext(context, actionResult));
        }

        /// <summary>
        /// Generate an action context for execution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        protected virtual ActionContext GenerateActionContext(RequestExecutionContext context, IActionResult actionResult)
        {
            var routeData = GenerateRouteData(context, actionResult);

            var actionDescription = GenerateActionDescriptor(context, actionResult);

            return new ActionContext(context.HttpContext, routeData, actionDescription);
        }

        /// <summary>
        /// Generate action descriptor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        protected virtual ActionDescriptor GenerateActionDescriptor(RequestExecutionContext context, IActionResult actionResult)
        {
            return new ActionDescriptor();
        }

        /// <summary>
        /// Generate route data for action
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actionResult"></param>
        /// <returns></returns>
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
