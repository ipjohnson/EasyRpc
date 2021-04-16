using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace EasyRpc.AspNetCore.Assets
{
    public class DirectoryAssetProvider : BaseAssetProvider
    {
        private readonly string _resourceDirectory;
        private string _fallback;
        
        public DirectoryAssetProvider(IFileAssetCache fileAssetCache, FileExtensionContentTypeProvider contentTypeProvider, string resourceDirectory, string fallback) : base(fileAssetCache, contentTypeProvider)
        {
            _resourceDirectory = resourceDirectory;
            _fallback = fallback;
        }
        
        public override Task ProvideAsset(HttpContext context, string path)
        {
            var entry = _fileAssetCache.LocateEntry(path);

            if (entry != null)
            {
                return WriteAssetFile(context, entry);
            }

            var fileLocation = string.IsNullOrEmpty(_resourceDirectory) ?
                Path.Combine(_baseDirectoryPath, path) :
                Path.Combine(_baseDirectoryPath, _resourceDirectory, path);

            return ProcessResourceRequest(context, path, fileLocation);
        }

        protected override Task UnknownFile(HttpContext context, string path, string fileLocation)
        {
            if (fileLocation == _fallback)
            {
                return FileNotFound(context);
            }

            if (_fallback != null)
            {
                return ProvideAsset(context, _fallback);
            }

            return FileNotFound(context);
        }
    }
}
