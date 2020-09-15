using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Encoding
{
    /// <summary>
    /// Methods that have been attributed to compress 
    /// </summary>
    public class CompressAttribute : Attribute
    {
        /// <summary>
        /// Compress requests
        /// </summary>
        public bool Request { get; set; }

        /// <summary>
        /// Min number of instances in list before compress
        /// </summary>
        public int? Min { get; set; }
    }
}
