using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.Middleware;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface ITypeDefinitionPackageProvider
    {
        void SetupTypeDefinitions(ICollection<ExposedMethodInformation> packages);

        IEnumerable<TypeDefinition> GetDefinitionsPackage();
    }

    public class TypeDefinitionPackageProvider : ITypeDefinitionPackageProvider
    {
        private Dictionary<Type, TypeDefinition> _typeDefinitions = new Dictionary<Type, TypeDefinition>();
        public IEnumerable<TypeDefinition> GetDefinitionsPackage()
        {
            return _typeDefinitions.Values;
        }

        public void SetupTypeDefinitions(ICollection<ExposedMethodInformation> exposedMethods)
        {
            foreach (var exposedMethod in exposedMethods)
            {
                var methodInfo = exposedMethod.MethodInfo;

                GenerateTypeDefinitionFor(methodInfo.ReturnType);

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    GenerateTypeDefinitionFor(parameterInfo.ParameterType);
                }
            }
        }

        private void GenerateTypeDefinitionFor(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (type == typeof(string) ||
                type == typeof(DateTime) ||
                typeInfo.IsPrimitive)
            {
                return;
            }

            if (_typeDefinitions.ContainsKey(type))
            {
                return;
            }

            if (typeInfo.ImplementedInterfaces.Any(t =>
                t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                return;
            }

            var properties = new List<PropertyDefinition>();

            var typeDefinition = new TypeDefinition
            {
                Name = type.Name,
                FullName = type.FullName,
                Properties = properties
            };

            _typeDefinitions[type] = typeDefinition;

            foreach (var property in typeInfo.DeclaredProperties)
            {
                if (property.CanRead &&
                    property.GetMethod.IsPublic &&
                    !property.GetMethod.IsStatic)
                {
                    var propertyDef = new PropertyDefinition
                    {
                        Name = property.Name,
                        PropertyType = property.PropertyType.FullName,
                        Required = property.GetCustomAttributes<Attribute>().Any(a => a.GetType().Name == "RequiredAttribute")
                    };

                    properties.Add(propertyDef);

                    GenerateTypeDefinitionFor(property.PropertyType);
                }
            }
        }
    }
}
