using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public class StaticResourceEndPointProvider
    {
        private ISwaggerStaticResourceProvider _swaggerAssetProvider;

        public StaticResourceEndPointProvider(ISwaggerStaticResourceProvider swaggerAssetProvider)
        {
            _swaggerAssetProvider = swaggerAssetProvider;
        }
    }
}
