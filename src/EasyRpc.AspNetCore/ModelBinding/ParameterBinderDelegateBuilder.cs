using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.EndPoints.MethodHandlers;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Serializers;

namespace EasyRpc.AspNetCore.ModelBinding
{
    /// <summary>
    /// Interface for building a delegate that can be used to create a bound request parameter object
    /// </summary>
    public interface IParameterBinderDelegateBuilder
    {
        /// <summary>
        /// Create delegate that instantiates a request parameter object using values found in the request url and body
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="parametersType"></param>
        /// <returns></returns>
        MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, out Type parametersType);
    }

    /// <summary>
    /// Builds a delegate that can be used to bind a request parameter object
    /// </summary>
    public class ParameterBinderDelegateBuilder : IParameterBinderDelegateBuilder, IApiConfigurationCompleteAware
    {
        private readonly IContentSerializationService _serializationService;
        private readonly IDeserializationTypeCreator _deserializationTypeCreator;
        private readonly INoBodyParameterBinderDelegateBuilder _noBodyParameterBinder;
        private readonly IBodyParameterBinderDelegateBuilder _bodyParameterBinder;
        private readonly IBodySingleParameterBinderDelegateBuilder _bodySingleParameterBinder;

        private static readonly EmptyParameters _emptyParameters = new EmptyParameters();
        private static readonly MethodEndPointDelegate _emptyParameterDelegate = EmptyParameterFunc;

        private static readonly MethodInfo _defaultBodyDeserialize =
            typeof(ParameterBinderDelegateBuilder).GetMethod("DefaultBodyDeserialize", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationService"></param>
        /// <param name="deserializationTypeCreator"></param>
        /// <param name="noBodyParameterBinder"></param>
        /// <param name="bodyParameterBinder"></param>
        /// <param name="bodySingleParameterBinder"></param>
        public ParameterBinderDelegateBuilder(IContentSerializationService serializationService,
            IDeserializationTypeCreator deserializationTypeCreator,
            INoBodyParameterBinderDelegateBuilder noBodyParameterBinder, 
            IBodyParameterBinderDelegateBuilder bodyParameterBinder,
            IBodySingleParameterBinderDelegateBuilder bodySingleParameterBinder)
        {
            _serializationService = serializationService;
            _deserializationTypeCreator = deserializationTypeCreator;
            _noBodyParameterBinder = noBodyParameterBinder;
            _bodyParameterBinder = bodyParameterBinder;
            _bodySingleParameterBinder = bodySingleParameterBinder;
        }
        
        /// <inheritdoc />
        public MethodEndPointDelegate CreateParameterBindingMethod(EndPointMethodConfiguration configuration, out Type parametersType)
        {
            if (configuration.Parameters.Count == 0)
            {
                return EmptyParameterBinding(configuration, out parametersType);
            }

            parametersType = _deserializationTypeCreator.CreateTypeForMethod(configuration);

            if (configuration.Parameters.TrueForAll(p =>
                p.ParameterSource == EndPointMethodParameterSource.PostParameter))
            {
                return CreateDefaultBodyBinding(configuration, parametersType);
            }

            if (configuration.Parameters.Any(p => p.ParameterSource == EndPointMethodParameterSource.PostParameter))
            {
                return CreateBodyParameterBinding(configuration, parametersType);
            }

            var bodyBinding =
                configuration.Parameters.SingleOrDefault(p =>
                    p.ParameterSource == EndPointMethodParameterSource.PostBody);

            if (bodyBinding != null)
            {
                return CreateBodySingleParameterBinding(configuration, parametersType);
            }

            return CreateNoBodyParameterBinding(configuration, parametersType);
        }

        protected virtual MethodEndPointDelegate CreateNoBodyParameterBinding(EndPointMethodConfiguration configuration, Type parametersType)
        {
            return _noBodyParameterBinder.CreateParameterBindingMethod(configuration, parametersType);
        }

        private MethodEndPointDelegate CreateBodySingleParameterBinding(EndPointMethodConfiguration configuration, Type parametersType)
        {
            return _bodySingleParameterBinder.CreateParameterBindingMethod(configuration, parametersType);
        }

        private MethodEndPointDelegate CreateBodyParameterBinding(EndPointMethodConfiguration configuration, Type parametersType)
        {
            return _bodyParameterBinder.CreateParameterBindingMethod(configuration, parametersType);
        }

        protected virtual MethodEndPointDelegate CreateDefaultBodyBinding(EndPointMethodConfiguration configuration, Type parametersType)
        {
            var closedMethod = _defaultBodyDeserialize.MakeGenericMethod(parametersType);

            return (MethodEndPointDelegate)closedMethod.CreateDelegate(typeof(MethodEndPointDelegate), this);
        }

        protected virtual MethodEndPointDelegate EmptyParameterBinding(EndPointMethodConfiguration configuration, out Type parametersType)
        {
            parametersType = typeof(EmptyParameters);

            return _emptyParameterDelegate;
        }

        protected async Task DefaultBodyDeserialize<T>(RequestExecutionContext context) where T : IRequestParameters
        {
            context.Parameters = await _serializationService.DeserializeFromRequest<T>(context);
        }

        protected static Task EmptyParameterFunc(RequestExecutionContext context)
        {
            context.Parameters = _emptyParameters;

            return Task.CompletedTask;
        }

        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            _noBodyParameterBinder.ApiConfigurationComplete(serviceScope);
            _bodyParameterBinder.ApiConfigurationComplete(serviceScope);
            _bodySingleParameterBinder.ApiConfigurationComplete(serviceScope);
        }
    }
}
