using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using EasyRpc.AspNetCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Documentation
{
    public class BundleConfigFile
    {
        public List<BundleConfig> Bundles { get; set; }
    }

    public class BundleConfig
    {
        public string Name { get; set; }

        public List<string> Files { get; set; }
    }

    public interface IWebAssetProvider
    {
        void Configure(EndPointConfiguration configuration);

        Task<bool> ProcessRequest(HttpContext context);
    }

    public class WebAssetProvider : IWebAssetProvider
    {
        private IMethodPackageMetadataCreator _methodPackageMetadataCreator;
        private int _routeLength;
        private IVariableReplacementService _variableReplacementService;
        private IOptions<RpcServiceConfiguration> _configuration;
        protected string ExtractedAssetPath;


        public WebAssetProvider(IMethodPackageMetadataCreator methodPackageMetadataCreator,
            IVariableReplacementService variableReplacementService,
            IOptions<RpcServiceConfiguration> configuration)
        {
            _methodPackageMetadataCreator = methodPackageMetadataCreator;
            _variableReplacementService = variableReplacementService;
            _configuration = configuration;
        }

        public async Task<bool> ProcessRequest(HttpContext context)
        {
            var assetPath = context.Request.Path.Value.Substring(_routeLength);
            
            if (assetPath.Length == 0)
            {
                assetPath = "templates/main.html";
            }
            
            try
            {
                var shouldReplaceVar = ShouldReplaceVariables(assetPath);
                var file = Path.Combine(ExtractedAssetPath, assetPath);
                
                if (File.Exists(file))
                {
                    if (_configuration.Value != null && 
                        _configuration.Value.SupportResponseCompression &&
                        !shouldReplaceVar)
                    {
                        if (File.Exists(file + ".gz"))
                        {
                            file = file + ".gz";
                            context.Response.Headers.Add("Content-Encoding", "gzip");
                        }
                    }

                    var bytes = File.ReadAllBytes(file);
                    context.Response.StatusCode = StatusCodes.Status200OK;

                    var lastPeriod = assetPath.LastIndexOf('.');
                    if (lastPeriod > 0)
                    {
                        var extension = assetPath.Substring(lastPeriod + 1);

                        switch (extension)
                        {
                            case "css":
                                context.Response.ContentType = "text/css";
                                break;
                            case "html":
                                context.Response.ContentType = "text/html";
                                break;
                            case "js":
                                context.Response.ContentType = "text/javascript";
                                break;
                            case "ico":
                                context.Response.ContentType = "image/x-icon";
                                break;
                        }
                    }

                    if (ShouldReplaceVariables(assetPath))
                    {
                        using (var streamWriter = new StreamWriter(context.Response.Body))
                        {
                            _variableReplacementService.ReplaceVariables(context, streamWriter,
                                Encoding.UTF8.GetString(bytes));
                        }
                    }
                    else
                    {
                        context.Response.Body.Write(bytes, 0, bytes.Length);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("File path: " + e);
            }

            if (assetPath == "interface-definition")
            {
                await _methodPackageMetadataCreator.CreatePackage(context);

                return true;
            }

            return false;
        }

        protected virtual bool ShouldReplaceVariables(string assetName) => assetName == "templates/main.html";

        public void Configure(EndPointConfiguration configuration)
        {
            _methodPackageMetadataCreator.SetConfiguration(configuration);
            _variableReplacementService.Configure(configuration);
            _routeLength = configuration.Route.Length;
            ProcessAssets(configuration);
        }

        private void ProcessAssets(EndPointConfiguration configuration)
        {
            SetupExtractAssetPath();

            ExtractZipFile();

            WriteCustomCss(configuration);

            CreateBundles();
        }

        private void WriteCustomCss(EndPointConfiguration configuration)
        {
            var cssPath = Path.Combine(ExtractedAssetPath, "css", "custom.css");

            using (var customCssFile = File.Open(cssPath, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(customCssFile))
                {
                    var width = configuration.DocumentationConfiguration.MenuWidth;

                    if (width.HasValue)
                    {
                        streamWriter.WriteLine($".docs-sidebar .docs-nav{{width:{width}rem}}");
                        streamWriter.WriteLine($".off-canvas .off-canvas-sidebar{{width:{width}rem}}");
                    }

                    streamWriter.Write(configuration.DocumentationConfiguration.CustomCss);
                }
            }
        }

        private void ExtractZipFile()
        {
            var assembly = typeof(WebAssetProvider).GetTypeInfo().Assembly;

            using (var resource = assembly.GetManifestResourceStream("EasyRpc.AspNetCore.Documentation.web-assets.zip"))
            {
                using (var zipFile = new ZipArchive(resource))
                {
                    foreach (var entry in zipFile.Entries)
                    {
                        var fullName = entry.FullName;
                        var directory = fullName.Substring(0, fullName.Length - entry.Name.Length);

                        directory = directory.Replace('\\', Path.DirectorySeparatorChar);
                        directory = Path.Combine(ExtractedAssetPath, directory);

                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        var filePath = Path.Combine(directory, entry.Name);

                        using (var zipStream = entry.Open())
                        {
                            using (var file = File.Open(filePath, FileMode.Create))
                            {
                                zipStream.CopyTo(file);
                            }
                        }
                    }
                }
            }
        }

        private void CreateBundles()
        {
            var bundleConfigFilePath = Path.Combine(ExtractedAssetPath, "bundle", "bundle-config.json");

            var configString = File.ReadAllText(bundleConfigFilePath);

            var bundleConfigFile = JsonConvert.DeserializeObject<BundleConfigFile>(configString);

            foreach (var bundle in bundleConfigFile.Bundles)
            {
                var bundleFilePath = Path.Combine(ExtractedAssetPath, bundle.Name);

                using (var memoryStream = new MemoryStream())
                {
                    using (var stream = new StreamWriter(memoryStream))
                    {
                        foreach (var file in bundle.Files)
                        {
                            var filePath = Path.Combine(ExtractedAssetPath, file);

                            var fileString = File.ReadAllText(filePath);

                            stream.Write(fileString);

                            if (!fileString.EndsWith("\n"))
                            {
                                stream.Write('\n');
                            }
                        }
                    }

                    var bytes = memoryStream.ToArray();

                    using (var bundleFile = File.Open(bundleFilePath, FileMode.Create))
                    {
                        bundleFile.Write(bytes, 0, bytes.Length);
                    }

                    if (_configuration.Value?.SupportResponseCompression ?? false)
                    {
                        using (var bundleFile = File.Open(bundleFilePath + ".gz", FileMode.Create))
                        {
                            using (var gzipStream = new GZipStream(bundleFile, CompressionLevel.Optimal))
                            {
                                gzipStream.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }
                }
            }
        }

        private void SetupExtractAssetPath()
        {
            ExtractedAssetPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(ExtractedAssetPath);
        }

    }
}
