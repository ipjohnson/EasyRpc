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
    /// <summary>
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

                switch (parameter.ParameterSource)
                {
                    case EndPointMethodParameterSource.PathParameter:
                        UrlParameterBinder.BindUrlParameter(context, configuration, parameter, parameterContext,
                            ref currentIndex, pathSpan);
                        break;
                    case EndPointMethodParameterSource.QueryStringParameter:
                        BindQueryStringParameter(context, configuration, parameter, parameterContext);
                        break;
                    case EndPointMethodParameterSource.RequestServices:
                        SpecialParameterBinder.BindRequestServicesParameter(context, parameter, parameterContext);
                        break;
                    case EndPointMethodParameterSource.HttpContext:
                        SpecialParameterBinder.BindHttpContextParameter(context, parameter, parameterContext);
                        break;
                    case EndPointMethodParameterSource.RequestExecutionContext:
                        SpecialParameterBinder.BindRequestExecutionContextParameter(context, parameter, parameterContext);
                        break;
                    case EndPointMethodParameterSource.HttpResponse:
                        SpecialParameterBinder.BindHttpResponseParameter(context, parameter, parameterContext);
                        break;
                    case EndPointMethodParameterSource.HttpRequest:
                        SpecialParameterBinder.BindHttpRequestParameter(context, parameter, parameterContext);
                        break;
                    case EndPointMethodParameterSource.HttpCancellationToken:

                        break;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual MethodInfo BindParametersMethod
        {
            get { return _bindParametersMethod ??= GetType().GetMethod("BindParameters", BindingFlags.NonPublic | BindingFlags.Instance); }
        }

        protected virtual void BindQueryStringParameter(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameters)
        {
            if (context.HttpContext.Request.Query.TryGetValue(parameter.Name, out var value))
            {
                var boundValue = _stringValueModelBinder.ConvertString(parameter, value);

                context.Parameters[parameter.Position] = boundValue;
            }
        }

        protected virtual IReadOnlyList<RpcParameterInfo> GenerateOrderedParameterList(EndPointMethodConfiguration configuration)
        {
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

                newList.Add(rpcParameter);
            }

            foreach (var configurationParameter in configuration.Parameters)
            {
                if (newList.Contains(configurationParameter))
                {
                    continue;
                }

                newList.Add(configurationParameter);
            }

            return newList.Equals(configuration.Parameters) ? configuration.Parameters : newList;
        }
    }
}
