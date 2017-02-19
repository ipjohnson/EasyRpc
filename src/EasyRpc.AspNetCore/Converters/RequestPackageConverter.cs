using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Converters
{
    public class RequestPackageConverter : JsonConverter
    {
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    try
                    {
                        var request = serializer.Deserialize<RequestMessage>(reader);

                        return new RequestPackage(request);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Malformed json");
                    }

                case JsonToken.StartArray:
                    try
                    {
                        var array = serializer.Deserialize<RequestMessage[]>(reader);

                        return new RequestPackage(array);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Malformed json");
                    }

                default:
                    throw new Exception("message is empty");
            }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RequestPackage);
        }
    }
}
