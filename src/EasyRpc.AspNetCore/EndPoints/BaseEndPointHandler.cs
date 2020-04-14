using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    public abstract class BaseEndPointHandler : IEndPointHandler
    {
        private object[] _serializerData = Array.Empty<object>();

        protected BaseEndPointHandler(string path, bool supportsLongerPaths)
        {
            Path = path;
            SupportsLongerPaths = supportsLongerPaths;
        }

        public string Path { get; }

        public bool SupportsLongerPaths { get; }

        public abstract Task HandleRequest(HttpContext context, RequestDelegate next);

        public abstract Task ExecuteHandler(RequestExecutionContext rpcContext);

        public object GetSerializerData(int serializerId)
        {
            return _serializerData.Length > serializerId ? _serializerData[serializerId] : null;
        }

        public void SetSerializerData(int serializerId, object data)
        {
            if (_serializerData.Length <= serializerId)
            {
                lock (this)
                {
                    var newData = new object[serializerId + 1];

                    Array.Copy(_serializerData, newData, _serializerData.Length);

                    _serializerData = newData;
                }
            }

            _serializerData[serializerId] = data;
        }

    }
}
