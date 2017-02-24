using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.DynamicClient;
using Newtonsoft.Json;

namespace EasyRpc.Client.Sample
{
    public class DataContextHeaderProcessor : IHeaderProcessor
    {
        private readonly Dictionary<string, object> _currentObjects = new Dictionary<string, object>();
        private readonly Dictionary<string, string> _serialized = new Dictionary<string, string>();

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
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Convert.FromBase64String(base64Value)));
            }

            return default(T);
        }

        public void ProcessRequestHeader(HttpRequestMessage message)
        {
            foreach (var valuePair in _currentObjects)
            {
                var serialized = JsonConvert.SerializeObject(valuePair.Value);

                var bytes = Encoding.UTF8.GetBytes(serialized);

                var base64String = Convert.ToBase64String(bytes);

                message.Headers.Add("RpcContext-" + valuePair.Key, base64String);
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
