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
        private IHttpContextAccessor _accessor;
        private ConcurrentDictionary<string, object> _values;

        public RpcHeaderContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            _values = new ConcurrentDictionary<string, object>();
        }

        public T GetValue<T>(string key = null)
        {
            var stringKey = typeof(T).Name + key;

            var headerValue = _accessor.HttpContext.Request.Headers["RpcContext-" + stringKey];

            if (headerValue.Count == 0)
            {
                return default(T);
            }

            var base64 = headerValue[0];

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
        }

        public void SetValue<T>(T value, string key = null)
        {
            var stringKey = typeof(T).Name + key;

            string valueString = value != null ? JsonConvert.SerializeObject(value) : "";

            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(valueString));

            _accessor.HttpContext.Response.Headers["RpcContext-" + stringKey] = new StringValues(base64String);
        }
    }
}
