using EasyRpc.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Assets
{
    public class FileAssetDynamicCompressionTest : BaseAssetTest
    {
        private int _jackCount = 10000;

        #region ResourceSetup

        public FileAssetDynamicCompressionTest()
        {
            SetupResource();
        }

        private void SetupResource()
        {
            var resourceString = (ReadOnlySpan<byte>)Encoding.UTF8.GetBytes("All work and no play makes Jack a dull boy\n");

            using var resourceFile = AddFile("resource.txt");

            for (var i = 0; i < _jackCount; i++)
            {
                resourceFile.Write(resourceString);
            }
        }

        #endregion
        
        #region Tests

        [Fact]
        public async Task Assets_FileDynamicCompressionTest()
        {
            AcceptEncoding = "gzip";

            var response = await Get("/jack.txt");

            Assert.True(response.IsSuccessStatusCode);
            
            Assert.Equal("gzip", response.Content.Headers.ContentEncoding.ToString());

            var responseStream = await response.Content.ReadAsStreamAsync();

            using var gzipStream = new GZipStream(responseStream,CompressionMode.Decompress);

            using var streamReader = new StreamReader(gzipStream);

            var fullText = streamReader.ReadToEnd();

            var totalJacks = Regex.Matches(fullText, "Jack").Count;

            Assert.Equal(_jackCount, totalJacks);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Assets("/jack.txt", "resource.txt");
        }

        #endregion
    }
}
