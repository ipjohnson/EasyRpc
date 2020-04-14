using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Response;
using EasyRpc.Abstractions.Services;

namespace EasyRpc.AspNetCore.Benchmarks.Services
{
    [SharedService]
    public class StringService
    {
        public StringService()
        {

        }

        public class Response
        {
            public string ArgA { get; set; }

            public string ArgB { get; set; }
        }

        [PostMethod("/StringService/Concat")]
        public Response Concat(string argA, string argB)
        {
            return new Response { ArgA = argA, ArgB = argB };
        }
        
        [GetMethod("/noparams")]
        public object NoParams()
        {
            return new { Prop1 = "1", Prop2 = "second" };
        }

        [RawContent("text/plain")]
        [GetMethod("/plaintext")]
        public string PlainText()
        {
            return "Hello World!";
        }

    }
}
