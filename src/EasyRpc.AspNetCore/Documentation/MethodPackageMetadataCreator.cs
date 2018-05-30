using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    }

    public class JsonMethodInfo
    {
        public string Path { get; set; }

        public string Comments { get; set; }

        public string Name { get; set; }

        public string Signature { get; set; }

        public List<JsonParameterInfo> Parameters { get; set; }

        public string ReturnType { get; set; }

        [JsonIgnore]
        public ExposedMethodInformation Method { get; set; }
    }

    public class JsonParameterInfo
    {
        public string Name { get; set; }

        public string ParameterType { get; set; }

        public string Comments { get; set; }
    }

    public interface IMethodPackageMetadataCreator
    {
        void SetConfiguration(EndPointConfiguration endPointConfiguration);

        Task CreatePackage(HttpContext context);
    }

    public class MethodPackageMetadataCreator : IMethodPackageMetadataCreator
    {
        private EndPointConfiguration _configuration;
        private JsonSerializer _serializer;
        private List<JsonDataPackage> _dataPackages;
        private bool _hasAuthorization = false;

        public MethodPackageMetadataCreator(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public void SetConfiguration(EndPointConfiguration endPointConfiguration)
        {
            _configuration = endPointConfiguration;
            
            var routes = new Dictionary<string, List<ExposedMethodInformation>>();

            foreach (var information in _configuration.Methods.Values)
            {
                foreach (var routeName in information.RouteNames)
                {
                    if (!routes.TryGetValue(routeName, out var methodList))
                    {
                        methodList = new List<ExposedMethodInformation>();

                        routes[routeName] = methodList;
                    }

                    if (!methodList.Contains(information))
                    {
                        methodList.Add(information);
                    }
                }
            }

            _dataPackages = new List<JsonDataPackage>();
            
            foreach (var route in routes)
            {
                var methods = new List<JsonMethodInfo>();

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

                        _serializer.Serialize(jsonStream, new { endpoints = _dataPackages });

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
                                        methodInformation.Method,
                                        new RequestMessage());
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

                    _serializer.Serialize(jsonStream, new { endpoints = returnList });
                }
            }
        }

        private JsonMethodInfo GenerateInfoForMethod(string route, ExposedMethodInformation methodInformation)
        {
            var method = methodInformation.Method;

            string displayString;
            string parameterString = " ";
            var parameters = method.GetParameters();
            var parameterList = new List<JsonParameterInfo>();

            if (parameters.Length > 0)
            {
                parameterString = "";

                foreach (var parameter in parameters)
                {
                    if (parameterString.Length > 0)
                    {
                        parameterString += ", ";
                    }

                    parameterString += $"{parameter.ParameterType.Name} {parameter.Name}";

                    parameterList.Add(new JsonParameterInfo { Name = parameter.Name, ParameterType = parameter.ParameterType.Name });
                }
            }

            if (methodInformation.MethodAuthorizations != null &&
                methodInformation.MethodAuthorizations.Length > 0)
            {
                _hasAuthorization = true;
            }

            displayString = $"{method.ReturnType.Name} {method.Name}({parameterString})";

            return new JsonMethodInfo
            {
                Path = route,
                Name = methodInformation.MethodName,
                Signature = displayString,
                ReturnType = methodInformation.Method.ReturnType.Name,
                Parameters = parameterList,
                Method = methodInformation
            };
        }
    }
}

