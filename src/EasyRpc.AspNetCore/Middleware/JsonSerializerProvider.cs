using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IJsonSerializerProvider
    {
        JsonSerializer ProvideSerializer();
    }

    public class JsonSerializerProvider : IJsonSerializerProvider
    {
        public JsonSerializer ProvideSerializer()
        {
            var serializer = new JsonSerializer();

            AddConverters(serializer);

            return serializer;
        }

        protected virtual void AddConverters(JsonSerializer serializer)
        {
            
        }
    }
}
