using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// Service that generates OpenApiSchema objects based on Type
    /// </summary>
    public interface IOpenApiSchemaGenerator
    {
        /// <summary>
        /// Get OpenApiSchema for type
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        OpenApiSchema GetSchemaType(Type objectType);

        /// <summary>
        /// Populate component 
        /// </summary>
        /// <param name="document"></param>
        void PopulateSchemaComponent(OpenApiDocument document);

        void Configure(DocumentationOptions documentationOptions);
    }

    /// <inheritdoc />
    public class OpenApiSchemaGenerator : IOpenApiSchemaGenerator
    {
        private int _unknownCount;
        private readonly ConcurrentDictionary<string, OpenApiSchema> _knownComponents;
        private readonly ConcurrentDictionary<Type, string> _referenceNames;
        private readonly ConcurrentDictionary<string, Type> _nameMap;
        private readonly IKnownOpenApiTypeMapper _simpleOpenApiTypeMapper;
        private readonly IXmlDocProvider _xmlDocProvider;

        public OpenApiSchemaGenerator(IKnownOpenApiTypeMapper simpleOpenApiTypeMapper, IXmlDocProvider xmlDocProvider)
        {
            _simpleOpenApiTypeMapper = simpleOpenApiTypeMapper;
            _xmlDocProvider = xmlDocProvider;

            _knownComponents = new ConcurrentDictionary<string, OpenApiSchema>();
            _referenceNames = new ConcurrentDictionary<Type, string>();
            _nameMap = new ConcurrentDictionary<string, Type>();
        }

        /// <inheritdoc />
        public OpenApiSchema GetSchemaType(Type objectType)
        {
            if (objectType.IsArray)
            {
                var elementSchema = GetSchemaType(objectType.GetElementType());
                
                return new OpenApiSchema
                {
                    Type = "array",
                    Items = elementSchema
                };
            }

            if (objectType != typeof(string) &&
                typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                var interfaces = objectType.GetInterfaces();

                var enumerableInterface = interfaces.FirstOrDefault(i =>
                    i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (enumerableInterface != null)
                {
                    var elementSchema = GetSchemaType(enumerableInterface.GenericTypeArguments[0]);
                    
                    return new OpenApiSchema
                    {
                        Type = "array",
                        Items = elementSchema
                    };
                }
            }
            
            var schema = _simpleOpenApiTypeMapper.GetMapping(objectType);

            if (schema != null)
            {
                return schema;
            }

            if (_referenceNames.TryGetValue(objectType, out var referenceName))
            {
                return new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = referenceName } };
            }

            schema = SpecialTypeReference(objectType);

            if (schema != null)
            {
                return schema;
            }

            referenceName = GenerateReferenceName(objectType);

            if (objectType.IsEnum)
            {
                ProcessEnumTypeForComponent(referenceName, objectType);
            }
            else
            {
                ProcessTypeForComponent(referenceName, objectType);
            }

            return new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = referenceName } };
        }

        private OpenApiSchema SpecialTypeReference(Type objectType)
        {
            Type interfaceType = null;

            if (objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                interfaceType = objectType;
            }

            if (interfaceType == null)
            {
                interfaceType = objectType.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(type => type.IsConstructedGenericType &&
                                                                                                      type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }

            if (interfaceType != null)
            {
                var enumerableType = interfaceType.GetGenericArguments()[0];

                return new OpenApiSchema{ Type = "array", Items = GetSchemaType(enumerableType)};
            }
            
            if (objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var schema = GetSchemaType(objectType.GetGenericArguments()[0]);

                schema.Nullable = true;

                return schema;
            }

            return null;
        }

        private void ProcessEnumTypeForComponent(string referenceName, Type objectType)
        {
            var schemaInstance = new OpenApiSchema { Type = "integer" };

            foreach (var value in Enum.GetValues(objectType))
            {
                if (value is int intValue)
                {
                    schemaInstance.Enum.Add(new OpenApiInteger(intValue));
                }
                else
                {
                    intValue = (int)Convert.ChangeType(value, typeof(int));

                    schemaInstance.Enum.Add(new OpenApiInteger(intValue));
                }
            }

            _knownComponents[referenceName] = schemaInstance;
        }

        private void ProcessTypeForComponent(string referenceName, Type objectType)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var schemaInstance = new OpenApiSchema { Type = "object", Properties = properties };

            var element = _xmlDocProvider.GetTypeDocumentation(objectType);

            schemaInstance.Description = element.GetSummary();

            foreach (var propertyInfo in objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var propertyTypeSchema = GetSchemaType(propertyInfo.PropertyType);

                element = _xmlDocProvider.GetPropertyDocumentation(propertyInfo);

                propertyTypeSchema.Description = element.GetSummary();

                properties[CasePropertyName(propertyInfo.Name)] = propertyTypeSchema;
            }

            _knownComponents[referenceName] = schemaInstance;
        }

        protected virtual string CasePropertyName(string propertyName)
        {
            if (propertyName.Length == 1)
            {
                return char.ToLowerInvariant(propertyName[0]).ToString();
            }

            return $"{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
        }

        public void PopulateSchemaComponent(OpenApiDocument document)
        {
            document.Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>(_knownComponents) };
        }

        /// <inheritdoc />
        public void Configure(DocumentationOptions documentationOptions)
        {
            _simpleOpenApiTypeMapper.Configure(documentationOptions);
        }

        private string GenerateReferenceName(Type objectType)
        {
            if (IsAnonymousType(objectType))
            {
                var anonName = "AnonymousType" + Interlocked.Increment(ref _unknownCount);

                _nameMap[anonName] = objectType;
                _referenceNames.TryAdd(objectType, anonName);

                return anonName;
            }

            if (_nameMap.TryAdd(objectType.Name, objectType))
            {
                _referenceNames.TryAdd(objectType, objectType.Name);

                return objectType.Name;
            }

            var fullName = objectType.FullName.Replace(".", "");

            if (_nameMap.TryAdd(fullName, objectType))
            {
                _referenceNames.TryAdd(objectType, fullName);

                return fullName;
            }

            var newName = "UnknownType" + Interlocked.Increment(ref _unknownCount);

            _nameMap[newName] = objectType;
            _referenceNames[objectType] = newName;

            return newName;
        }

        private bool IsAnonymousType(Type objectType)
        {
            return objectType.Name.StartsWith("<>f");
        }
    }
}
