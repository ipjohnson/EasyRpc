using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.AspNetCore
{
    public class DefaultMethodConfiguration
    {
        /// <summary>
        /// Delete has no body by default unless specified
        /// </summary>
        public bool DeleteHasResponseBody { get; set; } = false;

        /// <summary>
        /// Delete returns status of no content when successful
        /// </summary>
        public int DeleteSuccessStatusCode { get; set; } = (int)HttpStatusCode.NoContent;


    }
}
