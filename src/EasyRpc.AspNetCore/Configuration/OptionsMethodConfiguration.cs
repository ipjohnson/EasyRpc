using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.ResponseHeader;

namespace EasyRpc.AspNetCore.Configuration
{
    public class OptionsMethodConfiguration
    {
        public bool Enabled { get; set; }

        public string AllowHeader { get; set; } = "Allow";

        public List<string> SupportedMethods { get; set; } = new List<string>{ "GET", "POST", "DELETE", "PATCH", "PUT", "OPTIONS" };

        public List<IResponseHeader> Headers { get; set; } = new List<IResponseHeader>();
    }
}
