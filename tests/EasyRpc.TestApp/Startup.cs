using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.TestApp.Repositories;
using EasyRpc.TestApp.Services;
using EasyRpc.TestApp.Utilities;
using Grace.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

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

        public void ConfigureContainer(IInjectionScope scope)
        {
            scope.Configure(c =>
            {
                c.ExcludeTypeFromAutoRegistration("Microsoft.*");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseJsonRpc("/service-api/", api =>
            {
                api.Documentation(c => c.MenuWidth = 15);
                api.ExposeAssemblyContaining<Startup>().Where(type => type.Namespace.EndsWith(".Services"));
            });

            app.RedirectToDocumentation("/service-api/");
        }
    }
}
