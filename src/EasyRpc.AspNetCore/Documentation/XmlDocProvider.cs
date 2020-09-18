using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// Provides xml documentation for different types
    /// </summary>
    public interface IXmlDocProvider
    {
        /// <summary>
        /// Get XElement that represents the documentation for a given type, can be null
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        XElement GetTypeDocumentation(Type type);


        /// <summary>
        /// Get XElement that represents the documentation for a given member
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        XElement GetMethodDocumentation(MethodInfo methodInfo);

        /// <summary>
        /// Get XElement that represents the documentation for a given member
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        XElement GetPropertyDocumentation(PropertyInfo propertyInfo);
    }

    /// <inheritdoc />
    public class XmlDocProvider : IXmlDocProvider
    {
        private readonly ConcurrentDictionary<string, XElement> _types;
        private readonly ConcurrentDictionary<string, XElement> _properties;
        private readonly ConcurrentDictionary<string, XElement> _methods;
        private readonly ConcurrentBag<Assembly> _loadedAssemblies;

        /// <summary>
        /// Default constructor
        /// </summary>
        public XmlDocProvider()
        {
            _types = new ConcurrentDictionary<string, XElement>();
            _properties = new ConcurrentDictionary<string, XElement>();
            _methods = new ConcurrentDictionary<string, XElement>();
            _loadedAssemblies = new ConcurrentBag<Assembly>();
        }

        /// <inheritdoc />
        public XElement GetTypeDocumentation(Type type)
        {
            if (!_loadedAssemblies.Contains(type.Assembly))
            {
                LoadAssembly(type.Assembly);
            }

            var typeName = GenerateTypeName(type);

            return _types.GetValueOrDefault(typeName);
        }

        /// <inheritdoc />
        public XElement GetMethodDocumentation(MethodInfo methodInfo)
        {
            var assembly = methodInfo.DeclaringType?.Assembly;

            if (assembly == null)
            {
                return null;
            }

            if (!_loadedAssemblies.Contains(assembly))
            {
                LoadAssembly(assembly);
            }

            var memberName = GenerateMethodName(methodInfo);

            return _methods.GetValueOrDefault(memberName);
        }

        /// <inheritdoc />
        public XElement GetPropertyDocumentation(PropertyInfo propertyInfo)
        {
            var assembly = propertyInfo.DeclaringType?.Assembly;

            if (assembly == null)
            {
                return null;
            }

            if (!_loadedAssemblies.Contains(assembly))
            {
                LoadAssembly(assembly);
            }

            var memberName = GeneratePropertyName(propertyInfo);

            return _properties.GetValueOrDefault(memberName);
        }

        private void LoadAssembly(Assembly typeAssembly)
        {
            _loadedAssemblies.Add(typeAssembly);

            var xmlDocument = GetXmlDocument(typeAssembly);

            if (xmlDocument == null)
            {
                return;
            }

            var membersNode = xmlDocument.Descendants("members");

            foreach (var element in membersNode.Descendants("member"))
            {
                var name = element.Attribute("name");

                if (name != null)
                {
                    if (name.Value.StartsWith("T:"))
                    {
                        ProcessTypeElement(element, name);
                    }
                    else if (name.Value.StartsWith("M:"))
                    {
                        ProcessMethodElement(element, name);
                    }
                    else if (name.Value.StartsWith("P:"))
                    {
                        ProcessPropertyElement(element, name);
                    }
                }
            }
        }

        private void ProcessPropertyElement(XElement element, XAttribute name)
        {
            _properties[name.Value] = element;
        }

        private void ProcessMethodElement(XElement element, XAttribute name)
        {
            var substringIndex = name.Value.IndexOf('(');


            var key = name.Value;

            if (substringIndex > 0)
            {
                key = name.Value.Substring(0, substringIndex);
            }

            _methods[key] = element;
        }

        private void ProcessTypeElement(XElement element, XAttribute name)
        {
            _types[name.Value] = element;
        }

        private XDocument GetXmlDocument(Assembly assembly)
        {
            XDocument xDoc = null;

            try
            {
                var location = assembly.Location;

                var lastPeriodIndex = location.LastIndexOf('.');
                location = location.Substring(0, lastPeriodIndex);
                location += ".xml";

                if (File.Exists(location))
                {
                    using var file = File.OpenRead(location);

                    xDoc = XDocument.Load(file);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // this is usually caused by assembly.Location throwing an exception
            }
            
            return xDoc;
        }

        private string GeneratePropertyName(PropertyInfo memberInfo)
        {
            return $"P:{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}";
        }

        private string GenerateTypeName(Type type)
        {
            return $"T:{type.FullName}";
        }

        private string GenerateMethodName(MethodInfo methodInfo)
        {
            return $"M:{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}";
        }

    }
}
