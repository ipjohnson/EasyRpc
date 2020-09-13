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
        void Configure(DocumentationOptions options);

        ValueTask<bool> ProvideAsset(HttpContext context);
    }

    public class SwaggerAssetProvider : ISwaggerAssetProvider
    {
        private readonly ILogger<SwaggerAssetProvider> _logger;
        private readonly Dictionary<string, FileEntry> _fileEntries = new Dictionary<string, FileEntry>();
        private readonly ValueTask<bool> _false = new ValueTask<bool>(false);
        private readonly IHostEnvironment _hostEnvironment;
        private DocumentationOptions _options;

        public SwaggerAssetProvider(IHostEnvironment hostEnvironment, ILogger<SwaggerAssetProvider> logger)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public void Configure(DocumentationOptions options)
        {
            _options = options;
            UnpackAssets();

            ProcessContentPath(_options.ContentPath);
        }

        public ValueTask<bool> ProvideAsset(HttpContext context)
        {
            var fileName = context.Request.Path.Value.Substring(_options.UIBasePath.Length);

            if (_fileEntries.TryGetValue(fileName, out var fileEntry))
            {
                return SendFile(context, fileEntry);
            }

            return _false;
        }

        protected virtual async ValueTask<bool> SendFile(HttpContext context, FileEntry fileEntry)
        {
            context.Response.ContentType = fileEntry.ContentType;

            if (!fileEntry.IsCompressed || CompressionEnabled(context))
            {
                context.Response.ContentLength = fileEntry.Contents.Length;

                if (fileEntry.IsCompressed)
                {
                    context.Response.Headers.TryAdd("content-encoding", "br");
                }

                await context.Response.Body.WriteAsync(fileEntry.Contents, context.RequestAborted);
            }
            else
            {
                using var decompress = new BrotliStream(new MemoryStream(fileEntry.Contents), CompressionMode.Decompress);

                await decompress.CopyToAsync(context.Response.Body);
            }

            return true;
        }

        private bool CompressionEnabled(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("accept-encoding", out var encoding))
            {
                return encoding.ToString().Contains("br");
            }

            return false;
        }


        protected virtual void UnpackAssets()
        {
            var assembly = typeof(SwaggerAssetProvider).GetTypeInfo().Assembly;

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
            var fileEntry = new FileEntry
            {
                FileName = entry.FullName,
                IsCompressed = false
            };

            using var fileStream = entry.Open();
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            fileEntry.Contents = memoryStream.ToArray();

            SetContentType(fileEntry);

            _fileEntries.Add(fileEntry.FileName, fileEntry);
        }

        private void SetContentType(FileEntry fileEntry)
        {
            if (fileEntry.FileName.EndsWith(".png"))
            {
                fileEntry.ContentType = "image/png";
            }
            else if (fileEntry.FileName.EndsWith(".css.br"))
            {
                fileEntry.FileName = fileEntry.FileName.Substring(0, fileEntry.FileName.Length - 3);
                fileEntry.ContentType = "text/css";
            }
            else if (fileEntry.FileName.EndsWith(".html.br"))
            {
                fileEntry.FileName = fileEntry.FileName.Substring(0, fileEntry.FileName.Length - 3);
                fileEntry.ContentType = "text/html";
            }
            else if (fileEntry.FileName.EndsWith(".js.br"))
            {
                fileEntry.FileName = fileEntry.FileName.Substring(0, fileEntry.FileName.Length - 3);
                fileEntry.ContentType = "text/js";
            }
            else if (fileEntry.FileName.EndsWith(".js"))
            {
                fileEntry.ContentType = "text/js";
            }
            else if (fileEntry.FileName.EndsWith(".html"))
            {
                fileEntry.ContentType = "text/html";
            }
            else if (fileEntry.FileName.EndsWith(".css"))
            {
                fileEntry.ContentType = "text/css";
            }
        }

        private void ProcessBrFile(ZipArchiveEntry entry)
        {
            var fileEntry = new FileEntry
            {
                FileName = entry.FullName,
                IsCompressed = true
            };

            using var fileStream = entry.Open();
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            fileEntry.Contents = memoryStream.ToArray();

            SetContentType(fileEntry);
            _fileEntries.Add(fileEntry.FileName, fileEntry);
        }

        private void ProcessIndexHtmlFile(ZipArchiveEntry entry)
        {
            var fileEntry = new FileEntry
            {
                FileName = entry.FullName,
                IsCompressed = false
            };

            using var fileStream = entry.Open();
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            fileEntry.Contents = memoryStream.ToArray();

            SetContentType(fileEntry);
            TransformIndexFile(fileEntry);
            _fileEntries.Add(fileEntry.FileName, fileEntry);
        }

        private void TransformIndexFile(FileEntry fileEntry)
        {

        }

        public class FileEntry
        {
            public string FileName { get; set; }

            public bool IsCompressed { get; set; }

            public string ContentType { get; set; }

            public byte[] Contents { get; set; }
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
                            var fileEntry = new FileEntry
                            {
                                FileName = fileInfo.Name,
                                IsCompressed = false
                            };

                            SetContentType(fileEntry);

                            if (fileEntry.ContentType.StartsWith("text/"))
                            {
                                fileEntry.IsCompressed = true;
                                fileEntry.Contents = ReadAndCompressFile(fileInfo);
                            }
                            else
                            {
                                fileEntry.Contents = ReadFile(fileInfo);
                            }

                            _fileEntries[fileEntry.FileName] = fileEntry;
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
