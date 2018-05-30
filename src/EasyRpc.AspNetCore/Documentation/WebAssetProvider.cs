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
        private Dictionary<string, Tuple<string, byte[]>> _assets = new Dictionary<string, Tuple<string, byte[]>>();
        private int _routeLength;
        protected string ExtractedAssetPath;

        public WebAssetProvider(IMethodPackageMetadataCreator methodPackageMetadataCreator)
        {
            _methodPackageMetadataCreator = methodPackageMetadataCreator;
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

            if (assetPath.Length == 0)
            {
                assetPath = "templates/main.html";
            }

            try
            {
                var file = Path.Combine(ExtractedAssetPath, assetPath);

                var bytes = File.ReadAllBytes(file);
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.Body.Write(bytes, 0, bytes.Length);

                return true;
            }
            catch (Exception e)
            {

            }

            if (assetPath == "interface-definition")
            {
                await _methodPackageMetadataCreator.CreatePackage(context);

                return true;
            }

            return false;
        }


        public void Configure(EndPointConfiguration configuration)
        {
            _methodPackageMetadataCreator.SetConfiguration(configuration);

            _routeLength = configuration.Route.Length;
        }
    }
}
