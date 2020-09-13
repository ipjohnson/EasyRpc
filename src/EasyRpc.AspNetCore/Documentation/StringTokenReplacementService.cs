using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IStringTokenReplacementService
    {
        string ReplaceTokensInString(string tokenString);
        void Configure(DocumentationOptions options);
    }

    public class StringTokenReplacementService : IStringTokenReplacementService
    {
        private ITokenValueProvider _tokenValueProvider;

        public const string StartReplacement = "^*";
        public const string EndReplacement = "*^";

        public StringTokenReplacementService(ITokenValueProvider tokenValueProvider)
        {
            _tokenValueProvider = tokenValueProvider;
        }

        public string ReplaceTokensInString(string tokenString)
        {
            int index = 0;
            int previousIndex = 0;
            int lastIndex;
            var length = tokenString.Length;
            StringBuilder stringBuilder = new StringBuilder(tokenString.Length + 100);

            while (index < length)
            {
                index = tokenString.IndexOf(StartReplacement, index, StringComparison.CurrentCulture);

                if (index >= 0)
                {
                    lastIndex = tokenString.IndexOf(EndReplacement, index, StringComparison.CurrentCulture);

                    if (lastIndex > 0)
                    {
                        stringBuilder.Append(tokenString.Substring(previousIndex, index - previousIndex));
                        var token = tokenString.Substring(index + 2, lastIndex - (index + 2));
                        stringBuilder.Append(_tokenValueProvider.ProvideToken(token));

                        previousIndex = lastIndex + 2;
                        index = lastIndex;
                    }
                }
                else
                {
                    break;
                }
            }

            stringBuilder.Append(tokenString.Substring(previousIndex));

            return stringBuilder.ToString();
        }
    

        public void Configure(DocumentationOptions options)
        {
            _tokenValueProvider.Configure(options);
        }
    }
}
