using System.Collections.Generic;

namespace EasyRpc.AspNetCore.Documentation
{
    public class DocumentationUrlProvider
    {
        public virtual IEnumerable<string> CssUrls { get; set; }

        public virtual IEnumerable<string> JavascriptUrls { get; set; }
    }
}
