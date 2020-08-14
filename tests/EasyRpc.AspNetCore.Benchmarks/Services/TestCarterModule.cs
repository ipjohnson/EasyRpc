using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Benchmarks.Services
{
    public class TestCarterModule : CarterModule
    {
        private static readonly byte[] _helloWorldPayload = Encoding.UTF8.GetBytes("Hello, World!");

        public TestCarterModule()
        {
            Get("/plaintext",  (req, res) =>
            {
                var payloadLength = _helloWorldPayload.Length;
                res.StatusCode = 200;
                res.ContentType = "text/plain";
                res.ContentLength = payloadLength;
                return res.Body.WriteAsync(_helloWorldPayload, 0, payloadLength);
            });
        }
    }
}
