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
        public void ExposeExpression<TResult>(ICurrentApiInformation currentApi, ExpressionInstanceConfiguration instanceConfiguration, Expression<Func<TResult>> expression)
        {
            var func = expression.Compile();
            Delegate finalDelegate = func;

            RegisterExpression(currentApi, instanceConfiguration, expression, finalDelegate);
        }

        /// <inheritdoc />
        public void ExposeExpression<TArg1, TResult>(ICurrentApiInformation currentApi, ExpressionInstanceConfiguration instanceConfiguration,
            Expression<Func<TArg1, TResult>> expression)
        {
            var func = expression.Compile();
            Delegate finalDelegate = func;

            // this is to handle cases where the TResult is private (anonymous type and can't be referenced in dynamic type)
            if (typeof(TResult).IsNotPublic)
            {
                finalDelegate = new Func<TArg1, object>(arg1 => func(arg1));
            }

            RegisterExpression(currentApi, instanceConfiguration, expression, finalDelegate);
        }

        /// <inheritdoc />
        public void ExposeExpression<TArg1, TArg2, TResult>(ICurrentApiInformation currentApi, ExpressionInstanceConfiguration instanceConfiguration,
            Expression<Func<TArg1, TArg2, TResult>> expression)
        {
            var func = expression.Compile();
            Delegate finalDelegate = func;

            // this is to handle cases where the TResult is private (anonymous type and can't be referenced in dynamic type)
            if (typeof(TResult).IsNotPublic)
            {
                finalDelegate = new Func<TArg1, TArg2, object>((arg1, arg2) => func(arg1, arg2));
            }

            RegisterExpression(currentApi, instanceConfiguration, expression, finalDelegate);
        }

        /// <inheritdoc />
        public void ExposeExpression<TArg1, TArg2, TArg3, TResult>(ICurrentApiInformation currentApi, ExpressionInstanceConfiguration instanceConfiguration,
            Expression<Func<TArg1, TArg2, TArg3, TResult>> expression)
        {
            var func = expression.Compile();
            Delegate finalDelegate = func;

            // this is to handle cases where the TResult is private (anonymous type and can't be referenced in dynamic type)
            if (typeof(TResult).IsNotPublic)
            {
                finalDelegate = new Func<TArg1, TArg2, TArg3, object>((arg1, arg2, arg3) => func(arg1, arg2, arg3));
            }

            RegisterExpression(currentApi, instanceConfiguration, expression, finalDelegate);
        }

        /// <summary>
        /// Register expression end point
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="currentApi"></param>
        /// <param name="instanceConfiguration"></param>
        /// <param name="expression"></param>
        /// <param name="finalDelegate"></param>
        protected virtual void RegisterExpression<TDelegate>(ICurrentApiInformation currentApi, ExpressionInstanceConfiguration instanceConfiguration,
            Expression<TDelegate> expression, Delegate finalDelegate)
        {
            foreach (var configuration in CreateEndPointMethodConfigurationForFunc(currentApi, instanceConfiguration, finalDelegate, expression))
            {
                var endPointMethodHandler = CreateEndPointMethodHandler(currentApi, configuration);

                _handlers.Add(endPointMethodHandler);
            }
        }

        /// <summary>
        /// Create end point configuration for func delegate
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="currentApi"></param>
        /// <param name="instanceConfiguration"></param>
        /// <param name="func"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual IEnumerable<EndPointMethodConfiguration> CreateEndPointMethodConfigurationForFunc<TDelegate>(ICurrentApiInformation currentApi, ExpressionInstanceConfiguration instanceConfiguration, Delegate func, Expression<TDelegate> expression)
        {
            var fullPath = instanceConfiguration.Path;

            foreach (var routeInformation in GenerateRouteInformationList(fullPath, instanceConfiguration.Method, instanceConfiguration.HasBody, currentApi, typeof(object), "",
                func.Method, new List<Attribute>()))
            {
                var configuration = new EndPointMethodConfiguration(routeInformation, context => null,
                    new MethodInvokeInformation { DelegateToInvoke = func }, expression.ReturnType);

                var parameters = GenerateMethodParametersForExpression(currentApi, routeInformation, expression);

                configuration.Parameters.AddRange(parameters);

                configuration.RawContentType = instanceConfiguration.RawContentType;

                if (string.IsNullOrEmpty(configuration.RawContentType))
                {
                    var returnType = expression.ReturnType;

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
        /// Generate method parameter for expression
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="currentApi"></param>
        /// <param name="routeInformation"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual IEnumerable<RpcParameterInfo> GenerateMethodParametersForExpression<TDelegate>(ICurrentApiInformation currentApi, IRpcRouteInformation routeInformation, Expression<TDelegate> expression)
        {
            var parameterList = new List<RpcParameterInfo>();
            var bodyParams = 0;
            var i = 0;

            foreach (var expressionParameter in expression.Parameters)
            {
                var rpcParameter = new RpcParameterInfo
                {
                    ParamType = expressionParameter.Type,
                    Name = expressionParameter.Name,
                    Position = i++
                };

                SetParameterSource(routeInformation, expressionParameter.Type, rpcParameter);

                if (rpcParameter.ParameterSource == EndPointMethodParameterSource.PostParameter)
                {
                    bodyParams++;
                }

                parameterList.Add(rpcParameter);
            }
            
            foreach (var routeToken in routeInformation.Tokens)
            {
                if (routeToken.ParameterInfo == null)
                {
                    var rpcParam = new RpcParameterInfo
                    {
                        Position = i++,
                        Name = routeToken.Name,
                        HasDefaultValue = false,
                        DefaultValue = null,
                        ParamType = typeof(string),
                        ParameterSource = EndPointMethodParameterSource.PathParameter,
                        IsInvokeParameter = false
                    };

                    parameterList.Add(rpcParam);
                }
            }

            if (bodyParams == 1 && _exposeConfigurations.SingleParameterPostFromBody)
            {
                var parameter = parameterList.First(rpc =>
                    rpc.ParameterSource == EndPointMethodParameterSource.PostParameter);

                if (!_exposeConfigurations.TypeWrapSelector(parameter.ParamType))
                {
                    parameter.ParameterSource = EndPointMethodParameterSource.PostBody;
                }
            }

            return parameterList;
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
