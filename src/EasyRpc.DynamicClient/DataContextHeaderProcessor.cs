using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient
{
    public interface IRpcContextHeader
    {
        void SetValue<T>(T value, string key = null);

        T GetValue<T>(string key = null);
    }

    public class DataContextHeaderProcessor : IRpcContextHeader, IHeaderProcessor
    {
        private readonly Dictionary<string, object> _currentObjects = new Dictionary<string, object>();
        private readonly Dictionary<string, string> _serialized = new Dictionary<string, string>();
        private readonly JsonSerializer _serializer;

        public DataContextHeaderProcessor(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public void SetValue<T>(T value, string key = null)
        {
            _currentObjects[typeof(T).Name + key] = value;
        }

        public T GetValue<T>(string key = null)
        {
            var stringKey = typeof(T).Name + key;

            object value;

            if (_currentObjects.TryGetValue(stringKey, out value))
            {
                return (T)value;
            }

            string base64Value;

            if (_serialized.TryGetValue(stringKey, out base64Value))
            {
                using (var memoryStream = new MemoryStream(Convert.FromBase64String(base64Value)))
                {
                    using (var textReader = new StreamReader(memoryStream))
                    {
                        using (var jsonReader = new JsonTextReader(textReader))
                        {
                            var tValue = _serializer.Deserialize<T>(jsonReader);

                            _currentObjects[stringKey] = tValue;

                            return tValue;
                        }
                    }
                }
            }

            return default(T);
        }

        public void ProcessRequestHeader(HttpRequestMessage message)
        {
            foreach (var valuePair in _currentObjects)
            {
                using (var memorStream = new MemoryStream())
                {
                    using (var textStream = new StreamWriter(memorStream))
                    {
                        _serializer.Serialize(textStream,valuePair.Value);
                    }

                    var base64String = Convert.ToBase64String(memorStream.ToArray());

                    message.Headers.Add("RpcContext-" + valuePair.Key, base64String);
                }
            }

            foreach (var valuePair in _serialized)
            {
                if (message.Headers.Contains(valuePair.Key))
                {
                    continue;
                }

                message.Headers.Add(valuePair.Key, valuePair.Value);
            }
        }

        public void ProcessResponseHeader(HttpResponseMessage message)
        {
            foreach (var httpResponseHeader in message.Headers)
            {
                if (httpResponseHeader.Key.StartsWith("RpcContext-"))
                {
                    var key = httpResponseHeader.Key.Substring("RpcContext-".Length);

                    _currentObjects.Remove(key);
                    _serialized[key] = httpResponseHeader.Value.FirstOrDefault();
                }
            }
        }
    }
}
