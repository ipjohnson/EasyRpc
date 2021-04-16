using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace EasyRpc.AspNetCore.Assets
{
    public abstract class BaseAssetProvider
    {
        protected const string GzipString = "gzip";
        protected const string GZString = "gz";
        protected const string BRString = "br";

        protected readonly IFileAssetCache _fileAssetCache;
        protected readonly FileExtensionContentTypeProvider _contentTypeProvider;
        protected string _baseDirectoryPath;

        protected BaseAssetProvider(IFileAssetCache fileAssetCache, FileExtensionContentTypeProvider contentTypeProvider)
        {
            _fileAssetCache = fileAssetCache;
            _contentTypeProvider = contentTypeProvider;
            _baseDirectoryPath = Path.GetDirectoryName(GetType().Assembly.Location);
        }

        public bool NoCache { get; set; }

        public TimeSpan CacheMaxAge { get; set; }

        public bool InMemoryCache { get; set; }

        public bool WriteLastModified { get; set; } = true;

        /// <summary>
        /// Max file cache size in MB
        /// </summary>
        public double MaxFileCacheSize { get; set; } = 5.0;

        public abstract Task ProvideAsset(HttpContext context, string path);

        protected Task ProcessResourceRequest(HttpContext context, string path, string fileLocation)
        {
            var gzipString = $"{fileLocation}.{GZString}";

            if (File.Exists(gzipString))
            {
                return GZippedFile(context, path, gzipString);
            }

            var brString = $"{fileLocation}.{BRString}";

            if (File.Exists(brString))
            {
                return BrotliFile(context, path, brString);
            }

            if (File.Exists(fileLocation))
            {
                return FetchFile(context, path, fileLocation);
            }

            return UnknownFile(context, path, fileLocation);
        }


        protected virtual Task UnknownFile(HttpContext context, string path, string fileLocation)
        {
            return FileNotFound(context);
        }

        protected Task FileNotFound(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            return Task.CompletedTask;
        }

        protected Task FetchFile(HttpContext context, string path, string fileLocation)
        {
            var fileInfo = new FileInfo(fileLocation);

            if (ShouldCache(fileInfo))
            {
                _contentTypeProvider.TryGetContentType(fileLocation, out var contentTypeString);

                var entry = new FileAssetCacheEntry
                {
                    ContentType = contentTypeString,
                    FileName = path,
                    LastModified = fileInfo.LastWriteTime
                };

                if (ShouldCompress(fileLocation, contentTypeString, fileInfo))
                {
                    entry.ContentEncoding = "gzip";
                    entry.FileBytes = GZipCompressFile(fileLocation);
                }
                else
                {
                    entry.FileBytes = File.ReadAllBytes(fileLocation);
                }

                _fileAssetCache.AddEntry(entry);

                return WriteAssetFile(context, entry);
            }

            return StreamFileWithoutCache(context, fileLocation);
        }

        protected byte[] GZipCompressFile(string fileLocation)
        {
            using var memoryStream = new MemoryStream();
            using var compressedStream = new GZipStream(memoryStream, CompressionLevel.Fastest);
            using var openFile = File.OpenRead(fileLocation);

            openFile.CopyTo(compressedStream);

            compressedStream.Flush();

            return memoryStream.ToArray();
        }

        protected Task StreamFileWithoutCache(HttpContext context, string fileLocation, string contentEncoding = null)
        {
            if (_contentTypeProvider.TryGetContentType(fileLocation, out var contentTypeString))
            {
                context.Response.ContentType = contentTypeString;
            }

            if (!string.IsNullOrEmpty(contentEncoding))
            {
                context.Response.Headers.Add("Content-Encoding", contentEncoding);
            }

            return context.Response.SendFileAsync(fileLocation, context.RequestAborted);
        }

        protected bool ShouldCompress(string fileLocation, string contentTypeString, FileInfo fileInfo)
        {
            if (fileInfo.Length < 1000)
            {
                return false;
            }

            var extension = Path.GetExtension(fileLocation);

            switch (extension)
            {
                case "css":
                case "js":
                case "json":
                case "htm":
                case "html":
                case "xml":
                    return true;
            }

            if (contentTypeString == "text/plain" ||
                contentTypeString.Contains("html") ||
                contentTypeString.Contains("json") ||
                contentTypeString.Contains("javascript") ||
                contentTypeString.Contains("xml"))
            {
                return true;
            }

            return false;
        }

        protected bool ShouldCache(FileInfo fileInfo)
        {
            return fileInfo.Length < (MaxFileCacheSize * 1000 * 1024);
        }

        protected Task GZippedFile(HttpContext context, string path, string location)
        {
            _contentTypeProvider.TryGetContentType(path, out var contentType);

            var fileInfo = new FileInfo(location);

            if (ShouldCache(fileInfo))
            {
                var entry = new FileAssetCacheEntry
                {
                    FileName = path,
                    ContentType = contentType,
                    ContentEncoding = GzipString,
                    LastModified = fileInfo.LastWriteTime
                };

                entry.FileBytes = File.ReadAllBytes(location);

                return WriteAssetFile(context, entry);
            }

            return StreamFileWithoutCache(context, location, GzipString);
        }

        protected Task BrotliFile(HttpContext context, string path, string location)
        {
            _contentTypeProvider.TryGetContentType(path, out var contentType);

            var fileInfo = new FileInfo(location);

            if (ShouldCache(fileInfo))
            {
                var entry = new FileAssetCacheEntry
                {
                    FileName = path,
                    ContentType = contentType,
                    ContentEncoding = BRString,
                    LastModified = fileInfo.LastWriteTime
                };

                entry.FileBytes = File.ReadAllBytes(location);

                return WriteAssetFile(context, entry);
            }

            return StreamFileWithoutCache(context, location, BRString);
        }

        protected Task WriteAssetFile(HttpContext httpContext, FileAssetCacheEntry entry)
        {
            if (WriteLastModified)
            {
                httpContext.Response.Headers.Add("LastModified", 
                    entry.LastModified.ToUniversalTime().ToString("R"));
            }

            if (!string.IsNullOrEmpty(entry.ContentEncoding))
            {
                httpContext.Response.Headers.Add("Content-Encoding", entry.ContentEncoding);
            }

            httpContext.Response.ContentType = entry.ContentType;

            return httpContext.Response.Body.WriteAsync(entry.FileBytes, 0, entry.FileBytes.Length);
        }
    }
}
