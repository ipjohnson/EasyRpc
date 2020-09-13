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
        public string ProvideToken(string token)
        {
            throw new NotImplementedException();
        }

        public void Configure(DocumentationOptions options)
        {

        }
    }
}
