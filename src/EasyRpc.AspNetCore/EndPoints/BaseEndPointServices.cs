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
    public class BaseEndPointServices
    {
        public BaseEndPointServices(IEndPointAuthorizationService authorizationService,
            IContentSerializationService serializationService, 
            IParameterBinderDelegateBuilder parameterBinderDelegateBuilder, 
            IMethodInvokerCreationService methodInvokerCreationService, 
            IErrorHandler errorHandler, 
            IRawContentWriter rawContentWriter,
            IResponseDelegateCreator responseDelegateCreator)
        {
            AuthorizationService = authorizationService;
            SerializationService = serializationService;
            ParameterBinderDelegateBuilder = parameterBinderDelegateBuilder;
            MethodInvokerCreationService = methodInvokerCreationService;
            ErrorHandler = errorHandler;
            RawContentWriter = rawContentWriter;
            ResponseDelegateCreator = responseDelegateCreator;
        }

        public IEndPointAuthorizationService AuthorizationService { get; }

        public IContentSerializationService SerializationService { get; }

        public IParameterBinderDelegateBuilder ParameterBinderDelegateBuilder { get; }

        public IMethodInvokerCreationService MethodInvokerCreationService { get; }

        public IErrorHandler ErrorHandler { get; }

        public IRawContentWriter RawContentWriter { get; }

        public IResponseDelegateCreator ResponseDelegateCreator { get; }

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
        }
    }
}
