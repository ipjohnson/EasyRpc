using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface ISwaggerAssetProvider
    {
        IEnumerable<StaticResource> ProvideStaticResources(DocumentationOptions options);
    }

    public class SwaggerStaticResourceProvider : ISwaggerAssetProvider
    {
        private readonly ILogger<SwaggerStaticResourceProvider> _logger;
        private readonly Dictionary<string, StaticResource> _fileEntries = new Dictionary<string, StaticResource>();
        private readonly ValueTask<bool> _false = new ValueTask<bool>(false);
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IStringTokenReplacementService _stringTokenReplacementService;
        private DocumentationOptions _options;

        public SwaggerStaticResourceProvider(IHostEnvironment hostEnvironment, 
            ILogger<SwaggerStaticResourceProvider> logger,
            IStringTokenReplacementService stringTokenReplacementService)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _stringTokenReplacementService = stringTokenReplacementService;
        }

        public void Configure(DocumentationOptions options)
        {
            _options = options;

            _stringTokenReplacementService.Configure(options);

            UnpackAssets();

            ProcessContentPath(_options.ContentPath);
        }

        public IEnumerable<StaticResource> ProvideStaticResources(DocumentationOptions options)
        {
            _options = options;

            _stringTokenReplacementService.Configure(options);

            UnpackAssets();

            ProcessContentPath(_options.ContentPath);

            return _fileEntries.Values;
        }


        protected virtual void UnpackAssets()
        {
            var assembly = typeof(SwaggerStaticResourceProvider).GetTypeInfo().Assembly;

            using var resource = assembly.GetManifestResourceStream("EasyRpc.AspNetCore.Documentation.swagger-ui.zip");

            if (resource == null)
            {
                throw new Exception("Resource is missing, something has gone very very wrong in the build process");
            }

            using var zipFile = new ZipArchive(resource);

            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName == "index.html")
                {
                    ProcessIndexHtmlFile(entry);
                }
                else if (entry.FullName.EndsWith(".br"))
                {
                    ProcessBrFile(entry);
                }
                else
                {
                    ProcessOtherFile(entry);
                }
            }
        }

        private void ProcessOtherFile(ZipArchiveEntry entry)
        {
            var fileEntry = new StaticResource
            {
                Path = entry.FullName
            };

            using var fileStream = entry.Open();
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            fileEntry.Content = memoryStream.ToArray();

            SetContentType(fileEntry);

            _fileEntries.Add(fileEntry.Path, fileEntry);
        }

        private void SetContentType(StaticResource fileEntry)
        {
            if (fileEntry.Path.EndsWith(".png"))
            {
                fileEntry.ContentType = "image/png";
            }
            else if (fileEntry.Path.EndsWith(".css.br"))
            {
                fileEntry.Path = fileEntry.Path.Substring(0, fileEntry.Path.Length - 3);
                fileEntry.ContentType = "text/css";
            }
            else if (fileEntry.Path.EndsWith(".html.br"))
            {
                fileEntry.Path = fileEntry.Path.Substring(0, fileEntry.Path.Length - 3);
                fileEntry.ContentType = "text/html";
            }
            else if (fileEntry.Path.EndsWith(".js.br"))
            {
                fileEntry.Path = fileEntry.Path.Substring(0, fileEntry.Path.Length - 3);
                fileEntry.ContentType = "text/js";
            }
            else if (fileEntry.Path.EndsWith(".js"))
            {
                fileEntry.ContentType = "text/js";
            }
            else if (fileEntry.Path.EndsWith(".html"))
            {
                fileEntry.ContentType = "text/html";
            }
            else if (fileEntry.Path.EndsWith(".css"))
            {
                fileEntry.ContentType = "text/css";
            }
        }

        private void ProcessBrFile(ZipArchiveEntry entry)
        {
            var fileEntry = new StaticResource
            {
                Path = entry.FullName,
                IsBrCompressed = true
            };

            using var fileStream = entry.Open();
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            fileEntry.Content = memoryStream.ToArray();

            SetContentType(fileEntry);
            _fileEntries.Add(fileEntry.Path, fileEntry);
        }

        private void ProcessIndexHtmlFile(ZipArchiveEntry entry)
        {
            var fileEntry = new StaticResource
            {
                Path = entry.FullName
            };

            using var fileStream = entry.Open();
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            fileEntry.Content = memoryStream.ToArray();

            SetContentType(fileEntry);
            TransformIndexFile(fileEntry);
            _fileEntries.Add(fileEntry.Path, fileEntry);
        }

        private void TransformIndexFile(StaticResource fileEntry)
        {
            var indexString = Encoding.UTF8.GetString(fileEntry.Content);

            indexString = _stringTokenReplacementService.ReplaceTokensInString(indexString);

            fileEntry.Content = Encoding.UTF8.GetBytes(indexString);
        }
        
        private void ProcessContentPath(string path)
        {            
            var contents = _hostEnvironment.ContentRootFileProvider.GetDirectoryContents(path);
            
            if (contents.Exists)
            {
                foreach (var fileInfo in contents)
                {
                    if (fileInfo.IsDirectory)
                    {
                    }
                    else
                    {
                        try
                        {
                            var fileEntry = new StaticResource
                            {
                                Path = fileInfo.Name
                            };

                            SetContentType(fileEntry);

                            if (fileEntry.ContentType.StartsWith("text/"))
                            {
                                fileEntry.IsBrCompressed = true;
                                fileEntry.Content = ReadAndCompressFile(fileInfo);
                            }
                            else
                            {
                                fileEntry.Content = ReadFile(fileInfo);
                            }

                            _fileEntries[fileEntry.Path] = fileEntry;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Exception thrown while processing asset file {0}", fileInfo.Name);
                        }
                    }
                }
            }
        }

        private byte[] ReadFile(IFileInfo fileInfo)
        {
            using var memoryStream = new MemoryStream();

            fileInfo.CreateReadStream().CopyTo(memoryStream);
            
            return memoryStream.ToArray();
        }

        private byte[] ReadAndCompressFile(IFileInfo fileInfo)
        {
            using var memoryStream = new MemoryStream();
            using var brStream = new BrotliStream(memoryStream, CompressionLevel.Optimal);

            fileInfo.CreateReadStream().CopyTo(brStream);

            brStream.Flush();

            return memoryStream.ToArray();
        }
    }
}
