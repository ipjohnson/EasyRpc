using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
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
        protected string ExtractedAssetPath;

        public WebAssetProvider(IMethodPackageMetadataCreator methodPackageMetadataCreator, IVariableReplacementService variableReplacementService)
        {
            _methodPackageMetadataCreator = methodPackageMetadataCreator;
            _variableReplacementService = variableReplacementService;
            ProcessAssets();
        }

        private void ProcessAssets()
        {
            SetupExtractAssetPath();

            ExtractZipFile();

            CreateBundles();
        }

        private void ExtractZipFile()
        {
            var assembly = typeof(WebAssetProvider).GetTypeInfo().Assembly;

            using (var resource = assembly.GetManifestResourceStream("EasyRpc.AspNetCore.Documentation.web-assets.zip"))
            {
                using (var zipFile = new ZipArchive(resource))
                {
                    zipFile.ExtractToDirectory(ExtractedAssetPath);
                }
            }
        }

        private void CreateBundles()
        {
            var bundleConfigFilePath = Path.Combine(ExtractedAssetPath, "bundle", "bundle-config.json");

            if (!File.Exists(bundleConfigFilePath))
            {
                foreach (var directory in Directory.EnumerateDirectories(ExtractedAssetPath))
                {
                    Console.WriteLine("Directory: " + directory);
                    foreach (var file in Directory.EnumerateFiles(directory))
                    {
                        Console.WriteLine("File: " + file);
                    }
                }
            }

            var configString = File.ReadAllText(bundleConfigFilePath);

            var bundleConfigFile = JsonConvert.DeserializeObject<BundleConfigFile>(configString);

            foreach (var bundle in bundleConfigFile.Bundles)
            {
                var bundleFilePath = Path.Combine(ExtractedAssetPath, bundle.Name);

                using (var bundleFile = File.Open(bundleFilePath, FileMode.Create))
                {
                    using (var stream = new StreamWriter(bundleFile))
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
                }
            }
        }

        private void SetupExtractAssetPath()
        {
            ExtractedAssetPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(ExtractedAssetPath);
        }

        public async Task<bool> ProcessRequest(HttpContext context)
        {
            var assetPath = context.Request.Path.Value.Substring(_routeLength);
            
            Console.WriteLine("Asset path: " + assetPath);

            if (assetPath.Length == 0)
            {
                assetPath = "templates/main.html";
            }

            try
            {
                var file = Path.Combine(ExtractedAssetPath, assetPath);

                Console.WriteLine("File path: " + file);
                if (File.Exists(file))
                {
                    Console.WriteLine("File exists path: " + file);
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
            _variableReplacementService.Configure(configuration.Route);
            _routeLength = configuration.Route.Length;
        }
    }
}
