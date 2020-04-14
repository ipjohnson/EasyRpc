using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Serializers;

namespace EasyRpc.AspNetCore.ModelBinding
{
    /// <summary>
    /// Binds the body of the request to one parameter
    /// </summary>
    public interface IBodySingleParameterBinderDelegateBuilder : IApiConfigurationCompleteAware
    {
        MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, Type parametersType);
    }


    public class BodySingleParameterBinderDelegateBuilder : BaseParameterBinderDelegateBuilder, IBodySingleParameterBinderDelegateBuilder
    {
        private readonly IContentSerializationService _contentSerializationService;

        public BodySingleParameterBinderDelegateBuilder(ISpecialParameterBinder specialParameterBinder, 
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
            var requestParameter = Expression.Parameter(typeof(RequestExecutionContext));

            var parameter = configuration.Parameters
                .Single(p => p.ParameterSource == EndPointMethodParameterSource.PostBody);

            var closedMethod = BindParametersMethod.MakeGenericMethod(parameter.ParamType, parametersType);
            
            var orderedParameters = GenerateOrderedParameterList(configuration);

            var callExpression = Expression.Call(Expression.Constant(this), 
                closedMethod, 
                requestParameter, 
                Expression.Constant(configuration), 
                Expression.Constant(orderedParameters), 
                Expression.Constant(parameter.Position));

            return Expression.Lambda<MethodEndPointDelegate>(callExpression, requestParameter);
        }
        

        protected virtual async Task BindParameters<T, TContext>(RequestExecutionContext context,
            EndPointMethodConfiguration configuration, 
            List<RpcParameterInfo> parameters, 
            int parameterPosition)
            where TContext : IRequestParameters, new()
        {
            var requestParameters = new TContext();

            context.Parameters = requestParameters;
            
            var bodyParameter = await _contentSerializationService.DeserializeFromRequest<T>(context);

            requestParameters[parameterPosition] = bodyParameter;

            await BindNonBodyParameters(context, configuration, parameters, requestParameters);
        }
    }
}
