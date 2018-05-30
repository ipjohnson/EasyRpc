using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Documentation;

namespace EasyRpc.TestApp.Utilities
{
    public class CustomWebAssetProvider : WebAssetProvider
    {
        public CustomWebAssetProvider(IMethodPackageMetadataCreator methodPackageMetadataCreator) : base(methodPackageMetadataCreator)
        {
            ExtractedAssetPath = @"C:\Users\ian\Source\Repos\EasyRpc\src\EasyRpc.AspNetCore\Documentation\web-assets\";
        }
    }
}
