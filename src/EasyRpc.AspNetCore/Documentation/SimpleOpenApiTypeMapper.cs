using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface ISimpleOpenApiTypeMapper
    {
        OpenApiSchema MapSimpleType(Type type);
    }

    public class SimpleOpenApiTypeMapper : ISimpleOpenApiTypeMapper
    {
        private Dictionary<Type, Func<OpenApiSchema>> _mappings;

        public OpenApiSchema MapSimpleType(Type type)
        {
            return Mappings.GetValueOrDefault(type)?.Invoke();
        }

        protected Dictionary<Type, Func<OpenApiSchema>> Mappings => _mappings ??= GenerateMappings();

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

        private OpenApiSchema GenerateDateTimeSchema()
        {
            return new OpenApiSchema{ Type = "string", Format = "date-time"};
        }

        private OpenApiSchema GenerateGuidSchema()
        {
            return new OpenApiSchema { Type = "string", Format = "uuid" };
        }

        protected virtual OpenApiSchema GenerateStringSchema()
        {
            return new OpenApiSchema {Type = "string"};
        }

        protected virtual OpenApiSchema GenerateFloatSchema()
        {
            return new OpenApiSchema {Type = "number", Format = "float"};
        }

        protected virtual OpenApiSchema GenerateDoubleSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "double" };
        }

        protected virtual OpenApiSchema GenerateDecimalSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "double" };
        }

        protected virtual OpenApiSchema GenerateByteSchema()
        {
            return new OpenApiSchema {Type = "integer" };
        }

        protected virtual OpenApiSchema GenerateBoolSchema()
        {
            return new OpenApiSchema { Type = "boolean" };
        }

        protected virtual OpenApiSchema GenerateLongSchema()
        {
            return new OpenApiSchema { Type = "integer", Format = "int64" };
        }

        protected virtual OpenApiSchema GenerateIntSchema()
        {
            return new OpenApiSchema { Type = "integer", Format = "int32" };
        }

        private OpenApiSchema GenerateNullableDateTimeSchema()
        {
            return new OpenApiSchema { Type = "string", Format = "date-time", Nullable = true };
        }

        private OpenApiSchema GenerateNullableGuidSchema()
        {
            return new OpenApiSchema { Type = "string", Format = "uuid", Nullable = true };
        }
        
        protected virtual OpenApiSchema GenerateNullableFloatSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "float", Nullable = true };
        }

        protected virtual OpenApiSchema GenerateNullableDoubleSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "double", Nullable = true };
        }

        protected virtual OpenApiSchema GenerateNullableDecimalSchema()
        {
            return new OpenApiSchema { Type = "number", Format = "double", Nullable = true };
        }

        protected virtual OpenApiSchema GenerateNullableByteSchema()
        {
            return new OpenApiSchema { Type = "integer", Nullable = true };
        }

        protected virtual OpenApiSchema GenerateNullableBoolSchema()
        {
            return new OpenApiSchema { Type = "boolean", Nullable = true };
        }

        protected virtual OpenApiSchema GenerateNullableLongSchema()
        {
            return new OpenApiSchema { Type = "integer", Format = "int64", Nullable = true };
        }

        protected virtual OpenApiSchema GenerateNullableIntSchema()
        {
            return new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true };
        }

    }
}
