using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Converters
{
    public class RpcParameterConverter : JsonConverter
    {
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
			switch (reader.TokenType)
			{
				case JsonToken.StartObject:
					try
					{
						JObject jObject = JObject.Load(reader);
                        
                        return jObject.ToObject<Dictionary<string, object>>();
					}
					catch (Exception)
					{
						throw new Exception("Request parameters can only be an associative array, list or null.");
					}

				case JsonToken.StartArray:
					return JArray.Load(reader).ToObject<object[]>(serializer);

				case JsonToken.Null:
					return Array.Empty<object>();
			}

			throw new Exception("Request parameters can only be an associative array, list or null.");
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
            return true;
        }
    }
}
