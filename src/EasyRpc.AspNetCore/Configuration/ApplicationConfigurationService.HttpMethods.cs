using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Configuration
{
    public partial class ApplicationConfigurationService
    {
        /// <summary>
        /// Empty list of attributes
        /// </summary>
        protected readonly List<Attribute> EmptyList = new List<Attribute>();

        /// <inheritdoc />
        public void ExposeDelegate(ICurrentApiInformation currentApi, DelegateInstanceConfiguration delegateInstanceConfiguration, Delegate @delegate)
        {
            RegisterDelegate(currentApi, delegateInstanceConfiguration, @delegate);
        }

        /// <summary>
        /// Register a delegate
        /// </summary>
        /// <param name="currentApi"></param>
        /// <param name="instanceConfiguration"></param>
        /// <param name="delegate"></param>
        protected virtual void RegisterDelegate(ICurrentApiInformation currentApi, BaseDelegateInstanceConfiguration instanceConfiguration, Delegate @delegate)
        {
            foreach (var configuration in CreateEndPointMethodConfigurationForDelegate(currentApi, instanceConfiguration,@delegate))
            {
                var endPointMethodHandler = CreateEndPointMethodHandler(currentApi, configuration);

                _handlers.Add(endPointMethodHandler);
            }
        }
        
        /// <summary>
        /// Create endpoints for a delegate
        /// </summary>
        /// <param name="currentApi"></param>
        /// <param name="instanceConfiguration"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        protected virtual IEnumerable<EndPointMethodConfiguration> CreateEndPointMethodConfigurationForDelegate(ICurrentApiInformation currentApi, BaseDelegateInstanceConfiguration instanceConfiguration, Delegate func)
        {
            var fullPath = instanceConfiguration.Path;
            var funcMethod = func.Method;

            var routeList = GenerateRouteInformationList(fullPath, 
                instanceConfiguration.Method,
                GetDefaultRequestBody(instanceConfiguration.Method, instanceConfiguration.HasRequestBody), 
                currentApi,
                typeof(object), 
                "",
                funcMethod, 
                new List<Attribute>());

            foreach (var routeInformation in routeList)
            {
                var configuration = new EndPointMethodConfiguration(routeInformation, context => null,
                    new MethodInvokeInformation { DelegateToInvoke = func }, funcMethod.ReturnType);

                var parameters = GenerateMethodParameters(null, funcMethod, routeInformation);

                configuration.Parameters.AddRange(parameters);

                configuration.RawContentType = instanceConfiguration.RawContentType;

                SetStatusAndResponseBodyConfigValues(configuration, instanceConfiguration.HasResponseBody,
                    instanceConfiguration.SuccessStatusCode);

                if (string.IsNullOrEmpty(configuration.RawContentType))
                {
                    var returnType = funcMethod.ReturnType;

                    if (returnType.IsConstructedGenericType &&
                        (returnType.GetGenericTypeDefinition() == typeof(Task<>) ||
                         returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
                    {
                        returnType = returnType.GenericTypeArguments[0];
                    }

                    if (_exposeConfigurations.TypeWrapSelector(returnType))
                    {
                        configuration.WrappedType = _wrappedResultTypeCreator.GetTypeWrapper(returnType);
                    }
                }

                if (currentApi.Headers != ImmutableLinkedList<IResponseHeader>.Empty ||
                    instanceConfiguration.Headers != ImmutableLinkedList<IResponseHeader>.Empty)
                {
                    var responseHeaders = new List<IResponseHeader>();

                    responseHeaders.AddRange(currentApi.Headers);
                    responseHeaders.AddRange(instanceConfiguration.Headers);

                    configuration.ResponseHeaders = responseHeaders;
                }

                ApplyAuthorizations(currentApi, null, configuration, EmptyList, EmptyList);

                ApplyFilters(currentApi, GetFilterList(currentApi, configuration, EmptyList, EmptyList), configuration);

                if (_supportCompression)
                {
                    configuration.SupportsCompression = _compressionSelectorService.ShouldCompressResult(configuration);
                }

                yield return configuration;
            }
        }
        

        /// <summary>
        /// Set parameter source based on type
        /// </summary>
        /// <param name="routeInformation"></param>
        /// <param name="parameterType"></param>
        /// <param name="rpcParameter"></param>
        protected virtual void SetParameterSource(IRpcRouteInformation routeInformation, Type parameterType, RpcParameterInfo rpcParameter)
        {
            if (_exposeConfigurations.ResolveFromContainer(parameterType))
            {
                rpcParameter.ParameterSource = EndPointMethodParameterSource.RequestServices;
            }
            else if (parameterType == typeof(RequestExecutionContext))
            {
                rpcParameter.ParameterSource = EndPointMethodParameterSource.RequestExecutionContext;
            }
            else if (parameterType == typeof(HttpContext))
            {
                rpcParameter.ParameterSource = EndPointMethodParameterSource.HttpContext;
            }
            else if (parameterType == typeof(HttpResponse))
            {
                rpcParameter.ParameterSource = EndPointMethodParameterSource.HttpResponse;
            }
            else if (parameterType == typeof(HttpRequest))
            {
                rpcParameter.ParameterSource = EndPointMethodParameterSource.HttpRequest;
            }
            else if (parameterType == typeof(CancellationToken))
            {
                rpcParameter.ParameterSource = EndPointMethodParameterSource.HttpCancellationToken;
            }
            else
            {
                var matchingToken = 
                    routeInformation.Tokens.FirstOrDefault(token => string.Compare(token.Name, rpcParameter.Name, StringComparison.CurrentCultureIgnoreCase) == 0);

                if (matchingToken != null)
                {
                    rpcParameter.ParameterSource = EndPointMethodParameterSource.PathParameter;

                    matchingToken.ParameterInfo = rpcParameter;
                }
                else if (routeInformation.HasBody)
                {
                    rpcParameter.ParameterSource = EndPointMethodParameterSource.PostParameter;
                }
                else
                {
                    rpcParameter.ParameterSource = EndPointMethodParameterSource.QueryStringParameter;
                }
            }
        }
    }
}
