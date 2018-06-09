using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Documentation;
using Microsoft.Extensions.Options;

namespace EasyRpc.TestApp.Utilities
{
    public class CustomWebAssetProvider : WebAssetProvider
    {
        public CustomWebAssetProvider(IMethodPackageMetadataCreator methodPackageMetadataCreator, IVariableReplacementService variableReplacementService, IOptions<RpcServiceConfiguration> configuration) : 
            base(methodPackageMetadataCreator, variableReplacementService, configuration)
        {
            var personalPath =
                @"C:\Users\ian\Source\Repos\EasyRpc\src\EasyRpc.AspNetCore\Documentation\web-assets\";

            if (Directory.Exists(personalPath))
            {
                ExtractedAssetPath =personalPath;
            }
        }
    }
}
