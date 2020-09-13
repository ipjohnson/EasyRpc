using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface ITokenValueProvider
    {
        string ProvideToken(string token);

        void Configure(DocumentationOptions options);
    }

    public class TokenValueProvider : ITokenValueProvider
    {
        private DocumentationOptions _options;

        public string ProvideToken(string token)
        {
            switch (token.ToLowerInvariant())
            {
                case "title":
                    return TitleToken();

                case "extracss":
                    return ExtraCssToken();

                case "extrajavascript":
                    return ExtraJavaScript();

                case "apiurl":
                    return ApiUrlToken();
            }

            return "";
        }

        protected virtual string ApiUrlToken()
        {
            return _options.OpenApiJsonUrl;
        }

        protected virtual string ExtraJavaScript()
        {
            if (string.IsNullOrEmpty(_options.ExtraJavaScript))
            {
                return null;
            }

            if (_options.ExtraJavaScript.StartsWith("<"))
            {
                return _options.ExtraJavaScript;
            }

            return $"<script src=\"{_options.ExtraJavaScript}\" charset=\"UTF-8\"></script>";
        }

        protected virtual string TitleToken()
        {
            return _options.Title;
        }

        protected virtual string ExtraCssToken()
        {
            if (string.IsNullOrEmpty(_options.ExtraCss))
            {
                return null;
            }

            if (_options.ExtraCss.StartsWith("<"))
            {
                return _options.ExtraJavaScript;
            }

            return $"<rel=\"stylesheet\" type=\"text/css\" href=\"{_options.ExtraCss}\">";
        }

        public void Configure(DocumentationOptions options)
        {
            _options = options;
        }
    }
}
