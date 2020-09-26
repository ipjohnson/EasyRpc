using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Headers;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Response;
using EasyRpc.Abstractions.Services;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.ContentEncoding;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.EndPoints.MethodHandlers;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Configuration
{
    public partial class ApplicationConfigurationService : IApplicationConfigurationService
    {
        private readonly List<IEndPointMethodHandler> _handlers = new List<IEndPointMethodHandler>();
        private readonly List<object> _configurationObjects = new List<object>();
        private readonly EndPointServices _services;
        private readonly IConfigurationManager _configurationManager;
        private readonly IAuthorizationImplementationProvider _authorizationImplementationProvider;
        private readonly ICompressionSelectorService _compressionSelectorService;
        private ExposeConfigurations _exposeConfigurations;
        private readonly IWrappedResultTypeCreator _wrappedResultTypeCreator;
        private string _basePath;
        private bool _supportCompression;

        public ApplicationConfigurationService(EndPointServices services, 
            IConfigurationManager configurationManager, 
            IAuthorizationImplementationProvider authorizationImplementationProvider, 
            IWrappedResultTypeCreator wrappedResultTypeCreator, 
            ICompressionSelectorService compressionSelectorService)
        {
            _services = services;
            _configurationManager = configurationManager;
            _authorizationImplementationProvider = authorizationImplementationProvider;
            _wrappedResultTypeCreator = wrappedResultTypeCreator;
            _compressionSelectorService = compressionSelectorService;
        }

        public void AddConfigurationObject(object configurationObject)
        {
            _configurationObjects.Add(configurationObject);
        }

        public void ExposeType(ICurrentApiInformation currentApi, Type type, string name, List<IEndPointMethodAuthorization> authorizations, Func<MethodInfo, bool> methodFilter,
            string obsoleteMessage)
        {
            methodFilter ??= DefaultFilterMethod;
            var classAttributes = type.GetCustomAttributes<Attribute>().ToList();

            foreach (var methodInfo in type.GetMethods().Where(methodFilter))
            {
                ExposeMethod(currentApi, type, classAttributes, name, authorizations, obsoleteMessage, methodInfo);
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
                    foreach (var configuration in CreateEndPointMethodConfiguration(currentApi, type, classAttributes, name,
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
                foreach (var configuration in CreateEndPointMethodConfiguration(currentApi, type, classAttributes, name,
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
            else if (returnType == typeof(Task) || returnType == typeof(ValueTask))
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
        
        private IEnumerable<EndPointMethodConfiguration> CreateEndPointMethodConfiguration(ICurrentApiInformation currentApi,
            Type type, List<Attribute> classAttributes, string name, List<IEndPointMethodAuthorization> authorizations,
            string obsoleteMessage,
            MethodInfo methodInfo, List<Attribute> methodAttributes, IPathAttribute pathAttribute)
        {
            string methodPath;
            string methodVerb;
            bool methodHasBody;

            (methodPath,methodVerb,methodHasBody) = GenerateMethodPath(currentApi, type, name, methodInfo,classAttributes, methodAttributes, pathAttribute);

            var activationMethod = GenerateActivation(currentApi, type, classAttributes, name, methodInfo, methodAttributes);

            foreach (var routeInformation in GenerateRouteInformationList(methodPath, methodVerb, methodHasBody, currentApi, type, name, methodInfo, methodAttributes))
            {
                var configuration = new EndPointMethodConfiguration(routeInformation, activationMethod, new MethodInvokeInformation { MethodToInvoke = methodInfo }, methodInfo.ReturnType);

                if (pathAttribute != null)
                {
                    configuration.SuccessStatusCode = pathAttribute.SuccessCodeValue;
                    configuration.HasResponseBody = pathAttribute.HasResponseBody;
                }
                else
                {
                    configuration.HasResponseBody = true;
                }

                var methodParameters = GenerateMethodParameters(currentApi, type, name, methodInfo, methodAttributes, routeInformation);

                configuration.Parameters.AddRange(methodParameters);

                var rawAttribute = (IRawContentAttribute)methodAttributes.FirstOrDefault(a => a is IRawContentAttribute);

                if (rawAttribute != null)
                {
                    configuration.RawContentType = rawAttribute.ContentType;
                    configuration.RawContentEncoding = rawAttribute.ContentEncoding;
                }
                else if(string.IsNullOrEmpty(configuration.RawContentType))
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

                ApplyAuthorizations(currentApi, null, configuration, classAttributes, methodAttributes);
                ApplyFilters(currentApi, GetFilterList(currentApi, configuration, classAttributes, methodAttributes), configuration);

                if (_supportCompression)
                {
                    configuration.SupportsCompression = _compressionSelectorService.ShouldCompressResult(configuration);
                }

                yield return configuration;
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

                    token.ParseType = GetParseType(tokenTypeString);
                }

                var tokenName =
                    routeTemplate.Substring(tokenStartBracket + 1,
                         tokenEnd - (tokenStartBracket + 1));

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

        private List<RpcParameterInfo> GenerateMethodParameters(ICurrentApiInformation currentApi, Type type,
            string name, MethodInfo methodInfo, List<Attribute> attributes, RpcRouteInformation routeInformation)
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

                SetParameterSource(routeInformation, parameterInfo.ParameterType, rpcParam);

                if (rpcParam.ParameterSource == EndPointMethodParameterSource.PostParameter)
                {
                    bodyParams++;
                }

                parameterList.Add(rpcParam);
            }

            foreach (var routeToken in routeInformation.Tokens)
            {
                if (routeToken.ParameterInfo == null)
                {
                    var rpcParam = new RpcParameterInfo
                    {
                        Position = ++lastPosition,
                        Name = routeToken.Name,
                        HasDefaultValue = false,
                        DefaultValue = null,
                        ParamType = GetParameterTypeFromTokenType(routeToken.ParseType),
                        ParameterSource = EndPointMethodParameterSource.PathParameter,
                        IsInvokeParameter = false
                    };

                    parameterList.Add(rpcParam);
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
                    fullPathString = GeneratePath(name, methodInfo, parameters, pathAttribute.HasRequestBody);
                }

                return (fullPathString, pathAttribute.Method, pathAttribute.HasRequestBody);
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

        private string GeneratePath(string name,MethodInfo methodInfo, ParameterInfo[] parameters, bool hasBody)
        {
            name = name.Trim('/');

            var methodPath = $"/{name}/{_exposeConfigurations.MethodNameGenerator(methodInfo)}";

            if (!hasBody)
            {
                foreach (var parameterInfo in parameters)
                {
                    methodPath += "/{" + parameterInfo.Name + "}";
                }
            }

            return methodPath;
        }

        public IReadOnlyList<IEndPointMethodHandler> ProvideEndPointHandlers()
        {
            _exposeConfigurations = _configurationManager.GetConfiguration<ExposeConfigurations>();
            _supportCompression = _configurationManager.GetConfiguration<ContentEncodingConfiguration>()
                .CompressionEnabled;

            ProcessConfigurationObjects();

            return _handlers;
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
