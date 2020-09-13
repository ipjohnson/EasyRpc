using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IStringTokenReplacementService
    {
        string ReplaceTokensInString(string originalString);
        void Configure(DocumentationOptions options);
    }

    public class StringTokenReplacementService : IStringTokenReplacementService
    {
        private ITokenValueProvider _tokenValueProvider;

        public StringTokenReplacementService(ITokenValueProvider tokenValueProvider)
        {
            _tokenValueProvider = tokenValueProvider;
        }

        public string ReplaceTokensInString(string originalString)
        {
            throw new NotImplementedException();
        }

        public void Configure(DocumentationOptions options)
        {
            _tokenValueProvider.Configure(options);
        }
    }
}
