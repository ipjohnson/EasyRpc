using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Attributes
{
    public interface IRpcFilterProviderAttribute
    {
        Func<ICallExecutionContext, IEnumerable<ICallExecuteFilter>> Filter { get; }
    }

    public abstract class RpcFilterAttribute : Attribute, IRpcFilterProviderAttribute
    {
        private Type _attributeType;

        protected RpcFilterAttribute(Type attributeType)
        {
            _attributeType = attributeType;
        }

        public Func<ICallExecutionContext, IEnumerable<ICallExecuteFilter>> Filter => context =>
            new []{(ICallExecuteFilter)ActivatorUtilities.GetServiceOrCreateInstance(context.HttpContext.RequestServices, _attributeType)};
    }
}
