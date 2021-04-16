using EasyRpc.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Assets
{
    public abstract class BaseAssetTest : BaseRequestTest
    {
        private List<string> _filesWritten = new List<string>();

        protected Stream AddFile(string fileName)
        {
            _filesWritten.Add(fileName);

            return File.OpenWrite(fileName);
        }

        protected void AddFile(string fileName, byte[] fileBytes)
        {
            _filesWritten.Add(fileName);
        }

        public override void Dispose()
        {
            DeleteFiles();

            base.Dispose();
        }

        private void DeleteFiles()
        {
            foreach (var fileName in _filesWritten)
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
