using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    public class DocumentationConfiguration
    {
        /// <summary>
        /// Left menu width in rem
        /// </summary>
        public double? MenuWidth { get; set; }

        /// <summary>
        /// Menu title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Custom css
        /// </summary>
        public string CustomCss { get; set; }

        /// <summary>
        /// version string appended to resource urls. defaults to assembly verison
        /// </summary>
        public string VersionString { get; set; }
    }
}
