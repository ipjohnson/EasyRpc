using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    public class DocumentationOptions
    {
        public bool RedirectRootToDocumentation { get; set; } = true;

        public string RedirectUrl { get; set; }

        public string OpenApiJsonUrl { get; set; } = "api.json";

        public string SwaggerBasePath { get; set; } = "/swagger/";

        public string Title { get; set; }

        public string ApiDescription { get; set; }
    }
}
