using System.Diagnostics;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.TestApp.Repositories;
using EasyRpc.TestApp.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyRpc.TestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJsonRpc(c =>
            {
                c.DebugLogging = false;
                c.SupportResponseCompression = true;
            });

            services.AddSingleton<IPersonRepository, PersonRepository>();

            if (Debugger.IsAttached)
            {
                services.AddSingleton<IWebAssetProvider, CustomWebAssetProvider>();
            }
        }

        //public class ActivatorInstance : EasyRpc.AspNetCore.Middleware.IInstanceActivator
        //{
        //    public object ActivateInstance(HttpContext context, IServiceProvider serviceProvider, Type instanceType)
        //    {
        //        return serviceProvider.GetService(instanceType);
        //    }
        //}

        //public void ConfigureContainer(IInjectionScope scope)
        //{
        //    scope.Configure(c =>
        //    {
        //        c.ExcludeTypeFromAutoRegistration("Microsoft.*");
        //        c.Export<ActivatorInstance>().As<IInstanceActivator>();
        //    });
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
        {
            //factory.AddConsole(Configuration.GetSection("Logging"));

            app.UseJsonRpc("/service-api/", api =>
            {
                api.Documentation(c => c.MenuWidth = 15);
                api.ExposeAssemblyContaining<Startup>().Where(type => type.Namespace.EndsWith(".Services"));

                api.Expose("TestMethods").Methods(add =>
                {
                    add.Func("Test1", (int x, int y) => x + y);
                    add.Func("Test2", (int x, int y) => x + y);
                    add.Func("Test3", (int x, int y) => x + y);
                });
            });

            app.RedirectToDocumentation("/service-api/");
        }
    }
}
