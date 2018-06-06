using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Documentation;

namespace EasyRpc.TestApp.Utilities
{
    public class CustomWebAssetProvider : WebAssetProvider
    {
        public CustomWebAssetProvider(IMethodPackageMetadataCreator methodPackageMetadataCreator, IVariableReplacementService variableReplacementService, ITypeDefinitionPackageProvider typeDefinitionPackageProvider) : 
            base(methodPackageMetadataCreator, variableReplacementService, typeDefinitionPackageProvider)
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
