using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.AspNetCore
{
    public class DefaultMethodConfiguration
    {
        

        /// <summary>
        /// GET has body by default
        /// </summary>
        public bool GetHasResponseBody { get; set; } = true;

        /// <summary>
        /// GET default status code of 200
        /// </summary>
        public int GetSuccessStatusCode { get; set; } = (int) HttpStatusCode.OK;

        /// <summary>
        /// GET has body by default
        /// </summary>
        public bool HeadHasResponseBody { get; set; } = true;

        /// <summary>
        /// GET default status code of 200
        /// </summary>
        public int HeadSuccessStatusCode { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// GET has body by default
        /// </summary>
        public bool PostHasResponseBody { get; set; } = true;

        /// <summary>
        /// GET default status code of 200
        /// </summary>
        public int PostSuccessStatusCode { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Delete has no body by default unless specified
        /// </summary>
        public bool DeleteHasResponseBody { get; set; } = false;

        /// <summary>
        /// Delete returns status of no content when successful
        /// </summary>
        public int DeleteSuccessStatusCode { get; set; } = (int)HttpStatusCode.NoContent;

        /// <summary>
        /// PUT default is no response body
        /// </summary>
        public bool PutHasResponseBody { get; set; } = false;

        /// <summary>
        /// PUT default status is no content (204)
        /// </summary>
        public int PutSuccessStatusCode { get; set; } = (int)HttpStatusCode.NoContent;

        /// <summary>
        /// PATCH default no response body
        /// </summary>
        public bool PatchHasResponseBody { get; set; } = false;

        /// <summary>
        /// PATCH default status is no content (204)
        /// </summary>
        public int PatchSuccessStatusCode { get; set; } = (int)HttpStatusCode.NoContent;

        /// <summary>
        /// GET has body by default
        /// </summary>
        public bool OptionsHasResponseBody { get; set; } = false;

        /// <summary>
        /// GET default status code of 200
        /// </summary>
        public int OptionsSuccessStatusCode { get; set; } = (int)HttpStatusCode.NoContent;

        /// <summary>
        /// UNKNOWN methods get response body true
        /// </summary>
        public Func<string, bool> UnknownMethodResponseBody { get; set; } = s => true;

        /// <summary>
        /// UNKNOWN methods get status code 200
        /// </summary>
        public Func<string, int> UnknownMethodStatusCode { get; set; } = s => 200;
    }
}
