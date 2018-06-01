using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IXmlDocumentationProvider
    {
        void PopulateMethodDocumentation(List<JsonDataPackage> packages);
    }

    public class XmlDocumentationProvider : IXmlDocumentationProvider
    {
        public void PopulateMethodDocumentation(List<JsonDataPackage> packages)
        {
            var xmlDocs = new Dictionary<Assembly, XDocument>();

            foreach (var package in packages)
            {
                foreach (var method in package.Methods)
                {
                    var methodInfo = method.Method.Method;
                    var assembly = methodInfo.DeclaringType.GetTypeInfo().Assembly;

                    var xdoc = GetDocumentForAssembly(xmlDocs, assembly);

                    var membersNode = xdoc.Descendants("members");

                    foreach (var element in membersNode.Descendants("member"))
                    {
                        var name = element.Attribute("name");

                        if (name != null)
                        {
                            if (name.Value.StartsWith("M:" + methodInfo.DeclaringType.FullName + "." + methodInfo.Name))
                            {
                                var summary = element.Descendants("summary").FirstOrDefault();

                                if (summary != null)
                                {
                                    method.Comments = summary.Value?.Trim('\n',' ');
                                }

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
                                            paramDocNode.Comments = parameterElement.Value?.Trim('\n',' ');
                                        }
                                    }
                                }
                            }
                            else if (name.Value.StartsWith("T:" + methodInfo.DeclaringType.FullName) && 
                                     package.Comments == null)
                            {
                                var summary = element.Descendants("summary").FirstOrDefault();

                                if (summary != null)
                                {
                                    package.Comments = summary.Value?.Trim('\n',' ');
                                }

                            }
                        }
                    }
                }
            }
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
                }
                catch (Exception e)
                {
                }
            }

            return xdoc;
        }
    }
}
