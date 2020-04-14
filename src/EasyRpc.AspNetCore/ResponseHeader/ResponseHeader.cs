using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.ResponseHeader
{
    public class ResponseHeader : IResponseHeader
    {
        private readonly string _headerName;
        private readonly string _headerValue;

        public ResponseHeader(string headerName, string headerValue)
        {
            _headerName = headerName;
            _headerValue = headerValue;
        }

        public void ApplyHeader(RequestExecutionContext context, IHeaderDictionary headers)
        {
            headers[_headerName] = _headerValue;
        }
    }
}
