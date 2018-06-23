using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IReplacementValueProvider
    {
        void ServicePath(EndPointConfiguration path);

        string GetReplacementValue(HttpContext context,string valueName);
    }

    public class ReplacementValueProvider : IReplacementValueProvider
    {
        private string _path;
        private string _title;

        public void ServicePath(EndPointConfiguration configuration)
        {
            _path = configuration.Route;
            _title = configuration.DocumentationConfiguration.Title ?? GenerateTitle(configuration);
        }

        protected virtual string GenerateTitle(EndPointConfiguration configuration)
        {
            var method = configuration.Methods.Values.FirstOrDefault()?.MethodInfo;

            if (method != null)
            {
                var title = method.DeclaringType.FullName;
                var index = title.IndexOf('.');

                if (index > 0)
                {
                    return title.Substring(0, index);
                }
            }

            return "Api";
        }

        public virtual string GetReplacementValue(HttpContext context, string valueName)
        {
            switch (valueName)
            {
                case nameof(CssUrls):
                    return CssUrls(context);

                case nameof(JavaScriptUrls):
                    return JavaScriptUrls(context);

                case nameof(InterfaceTitle):
                    return InterfaceTitle(context);

                case "ServiceUrl":
                    return context.Request.PathBase.Value + _path;
            }

            throw new Exception("Unkown replacement string " + valueName);
        }

        protected string CssUrls(HttpContext context)
        {
            if (Debugger.IsAttached)
            {
                return $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/spectre.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/spectre-icons.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/spectre-exp.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/docs.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/easy-rpc-styles.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/custom.css\"/>";
            }

            return $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/bundle.css\"/>";
        }

        protected string JavaScriptUrls(HttpContext context)
        {
            if (Debugger.IsAttached)
            {
                return $"<script src = \"{context.Request.PathBase}{_path}javascript/umbrella.min.js\"></script>" +
                       $"<script src = \"{context.Request.PathBase}{_path}javascript/rivets.bundled.min.js\"></script>" +
                       $"<script src = \"{context.Request.PathBase}{_path}javascript/easy-rpc-javascript.js\"></script>";
            }

            return $"<script src = \"{context.Request.PathBase}{_path}javascript/bundle.js\"></script>";
        }

        protected string InterfaceTitle(HttpContext context) => _title;
    }
}
