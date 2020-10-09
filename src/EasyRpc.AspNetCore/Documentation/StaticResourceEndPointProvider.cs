using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public class StaticResourceEndPointProvider
    {
        private ISwaggerAssetProvider _swaggerAssetProvider;

        public StaticResourceEndPointProvider(ISwaggerAssetProvider swaggerAssetProvider)
        {
            _swaggerAssetProvider = swaggerAssetProvider;
        }
    }
}
