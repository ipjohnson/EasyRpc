using System.IO;
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
                @"C:\Users\ian\Source\Repos\EasyRpc-3.0\src\EasyRpc.AspNetCore\Documentation\web-assets\";

            if (Directory.Exists(personalPath))
            {
                ExtractedAssetPath = personalPath;
            }
        }
    }
}
