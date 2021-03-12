using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Binding;
using EasyRpc.Abstractions.Headers;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Response;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;
using EasyRpc.AspNetCore.ContentEncoding;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.EndPoints.MethodHandlers;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Configuration
{
    /// <summary>
    /// Class used to configure exposures
    /// </summary>
    public partial class ApplicationConfigurationService : IApplicationConfigurationService
    {
        private readonly List<IEndPointMethodHandler> _handlers = new List<IEndPointMethodHandler>();
        private readonly List<object> _configurationObjects = new List<object>();
        private readonly EndPointServices _services;
        private readonly IConfigurationManager _configurationManager;
        private readonly IAuthorizationImplementationProvider _authorizationImplementationProvider;
        private readonly ICompressionSelectorService _compressionSelectorService;
        private ExposeConfigurations _exposeConfigurations;
        private DefaultMethodConfiguration _defaultMethodConfiguration;
        private readonly IWrappedResultTypeCreator _wrappedResultTypeCreator;
        private readonly IApiEndPointInspector[] _endPointInspectors;
        private string _basePath;
        private bool _supportCompression;

        public ApplicationConfigurationService(EndPointServices services,
            IConfigurationManager configurationManager,
            IAuthorizationImplementationProvider authorizationImplementationProvider,
            IWrappedResultTypeCreator wrappedResultTypeCreator,
            ICompressionSelectorService compressionSelectorService,
            IEnumerable<IApiEndPointInspector> endPointInspectors)
        {
            _services = services;
            _configurationManager = configurationManager;
            _authorizationImplementationProvider = authorizationImplementationProvider;
            _wrappedResultTypeCreator = wrappedResultTypeCreator;
            _compressionSelectorService = compressionSelectorService;
            _endPointInspectors = endPointInspectors.ToArray();
        }

        public void AddConfigurationObject(object configurationObject)
        {
            _configurationObjects.Add(configurationObject);
        }

        public void ExposeType(ICurrentApiInformation currentApi, 
            Type type,
            ServiceActivationMethod serviceActivationMethod,
            Func<RequestExecutionContext, object> activationFunc, 
            string name,
            List<IEndPointMethodAuthorization> authorizations, Func<MethodInfo, bool> methodFilter,
            string obsoleteMessage)
        {
            methodFilter ??= DefaultFilterMethod;
            var classAttributes = type.GetCustomAttributes<Attribute>().ToList();

            foreach (var methodInfo in type.GetMethods().Where(methodFilter))
            {
                ExposeMethod(currentApi, type,serviceActivationMethod, activationFunc, classAttributes, name, authorizations, obsoleteMessage, methodInfo);
            }
        }

        protected string BasePath => _basePath ??= _configurationManager.GetConfiguration<BasePathOptions>().Path;

        protected virtual bool DefaultFilterMethod(MethodInfo arg)
        {
            if (arg.IsPrivate ||
                arg.IsStatic ||
                arg.DeclaringType == typeof(object))
            {
                return false;
            }

            return true;
        }

        protected virtual void ExposeMethod(ICurrentApiInformation currentApi, Type type,
            ServiceActivationMethod serviceActivationMethod,
            Func<RequestExecutionContext, object> activationFunc,
            List<Attribute> classAttributes, string name, List<IEndPointMethodAuthorization> authorizations,
            string obsoleteMessage, MethodInfo methodInfo)
        {
            var methodAttributes = methodInfo.GetCustomAttributes<Attribute>().ToList();

            // skip methods that have IgnoreMethodAttribute
            if (methodAttributes.Any(a => a is IgnoreMethodAttribute))
            {
                return;
            }

            var pathAttributes = methodAttributes.Where(a => a is IPathAttribute).ToArray();

            if (pathAttributes.Length > 0)
            {
                foreach (var pathAttribute in pathAttributes)
                {
                    foreach (var configuration in CreateEndPointMethodConfiguration(currentApi, type, 
                        serviceActivationMethod, activationFunc, classAttributes, name,
                        authorizations, obsoleteMessage, methodInfo, methodAttributes, pathAttribute as IPathAttribute))
                    {
                        var endPointMethodHandler =
                            CreateEndPointMethodHandler(currentApi, configuration);

                        _handlers.Add(endPointMethodHandler);
                    }
                }
            }
            else
            {
                foreach (var configuration in CreateEndPointMethodConfiguration(currentApi, type, serviceActivationMethod, activationFunc, classAttributes, name,
                    authorizations, obsoleteMessage, methodInfo, methodAttributes, null))
                {
                    var endPointMethodHandler =
                        CreateEndPointMethodHandler(currentApi, configuration);

                    _handlers.Add(endPointMethodHandler);
                }
            }
        }

        private IEndPointMethodHandler CreateEndPointMethodHandler(ICurrentApiInformation currentApi, EndPointMethodConfiguration configuration)
        {
            var returnType = configuration.WrappedType ?? configuration.ReturnType;
            Type closedType;

            if (returnType.IsConstructedGenericType && (returnType.GetGenericTypeDefinition() == typeof(Task<>) ||
                                                        returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
            {
                returnType = returnType.GenericTypeArguments[0];
            }
            else if (returnType == typeof(Task) ||
                     returnType == typeof(ValueTask) ||
                     returnType == typeof(void))
            {
                // set it to object and it will get wrapped later
                returnType = typeof(object);
            }
            
            if (configuration.Authorizations == null ||
                configuration.Authorizations.Count == 0)
            {
                if (configuration.Parameters.Count == 0 &&
                    (configuration.Filters == null || configuration.Filters.Count == 0))
                {
                    closedType = typeof(NoParamsEndPointMethodHandler<>).MakeGenericType(returnType);

                    return (IEndPointMethodHandler)Activator.CreateInstance(closedType, configuration, _services);
                }
            }

            closedType = typeof(StateBasedEndPointMethodHandler<>).MakeGenericType(returnType);

            return (IEndPointMethodHandler)Activator.CreateInstance(closedType, configuration, _services);
        }

        private IEnumerable<EndPointMethodConfiguration> CreateEndPointMethodConfiguration(
            ICurrentApiInformation currentApi,
            Type type,
            ServiceActivationMethod serviceActivationMethod,
            Func<RequestExecutionContext, object> activationFunc,
            List<Attribute> classAttributes,
            string name,
            List<IEndPointMethodAuthorization> authorizations,
            string obsoleteMessage,
            MethodInfo methodInfo, List<Attribute> methodAttributes, IPathAttribute pathAttribute)
        {
            string methodPath;
            string methodVerb;
            bool methodHasBody;

            (methodPath, methodVerb, methodHasBody) = GenerateMethodPath(currentApi, type, name, methodInfo, classAttributes, methodAttributes, pathAttribute);

            if (activationFunc == null)
            {
                activationFunc = GenerateActivation(currentApi, type, classAttributes, name, methodInfo, methodAttributes);
            }

            foreach (var routeInformation in GenerateRouteInformationList(methodPath, methodVerb, methodHasBody, currentApi, type, name, methodInfo, methodAttributes))
            {
                var configuration = new EndPointMethodConfiguration(routeInformation, activationFunc, new MethodInvokeInformation { MethodToInvoke = methodInfo }, methodInfo.ReturnType);

                AssignDefaultValues(configuration, pathAttribute);

                var methodParameters = GenerateMethodParameters(type, methodInfo, routeInformation);

                configuration.Parameters.AddRange(methodParameters);

                configuration.Parameters.AddRange(AddInstancePropertyBindingParameters(configuration, type));

                var rawAttribute = (IRawContentAttribute)methodAttributes.FirstOrDefault(a => a is IRawContentAttribute);

                if (rawAttribute != null)
                {
                    configuration.RawContentType = rawAttribute.ContentType;
                    configuration.RawContentEncoding = rawAttribute.ContentEncoding;
                }
                else if (string.IsNullOrEmpty(configuration.RawContentType))
                {
                    var returnType = methodInfo.ReturnType;

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

                var headerAttributes = classAttributes.Where(a => a is ResponseHeaderAttribute).ToList();
                headerAttributes.AddRange(methodAttributes.Where(a => a is ResponseHeaderAttribute));

                if (headerAttributes.Count > 0 ||
                    currentApi.Headers != ImmutableLinkedList<IResponseHeader>.Empty)
                {
                    var headers = new List<IResponseHeader>();

                    headers.AddRange(currentApi.Headers);

                    foreach (ResponseHeaderAttribute headerAttribute in headerAttributes)
                    {
                        headers.Add(new ResponseHeader.ResponseHeader(headerAttribute.Name, headerAttribute.Value));
                    }

                    configuration.ResponseHeaders = headers;
                }

                Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>[]
                    authorizationFunc = null;

                if (authorizations != null)
                {
                    authorizationFunc =
                        new Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>[]
                        {
                            config => authorizations
                        };
                }

                ApplyAuthorizations(currentApi, authorizationFunc, configuration, classAttributes, methodAttributes);
                ApplyFilters(currentApi, GetFilterList(currentApi, configuration, classAttributes, methodAttributes), configuration);

                if (_supportCompression)
                {
                    configuration.SupportsCompression = _compressionSelectorService.ShouldCompressResult(configuration);
                }

                var returnTypeAttribute = (ReturnsTypeAttribute)methodAttributes.FirstOrDefault(a => a is ReturnsTypeAttribute);
                
                configuration.DocumentationReturnType = returnTypeAttribute?.ReturnType;

                yield return configuration;
            }
        }

        private IEnumerable<RpcParameterInfo> AddInstancePropertyBindingParameters(EndPointMethodConfiguration configuration, Type type)
        {
            var position = 0;

            configuration.Parameters.ForEach(p =>
            {
                if (p.Position > position)
                {
                    position = p.Position + 1;
                }
            });

            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (propertyInfo.PropertyType == typeof(RequestExecutionContext))
                {
                    yield return new RpcParameterInfo
                    {
                        BindingType = EndPointBindingType.InstanceProperty,
                        Name = propertyInfo.Name,
                        HasDefaultValue = false,
                        ParamType = propertyInfo.PropertyType,
                        ParameterSource = EndPointMethodParameterSource.RequestExecutionContext,
                        Position = position++
                    };
                }
                else if (propertyInfo.PropertyType == typeof(HttpContext))
                {
                    yield return new RpcParameterInfo
                    {
                        BindingType = EndPointBindingType.InstanceProperty,
                        Name = propertyInfo.Name,
                        ParamType = propertyInfo.PropertyType,
                        ParameterSource = EndPointMethodParameterSource.HttpContext,
                        Position = position++
                    };
                }
                else
                {

                }
            }
        }

        private void AssignDefaultValues(EndPointMethodConfiguration configuration, IPathAttribute pathAttribute)
        {
            if (pathAttribute != null)
            {
                SetStatusAndResponseBodyConfigValues(configuration, pathAttribute.HasResponseBody, pathAttribute.SuccessCodeValue);
            }
            else
            {
                SetStatusAndResponseBodyConfigValues(configuration, null, null);
            }
        }

        private void SetStatusAndResponseBodyConfigValues(EndPointMethodConfiguration configuration,
            bool? hasResponseBody,
            int? successStatusCode)
        {
            var finalResponseBody = hasResponseBody;
            var finalStatusCode = successStatusCode;

            if (configuration.RouteInformation.Method == HttpMethods.Get)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.GetHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.GetSuccessStatusCode);
            }
            else if (configuration.RouteInformation.Method == HttpMethods.Head)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.HeadHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.HeadSuccessStatusCode);
            }
            else if (configuration.RouteInformation.Method == HttpMethods.Post)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.PostHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.PostSuccessStatusCode);
            }
            else if (configuration.RouteInformation.Method == HttpMethods.Put)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.PutHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.PutSuccessStatusCode);
            }
            else if (configuration.RouteInformation.Method == HttpMethods.Patch)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.PatchHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.PatchSuccessStatusCode);
            }
            else if (configuration.RouteInformation.Method == HttpMethods.Delete)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.DeleteHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.DeleteSuccessStatusCode);
            }
            else if (configuration.RouteInformation.Method == HttpMethods.Options)
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.OptionsHasResponseBody);
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.OptionsSuccessStatusCode);
            }
            else
            {
                configuration.HasResponseBody =
                    finalResponseBody.GetValueOrDefault(_defaultMethodConfiguration.UnknownMethodResponseBody(configuration.RouteInformation.Method));
                configuration.SuccessStatusCode =
                    finalStatusCode.GetValueOrDefault(_defaultMethodConfiguration.UnknownMethodStatusCode(configuration.RouteInformation.Method));
            }
        }

        private IReadOnlyList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<Func<RequestExecutionContext, IRequestFilter>>>> GetFilterList(
                ICurrentApiInformation currentApi,
                EndPointMethodConfiguration configuration,
                List<Attribute> classAttributes,
                List<Attribute> methodAttributes)
        {
            var returnList =
                new List<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<Func<RequestExecutionContext, IRequestFilter>>>>();

            foreach (var classAttribute in classAttributes)
            {
                if (classAttribute is IRequestFilterAttribute requestFilterAttribute)
                {
                    var filters = requestFilterAttribute.ProvideFilters(currentApi, configuration);

                    returnList.Add(configReadOnly => filters);
                }
            }

            foreach (var classAttribute in methodAttributes)
            {
                if (classAttribute is IRequestFilterAttribute requestFilterAttribute)
                {
                    var filters = requestFilterAttribute.ProvideFilters(currentApi, configuration);

                    returnList.Add(configReadOnly => filters);
                }
            }

            return returnList;
        }

        private IEnumerable<RpcRouteInformation> GenerateRouteInformationList(string path, string method, bool hasBody, ICurrentApiInformation currentApi, Type type, string name, MethodInfo methodInfo, List<Attribute> attributes)
        {
            var prefixed = false;

            foreach (var prefixFunc in currentApi.Prefixes)
            {
                foreach (var prefixString in prefixFunc(type))
                {
                    yield return GenerateRouteInformation(path, method, hasBody, prefixString, currentApi, type, name, methodInfo, attributes);
                    prefixed = true;
                }
            }

            if (!prefixed)
            {
                yield return GenerateRouteInformation(path, method, hasBody, "", currentApi, type, name, methodInfo, attributes);
            }
        }

        private RpcRouteInformation GenerateRouteInformation(string path, string method, bool hasBody, string prefixString, ICurrentApiInformation currentApi, Type type, string name, MethodInfo methodInfo, List<Attribute> attributes)
        {
            var fullPath = BasePath;

            if (!string.IsNullOrEmpty(prefixString))
            {
                if (fullPath.EndsWith('/') && prefixString.StartsWith('/'))
                {
                    fullPath = fullPath.TrimEnd('/');
                }

                fullPath += prefixString;
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            if (fullPath.EndsWith('/') && path.StartsWith('/'))
            {
                fullPath = fullPath.TrimEnd('/');
            }

            fullPath += path;

            var rpcRouteInformation = new RpcRouteInformation
            {
                RouteTemplate = fullPath,
                HasBody = hasBody,
                Method = method
            };

            CalculateRouteTokens(rpcRouteInformation);

            return rpcRouteInformation;
        }

        private void CalculateRouteTokens(RpcRouteInformation rpcRouteInformation)
        {
            var routeTemplate = rpcRouteInformation.RouteTemplate;

            var rpcTokens = new List<RpcRouteToken>();
            rpcRouteInformation.Tokens = rpcTokens;

            var tokenStartBracket = routeTemplate.IndexOf('{');

            if (tokenStartBracket == -1)
            {
                rpcRouteInformation.RouteBasePath = rpcRouteInformation.RouteTemplate;
                return;
            }

            rpcRouteInformation.RouteBasePath = routeTemplate.Substring(0, tokenStartBracket);

            while (tokenStartBracket > 0)
            {
                var tokenEndBracket = routeTemplate.IndexOf('}', tokenStartBracket);

                var colonIndex = routeTemplate.IndexOf(':', tokenStartBracket, tokenEndBracket - tokenStartBracket);

                var tokenEnd = tokenEndBracket;

                var token = new RpcRouteToken();

                if (colonIndex > 0)
                {
                    tokenEnd = colonIndex;

                    var tokenTypeString = routeTemplate.Substring(colonIndex + 1,
                        tokenEndBracket - (colonIndex + 1));

                    if (tokenTypeString.EndsWith('?'))
                    {
                        tokenTypeString = tokenTypeString.TrimEnd('?');
                        token.Optional = true;
                    }

                    token.ParseType = GetParseType(tokenTypeString);
                }

                var tokenName =
                    routeTemplate.Substring(tokenStartBracket + 1,
                         tokenEnd - (tokenStartBracket + 1));

                if (tokenName.EndsWith('?'))
                {
                    tokenName = tokenName.TrimEnd('?');
                    token.Optional = true;
                }

                token.Name = tokenName;

                rpcTokens.Add(token);

                tokenStartBracket = routeTemplate.IndexOf('{', tokenEnd);
            }
        }

        private RpcRouteTokenParseType GetParseType(string tokenTypeString)
        {
            switch (tokenTypeString.ToLowerInvariant())
            {
                case "int":
                case "integer":
                    return RpcRouteTokenParseType.Integer;
                case "double":
                    return RpcRouteTokenParseType.Double;
                case "decimal":
                    return RpcRouteTokenParseType.Decimal;
                case "guid":
                    return RpcRouteTokenParseType.GUID;
                case "datetime":
                    return RpcRouteTokenParseType.DateTime;

                default:
                    return RpcRouteTokenParseType.String;
            }
        }

        private Func<RequestExecutionContext, object> GenerateActivation(ICurrentApiInformation currentApi, Type type, List<Attribute> classAttributes, string name, MethodInfo methodInfo, List<Attribute> attributes)
        {
            var serviceActivationMethod = currentApi.ServiceActivationMethod;
            var activationAttribute = (IServiceActivationAttribute)(attributes.FirstOrDefault(a => a is IServiceActivationAttribute) ??
                                       classAttributes.FirstOrDefault(a => a is IServiceActivationAttribute));

            if (activationAttribute != null)
            {
                serviceActivationMethod = activationAttribute.ActivationMethod;
            }

            object instance = null;
            switch (serviceActivationMethod)
            {
                case ServiceActivationMethod.ActivationUtility:
                    return context => ActivatorUtilities.CreateInstance(context.HttpContext.RequestServices, type);
                case ServiceActivationMethod.ServiceContainer:
                    return context => context.HttpContext.RequestServices.GetRequiredService(type);
                case ServiceActivationMethod.SharedActivationUtility:
                    instance = ActivatorUtilities.CreateInstance(currentApi.ServiceProvider, type);
                    return context => instance;
                case ServiceActivationMethod.SharedServiceContainer:
                    instance = currentApi.ServiceProvider.GetRequiredService(type);
                    return context => instance;

                default:
                    throw new Exception($"Unknown ServiceActivationMethod {serviceActivationMethod}");
            }
        }

        private List<RpcParameterInfo> GenerateMethodParameters(Type instanceType, MethodInfo methodInfo, RpcRouteInformation routeInformation)
        {
            var parameterList = new List<RpcParameterInfo>();
            var bodyParams = 0;
            var lastPosition = 0;

            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                var rpcParam = new RpcParameterInfo
                {
                    Position = parameterInfo.Position,
                    Name = parameterInfo.Name,
                    HasDefaultValue = parameterInfo.HasDefaultValue,
                    DefaultValue = parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue : null,
                    ParamType = parameterInfo.ParameterType
                };

                lastPosition = parameterInfo.Position;

                SetParameterSource(routeInformation, parameterInfo.ParameterType, rpcParam, parameterInfo);

                if (rpcParam.ParameterSource == EndPointMethodParameterSource.PostParameter)
                {
                    bodyParams++;
                }

                parameterList.Add(rpcParam);
            }

            if (instanceType != null)
            {
                foreach (var propertyInfo in instanceType.GetProperties())
                {
                    var rpcParam = ProcessProperty(propertyInfo);

                    if (rpcParam != null)
                    {
                        rpcParam.BindingType = EndPointBindingType.InstanceProperty;
                        rpcParam.Name = propertyInfo.Name;
                        rpcParam.ParamType = propertyInfo.PropertyType;

                        parameterList.Add(rpcParam);

                        rpcParam.Position = parameterList.Count - 1;
                    }
                }
            }

            foreach (var routeToken in routeInformation.Tokens)
            {
                if (routeToken.ParameterInfo == null)
                {
                    var rpcParam = new RpcParameterInfo
                    {
                        Name = routeToken.Name,
                        HasDefaultValue = false,
                        DefaultValue = null,
                        ParamType = GetParameterTypeFromTokenType(routeToken.ParseType),
                        ParameterSource = EndPointMethodParameterSource.PathParameter,
                        BindingType = EndPointBindingType.Other
                    };

                    parameterList.Add(rpcParam);

                    rpcParam.Position = parameterList.Count - 1;
                }
            }

            if (bodyParams == 1 &&
                _exposeConfigurations.SingleParameterPostFromBody)
            {
                var parameter = parameterList.First(rpc =>
                    rpc.ParameterSource == EndPointMethodParameterSource.PostParameter);

                if (!DefaultExposeDelegates.SimpleTypeWrapSelector(parameter.ParamType))
                {
                    parameter.ParameterSource = EndPointMethodParameterSource.PostBody;
                }
            }

            return parameterList;
        }

        private RpcParameterInfo ProcessProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(HttpContext))
            {
                return new RpcParameterInfo
                {
                    ParameterSource = EndPointMethodParameterSource.HttpContext
                };
            }

            if (propertyInfo.PropertyType == typeof(RequestExecutionContext))
            {
                return new RpcParameterInfo
                {
                    ParameterSource = EndPointMethodParameterSource.RequestExecutionContext
                };
            }

            foreach (var attribute in propertyInfo.GetCustomAttributes())
            {
                if (attribute is BindFromHeaderAttribute headerAttribute)
                {
                    return new RpcParameterInfo
                    {
                        BindName = headerAttribute.Name,
                        ParameterSource = EndPointMethodParameterSource.HeaderParameter
                    };
                }
                
                if (attribute is BindFromServicesAttribute)
                {
                    return new RpcParameterInfo
                    {
                        ParameterSource = EndPointMethodParameterSource.RequestServices
                    };
                }
                
                if (attribute is BindFromQueryAttribute queryAttribute)
                {
                    return new RpcParameterInfo
                    {
                        BindName = queryAttribute.Name,
                        ParameterSource = EndPointMethodParameterSource.QueryStringParameter
                    };
                }

                if (attribute is BindNewDataAttribute)
                {
                    return new RpcParameterInfo
                    {
                        ParameterSource = EndPointMethodParameterSource.NewData
                    };
                }
            }

            return null;
        }

        private Type GetParameterTypeFromTokenType(RpcRouteTokenParseType routeTokenParseType)
        {
            switch (routeTokenParseType)
            {
                case RpcRouteTokenParseType.Integer:
                    return typeof(int);

                case RpcRouteTokenParseType.Double:
                    return typeof(double);

                case RpcRouteTokenParseType.Decimal:
                    return typeof(decimal);

                case RpcRouteTokenParseType.DateTime:
                    return typeof(DateTime);

                case RpcRouteTokenParseType.GUID:
                    return typeof(Guid);

                default:
                    return typeof(string);
            }
        }

        private (string, string, bool) GenerateMethodPath(ICurrentApiInformation currentApi, Type type, string name,
            MethodInfo methodInfo, List<Attribute> classAttributes, List<Attribute> methodAttributes,
            IPathAttribute pathAttribute)
        {
            if (string.IsNullOrEmpty(name))
            {
                var basePath = (IBasePathAttribute)classAttributes.FirstOrDefault(a => a is IBasePathAttribute);

                name = basePath != null ?
                    basePath.BasePath :
                    _exposeConfigurations.RouteNameGenerator(type);
            }

            string fullPathString = null;

            var parameters = methodInfo.GetParameters();

            if (pathAttribute != null)
            {
                fullPathString = pathAttribute.Path;

                if (string.IsNullOrEmpty(fullPathString))
                {
                    fullPathString = GeneratePath(name, methodInfo, parameters, GetDefaultRequestBody(pathAttribute.Method, pathAttribute.HasRequestBody));
                }

                return (fullPathString, pathAttribute.Method, GetDefaultRequestBody(pathAttribute.Method, pathAttribute.HasRequestBody));
            }

            if (currentApi.DefaultMethod == ExposeDefaultMethod.PostOnly ||
                (currentApi.DefaultMethod == ExposeDefaultMethod.PostAndGet && parameters.Length > 0) ||
                (currentApi.DefaultMethod == ExposeDefaultMethod.PostAndGetInt && parameters.Any(p => p.ParameterType != typeof(int))))
            {

                fullPathString = GeneratePath(name, methodInfo, parameters, true);

                return (fullPathString, HttpMethods.Post, true);
            }

            fullPathString = GeneratePath(name, methodInfo, parameters, false);

            return (fullPathString, HttpMethods.Get, false);
        }

        private bool GetDefaultRequestBody(string httpMethod, bool? pathAttributeHasRequestBody)
        {
            if (pathAttributeHasRequestBody.HasValue)
            {
                return pathAttributeHasRequestBody.Value;
            }

            if (httpMethod == HttpMethods.Get ||
                httpMethod == HttpMethods.Delete ||
                httpMethod == HttpMethods.Options)
            {
                return false;
            }

            return true;
        }

        private string GeneratePath(string name, MethodInfo methodInfo, ParameterInfo[] parameters, bool hasBody)
        {
            name = name.Trim('/');

            var methodPath = $"/{name}/{_exposeConfigurations.MethodNameGenerator(methodInfo)}";

            if (!hasBody)
            {
                foreach (var parameterInfo in parameters)
                {
                    if (ShouldExcludeParameterFromPath(parameterInfo))
                    {
                        continue;
                    }

                    var nameString = parameterInfo.Name;

                    if (parameterInfo.HasDefaultValue)
                    {
                        nameString += '?';
                    }

                    methodPath += "/{" + nameString + "}";
                }
            }

            return methodPath;
        }

        private bool ShouldExcludeParameterFromPath(ParameterInfo parameterInfo)
        {
            var parameterType = parameterInfo.ParameterType;

            if (_exposeConfigurations.ResolveFromContainer(parameterType) ||
                parameterType == typeof(RequestExecutionContext) ||
                parameterType == typeof(HttpContext) ||
                parameterType == typeof(HttpResponse) ||
                parameterType == typeof(HttpRequest) ||
                parameterType == typeof(CancellationToken))
            {
                return true;
            }

            return false;
        }

        public Dictionary<string, Dictionary<string, IEndPointMethodHandler>> ProvideEndPointHandlers()
        {
            _exposeConfigurations = _configurationManager.GetConfiguration<ExposeConfigurations>();
            _supportCompression = _configurationManager.GetConfiguration<ContentEncodingConfiguration>()
                .CompressionEnabled;

            _defaultMethodConfiguration = _configurationManager.GetConfiguration<DefaultMethodConfiguration>();

            ProcessConfigurationObjects();

            return ProcessInspectors();
        }

        private Dictionary<string, Dictionary<string, IEndPointMethodHandler>> ProcessInspectors()
        {
            var handlersDictionary = new Dictionary<string, Dictionary<string, IEndPointMethodHandler>>();

            AddHandlersToDictionary(handlersDictionary, _handlers);

            foreach (var inspector in _endPointInspectors)
            {
                var endPoints = inspector.InspectEndPoints(handlersDictionary);

                if (endPoints != null)
                {
                    AddHandlersToDictionary(handlersDictionary, endPoints);
                }
            }

            return handlersDictionary;
        }

        private void AddHandlersToDictionary(Dictionary<string, Dictionary<string, IEndPointMethodHandler>> endPointMethodHandlersDictionary, IEnumerable<IEndPointMethodHandler> handlers)
        {
            foreach (var handler in handlers)
            {


                if (!endPointMethodHandlersDictionary.TryGetValue(handler.RouteInformation.RouteBasePath,
                    out var methodDictionary))
                {
                    methodDictionary = new Dictionary<string, IEndPointMethodHandler>();

                    endPointMethodHandlersDictionary[handler.RouteInformation.RouteBasePath] = methodDictionary;
                }

                methodDictionary[handler.HttpMethod] = handler;
            }
        }

        protected virtual void ProcessConfigurationObjects()
        {
            foreach (var configurationObject in _configurationObjects)
            {
                if (configurationObject is IConfigurationInformationProvider configurationInformationProvider)
                {
                    configurationInformationProvider.ExecuteConfiguration(this);
                }
            }
        }

        protected virtual void ApplyAuthorizations(ICurrentApiInformation currentApi,
            IReadOnlyList<Func<IEndPointMethodConfigurationReadOnly, IEnumerable<IEndPointMethodAuthorization>>>
                authorizationFuncList,
            EndPointMethodConfiguration configuration, List<Attribute> classAttributes,
            List<Attribute> methodAttributes)
        {
            // anything marked as anonymous we want to allow
            if (classAttributes.Any(a => a is AllowAnonymousAttribute) ||
                methodAttributes.Any(a => a is AllowAnonymousAttribute))
            {
                return;
            }

            var authorizationList = new List<IEndPointMethodAuthorization>();

            foreach (var authorizationFunc in currentApi.Authorizations)
            {
                var authorizations = authorizationFunc(configuration);

                if (authorizations != null)
                {
                    authorizationList.AddRange(authorizations);
                }
            }

            if (authorizationFuncList != null)
            {
                foreach (var authorizationFunc in authorizationFuncList)
                {
                    var authorizations = authorizationFunc(configuration);

                    if (authorizations != null)
                    {
                        authorizationList.AddRange(authorizations);
                    }
                }
            }

            ProcessAuthorizeAttributes(classAttributes, authorizationList);
            ProcessAuthorizeAttributes(methodAttributes, authorizationList);

            if (authorizationList.Count > 0)
            {
                configuration.Authorizations = authorizationList;
            }
        }

        private void ProcessAuthorizeAttributes(List<Attribute> attributes, List<IEndPointMethodAuthorization> authorizationList)
        {
            foreach (var methodAttribute in attributes)
            {
                if (methodAttribute is AuthorizeAttribute authorizeAttribute)
                {
                    if (!string.IsNullOrEmpty(authorizeAttribute.Policy))
                    {
                        authorizationList.Add(_authorizationImplementationProvider.UserHasPolicy(authorizeAttribute.Policy));
                    }
                    else if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
                    {
                        authorizationList.Add(_authorizationImplementationProvider.UserHasRole(authorizeAttribute.Roles));
                    }
                    else
                    {
                        authorizationList.Add(_authorizationImplementationProvider.Authorized());
                    }
                }
            }
        }

        private void ApplyFilters(ICurrentApiInformation currentApi,
            IReadOnlyList<Func<IEndPointMethodConfigurationReadOnly,
                IEnumerable<Func<RequestExecutionContext, IRequestFilter>>>> filters,
            EndPointMethodConfiguration configuration)
        {
            var filterList = new List<Func<RequestExecutionContext, IRequestFilter>>();

            foreach (var filterFunc in currentApi.Filters)
            {
                var filter = filterFunc(configuration);

                if (filter != null)
                {
                    filterList.Add(filter);
                }
            }

            if (filters != null)
            {
                foreach (var filterFunc in filters)
                {
                    var filter = filterFunc(configuration);

                    if (filter != null)
                    {
                        filterList.AddRange(filter);
                    }
                }
            }

            if (filterList.Count > 0)
            {
                configuration.Filters = filterList;
            }
        }


    }
}
