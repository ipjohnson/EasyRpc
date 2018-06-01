using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IReplacementValueProvider
    {
        void ServicePath(string path);

        string GetReplacementValue(HttpContext context,string valueName);
    }

    public class ReplacementValueProvider : IReplacementValueProvider
    {
        private string _path;

        public void ServicePath(string path)
        {
            _path = path;
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
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/spectre-exp.min.css\"/>" + 
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/docs.min.css\"/>" +
                       $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/easy-rpc-styles.css\"/>";
            }

            return $"<link rel=\"stylesheet\" href=\"{context.Request.PathBase}{_path}css/bundle.css\"/>";
        }

        protected string JavaScriptUrls(HttpContext context)
        {
            if (Debugger.IsAttached)
            {
                return $"<script src = \"{context.Request.PathBase}{_path}/javascript/umbrella.min.js\"></script>" +
                       $"<script src = \"{context.Request.PathBase}{_path}/javascript/rivets.bundled.min.js\"></script>" +
                       $"<script src = \"{context.Request.PathBase}{_path}/javascript/highlight.pack.js\"></script>" +
                       $"<script src = \"{context.Request.PathBase}{_path}/javascript/easy-rpc-javascript.js\"></script>";
            }

            return $"<script src = \"{context.Request.PathBase}{_path}/javascript/bundle.js\"></script>";
        }

        protected string InterfaceTitle(HttpContext context) => "Test API";
    }
}
