﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IOpenApiSchemaGenerator
    {
        OpenApiSchema GetSchemaType(Type objectType);

        void PopulateSchemaComponent(OpenApiDocument document);
    }

    public class OpenApiSchemaGenerator : IOpenApiSchemaGenerator
    {
        private int _unknownCount;
        private ConcurrentDictionary<string, OpenApiSchema> _knownComponents;
        private ConcurrentDictionary<Type, string> _referenceNames;
        private ConcurrentDictionary<string, Type> _nameMap;
        private ISimpleOpenApiTypeMapper _simpleOpenApiTypeMapper;

        public OpenApiSchemaGenerator(ISimpleOpenApiTypeMapper simpleOpenApiTypeMapper)
        {
            _simpleOpenApiTypeMapper = simpleOpenApiTypeMapper;

            _knownComponents = new ConcurrentDictionary<string, OpenApiSchema>();
            _referenceNames = new ConcurrentDictionary<Type, string>();
            _nameMap = new ConcurrentDictionary<string, Type>();
        }

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

            var schema = _simpleOpenApiTypeMapper.MapSimpleType(objectType);

            if (schema != null)
            {
                return schema;
            }

            if (_referenceNames.TryGetValue(objectType, out var referenceName))
            {
                return new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = referenceName } };
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

        private void ProcessEnumTypeForComponent(string referenceName, Type objectType)
        {
            var schemaInstance = new OpenApiSchema { Type = "integer" };

            foreach (var value in Enum.GetValues(objectType))
            {
                schemaInstance.Enum.Add(new OpenApiInteger((int)value));
            }

            _knownComponents[referenceName] = schemaInstance;
        }

        private void ProcessTypeForComponent(string referenceName, Type objectType)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var schemaInstance = new OpenApiSchema { Type = "object", Properties = properties };

            foreach (var propertyInfo in objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var propertyTypeSchema = GetSchemaType(propertyInfo.PropertyType);

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
