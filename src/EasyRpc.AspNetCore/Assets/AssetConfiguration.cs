using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Assets
{
    public class AssetConfiguration : IAssetsConfiguration, IConfigurationInformationProvider
    {
        private string _fallback;
        private bool _noCache;
        private bool _inMemoryCache = true;
        private string _urlPath;
        private string _fileSystemPath;
        private ICurrentApiInformation _currentApiInformation;
        private TimeSpan _cacheMaxAge;

        public AssetConfiguration(string urlPath, string fileSystemPath, ICurrentApiInformation currentApiInformation)
        {
            _urlPath = urlPath;
            _fileSystemPath = fileSystemPath;
            _currentApiInformation = currentApiInformation;
        }

        public IAssetsConfiguration Authorize(string role = null, string policy = null)
        {
            return this;
        }

        public IAssetsConfiguration HttpMaxCacheAge(TimeSpan timeSpan)
        {
            _cacheMaxAge = timeSpan;

            return this;
        }

        public IAssetsConfiguration HttpSetCache(Action<HttpContext, string> cacheAction)
        {
            return this;
        }

        public IAssetsConfiguration HttpNoCache()
        {
            _noCache = true;

            return this;
        }

        public IAssetsConfiguration InMemoryCache(bool enable = true)
        {
            _inMemoryCache = enable;

            return this;
        }

        public IAssetsConfiguration SetLastModified(bool lastModified = true)
        {
            return this;
        }

        public IAssetsConfiguration SetETag(bool etag = true)
        {
            return this;
        }

        public IAssetsConfiguration Fallback(string fallback)
        {
            _fallback = fallback;

            return this;
        }

        public IAssetsConfiguration MaxFileCacheSize(double fileSize)
        {
            return this;
        }

        public IAssetsConfiguration Where(Func<string, bool> @where)
        {
            return this;
        }

        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            BaseAssetProvider assetProvider = null;
            Delegate handler;

            if (File.Exists(_fileSystemPath))
            {
                assetProvider = new FileAssetProvider(
                    _currentApiInformation.ServiceProvider.GetService<IFileAssetCache>(),
                    _currentApiInformation.ServiceProvider.GetService<FileExtensionContentTypeProvider>(),
                    _fileSystemPath
                    );
                
                handler = (Func<HttpContext, Task>)(context => assetProvider.ProvideAsset(context, _urlPath));

                var delegateConfig = new DelegateInstanceConfiguration(_currentApiInformation, handler)
                {
                    Method = HttpMethods.Get,
                    Path = _urlPath
                };

                service.ExposeDelegate(_currentApiInformation, delegateConfig, handler);
            }
            else if (Directory.Exists(_fileSystemPath))
            {
                assetProvider = new DirectoryAssetProvider(
                    _currentApiInformation.ServiceProvider.GetService<IFileAssetCache>(),
                    _currentApiInformation.ServiceProvider.GetService<FileExtensionContentTypeProvider>(),
                    _fileSystemPath,
                    _fallback);
                
                handler = new Func<HttpContext,string,Task>((context, filePath) => assetProvider.ProvideAsset(context, filePath));
                
                var delegateConfig = new DelegateInstanceConfiguration(_currentApiInformation, handler)
                {
                    Method = HttpMethods.Get,
                    Path = _urlPath + "{filePath}"
                };

                service.ExposeDelegate(_currentApiInformation, delegateConfig, handler);
            }
            else
            {
                throw new Exception($"Could not find asset path {_fileSystemPath}");
            }

            assetProvider.CacheMaxAge = _cacheMaxAge;
            assetProvider.NoCache = _noCache;
            assetProvider.InMemoryCache = _inMemoryCache;

        }
    }
}
