using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace EasyRpc.AspNetCore.Assets
{
    public class FileAssetProvider : BaseAssetProvider
    {
        private readonly string _resourceLocation;

        public FileAssetProvider(IFileAssetCache fileAssetCache, FileExtensionContentTypeProvider contentTypeProvider, string resourceLocation) : base(fileAssetCache, contentTypeProvider)
        {
            _resourceLocation = resourceLocation;
        }
        
        public override Task ProvideAsset(HttpContext context, string path)
        {
            var entry = _fileAssetCache.LocateEntry(path);

            if (entry != null)
            {
                return WriteAssetFile(context, entry);
            }

            return ProcessResourceRequest(context, path, Path.Combine(_baseDirectoryPath, _resourceLocation));
        }
    }
}
