using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.ModelBinding.AspNetRouting;
using EasyRpc.AspNetCore.ModelBinding.InternalRouting;
using EasyRpc.AspNetCore.Routing;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public interface IBodyParameterBinderDelegateBuilder : IApiConfigurationCompleteAware
    {
        MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, Type parametersType);
    }

    public class BodyParameterBinderDelegateBuilder : BaseParameterBinderDelegateBuilder, IBodyParameterBinderDelegateBuilder
    {
        private readonly IContentSerializationService _contentSerializationService;
        
        public BodyParameterBinderDelegateBuilder(ISpecialParameterBinder specialParameterBinder, 
            IContentSerializationService contentSerializationService, 
            IStringValueModelBinder stringValueModelBinder) 
            : base(specialParameterBinder, stringValueModelBinder)
        {
            _contentSerializationService = contentSerializationService;
        }

        public MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, Type parametersType)
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
        
        protected virtual async Task BindParameters<T>(RequestExecutionContext context,
            EndPointMethodConfiguration configuration, 
            List<RpcParameterInfo> parameters)
            where T : IRequestParameters
        {
            var requestParameters = await _contentSerializationService.DeserializeFromRequest<T>(context);

            context.Parameters = requestParameters;

            await BindNonBodyParameters(context, configuration, parameters, requestParameters);
        }
    }
}
