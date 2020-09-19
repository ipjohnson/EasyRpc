using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// Maps a simple type to an OpenApiSchema element
    /// </summary>
    public interface IKnownOpenApiTypeMapper
    {
        /// <summary>
        /// Configure documentation
        /// </summary>
        /// <param name="options"></param>
        void Configure(DocumentationOptions options);

        /// <summary>
        /// Get type mapping
        /// </summary>
        /// <param name="type">simple type</param>
        /// <returns>open api schema element</returns>
        OpenApiSchema GetMapping(Type type);

        /// <summary>
        /// List of mapped types
        /// </summary>
        IEnumerable<Type> MappedTypes { get; }
    }

    /// <summary>
    /// Maps simple types to OpenApiSchema objects 
    /// </summary>
    public class KnownOpenApiTypeMapper : IKnownOpenApiTypeMapper
    {
        private Dictionary<Type, Func<OpenApiSchema>> _mappings;

        /// <inheritdoc />
        public void Configure(DocumentationOptions options)
        {
            foreach (var typeMapping in options.TypeMappings)
            {
                Mappings[typeMapping.Key] = typeMapping.Value;
            }
        }

        /// <inheritdoc />
        public OpenApiSchema GetMapping(Type type)
        {
            return Mappings.GetValueOrDefault(type)?.Invoke();
        }

        /// <inheritdoc />
        public IEnumerable<Type> MappedTypes => Mappings.Keys;

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<Type, Func<OpenApiSchema>> Mappings => _mappings ??= GenerateMappings();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<Type, Func<OpenApiSchema>> GenerateMappings()
        {
            return new Dictionary<Type, Func<OpenApiSchema>>
            {
                {typeof(bool), GenerateBoolSchema },
                {typeof(byte), GenerateByteSchema },
                {typeof(sbyte), GenerateByteSchema },
                {typeof(ushort), GenerateIntSchema },
                {typeof(short), GenerateIntSchema },
                {typeof(int), GenerateIntSchema },
                {typeof(uint), GenerateIntSchema },
                {typeof(long), GenerateLongSchema },
                {typeof(ulong), GenerateLongSchema },
                {typeof(float), GenerateFloatSchema },
                {typeof(double), GenerateDoubleSchema },
                {typeof(decimal), GenerateDecimalSchema },
                {typeof(Guid), GenerateGuidSchema },
                {typeof(DateTime), GenerateDateTimeSchema },
                {typeof(DateTimeOffset), GenerateDateTimeSchema },
                {typeof(bool?), GenerateNullableBoolSchema },
                {typeof(byte?), GenerateNullableByteSchema },
                {typeof(sbyte?), GenerateNullableByteSchema },
                {typeof(ushort?), GenerateNullableIntSchema },
                {typeof(short?), GenerateNullableIntSchema },
                {typeof(int?), GenerateNullableIntSchema },
                {typeof(uint?), GenerateNullableIntSchema },
                {typeof(long?), GenerateNullableLongSchema },
                {typeof(ulong?), GenerateNullableLongSchema },
                {typeof(float?), GenerateNullableFloatSchema },
                {typeof(double?), GenerateNullableDoubleSchema },
                {typeof(decimal?), GenerateNullableDecimalSchema },
                {typeof(Guid?), GenerateNullableGuidSchema },
                {typeof(DateTime?), GenerateNullableDateTimeSchema },
                {typeof(DateTimeOffset?), GenerateNullableDateTimeSchema },
                
                {typeof(string), GenerateStringSchema }
            };
        }

        /// <summary>
        /// Generate schema for DateTime
        /// </summary>
        /// <returns></returns>
        private OpenApiSchema GenerateDateTimeSchema()
        {
            return new OpenApiSchema{ Type = "string", Format = "date-time", Nullable = false };
        }

        /// <summary>
        /// Generate schema for guid
        /// </summary>
        /// <returns></returns>
        private OpenApiSchema GenerateGuidSchema()
        {
            return new OpenApiSchema { Type = "string", Format = "uuid", Nullable = false };
        }

        /// <summary>
        /// Generate schema for string
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateStringSchema()
        {
            return new OpenApiSchema {Type = "string"};
        }

        /// <summary>
        /// Generate schema for float
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateFloatSchema()
        {
            return new OpenApiSchema {Type = "number", Format = "float", Nullable = false };
        }

        /// <summary>
        /// Generate schema for double
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateDoubleSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "double", Nullable = false };
        }

        /// <summary>
        /// Generate schema for decimal 
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateDecimalSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "double", Nullable = false };
        }

        /// <summary>
        /// Generate schema for byte
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateByteSchema()
        {
            return new OpenApiSchema {Type = "integer", Nullable = false };
        }

        /// <summary>
        /// Generate schema for bool
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateBoolSchema()
        {
            return new OpenApiSchema { Type = "boolean", Nullable = false };
        }

        /// <summary>
        /// Generate schema for long
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateLongSchema()
        {
            return new OpenApiSchema { Type = "integer", Format = "int64", Nullable = false };
        }

        /// <summary>
        /// Generate schema for int
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateIntSchema()
        {
            return new OpenApiSchema { Type = "integer", Format = "int32", Nullable = false};
        }

        /// <summary>
        /// generate schema for DateTime?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableDateTimeSchema()
        {
            var schema = GenerateDateTimeSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for guid?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableGuidSchema()
        {
            var schema = GenerateGuidSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for float?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableFloatSchema()
        {
            var schema = GenerateFloatSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for double?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableDoubleSchema()
        {
            var schema = GenerateDoubleSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for decimal?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableDecimalSchema()
        {
            var schema = GenerateDecimalSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for byte?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableByteSchema()
        {
            var schema = GenerateByteSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for bool?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableBoolSchema()
        {
            var schema = GenerateBoolSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for long?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableLongSchema()
        {
            var schema = GenerateLongSchema();

            schema.Nullable = true;

            return schema;
        }

        /// <summary>
        /// Generate schema for int?
        /// </summary>
        /// <returns></returns>
        protected virtual OpenApiSchema GenerateNullableIntSchema()
        {
            var schema = GenerateIntSchema();

            schema.Nullable = true;

            return schema;
        }

    }
}
