using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyRpc.TestApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRpcServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRpcServices(api =>
            {
                //api.GetMethod("/test/{id}", (int id) => new { value = id });
                //api.PostMethod("/test/{id}", (int id, BodyTest body) => id + body.Value);
                //api.PostMethod("/another/{id}", (int id, int id2, int id3) => id + id2 + id3);
                //api.GetMethod("/test2/{id}/{id2}", (int id, int id2) => new { value = id, value2 = id });
                //api.GetMethod("/StringTest/{stringValue}", (string stringValue) => stringValue + " Hello world!");
                api.GetMethod("/plaintext", () => "Hello, World!").Raw("text/plain");
                //api.GetMethod("/json", () => new { message = "Hello, World!" });
                api.PostMethod("/lists", (TestClass value) => Task.CompletedTask);
            });
        }

        public class BodyTest
        {
            public int Value { get; set; }
        }

        public enum SomeEnum
        {
            Red,
            Black,
            Blue
        }

        public class TestClass
        {
            public SomeEnum Enum { get; set; }
            public List<int> IntValues { get; set; }
        }
    }


}
