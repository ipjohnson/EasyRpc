using System;
using System.Collections.Generic;
using System.Linq;
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

    // <summary>
    /// Abstract base class for parameter binding builders
    /// </summary>
    public abstract class BaseParameterBinderDelegateBuilder
    {
        private MethodInfo _bindParametersMethod;

        /// <summary>
        /// Url parameter binder
        /// </summary>
        protected IUrlParameterBinder UrlParameterBinder;

        /// <summary>
        /// Special parameter binder (CancellationToken, HttpContext, etc)
        /// </summary>
        protected ISpecialParameterBinder SpecialParameterBinder;

        /// <summary>
        /// String value binder
        /// </summary>
        protected IStringValueModelBinder _stringValueModelBinder;

        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="specialParameterBinder"></param>
        /// <param name="stringValueModelBinder"></param>
        protected BaseParameterBinderDelegateBuilder(ISpecialParameterBinder specialParameterBinder, IStringValueModelBinder stringValueModelBinder)
        {
            SpecialParameterBinder = specialParameterBinder;
            _stringValueModelBinder = stringValueModelBinder;
        }

        /// <summary>
        /// Api configuration has completed
        /// </summary>
        /// <param name="serviceScope"></param>
        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            var routingConfiguration = serviceScope.GetRequiredService<IConfigurationManager>().GetConfiguration<RoutingConfiguration>();

            if (routingConfiguration.UseAspNetCoreRouting)
            {
                UrlParameterBinder = serviceScope.GetRequiredService<IAspNetRoutingParameterBinder>();
            }
            else
            {
                UrlParameterBinder = serviceScope.GetRequiredService<IInternalRoutingParameterBinder>();
            }
        }

        /// <summary>
        /// Bind non body parameters (url, query string, etc)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="parameters"></param>
        /// <param name="parameterContext"></param>
        /// <returns></returns>
        protected virtual Task BindNonBodyParameters<T>(RequestExecutionContext context, 
            EndPointMethodConfiguration configuration,
            IReadOnlyList<RpcParameterInfo> parameters, 
            T parameterContext) where T : IRequestParameters
        {
            var pathSpan = context.HttpContext.Request.Path.Value.AsSpan();
            var currentIndex = configuration.RouteInformation.RouteBasePath.Length;

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter.BindAction != null)
                {
                    parameter.BindAction(context, configuration, parameter, parameterContext);
                }
                else if(parameter.ParameterSource == EndPointMethodParameterSource.PathParameter)
                {
                    UrlParameterBinder.BindUrlParameter(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
                }
            }

            return Task.CompletedTask;
        }

        protected virtual void BindNewData<TValue>(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext) where TValue : new()
        {
            parameterContext[parameter.Position] = new TValue();
        }

        #region Bind header

        private BindAction _headerBindAction;
        private BindAction HeaderBindAction => _headerBindAction ??= BindHeaderParameter;
        
        private void BindHeaderParameter(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameters)
        {
            if (context.HttpContext.Request.Headers.TryGetValue(parameter.BindName, out var value))
            {
                var boundValue = _stringValueModelBinder.ConvertString(parameter, value);

                parameters[parameter.Position] = boundValue;
            }
            else if (parameter.HasDefaultValue)
            {
                parameters[parameter.Position] = parameter.DefaultValue;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected virtual MethodInfo BindParametersMethod
        {
            get { return _bindParametersMethod ??= GetType().GetMethod("BindParameters", BindingFlags.NonPublic | BindingFlags.Instance); }
        }

        protected virtual void BindQueryStringParameter(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameters)
        {
            if (context.HttpContext.Request.Query.TryGetValue(parameter.BindName, out var value))
            {
                var boundValue = _stringValueModelBinder.ConvertString(parameter, value);

                context.Parameters[parameter.Position] = boundValue;
            }
            else if(parameter.HasDefaultValue)
            {
                context.Parameters[parameter.Position] = parameter.DefaultValue;
            }
        }

        protected virtual IReadOnlyList<RpcParameterInfo> GenerateOrderedNonBodyParameterList(IEndPointMethodConfigurationReadOnly configuration)
        {
            AssignBindAction(configuration.Parameters);

            if (configuration.Parameters.All(parameter => parameter.ParameterSource != EndPointMethodParameterSource.PathParameter))
            {
                return configuration.Parameters;
            }

            var newList = new List<RpcParameterInfo>();

            foreach (var routeInformationToken in configuration.RouteInformation.Tokens)
            {
                var rpcParameter = configuration.Parameters.First(parameter =>
                    string.Compare(routeInformationToken.Name, parameter.Name,
                        StringComparison.CurrentCultureIgnoreCase) == 0);

                rpcParameter.TokenStopCharacter = routeInformationToken.StopCharacter;

                newList.Add(rpcParameter);
            }

            foreach (var configurationParameter in configuration.Parameters)
            {
                if (newList.Contains(configurationParameter))
                {
                    continue;
                }

                if (configurationParameter.ParameterSource != EndPointMethodParameterSource.PostBody &&
                    configurationParameter.ParameterSource != EndPointMethodParameterSource.PostParameter)
                {
                    newList.Add(configurationParameter);
                }
            }

            if (newList.Count == 0)
            {
                return Array.Empty<RpcParameterInfo>();
            }

            return newList.Equals(configuration.Parameters) ? configuration.Parameters : newList;
        }


        private BindAction _httpContextAction;
        private BindAction HttpContextAction => _httpContextAction ??= SpecialParameterBinder.BindHttpContextParameter;

        private BindAction _httpRequestAction;
        private BindAction HttpRequestAction => _httpRequestAction ??= SpecialParameterBinder.BindHttpRequestParameter;

        private BindAction _httpCancellationAction;
        private BindAction HttpCancellationAction =>
            _httpCancellationAction ??= SpecialParameterBinder.BindHttpCancellationTokenParameter;

        private BindAction _httpResponseAction;
        private BindAction HttpResponseAction =>
            _httpResponseAction ??= SpecialParameterBinder.BindHttpResponseParameter;

        private BindAction _queryStringAction;
        private BindAction QueryStringAction => _queryStringAction ??= BindQueryStringParameter;

        private BindAction _requestExecutionContextAction;
        private BindAction RequestExecutionContextAction => _requestExecutionContextAction ??=
            SpecialParameterBinder.BindRequestExecutionContextParameter;

        private BindAction _requestServiceAction;
        private BindAction RequestServiceAction =>
            _requestServiceAction ??= SpecialParameterBinder.BindRequestServicesParameter;

        private MethodInfo _newDataMethodInfo;
        private MethodInfo NewDataMethodInfo => _newDataMethodInfo ??= GetType().GetMethod("BindNewData", BindingFlags.Instance | BindingFlags.NonPublic);

        private void AssignBindAction(IReadOnlyList<RpcParameterInfo> configurationParameters)
        {
            foreach (var parameter in configurationParameters)
            {
                switch (parameter.ParameterSource)
                {
                    case EndPointMethodParameterSource.HeaderParameter:
                        parameter.BindAction = HeaderBindAction;
                        break;

                    case EndPointMethodParameterSource.HttpContext:
                        parameter.BindAction = HttpContextAction;
                        break;

                    case EndPointMethodParameterSource.HttpRequest:
                        parameter.BindAction = HttpRequestAction;
                        break;

                    case EndPointMethodParameterSource.HttpCancellationToken:
                        parameter.BindAction = HttpCancellationAction;
                        break;

                    case EndPointMethodParameterSource.HttpResponse:
                        parameter.BindAction = HttpResponseAction;
                        break;

                    case EndPointMethodParameterSource.QueryStringParameter:
                        parameter.BindAction = QueryStringAction;
                        break;

                    case EndPointMethodParameterSource.RequestExecutionContext:
                        parameter.BindAction = RequestExecutionContextAction;
                        break;

                    case EndPointMethodParameterSource.RequestServices:
                        parameter.BindAction = RequestServiceAction;
                        break;

                    case EndPointMethodParameterSource.NewData:
                        var genericMethod = NewDataMethodInfo.MakeGenericMethod(parameter.ParamType);

                        parameter.BindAction = (BindAction)genericMethod.CreateDelegate(typeof(BindAction), this);
                        break;
                }
            }
        }
    }
}
