using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.ModelBinding.AspNetRouting;
using EasyRpc.AspNetCore.ModelBinding.InternalRouting;
using EasyRpc.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public interface INoBodyParameterBinderDelegateBuilder : IApiConfigurationCompleteAware
    {
        MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, Type parametersType);
    }

    public class NoBodyParameterBinderDelegateBuilder : BaseParameterBinderDelegateBuilder, INoBodyParameterBinderDelegateBuilder
    {
        public NoBodyParameterBinderDelegateBuilder(ISpecialParameterBinder specialParameterBinder,
            IStringValueModelBinder stringValueModelBinder) 
            : base(specialParameterBinder,stringValueModelBinder)
        {
            
        }
        
        public virtual MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, Type parametersType)
        {
            var methodExpression = CreateMethodExpression(configuration, parametersType);

            return methodExpression.Compile();
        }

        protected virtual Expression<MethodEndPointDelegate> CreateMethodExpression(EndPointMethodConfiguration configuration, Type parametersType)
        {
            var closedMethod = BindParametersMethod.MakeGenericMethod(parametersType);

            var requestParameter = Expression.Parameter(typeof(RequestExecutionContext));

            var orderedParameters = GenerateOrderedParameterList(configuration);

            var callExpression = Expression.Call(Expression.Constant(this), 
                closedMethod, 
                requestParameter, 
                Expression.Constant(configuration), 
                Expression.Constant(orderedParameters));

            return Expression.Lambda<MethodEndPointDelegate>(callExpression, requestParameter);
        }
        
        protected virtual Task BindParameters<T>(RequestExecutionContext context, 
            EndPointMethodConfiguration configuration, 
            IReadOnlyList<RpcParameterInfo> parameters) 
            where T : IRequestParameters, new()
        {
            var parameterContext = new T();

            context.Parameters = parameterContext;

            return BindNonBodyParameters(context, configuration, parameters, parameterContext);
        }
    }
}
