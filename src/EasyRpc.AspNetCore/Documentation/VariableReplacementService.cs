using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IVariableReplacementService
    {
        void Configure(EndPointConfiguration path);

        void ReplaceVariables(HttpContext context, StreamWriter outputStream, string tokenString);
    }

    public class VariableReplacementService : IVariableReplacementService
    {
        private IReplacementValueProvider _replacementValueProvider;

        public const string StartReplacement = "^*";
        public const string EndReplacement = "*^";

        public VariableReplacementService(IReplacementValueProvider replacementValueProvider)
        {
            _replacementValueProvider = replacementValueProvider;
        }

        public void Configure(EndPointConfiguration configuration)
        {
            _replacementValueProvider.ServicePath(configuration);
        }

        public void ReplaceVariables(HttpContext context,StreamWriter outputStream, string tokenString)
        {
            int index = 0;
            int previousIndex = 0;
            int lastIndex = 0;
            var length = tokenString.Length;

            while (index < length)
            {
                index = tokenString.IndexOf(StartReplacement, index, StringComparison.CurrentCulture);

                if (index >= 0)
                {
                    lastIndex = tokenString.IndexOf(EndReplacement, index, StringComparison.CurrentCulture);

                    if (lastIndex > 0)
                    {
                        outputStream.Write(tokenString.Substring(previousIndex, index - previousIndex));
                        var token = tokenString.Substring(index + 2, lastIndex - (index + 2));
                        outputStream.Write(_replacementValueProvider.GetReplacementValue(context, token));

                        previousIndex = lastIndex + 2;
                        index = lastIndex;
                    }
                }
                else
                {
                    break;
                }
            }

            outputStream.Write(tokenString.Substring(previousIndex));
        }
    }
}
