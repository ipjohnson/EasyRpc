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
                {typeof(int), GenerateIntSchema },
                {typeof(uint), GenerateIntSchema },
                {typeof(long), GenerateLongSchema },
                {typeof(ulong), GenerateLongSchema },
                {typeof(float), GenerateFloatSchema },
                {typeof(string), GenerateStringSchema }
            };
        }

        protected virtual OpenApiSchema GenerateStringSchema()
        {
            return new OpenApiSchema {Type = "string"};
        }

        protected virtual OpenApiSchema GenerateFloatSchema()
        {
            return new OpenApiSchema {Type = "number", Format = "float"};
        }

        protected virtual OpenApiSchema GenerateByteSchema()
        {
            return new OpenApiSchema {Type = "string", Format = "byte"};
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
    }
}
