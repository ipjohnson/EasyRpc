using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.ModelBinding;
using EasyRpc.AspNetCore.Serializers;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// Collection of services
    /// </summary>
    public class BaseEndPointServices
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="authorizationService"></param>
        /// <param name="serializationService"></param>
        /// <param name="parameterBinderDelegateBuilder"></param>
        /// <param name="methodInvokerCreationService"></param>
        /// <param name="errorHandler"></param>
        /// <param name="rawContentWriter"></param>
        /// <param name="responseDelegateCreator"></param>
        /// <param name="unmappedEndPointHandler"></param>
        public BaseEndPointServices(IEndPointAuthorizationService authorizationService,
            IContentSerializationService serializationService, 
            IParameterBinderDelegateBuilder parameterBinderDelegateBuilder, 
            IMethodInvokerCreationService methodInvokerCreationService, 
            IErrorHandler errorHandler, 
            IRawContentWriter rawContentWriter,
            IResponseDelegateCreator responseDelegateCreator, 
            IUnmappedEndPointHandler unmappedEndPointHandler)
        {
            AuthorizationService = authorizationService;
            SerializationService = serializationService;
            ParameterBinderDelegateBuilder = parameterBinderDelegateBuilder;
            MethodInvokerCreationService = methodInvokerCreationService;
            ErrorHandler = errorHandler;
            RawContentWriter = rawContentWriter;
            ResponseDelegateCreator = responseDelegateCreator;
            UnmappedEndPointHandler = unmappedEndPointHandler;
        }

        /// <summary>
        /// Authorization service
        /// </summary>
        public IEndPointAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// Serialization service
        /// </summary>
        public IContentSerializationService SerializationService { get; }

        /// <summary>
        /// Parameter binding service
        /// </summary>
        public IParameterBinderDelegateBuilder ParameterBinderDelegateBuilder { get; }

        /// <summary>
        /// Method invoker creation service
        /// </summary>
        public IMethodInvokerCreationService MethodInvokerCreationService { get; }

        /// <summary>
        /// Error handler service
        /// </summary>
        public IErrorHandler ErrorHandler { get; }

        /// <summary>
        /// Raw content writer service
        /// </summary>
        public IRawContentWriter RawContentWriter { get; }

        /// <summary>
        /// Response delegate creator
        /// </summary>
        public IResponseDelegateCreator ResponseDelegateCreator { get; }

        /// <summary>
        /// Unmapped end point handler
        /// </summary>
        public IUnmappedEndPointHandler UnmappedEndPointHandler { get; }
        
        /// <summary>
        /// Configuration is complete
        /// </summary>
        /// <param name="serviceScope"></param>
        public void ConfigurationComplete(IServiceProvider serviceScope)
        {
            if (AuthorizationService is IApiConfigurationCompleteAware authorizationAware)
            {
                authorizationAware.ApiConfigurationComplete(serviceScope);
            }

            if (SerializationService is IApiConfigurationCompleteAware serializationAware)
            {
                serializationAware.ApiConfigurationComplete(serviceScope);
            }

            if (ParameterBinderDelegateBuilder is IApiConfigurationCompleteAware parameterBinderAware)
            {
                parameterBinderAware.ApiConfigurationComplete(serviceScope);
            }

            if (MethodInvokerCreationService is IApiConfigurationCompleteAware methodInvokerAware)
            {
                methodInvokerAware.ApiConfigurationComplete(serviceScope);
            }

            if (ErrorHandler is IApiConfigurationCompleteAware errorHandlerAware)
            {
                errorHandlerAware.ApiConfigurationComplete(serviceScope);
            }

            if (RawContentWriter is IApiConfigurationCompleteAware rawContentWriterAware)
            {
                rawContentWriterAware.ApiConfigurationComplete(serviceScope);
            }

            if (ResponseDelegateCreator is IApiConfigurationCompleteAware responseDelegateAware)
            {
                responseDelegateAware.ApiConfigurationComplete(serviceScope);
            }

            if (UnmappedEndPointHandler is IApiConfigurationCompleteAware unmappedEndAware)
            {
                unmappedEndAware.ApiConfigurationComplete(serviceScope);
            }
        }
    }
}
