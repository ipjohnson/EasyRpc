using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Middleware
{
    public class RpcHeaderContext : IRpcHeaderContext
    {
        private readonly IHttpContextAccessor _accessor;

        public RpcHeaderContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
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

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
        }

        public void SetValue<T>(T value, string key = null)
        {
            var httpContext = _accessor.HttpContext;

            var stringKey = typeof(T).Name + key;

            string valueString = value != null ? JsonConvert.SerializeObject(value) : "";

            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(valueString));

            httpContext.Response.Headers["RpcContext-" + stringKey] = new StringValues(base64String);

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
