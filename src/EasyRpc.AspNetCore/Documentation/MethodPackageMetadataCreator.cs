using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Documentation
{
    public class JsonDataPackage
    {
        public string Route { get; set; }

        public List<JsonMethodInfo> Methods { get; set; }

        public string DisplayName { get; set; }

        public string Comments { get; set; }
    }

    public class TypeRef
    {
        public string FullName { get; set; }

        public string DisplayName { get; set; }

        public bool Array { get; set; }

        [JsonIgnore]
        public Type Type { get; set; }
    }

    public class JsonMethodInfo
    {
        public string Path { get; set; }

        public string Comments { get; set; }

        public string Name { get; set; }

        public string Signature { get; set; }

        public List<JsonParameterInfo> Parameters { get; set; }

        public TypeRef ReturnType { get; set; }

        public string ObsoleteMessage { get; set; }

        [JsonIgnore]
        public IExposedMethodInformation Method { get; set; }
    }

    public class JsonParameterInfo
    {
        public string Name { get; set; }

        public TypeRef ParameterType { get; set; }

        public bool Stringify { get; set; }

        public bool Enumeration { get; set; }

        public bool Optional { get; set; }

        public object DefaultValue { get; set; }

        public string Comments { get; set; }

        public string HtmlType { get; set; }

        public bool Array { get; set; }

        [JsonIgnore]
        public ParameterInfo ParameterInfo { get; set; }
    }

    public interface IMethodPackageMetadataCreator
    {
        void SetConfiguration(EndPointConfiguration endPointConfiguration);

        Task CreatePackage(HttpContext context);
    }

    public class MethodPackageMetadataCreator : IMethodPackageMetadataCreator
    {
        private EndPointConfiguration _configuration;
        private readonly JsonSerializer _serializer;
        private List<JsonDataPackage> _dataPackages;
        private bool _hasAuthorization = false;
        private readonly IXmlDocumentationProvider _xmlDocumentationProvider;
        private readonly ITypeDefinitionPackageProvider _typeDefinitionPackageProvider;
        private List<TypeDefinition> _typeDefinitions;
        private readonly IFromServicesManager _fromServicesManager;

        public MethodPackageMetadataCreator(JsonSerializer serializer, IXmlDocumentationProvider xmlDocumentationProvider, ITypeDefinitionPackageProvider typeDefinitionPackageProvider, IFromServicesManager fromServicesManager)
        {
            _serializer = serializer;
            _xmlDocumentationProvider = xmlDocumentationProvider;
            _typeDefinitionPackageProvider = typeDefinitionPackageProvider;
            _fromServicesManager = fromServicesManager;
        }

        public void SetConfiguration(EndPointConfiguration endPointConfiguration)
        {
            _configuration = endPointConfiguration;

            var routes = new Dictionary<string, List<IExposedMethodInformation>>();

            foreach (var information in _configuration.Methods.Values)
            {
                foreach (var routeName in information.RouteNames)
                {
                    if (!routes.TryGetValue(routeName, out var methodList))
                    {
                        methodList = new List<IExposedMethodInformation>();

                        routes[routeName] = methodList;
                    }

                    if (!methodList.Contains(information))
                    {
                        methodList.Add(information);
                    }
                }
            }

            _dataPackages = new List<JsonDataPackage>();

            var sortedRoutes = routes.OrderBy(kvp => kvp.Key);

            foreach (var route in sortedRoutes)
            {
                var methods = new List<JsonMethodInfo>();

                route.Value.Sort((x, y) => string.Compare(x.MethodName, y.MethodName, StringComparison.OrdinalIgnoreCase));

                foreach (var methodInformation in route.Value)
                {
                    methods.Add(GenerateInfoForMethod(route.Key, methodInformation));
                }

                if (methods.Count > 0)
                {
                    _dataPackages.Add(new JsonDataPackage
                    {
                        Route = _configuration.Route,
                        DisplayName = route.Key,
                        Methods = methods
                    });
                }
            }

            _typeDefinitions = _typeDefinitionPackageProvider.GetTypeDefinitions(_dataPackages).ToList();
            _xmlDocumentationProvider.PopulateMethodDocumentation(_dataPackages, _typeDefinitions);
        }

        public async Task CreatePackage(HttpContext context)
        {
            using (var outputStream = new StreamWriter(context.Response.Body))
            {
                using (var jsonStream = new JsonTextWriter(outputStream))
                {
                    if (!_hasAuthorization)
                    {
                        context.Response.ContentType = "application/json; charset=utf-8";

                        _serializer.Serialize(jsonStream, new { endpoints = _dataPackages, dataTypes = _typeDefinitions });

                        return;
                    }

                    var returnList = new List<JsonDataPackage>();

                    foreach (var dataPackage in _dataPackages)
                    {
                        var authorizedMethods = new List<JsonMethodInfo>();

                        foreach (var method in dataPackage.Methods)
                        {
                            var authorizations = method.Method.MethodAuthorizations;

                            if (authorizations == null || authorizations.Length == 0)
                            {
                                authorizedMethods.Add(method);
                            }
                            else
                            {
                                var methodInformation = method.Method;

                                var callContext =
                                    new CallExecutionContext(context, methodInformation.Type,
                                        methodInformation.MethodInfo,
                                        new RpcRequestMessage());
                                var add = true;

                                foreach (var authorization in methodInformation.MethodAuthorizations)
                                {
                                    if (!await authorization.AsyncAuthorize(callContext))
                                    {
                                        add = false;
                                        break;
                                    }
                                }

                                if (add)
                                {
                                    authorizedMethods.Add(method);
                                }
                            }
                        }

                        if (authorizedMethods.Count > 0)
                        {
                            returnList.Add(new JsonDataPackage
                            {
                                DisplayName = dataPackage.DisplayName,
                                Methods = authorizedMethods,
                                Route = dataPackage.Route
                            });
                        }
                    }

                    context.Response.ContentType = "application/json; charset=utf-8";

                    _serializer.Serialize(jsonStream, new { endpoints = returnList, dataTypes = _typeDefinitions });
                }
            }
        }

        private JsonMethodInfo GenerateInfoForMethod(string route, IExposedMethodInformation methodInformation)
        {
            var method = methodInformation.MethodInfo;

            string displayString;
            string parameterString = " ";
            var parameters = methodInformation.Parameters.ToArray();
            var parameterList = new List<JsonParameterInfo>();

            if (parameters.Length > 0)
            {
                parameterString = "";

                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType == typeof(IServiceProvider) ||
                        parameter.ParameterType == typeof(HttpContext) ||
                        _fromServicesManager.ParameterIsFromServices(parameter.ParameterInfo))
                    {
                        continue;
                    }
                    
                    if (parameterString.Length > 0)
                    {
                        parameterString += ", ";
                    }

                    var friendlyName = TypeUtilities.GetFriendlyTypeName(parameter.ParameterType, out var currentType, out var isArray);

                    parameterString += $"{friendlyName} {parameter.Name}";

                    bool stringify = !(currentType == typeof(string) ||
                                       currentType.GetTypeInfo().IsEnum ||
                                       currentType == typeof(DateTime) ||
                                       currentType == typeof(DateTime?));

                    var htmlType = "text";

                    var type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;

                    if (type == typeof(int) || type == typeof(uint) ||
                        type == typeof(short) || type == typeof(ushort) ||
                        type == typeof(long) || type == typeof(ulong) ||
                        type == typeof(double) || type == typeof(float) || type == typeof(decimal))
                    {
                        htmlType = "number";
                    }
                    else if (type == typeof(DateTime))
                    {
                        htmlType = "datetime-local";
                    }

                    parameterList.Add(new JsonParameterInfo
                    {
                        Name = parameter.Name,
                        ParameterType = TypeUtilities.CreateTypeRef(parameter.ParameterType),
                        Array = isArray,
                        Stringify = stringify,
                        Optional = parameter.HasDefaultValue,
                        DefaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : null,
                        HtmlType = htmlType
                    });
                }
            }

            if (methodInformation.MethodAuthorizations != null &&
                methodInformation.MethodAuthorizations.Length > 0)
            {
                _hasAuthorization = true;
            }

            displayString = $"{TypeUtilities.GetFriendlyTypeName(method.ReturnType,out var unusedType, out var unused)} {methodInformation.MethodName}({parameterString})";

            return new JsonMethodInfo
            {
                Path = route,
                Name = methodInformation.MethodName,
                Signature = displayString,
                ReturnType = TypeUtilities.CreateTypeRef( methodInformation.MethodInfo.ReturnType),
                Parameters = parameterList,
                Method = methodInformation,
                ObsoleteMessage = methodInformation.ObsoleteMessage
            };
        }

    }
}

