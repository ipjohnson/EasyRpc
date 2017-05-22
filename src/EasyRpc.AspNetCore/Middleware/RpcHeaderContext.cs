using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Middleware
{
    public class RpcHeaderContext : IRpcHeaderContext
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly JsonSerializer _serializer;

        public RpcHeaderContext(IHttpContextAccessor accessor, JsonSerializer serializer)
        {
            _accessor = accessor;
            _serializer = serializer;
        }

        public T GetValue<T>(string key = null)
        {
            var httpContext = _accessor.HttpContext;

            var currentValues = GetCurrentValues(httpContext);

            var stringKey = typeof(T).Name + key;

            object value;

            if (currentValues.TryGetValue(stringKey, out value))
            {
                return (T) value;
            }

            var headerValue = httpContext.Request.Headers["RpcContext-" + stringKey];

            if (headerValue.Count == 0)
            {
                return default(T);
            }

            var base64 = headerValue[0];

            using (var memoryStream = new MemoryStream(Convert.FromBase64String(base64)))
            {
                using (var textReader = new StreamReader(memoryStream))
                {
                    using (var jsonStream = new JsonTextReader(textReader))
                    {
                        var tValue = _serializer.Deserialize<T>(jsonStream);

                        currentValues[stringKey] = tValue;

                        return tValue;
                    }
                }
            }
        }

        public void SetValue<T>(T value, string key = null)
        {
            var httpContext = _accessor.HttpContext;

            var stringKey = typeof(T).Name + key;

            if (value != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var textWriter = new StreamWriter(memoryStream))
                    {
                        _serializer.Serialize(textWriter, value);
                    }

                    var base64String = Convert.ToBase64String(memoryStream.ToArray());

                    httpContext.Response.Headers["RpcContext-" + stringKey] = new StringValues(base64String);
                }
            }
            else
            {
                httpContext.Response.Headers["RpcContext-" + stringKey] = new StringValues("");
            }
            
            var currentValues = GetCurrentValues(httpContext);

            currentValues[stringKey] = value;
        }

        protected ConcurrentDictionary<string, object> GetCurrentValues(HttpContext context)
        {
            var currentValues = context.Items["RpcHeaderValues"] as ConcurrentDictionary<string, object>;

            if (currentValues != null)
            {
                return currentValues;
            }

            currentValues = new ConcurrentDictionary<string, object>();
            
            context.Items["RpcHeaderValues"] = currentValues;

            return currentValues;
        }
    }
}
