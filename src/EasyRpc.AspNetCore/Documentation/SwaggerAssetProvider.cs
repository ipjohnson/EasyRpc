using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface ISwaggerAssetProvider
    {
        void Configure();

        ValueTask<bool> ProvideAsset(HttpContext context);
    }

    public class SwaggerAssetProvider : ISwaggerAssetProvider
    {
        private readonly Dictionary<string, FileEntry> _fileEntries = new Dictionary<string, FileEntry>();
        private readonly string _pathBase = "/swagger/";
        private readonly ValueTask<bool> _false = new ValueTask<bool>(false);
        
        public void Configure()
        {
            UnpackAssets();
        }

        public ValueTask<bool> ProvideAsset(HttpContext context)
        {
            var fileName = context.Request.Path.Value.Substring(_pathBase.Length);

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
            else if (fileEntry.FileName.EndsWith(".html"))
            {
                fileEntry.ContentType = "text/html";
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

    }
}
