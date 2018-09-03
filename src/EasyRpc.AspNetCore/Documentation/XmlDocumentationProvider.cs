using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IXmlDocumentationProvider
    {
        void PopulateMethodDocumentation(List<JsonDataPackage> packages, List<TypeDefinition> typeDefinitions);

    }

    public class XmlDocumentationProvider : IXmlDocumentationProvider
    {
        public void PopulateMethodDocumentation(List<JsonDataPackage> packages, List<TypeDefinition> typeDefinitions)
        {
            var xmlDocs = new Dictionary<Assembly, XDocument>();

            foreach (var package in packages)
            {
                foreach (var method in package.Methods)
                {
                    var methodInfo = method.Method.MethodInfo;
                    var assembly = methodInfo.DeclaringType.GetTypeInfo().Assembly;

                    var xdoc = GetDocumentForAssembly(xmlDocs, assembly);

                    if (xdoc == null)
                    {
                        continue;
                    }

                    var membersNode = xdoc.Descendants("members");

                    foreach (var element in membersNode.Descendants("member"))
                    {
                        var name = element.Attribute("name");

                        if (name != null)
                        {
                            if (name.Value == "M:EasyRpc.TestApp.Services.TestService.TestMethod(System.Int32)")
                            {
                                this.ToString();
                            }
                            if (name.Value.StartsWith("M:" + methodInfo.DeclaringType.FullName + "." + methodInfo.Name))
                            {
                                if (element.Descendants("inheritdoc").Any())
                                {
                                    ProcessInheritedDocumentation(xmlDocs, method);
                                }
                                else
                                {
                                    ExtractMethodDocumentation(element, method);
                                }
                            }
                            else if (name.Value.StartsWith("T:" + methodInfo.DeclaringType.FullName) &&
                                     package.Comments == null)
                            {
                                var summary = element.Descendants("summary").FirstOrDefault();

                                if (summary != null)
                                {
                                    package.Comments = summary.Value?.Trim('\n', ' ');
                                }
                            }
                        }
                    }
                }
            }

            foreach (var typeDefinition in typeDefinitions)
            {
                var xdoc = GetDocumentForAssembly(xmlDocs, typeDefinition.Type.GetTypeInfo().Assembly);

                if (xdoc == null)
                {
                    continue;
                }

                var membersNode = xdoc.Descendants("members");

                foreach (var element in membersNode.Descendants("member"))
                {
                    var name = element.Attribute("name");

                    if (name != null)
                    {
                        if (name.Value == "T:" + typeDefinition.FullName)
                        {
                            var summary = element.Descendants("summary").FirstOrDefault();

                            typeDefinition.Comment = summary?.Value?.Replace('\n', ' ').Trim(' ');
                        }
                        else
                        {
                            var propertyStart = "P:" + typeDefinition.FullName + ".";

                            if (name.Value.StartsWith(propertyStart))
                            {
                                var summary = element.Descendants("summary").FirstOrDefault();

                                if (summary?.Value != null)
                                {
                                    foreach (var typeDefinitionProperty in typeDefinition.Properties)
                                    {
                                        if (name.Value.Substring(propertyStart.Length) == typeDefinitionProperty.Name)
                                        {
                                            typeDefinitionProperty.Comment = summary.Value.Replace('\n', ' ').Trim(' ');
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ProcessInheritedDocumentation(Dictionary<Assembly, XDocument> xmlDocs, RpcMethodInfo rpcMethodInfo)
        {
            var currentType = rpcMethodInfo.Method.MethodInfo.DeclaringType;
            var processed = false;

            while (processed == false &&
                   currentType != typeof(object))

            {
                currentType = currentType.GetTypeInfo().BaseType;
                processed = ProcessMethodOnType(xmlDocs, rpcMethodInfo, currentType);
            }

            if (!processed)
            {
                foreach (var implementedInterface in 
                    rpcMethodInfo.Method.MethodInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces)
                {
                    if (ProcessMethodOnType(xmlDocs, rpcMethodInfo, implementedInterface))
                    {
                        break;
                    }
                }
            }
        }

        private bool ProcessMethodOnType(Dictionary<Assembly, XDocument> xmlDocs, RpcMethodInfo rpcMethodInfo, Type type)
        {
            var method = type.GetMethod(rpcMethodInfo.Method.MethodName);

            if (method != null)
            {
                var methodFullName = "M:" + method.DeclaringType.FullName + "." + method.Name;

                var xdoc = GetDocumentForAssembly(xmlDocs, method.DeclaringType.GetTypeInfo().Assembly);

                if (xdoc != null)
                {
                    var membersNode = xdoc.Descendants("members");

                    foreach (var element in membersNode.Descendants("member"))
                    {
                        var name = element.Attribute("name");

                        if (name != null && name.Value.StartsWith(methodFullName))
                        {
                            return ExtractMethodDocumentation(element, rpcMethodInfo);
                        }
                    }
                }
            }

            return false;
        }

        private bool ExtractMethodDocumentation(XElement element, RpcMethodInfo method)
        {
            var summary = element.Descendants("summary").FirstOrDefault();

            if (summary == null)
            {
                return false;
            }

            method.Comments = summary.Value?.Trim('\n', ' ');

            var parameters = element.Descendants("param");

            foreach (var parameterElement in parameters)
            {
                var paramName = parameterElement.Attribute("name");

                if (paramName != null)
                {
                    var paramDocNode =
                        method.Parameters.FirstOrDefault(p => p.Name == paramName.Value);

                    if (paramDocNode != null)
                    {
                        paramDocNode.Comments = parameterElement.Value?.Trim('\n', ' ');
                    }
                }
            }

            method.ReturnComment = element.Descendants("returns").FirstOrDefault()?.Value;

            return true;
        }

        private static XDocument GetDocumentForAssembly(Dictionary<Assembly, XDocument> xmlDocs, Assembly assembly)
        {
            if (!xmlDocs.TryGetValue(assembly, out var xdoc))
            {
                try
                {
                    var location = assembly.Location;

                    var lastPeriodIndex = location.LastIndexOf('.');
                    location = location.Substring(0, lastPeriodIndex);
                    location += ".xml";

                    if (File.Exists(location))
                    {
                        using (var file = File.OpenRead(location))
                        {
                            xdoc = XDocument.Load(file);
                            xmlDocs[assembly] = xdoc;
                        }
                    }
                    else
                    {
                        xmlDocs[assembly] = null;
                    }
                }
                catch (Exception e)
                {
                }
            }

            return xdoc;
        }
    }
}

