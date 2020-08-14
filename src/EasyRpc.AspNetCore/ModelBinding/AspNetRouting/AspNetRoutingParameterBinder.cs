using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;
using Microsoft.AspNetCore.Routing;

namespace EasyRpc.AspNetCore.ModelBinding.AspNetRouting
{
    public interface IAspNetRoutingParameterBinder : IUrlParameterBinder
    {

    }

    public class AspNetRoutingParameterBinder : IAspNetRoutingParameterBinder
    {
        private IStringValueModelBinder _stringValueModelBinder;

        public AspNetRoutingParameterBinder(IStringValueModelBinder stringValueModelBinder)
        {
            _stringValueModelBinder = stringValueModelBinder;
        }

        public void BindUrlParameter(RequestExecutionContext context, 
            EndPointMethodConfiguration configuration,
            IRpcParameterInfo parameter, 
            IRequestParameters parameterContext, 
            ref int currentIndex, 
            in ReadOnlySpan<char> pathSpan)
        {
            var routeData = context.HttpContext.GetRouteData();

            if (routeData.Values.TryGetValue(parameter.Name, out var routeValue))
            {
                context.Parameters[parameter.Position] =
                    _stringValueModelBinder.ConvertString(parameter, routeValue.ToString());
            }
            else if (parameter.HasDefaultValue)
            {
                // don't do anything as it's already set when the context is created
            }
            else
            {
                HandleMissingParameter(context, configuration, parameter, parameterContext);
            }
        }

        protected virtual void HandleMissingParameter(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            
        }
    }
}
