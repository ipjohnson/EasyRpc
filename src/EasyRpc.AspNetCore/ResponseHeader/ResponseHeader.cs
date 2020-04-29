using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.ResponseHeader
{
    /// <inheritdoc />
    public class ResponseHeader : IResponseHeader
    {
        private readonly string _headerName;
        private readonly string _headerValue;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        public ResponseHeader(string headerName, string headerValue)
        {
            _headerName = headerName;
            _headerValue = headerValue;
        }

        /// <inheritdoc />
        public void ApplyHeader(RequestExecutionContext context, IHeaderDictionary headers)
        {
            headers[_headerName] = _headerValue;
        }
    }
}
