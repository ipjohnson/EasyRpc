using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        private string _urlBase;
        private string _versionString;

        public void ServicePath(EndPointConfiguration configuration)
        {
            _path = configuration.Route;
            _title = configuration.DocumentationConfiguration.Title ?? GenerateTitle(configuration);
            _versionString = configuration.DocumentationConfiguration.VersionString ?? GenerateVersionString();
            _urlBase = configuration.DocumentationConfiguration.CustomBaseUrl;
        }

        private string GenerateVersionString()
        {
            return Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? 
                Assembly.GetEntryAssembly().GetName().Version.ToString();
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

                case "VersionString":
                    return _versionString;

                case "ServiceUrl":
                    return _urlBase ?? (context.Request.PathBase.Value + _path);
            }

            throw new Exception("Unkown replacement string " + valueName);
        }

        protected string CssUrls(HttpContext context)
        {
            var path = _urlBase ?? (context.Request.PathBase + _path);

            if (Debugger.IsAttached)
            {                
                return $"<link rel=\"stylesheet\" href=\"{path}css/spectre.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{path}css/spectre-icons.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{path}css/spectre-exp.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{path}css/docs.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{path}css/easy-rpc-styles.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{path}css/custom.css\"/>";
            }

            return $"<link rel=\"stylesheet\" href=\"{path}css/bundle.css?version={_versionString}\"/>";
        }

        protected string JavaScriptUrls(HttpContext context)
        {
            var path = _urlBase ?? (context.Request.PathBase + _path);

            if (Debugger.IsAttached)
            {
                return $"<script src = \"{path}javascript/umbrella.min.js\"></script>" +
                       $"<script src = \"{path}javascript/rivets.bundled.min.js\"></script>" +
                       $"<script src = \"{path}javascript/easy-rpc-javascript.js\"></script>";
            }

            return $"<script src = \"{path}javascript/bundle.js?version={_versionString}\"></script>";
        }

        protected string InterfaceTitle(HttpContext context) => _title;
    }
}
